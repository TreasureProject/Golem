using System;
using UnityEngine;
using UnityEngine.AI;
using Golem;

[RequireComponent(typeof(NavMeshAgent))]
public class PointClickController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask walkableMask = ~0;
    [SerializeField] private float maxSampleDistance = 1.0f;
    [SerializeField] private float stoppingDistance = 0.1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private float animatorDampTime = 0.1f;
    [SerializeField] private bool normalizeSpeed = true;

    [Header("Animator Safety")]
    [Tooltip("Name of a safe animator state to crossfade into when forcing exit from interaction animations.")]
    [SerializeField] private string forcedExitState = "Idle";
    [Tooltip("Substrings to search for in current animation clip names that indicate an interaction state to forcibly exit.")]
    [SerializeField] private string[] interactionStateNameSubstrings = new string[] { "play", "arcade", "claw", "sit", "stand", "lean", "look" };

    [Header("UI Blocking")]
    [Tooltip("Reference to ClaudeCodeController to prevent clicks when mouse is over its UI.")]
    [SerializeField] private ClaudeCodeController claudeCodeController;

    [Header("Interactions")]
    private RaycastHit hit;
    private Collider disabledCollider;
    private Transform interactionSpot;
    private InteractableObject currentInteractable;
    private Vector3 interactableForwardDirection = new(0, 0, 0);
    private bool goingToSit = false;
    private bool isSitting = false;
    private bool goingToLook = false;
    private bool isLooking = false;
    private bool goingToLean = false;
    private bool isLeaning = false;
    private bool goingToPlayClaw = false;
    private bool isPlayingClaw = false;
    private bool goingToPlayArcade = false;
    private bool isPlayingArcade = false;
    private bool isStandingUp = false;
    private bool enteredStandState = false;
    private Vector3 pendingDestination = Vector3.zero;
    private Transform pendingExamineInteraction = null;
    private Transform lookAtTargetAfterArrival = null;
    private Coroutine currentRotationCoroutine = null;

    // Stuck detection
    private Vector3 lastPosition = Vector3.zero;
    private float stuckTimer = 0f;
    private float stuckCheckInterval = 0.5f;
    private float stuckThreshold = 0.1f;
    private Vector3 currentDestination = Vector3.zero;
    private float stuckRepathTime = 0f;

    private NavMeshAgent agent;

    public float rotationSpeed = 180f;

    // Public state accessors and events
    public bool IsSitting => isSitting;
    public bool IsStandingUp => isStandingUp;
    public bool IsLooking => isLooking;
    public bool IsLeaning => isLeaning;
    public bool IsPlayingClaw => isPlayingClaw;
    public bool IsPlayingArcade => isPlayingArcade;
    public event Action OnFinishedStanding;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (cam == null) cam = Camera.main;
        agent.stoppingDistance = stoppingDistance;

        agent.autoRepath = true;
        if (agent.obstacleAvoidanceType == ObstacleAvoidanceType.NoObstacleAvoidance)
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("PointClickController: No Animator found on object or children.");
            }
        }

        if (claudeCodeController == null)
        {
            claudeCodeController = FindObjectOfType<ClaudeCodeController>();
        }
    }

    void Update()
    {
        if (!enabled) return;
        if (cam == null) return;

        CheckForStuckAndRepath();

        if (Input.GetMouseButtonDown(0))
        {
            if (claudeCodeController != null && claudeCodeController.IsMouseOverUI())
                return;

            if (disabledCollider != null)
                disabledCollider.enabled = true;

            if (isSitting && !isStandingUp)
            {
                Debug.Log("Standing up from sitting.");
                CancelInteractions();
                CheckAndForceExitInteractionState(true);
                animator.SetTrigger("ToStand");
                isStandingUp = true;
                enteredStandState = false;

                Ray pendingRay = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(pendingRay, out hit, 100f, walkableMask))
                {
                    pendingDestination = hit.point;
                }
                return;
            }
            else if (isLooking)
            {
                isLooking = false;
                agent.enabled = true;
                animator.SetBool("LookingDown", false);
                if (currentInteractable != null) currentInteractable.SetOccupied(false);
            }
            else if (isLeaning)
            {
                isLeaning = false;
                agent.enabled = true;
                animator.SetBool("Leaning", false);
                if (currentInteractable != null) currentInteractable.SetOccupied(false);
            }
            else if (isPlayingClaw)
            {
                isPlayingClaw = false;
                agent.enabled = true;
                animator.SetBool("PlayingClawMachine", false);
                if (currentInteractable != null) currentInteractable.SetOccupied(false);
            }
            else if (isPlayingArcade)
            {
                isPlayingArcade = false;
                agent.enabled = true;
                animator.SetTrigger("ToStopArcade");
                if (currentInteractable != null) currentInteractable.SetOccupied(false);
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100f, walkableMask))
            {
                // Check for InteractableObject on hit object or its parents
                var interactable = hit.collider.GetComponentInParent<InteractableObject>();

                if (interactable != null && interactable.CanInteract())
                {
                    currentInteractable = interactable;

                    // Determine interaction based on affordances
                    if (interactable.HasAffordance(Affordances.Sit))
                    {
                        Debug.Log($"Going to sit at: {interactable.displayName}");
                        goingToSit = true;
                        disabledCollider = hit.collider;
                        disabledCollider.enabled = false;
                        MoveCharacterToInteractable(interactable);
                    }
                    else if (interactable.HasAffordance(Affordances.Examine) || interactable.HasAffordance(Affordances.LookAt))
                    {
                        Debug.Log($"Going to examine: {interactable.displayName}");
                        goingToLook = true;
                        MoveCharacterToInteractable(interactable);
                    }
                    else if (interactable.HasAffordance(Affordances.Lean))
                    {
                        Debug.Log($"Going to lean at: {interactable.displayName}");
                        goingToLean = true;
                        disabledCollider = hit.collider;
                        disabledCollider.enabled = false;
                        MoveCharacterToInteractable(interactable);
                    }
                    else if (interactable.HasAffordance(Affordances.Play))
                    {
                        Debug.Log($"Going to play: {interactable.displayName}");
                        // Check object type to distinguish claw vs arcade
                        if (interactable.objectType == "claw" || interactable.displayName.ToLower().Contains("claw"))
                        {
                            goingToPlayClaw = true;
                        }
                        else
                        {
                            goingToPlayArcade = true;
                        }
                        disabledCollider = hit.collider;
                        disabledCollider.enabled = false;
                        MoveCharacterToInteractable(interactable);
                    }
                    else if (interactable.HasAffordance(Affordances.Use))
                    {
                        Debug.Log($"Going to use: {interactable.displayName}");
                        // Default use behavior - just move to interaction point
                        MoveCharacterToInteractable(interactable);
                    }
                }
                else
                {
                    // No interactable - just move to point
                    goingToLook = false;
                    goingToSit = false;
                    goingToLean = false;
                    goingToPlayClaw = false;
                    goingToPlayArcade = false;
                    currentInteractable = null;
                    MoveCharacterToPoint(hit.point);
                }
            }
        }

        // Check if standing up animation finished
        if (isStandingUp)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool inTransition = animator.IsInTransition(0);

            if (!enteredStandState && stateInfo.IsName("SitToStand"))
            {
                enteredStandState = true;
            }

            if (enteredStandState && !stateInfo.IsName("SitToStand") && !inTransition)
            {
                isStandingUp = false;
                isSitting = false;
                agent.enabled = true;

                if (currentInteractable != null)
                {
                    currentInteractable.SetOccupied(false);
                    currentInteractable = null;
                }

                if (pendingDestination != Vector3.zero)
                {
                    MoveCharacterToPoint(pendingDestination);
                    pendingDestination = Vector3.zero;
                    pendingExamineInteraction = null;
                }
                else if (pendingExamineInteraction != null)
                {
                    ExamineAtInteractionSpot(pendingExamineInteraction);
                    pendingExamineInteraction = null;
                }

                OnFinishedStanding?.Invoke();
            }
        }

        // Update animator speed parameter
        if (animator != null && agent != null)
        {
            bool arrived = agent.enabled && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;

            if (goingToSit && interactionSpot != null && !arrived)
            {
                float distToInteractionSpot = Vector3.Distance(transform.position, interactionSpot.position);
                if (distToInteractionSpot <= 0.6f)
                {
                    arrived = true;
                }
            }

            float v;
            if (arrived)
            {
                v = 0f;

                if (lookAtTargetAfterArrival != null && !goingToSit && !goingToLook && !goingToLean && !goingToPlayClaw && !goingToPlayArcade)
                {
                    RotateTowardsTarget(lookAtTargetAfterArrival);
                    lookAtTargetAfterArrival = null;
                }

                if (goingToSit)
                {
                    HandleArrivalAtSit();
                }
                else if (goingToLook)
                {
                    HandleArrivalAtLook();
                }
                else if (goingToLean)
                {
                    HandleArrivalAtLean();
                }
                else if (goingToPlayClaw)
                {
                    HandleArrivalAtClaw();
                }
                else if (goingToPlayArcade)
                {
                    HandleArrivalAtArcade();
                }
            }
            else
            {
                v = agent.desiredVelocity.magnitude;
                if (v <= 0.0001f)
                    v = agent.velocity.magnitude;
            }

            float value = normalizeSpeed ? (agent.speed > 0f ? v / agent.speed : 0f) : v;
            animator.SetFloat(speedParameter, value, animatorDampTime, Time.deltaTime);
        }
    }

    private void HandleArrivalAtSit()
    {
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.enabled = false;

        if (interactionSpot != null)
        {
            transform.position = interactionSpot.position;
            bool wasUpdateRotation = agent.updateRotation;
            agent.updateRotation = false;
            transform.rotation = interactionSpot.rotation;
            agent.updateRotation = wasUpdateRotation;
            Debug.Log($"Sitting: Snapped to position {interactionSpot.position}");
        }

        goingToSit = false;
        isSitting = true;

        if (currentInteractable != null)
        {
            currentInteractable.SetOccupied(true);
            currentInteractable.BeginInteraction(Affordances.Sit);
        }

        animator.ResetTrigger("ToPlayArcade");
        animator.ResetTrigger("ToStopArcade");
        animator.ResetTrigger("ToStand");
        animator.ResetTrigger("ToSit");
        animator.SetBool("PlayingClawMachine", false);
        animator.SetTrigger("ToSit");
    }

    private void HandleArrivalAtLook()
    {
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
        }
        agent.enabled = false;
        transform.SetPositionAndRotation(interactionSpot.position, interactionSpot.rotation);
        goingToLook = false;
        isLooking = true;

        if (currentInteractable != null)
        {
            currentInteractable.SetOccupied(true);
            currentInteractable.BeginInteraction(Affordances.Examine);
        }

        animator.SetBool("LookingDown", true);
    }

    private void HandleArrivalAtLean()
    {
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
        }
        Debug.Log("Leaning at the slot machine.");
        agent.enabled = false;
        transform.SetPositionAndRotation(interactionSpot.position, interactionSpot.rotation);
        goingToLean = false;
        isLeaning = true;

        if (currentInteractable != null)
        {
            currentInteractable.SetOccupied(true);
            currentInteractable.BeginInteraction(Affordances.Lean);
        }

        animator.SetBool("Leaning", true);
    }

    private void HandleArrivalAtClaw()
    {
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
        }
        Debug.Log("Playing the claw machine.");
        agent.enabled = false;
        transform.SetPositionAndRotation(interactionSpot.position, interactionSpot.rotation);
        goingToPlayClaw = false;
        isPlayingClaw = true;

        if (currentInteractable != null)
        {
            currentInteractable.SetOccupied(true);
            currentInteractable.BeginInteraction(Affordances.Play);
        }

        animator.ResetTrigger("ToPlayArcade");
        animator.ResetTrigger("ToStopArcade");
        animator.SetBool("LookingDown", false);
        animator.SetBool("Leaning", false);
        animator.SetBool("PlayingClawMachine", true);
    }

    private void HandleArrivalAtArcade()
    {
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
        }
        Debug.Log("Playing the arcade.");
        agent.enabled = false;
        transform.SetPositionAndRotation(interactionSpot.position, interactionSpot.rotation);
        goingToPlayArcade = false;
        isPlayingArcade = true;

        if (currentInteractable != null)
        {
            currentInteractable.SetOccupied(true);
            currentInteractable.BeginInteraction(Affordances.Play);
        }

        animator.ResetTrigger("ToSit");
        animator.ResetTrigger("ToStand");
        animator.SetBool("LookingDown", false);
        animator.SetBool("Leaning", false);
        animator.SetBool("PlayingClawMachine", false);
        animator.SetTrigger("ToPlayArcade");
    }

    private void MoveCharacterToInteractable(InteractableObject interactable)
    {
        interactionSpot = interactable.InteractionTransform;

        // Set look-at target from interactable if available
        if (interactable.lookAtTarget != null)
        {
            lookAtTargetAfterArrival = interactable.lookAtTarget;
        }

        // For seats, use better pathfinding to avoid walking through
        if (interactable.HasAffordance(Affordances.Sit))
        {
            MoveToSeatWithBetterPathfinding(interactionSpot);
        }
        else
        {
            MoveCharacterToPoint(interactable.InteractionPosition);
        }
    }

    private void MoveToSeatWithBetterPathfinding(Transform seatInteractionSpot)
    {
        Vector3 seatForward = seatInteractionSpot.forward;
        Vector3 approachPosition = seatInteractionSpot.position - seatForward * 0.5f;

        if (NavMesh.SamplePosition(approachPosition, out NavMeshHit approachHit, maxSampleDistance * 2f, NavMesh.AllAreas))
        {
            MoveCharacterToPoint(approachHit.position);
            Debug.Log($"Moving to seat via approach position: {approachHit.position}");
        }
        else
        {
            MoveCharacterToPoint(seatInteractionSpot.position);
            Debug.LogWarning("Could not find approach position for seat, using direct position");
        }
    }

    private void MoveCharacterToPoint(Vector3 targetPoint)
    {
        if (targetPoint == Vector3.zero)
            return;

        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
        }

        if (animator != null)
        {
            var clips = animator.GetCurrentAnimatorClipInfo(0);
            string clipName = clips.Length > 0 && clips[0].clip != null ? clips[0].clip.name.ToLower() : "";

            if (clipName.Contains("sit") || clipName.Contains("lean") || clipName.Contains("look") ||
                clipName.Contains("play") || clipName.Contains("arcade") || clipName.Contains("claw"))
            {
                Debug.Log($"MoveCharacterToPoint: Forcing animator exit from '{clipName}' to Idle before movement.");
                animator.CrossFade(forcedExitState, 0.02f);
                animator.SetBool("PlayingClawMachine", false);
                animator.SetBool("LookingDown", false);
                animator.SetBool("Leaning", false);
                animator.ResetTrigger("ToSit");
                animator.ResetTrigger("ToPlayArcade");

                isSitting = false;
                isLooking = false;
                isLeaning = false;
                isPlayingClaw = false;
                isPlayingArcade = false;
            }
        }

        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (!agent.enabled)
            agent.enabled = true;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit agentHit, maxSampleDistance * 2f, NavMesh.AllAreas))
            {
                agent.Warp(agentHit.position);
            }
            else
            {
                Debug.LogWarning("PointClickController: Agent is not on NavMesh and no nearby NavMesh point found.");
                return;
            }
        }

        if (NavMesh.SamplePosition(targetPoint, out NavMeshHit navHit, maxSampleDistance, NavMesh.AllAreas))
        {
            if (!goingToSit && !goingToLook && !goingToLean && !goingToPlayClaw && !goingToPlayArcade)
            {
                Vector3 directionToTarget = (navHit.position - transform.position);
                directionToTarget.y = 0;

                if (directionToTarget.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    if (currentRotationCoroutine != null)
                    {
                        StopCoroutine(currentRotationCoroutine);
                    }
                    currentRotationCoroutine = StartCoroutine(QuickRotateToTarget(targetRotation));
                }
            }

            agent.SetDestination(navHit.position);
            currentDestination = navHit.position;
            lastPosition = transform.position;
            stuckTimer = 0f;
            stuckRepathTime = 0f;
        }
        else
        {
            if (!goingToSit && !goingToLook && !goingToLean && !goingToPlayClaw && !goingToPlayArcade)
            {
                Vector3 directionToTarget = (targetPoint - transform.position);
                directionToTarget.y = 0;

                if (directionToTarget.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    if (currentRotationCoroutine != null)
                    {
                        StopCoroutine(currentRotationCoroutine);
                        currentRotationCoroutine = null;
                    }
                    transform.rotation = targetRotation;
                }
            }

            agent.SetDestination(targetPoint);
            currentDestination = targetPoint;
            lastPosition = transform.position;
            stuckTimer = 0f;
            stuckRepathTime = 0f;
        }
    }

    private void CheckForStuckAndRepath()
    {
        if (agent == null || !agent.enabled) return;
        if (!agent.hasPath || agent.pathPending) return;

        if (goingToSit || goingToLook || goingToLean || goingToPlayClaw || goingToPlayArcade ||
            isSitting || isLooking || isLeaning || isPlayingClaw || isPlayingArcade || isStandingUp)
        {
            lastPosition = transform.position;
            stuckTimer = 0f;
            stuckRepathTime = 0f;
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            lastPosition = transform.position;
            stuckTimer = 0f;
            stuckRepathTime = 0f;
            return;
        }

        stuckTimer += Time.deltaTime;

        if (stuckTimer >= stuckCheckInterval)
        {
            stuckTimer = 0f;

            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < stuckThreshold)
            {
                if (agent.remainingDistance > agent.stoppingDistance + 0.5f &&
                    agent.velocity.magnitude < 0.1f)
                {
                    stuckRepathTime += stuckCheckInterval;

                    if (stuckRepathTime >= 1f)
                    {
                        Debug.LogWarning($"Character appears stuck, attempting to recalculate path. Distance: {agent.remainingDistance}");

                        Vector3 dest = currentDestination;
                        if (dest == Vector3.zero && agent.hasPath)
                        {
                            dest = agent.destination;
                        }

                        if (dest != Vector3.zero)
                        {
                            if (NavMesh.SamplePosition(dest, out NavMeshHit hit, maxSampleDistance * 2f, NavMesh.AllAreas))
                            {
                                agent.SetDestination(hit.position);
                                currentDestination = hit.position;
                                Debug.Log($"Recalculated path to: {hit.position}");
                            }
                            else
                            {
                                Vector3 nearbyPos = transform.position + transform.forward * 2f;
                                if (NavMesh.SamplePosition(nearbyPos, out NavMeshHit nearbyHit, maxSampleDistance * 3f, NavMesh.AllAreas))
                                {
                                    agent.SetDestination(nearbyHit.position);
                                    currentDestination = nearbyHit.position;
                                    Debug.Log($"Trying nearby position: {nearbyHit.position}");
                                }
                            }
                        }

                        stuckRepathTime = 0f;
                    }
                }
                else
                {
                    stuckRepathTime = 0f;
                }
            }
            else
            {
                stuckRepathTime = 0f;
            }

            lastPosition = transform.position;
        }
    }

    private System.Collections.IEnumerator QuickRotateToTarget(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRotation;
        currentRotationCoroutine = null;
    }

    public void SetLookAtTargetAfterArrival(Transform target)
    {
        lookAtTargetAfterArrival = target;
    }

    private void RotateTowardsTarget(Transform target)
    {
        if (target == null) return;

        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
        }

        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0;

        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            currentRotationCoroutine = StartCoroutine(QuickRotateToTarget(targetRotation));
            Debug.Log($"Rotating towards target: {target.name}");
        }
    }

    // Public API for other systems
    public void MoveToPointPublic(Vector3 point)
    {
        if (isSitting && !isStandingUp)
        {
            Debug.Log("MoveToPointPublic: Character is sitting, triggering stand up.");
            pendingDestination = point;
            CancelInteractions();
            CheckAndForceExitInteractionState(true);
            animator.SetTrigger("ToStand");
            isStandingUp = true;
            enteredStandState = false;
            StartCoroutine(StandUpFallbackWatcher());
            return;
        }

        if (isStandingUp)
        {
            Debug.Log("MoveToPointPublic: Already standing up, updating pending destination.");
            pendingDestination = point;
            goingToLook = goingToSit = goingToLean = goingToPlayClaw = goingToPlayArcade = false;
            pendingExamineInteraction = null;
            return;
        }

        CancelInteractions();

        if (animator != null)
        {
            try
            {
                animator.ResetTrigger("ToPlayArcade");
                animator.ResetTrigger("ToStopArcade");
                animator.ResetTrigger("ToSit");
                animator.ResetTrigger("ToStand");
                animator.SetBool("PlayingClawMachine", false);
                animator.SetBool("LookingDown", false);
                animator.SetBool("Leaning", false);
            }
            catch { }
        }

        isLooking = isLeaning = isPlayingClaw = isPlayingArcade = false;
        goingToLook = goingToSit = goingToLean = goingToPlayClaw = goingToPlayArcade = false;

        if (agent != null && !agent.enabled) agent.enabled = true;

        MoveCharacterToPoint(point);
    }

    public void InteractWithObject(InteractableObject interactable, string affordance)
    {
        if (interactable == null || !interactable.CanInteract(affordance))
        {
            Debug.LogWarning($"Cannot interact with object using affordance '{affordance}'");
            return;
        }

        currentInteractable = interactable;
        interactionSpot = interactable.InteractionTransform;

        if (affordance == Affordances.Sit)
        {
            SitAtInteractionSpot(interactionSpot);
        }
        else if (affordance == Affordances.Examine || affordance == Affordances.LookAt)
        {
            ExamineAtInteractionSpot(interactionSpot);
        }
        else if (affordance == Affordances.Play)
        {
            PlayArcadeAtSpot(interactionSpot);
        }
        else
        {
            // Generic use - just move to the interaction point
            MoveCharacterToInteractable(interactable);
        }
    }

    public void SitAtInteractionSpot(Transform interaction)
    {
        if (isSitting && !isStandingUp)
        {
            Debug.Log("SitAtInteractionSpot: Character is sitting, triggering stand up first.");
            CancelInteractions();
            CheckAndForceExitInteractionState(true);
            animator.SetTrigger("ToStand");
            isStandingUp = true;
            enteredStandState = false;
            StartCoroutine(StandUpFallbackWatcher());
            return;
        }

        if (isStandingUp)
        {
            Debug.Log("SitAtInteractionSpot: Already standing up, ignoring.");
            return;
        }

        CancelInteractions();

        if (disabledCollider != null)
            disabledCollider.enabled = true;

        disabledCollider = interaction.GetComponentInParent<Collider>();
        if (disabledCollider != null)
            disabledCollider.enabled = false;

        interactionSpot = interaction;
        goingToSit = true;
        lookAtTargetAfterArrival = null;

        MoveToSeatWithBetterPathfinding(interaction);
    }

    public void ExamineAtInteractionSpot(Transform interaction)
    {
        if (isSitting && !isStandingUp)
        {
            Debug.Log("ExamineAtInteractionSpot: Character is sitting, triggering stand up first.");
            CancelInteractions();
            CheckAndForceExitInteractionState(true);
            animator.SetTrigger("ToStand");
            isStandingUp = true;
            enteredStandState = false;
            StartCoroutine(StandUpFallbackWatcher());
            return;
        }

        if (isStandingUp)
        {
            pendingExamineInteraction = interaction;
            Debug.Log("ExamineAtInteractionSpot: Already standing up, queuing examine interaction.");
            return;
        }

        CancelInteractions();

        interactionSpot = interaction;
        goingToLook = true;
        lookAtTargetAfterArrival = null;
        MoveCharacterToPoint(interaction.position);
    }

    public void PlayArcadeAtSpot(Transform interaction)
    {
        if (isSitting && !isStandingUp)
        {
            Debug.Log("PlayArcadeAtSpot: Character is sitting, triggering stand up first.");
            CancelInteractions();
            CheckAndForceExitInteractionState(true);
            animator.SetTrigger("ToStand");
            isStandingUp = true;
            enteredStandState = false;
            StartCoroutine(StandUpFallbackWatcher());
            return;
        }

        if (isStandingUp)
        {
            Debug.Log("PlayArcadeAtSpot: Already standing up, ignoring.");
            return;
        }

        CancelInteractions();

        if (disabledCollider != null)
            disabledCollider.enabled = true;

        disabledCollider = interaction.GetComponentInParent<Collider>();
        if (disabledCollider != null)
            disabledCollider.enabled = false;

        interactionSpot = interaction;
        goingToPlayArcade = true;
        lookAtTargetAfterArrival = null;
        MoveCharacterToPoint(interaction.position);
    }

    public void CancelInteractions()
    {
        if (disabledCollider != null)
        {
            try { disabledCollider.enabled = true; } catch { }
            disabledCollider = null;
        }

        if (animator != null)
        {
            try
            {
                animator.ResetTrigger("ToPlayArcade");
                animator.ResetTrigger("ToStopArcade");
                animator.ResetTrigger("ToStand");
                animator.ResetTrigger("ToSit");
            }
            catch { }
        }

        bool wasInInteraction = isLooking || isLeaning || isPlayingClaw || isPlayingArcade;

        if (isLooking)
        {
            isLooking = false;
            if (agent != null) agent.enabled = true;
            if (animator != null) animator.SetBool("LookingDown", false);
        }

        if (isLeaning)
        {
            isLeaning = false;
            if (agent != null) agent.enabled = true;
            if (animator != null) animator.SetBool("Leaning", false);
        }

        if (isPlayingClaw)
        {
            isPlayingClaw = false;
            if (agent != null) agent.enabled = true;
            if (animator != null) animator.SetBool("PlayingClawMachine", false);
        }

        if (isPlayingArcade)
        {
            isPlayingArcade = false;
            if (agent != null) agent.enabled = true;
        }

        // Notify interactable that we're done
        if (currentInteractable != null && wasInInteraction)
        {
            currentInteractable.SetOccupied(false);
        }

        if (wasInInteraction && animator != null)
        {
            animator.SetBool("PlayingClawMachine", false);
            animator.SetBool("LookingDown", false);
            animator.SetBool("Leaning", false);
            animator.CrossFade(forcedExitState, 0.02f);
        }

        goingToLook = goingToSit = goingToLean = goingToPlayClaw = goingToPlayArcade = false;
    }

    public void ForceStandUp()
    {
        if (isSitting && !isStandingUp)
        {
            animator.SetTrigger("ToStand");
            isStandingUp = true;
            enteredStandState = false;
            StartCoroutine(StandUpFallbackWatcher());
        }
    }

    private System.Collections.IEnumerator StandUpFallbackWatcher()
    {
        if (animator == null)
            yield break;

        float timeout = 5f;
        float t = 0f;

        while (t < timeout)
        {
            if (!isStandingUp) yield break;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool inTransition = animator.IsInTransition(0);

            if (!inTransition && stateInfo.normalizedTime >= 1.0f)
            {
                if (isStandingUp)
                {
                    isStandingUp = false;
                    isSitting = false;
                    agent.enabled = true;

                    if (currentInteractable != null)
                    {
                        currentInteractable.SetOccupied(false);
                        currentInteractable = null;
                    }

                    if (pendingDestination != Vector3.zero)
                    {
                        MoveCharacterToPoint(pendingDestination);
                        pendingDestination = Vector3.zero;
                        pendingExamineInteraction = null;
                    }
                    else if (pendingExamineInteraction != null)
                    {
                        ExamineAtInteractionSpot(pendingExamineInteraction);
                        pendingExamineInteraction = null;
                    }

                    OnFinishedStanding?.Invoke();
                }
                yield break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        if (isStandingUp)
        {
            isStandingUp = false;
            isSitting = false;
            agent.enabled = true;

            if (currentInteractable != null)
            {
                currentInteractable.SetOccupied(false);
                currentInteractable = null;
            }

            if (pendingDestination != Vector3.zero)
            {
                MoveCharacterToPoint(pendingDestination);
                pendingDestination = Vector3.zero;
                pendingExamineInteraction = null;
            }
            else if (pendingExamineInteraction != null)
            {
                ExamineAtInteractionSpot(pendingExamineInteraction);
                pendingExamineInteraction = null;
            }

            OnFinishedStanding?.Invoke();
        }
    }

    private void CheckAndForceExitInteractionState(bool forceIfFlags = false)
    {
        if (animator == null) return;

        try
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            string clipNames = "";
            var clips = animator.GetCurrentAnimatorClipInfo(0);
            if (clips != null && clips.Length > 0)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    if (i > 0) clipNames += ",";
                    clipNames += clips[i].clip != null ? clips[i].clip.name : "(null)";
                }
            }

            bool looksLikeInteraction = false;
            var lower = clipNames.ToLower();
            foreach (var sub in interactionStateNameSubstrings)
            {
                if (!string.IsNullOrEmpty(sub) && lower.Contains(sub)) { looksLikeInteraction = true; break; }
            }

            if (looksLikeInteraction || (forceIfFlags && (isPlayingArcade || isPlayingClaw || isLooking || isLeaning)))
            {
                Debug.Log($"PointClickController: Forcing animator exit to state '{forcedExitState}'");
                try
                {
                    animator.CrossFade(forcedExitState, 0.12f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("PointClickController: Failed to crossfade animator: " + e);
                }

                try { animator.ResetTrigger("ToPlayArcade"); } catch { }
                try { animator.ResetTrigger("ToStopArcade"); } catch { }
                try { animator.ResetTrigger("ToSit"); } catch { }
                try { animator.ResetTrigger("ToStand"); } catch { }
                try { animator.SetBool("PlayingClawMachine", false); } catch { }
                try { animator.SetBool("LookingDown", false); } catch { }
                try { animator.SetBool("Leaning", false); } catch { }

                isPlayingArcade = isPlayingClaw = isLooking = isLeaning = false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("PointClickController: Exception while checking animator state: " + ex);
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PointClickController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask walkableMask = ~0;
    [SerializeField] private float maxSampleDistance = 1.0f; // how far to search for closest NavMesh point
    [SerializeField] private float stoppingDistance = 0.1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private float animatorDampTime = 0.1f;
    [SerializeField] private bool normalizeSpeed = true; // if true, send 0..1 based on agent.speed, else send world units/sec

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
    private Transform pendingExamineInteraction = null; // Queued examine interaction when standing up
    private Transform lookAtTargetAfterArrival = null; // Target to rotate towards after arriving at a location
    private Coroutine currentRotationCoroutine = null; // Track ongoing rotation coroutine to stop it if needed

    // Stuck detection
    private Vector3 lastPosition = Vector3.zero;
    private float stuckTimer = 0f;
    private float stuckCheckInterval = 0.5f; // Check every 0.5 seconds
    private float stuckThreshold = 0.1f; // Consider stuck if moved less than 0.1m in 0.5s
    private Vector3 currentDestination = Vector3.zero;
    private float stuckRepathTime = 0f;

    private NavMeshAgent agent;

    public float rotationSpeed = 180f;

    // Public state accessors and events so other controllers can coordinate
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
        
        // Ensure auto-repath is enabled for better obstacle avoidance
        agent.autoRepath = true;
        // Set obstacle avoidance to high quality if not already set
        if (agent.obstacleAvoidanceType == UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance)
        {
            agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("PointClickController: No Animator found on object or children. Assign one in inspector if you want animation driven by movement.");
            }
        }

        // Auto-find ClaudeCodeController if not assigned
        if (claudeCodeController == null)
        {
            claudeCodeController = FindObjectOfType<ClaudeCodeController>();
        }
    }

    void Update()
    {
        if (!enabled) return;
        if (cam == null) return;
        
        // Check for stuck state and handle path recalculation
        CheckForStuckAndRepath();

        if (Input.GetMouseButtonDown(0))
        {
            // Skip if clicking over the ClaudeCodeController UI
            if (claudeCodeController != null && claudeCodeController.IsMouseOverUI())
                return;

            if (disabledCollider != null)
                disabledCollider.enabled = true;

            if (isSitting && !isStandingUp)
            {
                Debug.Log("Standing up from sitting.");
                // ensure any interaction animations are cancelled before transitioning
                CancelInteractions();
                CheckAndForceExitInteractionState(true);
                animator.SetTrigger("ToStand");
                isStandingUp = true;
                enteredStandState = false;

                // Capture the target now so we can move there after standing
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
            }
            else if (isLeaning)
            {
                isLeaning = false;
                agent.enabled = true;
                animator.SetBool("Leaning", false);
            }
            else if (isPlayingClaw) {
                isPlayingClaw = false;
                agent.enabled = true;
                animator.SetBool("PlayingClawMachine", false);
            }
            else if (isPlayingArcade) {
                isPlayingArcade = false;
                agent.enabled = true;
                animator.SetTrigger("ToStopArcade");
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100f, walkableMask))
            {
                if (hit.collider.CompareTag("Caffee Chair"))
                {
                    Debug.Log("Going to a chair.");
                    goingToSit = true;
                    disabledCollider = hit.collider;
                    disabledCollider.enabled = false;
                    MoveCharacterToInteractionSpot();
                }
                else if (hit.collider.CompareTag("Cafe Ad Display"))
                {
                    Debug.Log("Going to an ad.");
                    goingToLook = true;
                    MoveCharacterToInteractionSpot();
                }
                else if (hit.collider.CompareTag("Slot Machine Chair"))
                {
                    Debug.Log("Going to a slot machine chair.");
                    goingToLean = true;
                    disabledCollider = hit.collider;
                    disabledCollider.enabled = false;
                    MoveCharacterToInteractionSpot();
                }
                else if (hit.collider.CompareTag("Claw Machine"))
                {
                    Debug.Log("Going to the claw machine.");
                    goingToPlayClaw = true;
                    disabledCollider = hit.collider;
                    disabledCollider.enabled = false;
                    MoveCharacterToInteractionSpot();
                }
                else if (hit.collider.CompareTag("Arcade"))
                {
                    Debug.Log("Going to the play an arcade.");
                    goingToPlayArcade = true;
                    disabledCollider = hit.collider;
                    disabledCollider.enabled = false;
                    MoveCharacterToInteractionSpot();
                }
                else
                {
                    goingToLook = false;
                    goingToSit = false;
                    goingToLean = false;
                    goingToPlayClaw = false;
                    goingToPlayArcade = false;
                    MoveCharacterToPoint(hit.point);
                }
            }
        }

        // Check if standing up animation finished
        if (isStandingUp)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool inTransition = animator.IsInTransition(0);

            // First, wait until we've actually entered the SitToStand state
            if (!enteredStandState && stateInfo.IsName("SitToStand"))
            {
                enteredStandState = true;
            }

            // Only check for exit after we've confirmed entry, and wait for transition to complete
            if (enteredStandState && !stateInfo.IsName("SitToStand") && !inTransition)
            {
                isStandingUp = false;
                isSitting = false;
                agent.enabled = true;

                // Movement takes priority over examine
                if (pendingDestination != Vector3.zero)
                {
                    MoveCharacterToPoint(pendingDestination);
                    pendingDestination = Vector3.zero;
                    pendingExamineInteraction = null; // Cancel examine if moving
                }
                else if (pendingExamineInteraction != null)
                {
                    // If no movement queued, execute examine interaction
                    ExamineAtInteractionSpot(pendingExamineInteraction);
                    pendingExamineInteraction = null;
                }

                // Notify listeners that standing finished
                OnFinishedStanding?.Invoke();
            }
        }

        // update animator speed parameter based on agent movement intent
        if (animator != null && agent != null)
        {
            // if agent has arrived or is stopping, treat as zero
            bool arrived = agent.enabled && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;
            
            // For sitting, also check if we're close to the InteractionSpot (even if we used an approach position)
            if (goingToSit && interactionSpot != null && !arrived)
            {
                // Also consider arrived if we're close to the InteractionSpot (within 0.6m)
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

                // If we have a look-at target set and we're not in any interaction, rotate towards it
                if (lookAtTargetAfterArrival != null && !goingToSit && !goingToLook && !goingToLean && !goingToPlayClaw && !goingToPlayArcade)
                {
                    RotateTowardsTarget(lookAtTargetAfterArrival);
                    lookAtTargetAfterArrival = null; // Clear after rotating
                }

                if (goingToSit)
                {
                    // Stop any ongoing rotation coroutine immediately
                    if (currentRotationCoroutine != null)
                    {
                        StopCoroutine(currentRotationCoroutine);
                        currentRotationCoroutine = null;
                    }
                    
                    // Stop agent completely before snapping
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                    agent.enabled = false;
                    
                    // Force exact position and rotation from interaction spot
                    if (interactionSpot != null)
                    {
                        transform.position = interactionSpot.position;
                        // Force rotation immediately - don't let any coroutines interfere
                        // Also disable agent update rotation to prevent NavMeshAgent from overriding
                        bool wasUpdateRotation = agent.updateRotation;
                        agent.updateRotation = false;
                        transform.rotation = interactionSpot.rotation;
                        agent.updateRotation = wasUpdateRotation;
                        Debug.Log($"Sitting: Snapped to position {interactionSpot.position}, rotation {interactionSpot.rotation.eulerAngles}");
                    }
                    else
                    {
                        Debug.LogWarning("Sitting: interactionSpot is null! Cannot set position/rotation.");
                    }
                    
                    goingToSit = false;
                    isSitting = true;
                    animator.ResetTrigger("ToPlayArcade");
                    animator.ResetTrigger("ToStopArcade");
                    animator.ResetTrigger("ToStand");
                    animator.ResetTrigger("ToSit");
                    animator.SetBool("PlayingClawMachine", false);
                    animator.SetTrigger("ToSit");
                }
                else if (goingToLook)
                {
                    // Stop any ongoing rotation coroutine
                    if (currentRotationCoroutine != null)
                    {
                        StopCoroutine(currentRotationCoroutine);
                        currentRotationCoroutine = null;
                    }
                    agent.enabled = false;
                    transform.SetPositionAndRotation(interactionSpot.position, interactionSpot.rotation);
                    goingToLook = false;
                    isLooking = true;
                    animator.SetBool("LookingDown", true);
                }
                else if (goingToLean)
                {
                    // Stop any ongoing rotation coroutine
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
                    animator.SetBool("Leaning", true);
                }
                else if (goingToPlayClaw) {
                    // Stop any ongoing rotation coroutine
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
                    // ensure other interaction triggers/bools are cleared
                    animator.ResetTrigger("ToPlayArcade");
                    animator.ResetTrigger("ToStopArcade");
                    animator.SetBool("LookingDown", false);
                    animator.SetBool("Leaning", false);
                    animator.SetBool("PlayingClawMachine", true);
                }
                else if (goingToPlayArcade) {
                    // Stop any ongoing rotation coroutine
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
                    // clear other states and triggers to prevent overlap
                    animator.ResetTrigger("ToSit");
                    animator.ResetTrigger("ToStand");
                    animator.SetBool("LookingDown", false);
                    animator.SetBool("Leaning", false);
                    animator.SetBool("PlayingClawMachine", false);
                    animator.SetTrigger("ToPlayArcade");
                }
            }
            else
            {
                // preferred: desiredVelocity represents the agent's intended movement even while path is being calculated
                v = agent.desiredVelocity.magnitude;

                // fallback to actual velocity if desiredVelocity is zero for some reason
                if (v <= 0.0001f)
                    v = agent.velocity.magnitude;
            }

            float value = normalizeSpeed ? (agent.speed > 0f ? v / agent.speed : 0f) : v;
            animator.SetFloat(speedParameter, value, animatorDampTime, Time.deltaTime);
        }
    }

    private void MoveCharacterToInteractionSpot()
    {
        interactionSpot = hit.collider.transform.Find("InteractionSpot");
        if (interactionSpot != null)
        {
            // For chairs, find a position in front of the chair to avoid walking through it
            if (hit.collider.CompareTag("Caffee Chair"))
            {
                MoveToChairWithBetterPathfinding(interactionSpot);
            }
            else
            {
                MoveCharacterToPoint(interactionSpot.position);
            }
        }
    }

    /// <summary>
    /// Moves to a chair by finding a position in front of it first, avoiding walking through the chair.
    /// </summary>
    private void MoveToChairWithBetterPathfinding(Transform chairInteractionSpot)
    {
        // Calculate a position in front of the chair (offset backwards from InteractionSpot in the direction it's facing)
        Vector3 chairForward = chairInteractionSpot.forward;
        Vector3 approachPosition = chairInteractionSpot.position - chairForward * 0.5f; // Move 0.5m back from the chair
        
        // Try to find a valid NavMesh position near the approach position
        if (NavMesh.SamplePosition(approachPosition, out NavMeshHit approachHit, maxSampleDistance * 2f, NavMesh.AllAreas))
        {
            // Use the approach position for pathfinding
            MoveCharacterToPoint(approachHit.position);
            Debug.Log($"Moving to chair via approach position: {approachHit.position}");
        }
        else
        {
            // Fallback to direct position if we can't find an approach position
            MoveCharacterToPoint(chairInteractionSpot.position);
            Debug.LogWarning("Could not find approach position for chair, using direct position");
        }
    }

    private void MoveCharacterToPoint(Vector3 targetPoint)
    {
        if (targetPoint == Vector3.zero)
            return;

        // Stop any ongoing rotation coroutine when starting a new movement
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
        }

        // Safety: Force animator to Idle before moving to prevent walking in sitting/interaction poses
        // This catches cases where flags are out of sync with animator state
        if (animator != null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            var clips = animator.GetCurrentAnimatorClipInfo(0);
            string clipName = clips.Length > 0 && clips[0].clip != null ? clips[0].clip.name.ToLower() : "";
            
            // If animator is in any interaction-like state, force crossfade to Idle
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
                
                // Also sync internal flags
                isSitting = false;
                isLooking = false;
                isLeaning = false;
                isPlayingClaw = false;
                isPlayingArcade = false;
            }
        }

        // Ensure agent reference
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        // If agent disabled, enable it so we can call SetDestination
        if (!agent.enabled)
            agent.enabled = true;

        // If agent is not currently on the NavMesh, try to place it near current transform
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit agentHit, maxSampleDistance * 2f, NavMesh.AllAreas))
            {
                agent.Warp(agentHit.position);
            }
            else
            {
                Debug.LogWarning("PointClickController: Agent is not on NavMesh and no nearby NavMesh point found. Aborting MoveCharacterToPoint.");
                return;
            }
        }

        if (NavMesh.SamplePosition(targetPoint, out NavMeshHit navHit, maxSampleDistance, NavMesh.AllAreas))
        {
            // Don't rotate during movement if we're going to an interaction - the InteractionSpot will set the correct rotation
            if (!goingToSit && !goingToLook && !goingToLean && !goingToPlayClaw && !goingToPlayArcade)
            {
                // Calculate direction to target and rotate immediately
                Vector3 directionToTarget = (navHit.position - transform.position);
                directionToTarget.y = 0; // Keep rotation on horizontal plane only
                
                if (directionToTarget.sqrMagnitude > 0.01f) // Only rotate if there's meaningful distance
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    // Stop any previous rotation and start new one
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
            // Same for fallback - don't rotate if going to interaction
            if (!goingToSit && !goingToLook && !goingToLean && !goingToPlayClaw && !goingToPlayArcade)
            {
                Vector3 directionToTarget = (targetPoint - transform.position);
                directionToTarget.y = 0;
                
                if (directionToTarget.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    // Stop any previous rotation and set immediately
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

    /// <summary>
    /// Checks if the agent is stuck and attempts to recalculate the path.
    /// </summary>
    private void CheckForStuckAndRepath()
    {
        if (agent == null || !agent.enabled) return;
        
        // Only check if agent is actively trying to move
        if (!agent.hasPath || agent.pathPending) return;
        
        // Don't check if we're in an interaction
        if (goingToSit || goingToLook || goingToLean || goingToPlayClaw || goingToPlayArcade || 
            isSitting || isLooking || isLeaning || isPlayingClaw || isPlayingArcade || isStandingUp)
        {
            lastPosition = transform.position;
            stuckTimer = 0f;
            stuckRepathTime = 0f;
            return;
        }
        
        // Check if we're close to destination (don't check for stuck if we're almost there)
        if (agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            lastPosition = transform.position;
            stuckTimer = 0f;
            stuckRepathTime = 0f;
            return;
        }
        
        stuckTimer += Time.deltaTime;
        
        // Check periodically if we're stuck
        if (stuckTimer >= stuckCheckInterval)
        {
            stuckTimer = 0f;
            
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            
            // If we've moved very little, we might be stuck
            if (distanceMoved < stuckThreshold)
            {
                // Check if we're actually stuck (not just stopped at destination)
                if (agent.remainingDistance > agent.stoppingDistance + 0.5f && 
                    agent.velocity.magnitude < 0.1f)
                {
                    // Try to recalculate path
                    stuckRepathTime += stuckCheckInterval;
                    
                    if (stuckRepathTime >= 1f) // Wait 1 second before trying to repath
                    {
                        Debug.LogWarning($"Character appears stuck, attempting to recalculate path. Distance to destination: {agent.remainingDistance}");
                        
                        // Force path recalculation
                        Vector3 dest = currentDestination;
                        if (dest == Vector3.zero && agent.hasPath)
                        {
                            dest = agent.destination;
                        }
                        
                        if (dest != Vector3.zero)
                        {
                            // Try to find a new path by sampling a nearby position
                            if (NavMesh.SamplePosition(dest, out NavMeshHit hit, maxSampleDistance * 2f, NavMesh.AllAreas))
                            {
                                agent.SetDestination(hit.position);
                                currentDestination = hit.position;
                                Debug.Log($"Recalculated path to: {hit.position}");
                            }
                            else
                            {
                                // If we can't find a path to the destination, try to find a path to a nearby valid position
                                Vector3 nearbyPos = transform.position + transform.forward * 2f;
                                if (NavMesh.SamplePosition(nearbyPos, out NavMeshHit nearbyHit, maxSampleDistance * 3f, NavMesh.AllAreas))
                                {
                                    agent.SetDestination(nearbyHit.position);
                                    currentDestination = nearbyHit.position;
                                    Debug.Log($"Could not reach original destination, trying nearby position: {nearbyHit.position}");
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
                // We're moving, reset stuck timer
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
        currentRotationCoroutine = null; // Clear reference when done
    }

    private bool RotateCharacterTowardsDirection(Vector3 direction)
    {
        if (direction == Vector3.zero)
            return false;

        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float rotationSpeed = 100f;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
        if (angleDifference < 1.0f)
        {
            transform.rotation = targetRotation;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets a target to look at after arriving at the destination. Call this before MoveToPointPublic.
    /// </summary>
    public void SetLookAtTargetAfterArrival(Transform target)
    {
        lookAtTargetAfterArrival = target;
    }

    /// <summary>
    /// Rotates the character to face a target transform.
    /// </summary>
    private void RotateTowardsTarget(Transform target)
    {
        if (target == null) return;
        
        // Stop any ongoing rotation coroutine
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
        }
        
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // Keep rotation on horizontal plane only
        
        if (directionToTarget.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            currentRotationCoroutine = StartCoroutine(QuickRotateToTarget(targetRotation));
            Debug.Log($"Rotating towards target: {target.name}");
        }
    }

    // Public API so other systems (like network events) can drive the character
    public void MoveToPointPublic(Vector3 point)
    {
        // If character is sitting, trigger stand up and queue the move destination
        if (isSitting && !isStandingUp)
        {
            Debug.Log("MoveToPointPublic: Character is sitting, triggering stand up and queuing destination.");
            pendingDestination = point;
            CancelInteractions();
            CheckAndForceExitInteractionState(true);
            animator.SetTrigger("ToStand");
            isStandingUp = true;
            enteredStandState = false;
            StartCoroutine(StandUpFallbackWatcher());
            return;
        }
        
        // If already standing up, just update the pending destination
        // This allows movement commands to override any queued interactions
        if (isStandingUp)
        {
            Debug.Log("MoveToPointPublic: Already standing up, updating pending destination (canceling any queued interactions).");
            pendingDestination = point;
            // Clear any interaction flags and queued interactions
            goingToLook = goingToSit = goingToLean = goingToPlayClaw = goingToPlayArcade = false;
            pendingExamineInteraction = null; // Cancel any queued examine interaction
            return;
        }

        // Aggressively cancel interactions so movement isn't stuck in an interaction animation
        CancelInteractions();

        // Additional animator safety: reset common triggers that might keep interaction states alive
        if (animator != null)
        {
            try
            {
                animator.ResetTrigger("ToPlayArcade");
                animator.ResetTrigger("ToStopArcade");
                animator.ResetTrigger("ToSit");
                animator.ResetTrigger("ToStand");
                // Ensure interaction bools are cleared
                animator.SetBool("PlayingClawMachine", false);
                animator.SetBool("LookingDown", false);
                animator.SetBool("Leaning", false);
            }
            catch { }
        }

        // Also clear internal flags
        isLooking = isLeaning = isPlayingClaw = isPlayingArcade = false;
        goingToLook = goingToSit = goingToLean = goingToPlayClaw = goingToPlayArcade = false;

        // Ensure agent is enabled for movement
        if (agent != null && !agent.enabled) agent.enabled = true;

        MoveCharacterToPoint(point);
    }

    public void SitAtInteractionSpot(Transform interaction)
    {
        // If character is sitting, trigger stand up first - the caller should queue this action
        if (isSitting && !isStandingUp)
        {
            Debug.Log("SitAtInteractionSpot: Character is sitting, triggering stand up first.");
            CancelInteractions();
            CheckAndForceExitInteractionState(true);
            animator.SetTrigger("ToStand");
            isStandingUp = true;
            enteredStandState = false;
            StartCoroutine(StandUpFallbackWatcher());
            // Store the interaction spot so we can go there after standing - but caller should handle queueing
            return;
        }

        if (isStandingUp)
        {
            Debug.Log("SitAtInteractionSpot: Already standing up, ignoring (caller should queue).");
            return;
        }

        // ensure previous interactions are fully cancelled
        CancelInteractions();

        if (disabledCollider != null)
            disabledCollider.enabled = true;

        disabledCollider = interaction.GetComponentInParent<Collider>();
        if (disabledCollider != null)
            disabledCollider.enabled = false;

        interactionSpot = interaction;
        goingToSit = true;
        // Clear any look-at target since InteractionSpot will set the correct rotation
        lookAtTargetAfterArrival = null;
        
        // For chairs, use better pathfinding to avoid walking through the chair
        // Check if this is a chair by looking at the parent collider tag
        Collider parentCollider = interaction.GetComponentInParent<Collider>();
        if (parentCollider != null && parentCollider.CompareTag("Caffee Chair"))
        {
            MoveToChairWithBetterPathfinding(interaction);
        }
        else
        {
            MoveCharacterToPoint(interactionSpot.position);
        }
    }

    public void ExamineAtInteractionSpot(Transform interaction)
    {
        // If character is sitting, trigger stand up first - the caller should queue this action
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
            // If standing up, queue the examine action to happen after standing completes
            // Store the interaction spot so we can examine it after standing
            pendingExamineInteraction = interaction;
            Debug.Log("ExamineAtInteractionSpot: Already standing up, queuing examine interaction.");
            return;
        }

        // ensure previous interactions are fully cancelled
        CancelInteractions();

        interactionSpot = interaction;
        goingToLook = true;
        // Clear any look-at target since InteractionSpot will set the correct rotation
        lookAtTargetAfterArrival = null;
        MoveCharacterToPoint(interaction.position);
    }

    public void PlayArcadeAtSpot(Transform interaction)
    {
        // If character is sitting, trigger stand up first - the caller should queue this action
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
            Debug.Log("PlayArcadeAtSpot: Already standing up, ignoring (caller should queue).");
            return;
        }

        // ensure previous interactions are fully cancelled
        CancelInteractions();

        if (disabledCollider != null)
            disabledCollider.enabled = true;

        disabledCollider = interaction.GetComponentInParent<Collider>();
        if (disabledCollider != null)
            disabledCollider.enabled = false;

        interactionSpot = interaction;
        goingToPlayArcade = true;
        // Clear any look-at target since InteractionSpot will set the correct rotation
        lookAtTargetAfterArrival = null;
        MoveCharacterToPoint(interaction.position);
    }

    // Public helper to cancel any current interaction (looking, leaning, playing) immediately
    public void CancelInteractions()
    {
        if (disabledCollider != null)
        {
            try { disabledCollider.enabled = true; } catch { }
            disabledCollider = null;
        }

        // Reset animator triggers that may have been set
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

        // Track if any interaction was active - we'll force crossfade at the end
        bool wasInInteraction = isLooking || isLeaning || isPlayingClaw || isPlayingArcade;

        // Clear flags and restore agent/animator state
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

        // Force crossfade to Idle for any interaction to ensure clean animator state
        if (wasInInteraction && animator != null)
        {
            animator.SetBool("PlayingClawMachine", false);
            animator.SetBool("LookingDown", false);
            animator.SetBool("Leaning", false);
            // Use very fast crossfade to prevent interaction animation showing while moving
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
            // start a fallback watcher to ensure we eventually notify listeners even if animator state names differ
            StartCoroutine(StandUpFallbackWatcher());
        }
    }

    private System.Collections.IEnumerator StandUpFallbackWatcher()
    {
        // If animator not assigned, bail
        if (animator == null)
            yield break;

        float timeout = 5f;
        float t = 0f;

        while (t < timeout)
        {
            // if regular logic already cleared isStandingUp, assume event already fired
            if (!isStandingUp) yield break;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool inTransition = animator.IsInTransition(0);

            // if animator is in a non-transition state and animation has progressed to end, treat as finished
            if (!inTransition && stateInfo.normalizedTime >= 1.0f)
            {
                if (isStandingUp)
                {
                    isStandingUp = false;
                    isSitting = false;
                    agent.enabled = true;

                    // Movement takes priority over examine
                    if (pendingDestination != Vector3.zero)
                    {
                        MoveCharacterToPoint(pendingDestination);
                        pendingDestination = Vector3.zero;
                        pendingExamineInteraction = null; // Cancel examine if moving
                    }
                    else if (pendingExamineInteraction != null)
                    {
                        // If no movement queued, execute examine interaction
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

        // timeout reached, force finish
        if (isStandingUp)
        {
            isStandingUp = false;
            isSitting = false;
            agent.enabled = true;

            // Movement takes priority over examine
            if (pendingDestination != Vector3.zero)
            {
                MoveCharacterToPoint(pendingDestination);
                pendingDestination = Vector3.zero;
                pendingExamineInteraction = null; // Cancel examine if moving
            }
            else if (pendingExamineInteraction != null)
            {
                // If no movement queued, execute examine interaction
                ExamineAtInteractionSpot(pendingExamineInteraction);
                pendingExamineInteraction = null;
            }

            OnFinishedStanding?.Invoke();
        }
    }

    // Inspect animator state and, if it looks like an interaction state, force a crossfade to a safe state.
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
                // join clip names for debugging
                for (int i = 0; i < clips.Length; i++)
                {
                    if (i > 0) clipNames += ",";
                    clipNames += clips[i].clip != null ? clips[i].clip.name : "(null)";
                }
            }

            Debug.Log($"PointClickController: Animator state hash={stateInfo.shortNameHash}, clip(s)={clipNames}, normalizedTime={stateInfo.normalizedTime:F2}");

            // Decide whether to force exit based on clip name substrings or interaction flags
            bool looksLikeInteraction = false;
            var lower = clipNames.ToLower();
            foreach (var sub in interactionStateNameSubstrings)
            {
                if (!string.IsNullOrEmpty(sub) && lower.Contains(sub)) { looksLikeInteraction = true; break; }
            }

            if (looksLikeInteraction || (forceIfFlags && (isPlayingArcade || isPlayingClaw || isLooking || isLeaning)))
            {
                Debug.Log($"PointClickController: Forcing animator exit to state '{forcedExitState}' because interaction detected (clips={clipNames}).");
                try
                {
                    // Crossfade to the configured safe state (short crossfade)
                    animator.CrossFade(forcedExitState, 0.12f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("PointClickController: Failed to crossfade animator to forcedExitState: " + e);
                }

                // clear common interaction params as a safety net
                try { animator.ResetTrigger("ToPlayArcade"); } catch { }
                try { animator.ResetTrigger("ToStopArcade"); } catch { }
                try { animator.ResetTrigger("ToSit"); } catch { }
                try { animator.ResetTrigger("ToStand"); } catch { }
                try { animator.SetBool("PlayingClawMachine", false); } catch { }
                try { animator.SetBool("LookingDown", false); } catch { }
                try { animator.SetBool("Leaning", false); } catch { }

                // also clear internal flags
                isPlayingArcade = isPlayingClaw = isLooking = isLeaning = false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("PointClickController: Exception while checking animator state: " + ex);
        }
    }
}

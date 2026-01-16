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

    private NavMeshAgent agent;

    public float rotationSpeed = 180f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (cam == null) cam = Camera.main;
        agent.stoppingDistance = stoppingDistance;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("PointClickController: No Animator found on object or children. Assign one in inspector if you want animation driven by movement.");
            }
        }
    }

    void Update()
    {
        if (!enabled) return;
        if (cam == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (disabledCollider != null)
                disabledCollider.enabled = true;

            if (isSitting && !isStandingUp)
            {
                Debug.Log("Standing up from sitting.");
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

                if (pendingDestination != Vector3.zero)
                {
                    MoveCharacterToPoint(pendingDestination);
                    pendingDestination = Vector3.zero;
                }
            }
        }

        // update animator speed parameter based on agent movement intent
        if (animator != null && agent != null)
        {
            // if agent has arrived or is stopping, treat as zero
            bool arrived = agent.enabled && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;

            float v;
            if (arrived)
            {
                v = 0f;

                if (goingToSit)
                {
                    agent.enabled = false;
                    transform.SetPositionAndRotation(interactionSpot.position, interactionSpot.rotation);
                    goingToSit = false;
                    isSitting = true;
                    animator.SetTrigger("ToSit");
                }
                else if (goingToLook)
                {
                    agent.enabled = false;
                    transform.SetPositionAndRotation(interactionSpot.position, interactionSpot.rotation);
                    goingToLook = false;
                    isLooking = true;
                    animator.SetBool("LookingDown", true);
                }
                else if (goingToLean)
                {
                    Debug.Log("Leaning at the slot machine.");
                    agent.enabled = false;
                    transform.SetPositionAndRotation(interactionSpot.position, interactionSpot.rotation);
                    goingToLean = false;
                    isLeaning = true;
                    animator.SetBool("Leaning", true);
                }
                else if (goingToPlayClaw) {
                    Debug.Log("Playing the claw machine.");
                    agent.enabled = false;
                    transform.SetPositionAndRotation(interactionSpot.position, interactionSpot.rotation);
                    goingToPlayClaw = false;
                    isPlayingClaw = true;
                    animator.SetBool("PlayingClawMachine", true);
                }
                else if (goingToPlayArcade) {
                    Debug.Log("Playing the arcade.");
                    agent.enabled = false;
                    transform.SetPositionAndRotation(interactionSpot.position, interactionSpot.rotation);
                    goingToPlayArcade = false;
                    isPlayingArcade = true;
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
            MoveCharacterToPoint(interactionSpot.position);
        }
    }

    private void MoveCharacterToPoint(Vector3 targetPoint)
    {
        if (targetPoint == Vector3.zero)
            return;

        if (NavMesh.SamplePosition(targetPoint, out NavMeshHit navHit, maxSampleDistance, NavMesh.AllAreas))
        {
            // Calculate direction to target and rotate immediately
            Vector3 directionToTarget = (navHit.position - transform.position);
            directionToTarget.y = 0; // Keep rotation on horizontal plane only
            
            if (directionToTarget.sqrMagnitude > 0.01f) // Only rotate if there's meaningful distance
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                StartCoroutine(QuickRotateToTarget(targetRotation));
            }
            
            agent.SetDestination(navHit.position);
        }
        else
        {
            // Same for fallback
            Vector3 directionToTarget = (targetPoint - transform.position);
            directionToTarget.y = 0;
            
            if (directionToTarget.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = targetRotation;
            }
            
            agent.SetDestination(targetPoint);
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

    // Public API so other systems (like network events) can drive the character
    public void MoveToPointPublic(Vector3 point)
    {
        goingToLook = goingToSit = goingToLean = goingToPlayClaw = goingToPlayArcade = false;
        MoveCharacterToPoint(point);
    }

    public void SitAtInteractionSpot(Transform interaction)
    {
        if (disabledCollider != null)
            disabledCollider.enabled = true;

        disabledCollider = interaction.GetComponentInParent<Collider>();
        if (disabledCollider != null)
            disabledCollider.enabled = false;

        interactionSpot = interaction;
        goingToSit = true;
        MoveCharacterToPoint(interactionSpot.position);
    }

    public void ExamineAtInteractionSpot(Transform interaction)
    {
        interactionSpot = interaction;
        goingToLook = true;
        MoveCharacterToPoint(interaction.position);
    }

    public void PlayArcadeAtSpot(Transform interaction)
    {
        if (disabledCollider != null)
            disabledCollider.enabled = true;

        disabledCollider = interaction.GetComponentInParent<Collider>();
        if (disabledCollider != null)
            disabledCollider.enabled = false;

        interactionSpot = interaction;
        goingToPlayArcade = true;
        MoveCharacterToPoint(interaction.position);
    }

    public void ForceStandUp()
    {
        if (isSitting && !isStandingUp)
        {
            animator.SetTrigger("ToStand");
            isStandingUp = true;
            enteredStandState = false;
        }
    }
}

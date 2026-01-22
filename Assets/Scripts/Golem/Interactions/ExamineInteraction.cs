using System;
using UnityEngine;

namespace Golem.Interactions
{
    /// <summary>
    /// Handles examine/look interactions with objects.
    /// The agent looks at an object for a period of time.
    /// </summary>
    public class ExamineInteraction : MonoBehaviour, IInteractionHandler
    {
        [Header("References")]
        public InteractionExecutor executor;
        public PointClickController controller;
        public Transform headTransform;

        [Header("Settings")]
        [Tooltip("How long to examine an object.")]
        public float examineTime = 3f;

        [Tooltip("Speed of head/body turn towards object.")]
        public float lookSpeed = 5f;

        private Action<bool, string> currentCallback;
        private InteractableObject currentTarget;
        private bool isExecuting;

        private void Awake()
        {
            if (executor == null)
                executor = GetComponent<InteractionExecutor>();

            if (controller == null)
                controller = GetComponent<PointClickController>();

            if (headTransform == null)
                headTransform = transform; // Fallback to root
        }

        private void Start()
        {
            if (executor != null)
            {
                executor.RegisterHandler(Affordances.Examine, this);
                executor.RegisterHandler("look", this);
            }
        }

        private void OnDestroy()
        {
            if (executor != null)
            {
                executor.UnregisterHandler(Affordances.Examine);
                executor.UnregisterHandler("look");
            }
        }

        public bool CanHandle(string affordance)
        {
            string lower = affordance.ToLowerInvariant();
            return lower == "examine" || lower == "look";
        }

        public void Execute(InteractableObject target, string affordance, Action<bool, string> onComplete)
        {
            if (target == null)
            {
                onComplete?.Invoke(false, "No target specified");
                return;
            }

            currentCallback = onComplete;
            currentTarget = target;
            isExecuting = true;

            // Use controller if available
            if (controller != null)
            {
                controller.InteractWithObject(target, "examine");
                StartCoroutine(MonitorExamine());
            }
            else
            {
                // Manual examine
                StartCoroutine(DoExamine());
            }
        }

        public void Cancel()
        {
            if (isExecuting)
            {
                StopAllCoroutines();
                isExecuting = false;
                currentCallback?.Invoke(false, "Cancelled");
                currentCallback = null;
            }
        }

        private System.Collections.IEnumerator MonitorExamine()
        {
            float timeout = 10f;
            float elapsed = 0f;

            // Wait for controller to enter looking state
            while (elapsed < timeout && !controller.IsLooking)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (controller.IsLooking)
            {
                // Wait for examine time
                yield return new WaitForSeconds(examineTime);
                CompleteExamine(true);
            }
            else
            {
                CompleteExamine(false, "Failed to start examining - timeout");
            }
        }

        private System.Collections.IEnumerator DoExamine()
        {
            if (currentTarget == null)
            {
                CompleteExamine(false, "Target lost");
                yield break;
            }

            Vector3 targetPos = currentTarget.transform.position;
            float elapsed = 0f;

            // Look at target for examine time
            while (elapsed < examineTime)
            {
                if (currentTarget == null)
                {
                    CompleteExamine(false, "Target lost during examination");
                    yield break;
                }

                // Smoothly rotate towards target
                Vector3 direction = (targetPos - transform.position).normalized;
                direction.y = 0; // Keep upright

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRotation,
                        lookSpeed * Time.deltaTime
                    );
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            CompleteExamine(true);
        }

        private void CompleteExamine(bool success, string message = null)
        {
            isExecuting = false;

            // Generate examination result
            if (success && currentTarget != null)
            {
                var result = new ExaminationResult
                {
                    objectId = currentTarget.UniqueId,
                    objectType = currentTarget.objectType,
                    displayName = currentTarget.displayName,
                    description = currentTarget.description,
                    affordances = currentTarget.affordances,
                    distance = Vector3.Distance(transform.position, currentTarget.transform.position),
                    isOccupied = currentTarget.isOccupied
                };

                OnExaminationComplete?.Invoke(result);
            }

            currentTarget = null;
            currentCallback?.Invoke(success, message);
            currentCallback = null;
        }

        /// <summary>
        /// Event fired when examination completes successfully.
        /// Contains detailed information about the examined object.
        /// </summary>
        public event Action<ExaminationResult> OnExaminationComplete;
    }

    /// <summary>
    /// Result of examining an object.
    /// </summary>
    [Serializable]
    public class ExaminationResult
    {
        public string objectId;
        public string objectType;
        public string displayName;
        public string description;
        public string[] affordances;
        public float distance;
        public bool isOccupied;
    }
}

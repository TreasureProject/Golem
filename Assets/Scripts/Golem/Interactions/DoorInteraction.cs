using System;
using UnityEngine;

namespace Golem.Interactions
{
    /// <summary>
    /// Handles open/close interactions with doors and similar objects.
    /// </summary>
    public class DoorInteraction : MonoBehaviour, IInteractionHandler
    {
        [Header("References")]
        public InteractionExecutor executor;
        public Animator animator;

        [Header("Settings")]
        [Tooltip("Animation trigger for opening.")]
        public string openTrigger = "Open";

        [Tooltip("Animation trigger for closing.")]
        public string closeTrigger = "Close";

        [Tooltip("Time to wait for door animation.")]
        public float animationTime = 1f;

        private Action<bool, string> currentCallback;
        private bool isExecuting;

        private void Awake()
        {
            if (executor == null)
                executor = GetComponent<InteractionExecutor>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            if (executor != null)
            {
                executor.RegisterHandler(Affordances.Open, this);
                executor.RegisterHandler(Affordances.Close, this);
            }
        }

        private void OnDestroy()
        {
            if (executor != null)
            {
                executor.UnregisterHandler(Affordances.Open);
                executor.UnregisterHandler(Affordances.Close);
            }
        }

        public bool CanHandle(string affordance)
        {
            return affordance.ToLowerInvariant() == "open" ||
                   affordance.ToLowerInvariant() == "close";
        }

        public void Execute(InteractableObject target, string affordance, Action<bool, string> onComplete)
        {
            if (target == null)
            {
                onComplete?.Invoke(false, "No target specified");
                return;
            }

            currentCallback = onComplete;
            isExecuting = true;

            switch (affordance.ToLowerInvariant())
            {
                case "open":
                    ExecuteOpen(target);
                    break;
                case "close":
                    ExecuteClose(target);
                    break;
                default:
                    isExecuting = false;
                    onComplete?.Invoke(false, $"DoorInteraction cannot handle '{affordance}'");
                    break;
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

        private void ExecuteOpen(InteractableObject door)
        {
            // Check if door has its own animator
            Animator doorAnimator = door.GetComponent<Animator>();

            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger(openTrigger);
            }

            // Also try to find a DoorController or similar component
            var doorController = door.GetComponent<IDoorController>();
            if (doorController != null)
            {
                doorController.Open();
            }

            StartCoroutine(WaitForAnimation(door, true));
        }

        private void ExecuteClose(InteractableObject door)
        {
            Animator doorAnimator = door.GetComponent<Animator>();

            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger(closeTrigger);
            }

            var doorController = door.GetComponent<IDoorController>();
            if (doorController != null)
            {
                doorController.Close();
            }

            StartCoroutine(WaitForAnimation(door, false));
        }

        private System.Collections.IEnumerator WaitForAnimation(InteractableObject door, bool opening)
        {
            yield return new WaitForSeconds(animationTime);

            isExecuting = false;

            // Update door state if it tracks open/closed
            var stateTracker = door.GetComponent<DoorState>();
            if (stateTracker != null)
            {
                stateTracker.isOpen = opening;
            }

            currentCallback?.Invoke(true, null);
            currentCallback = null;
        }
    }

    /// <summary>
    /// Interface for custom door controllers.
    /// Implement this on your door objects for custom behavior.
    /// </summary>
    public interface IDoorController
    {
        void Open();
        void Close();
        bool IsOpen { get; }
    }

    /// <summary>
    /// Simple door state tracker component.
    /// Add to doors to track open/closed state.
    /// </summary>
    public class DoorState : MonoBehaviour
    {
        public bool isOpen = false;
    }
}

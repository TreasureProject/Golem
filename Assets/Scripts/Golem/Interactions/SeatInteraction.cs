using System;
using UnityEngine;

namespace Golem.Interactions
{
    /// <summary>
    /// Handles sit/stand interactions with seatable objects.
    /// Attach to the GolemAgent or as a standalone component.
    /// </summary>
    public class SeatInteraction : MonoBehaviour, IInteractionHandler
    {
        [Header("References")]
        public InteractionExecutor executor;
        public PointClickController controller;
        public Animator animator;

        [Header("Settings")]
        [Tooltip("Time to wait before considering sit complete.")]
        public float sitSettleTime = 1f;

        private Action<bool, string> currentCallback;
        private InteractableObject currentSeat;
        private bool isExecuting;

        private void Awake()
        {
            if (executor == null)
                executor = GetComponent<InteractionExecutor>();

            if (controller == null)
                controller = GetComponent<PointClickController>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            // Register with executor
            if (executor != null)
            {
                executor.RegisterHandler(Affordances.Sit, this);
                executor.RegisterHandler(Affordances.Stand, this);
            }
        }

        private void OnDestroy()
        {
            if (executor != null)
            {
                executor.UnregisterHandler(Affordances.Sit);
                executor.UnregisterHandler(Affordances.Stand);
            }
        }

        public bool CanHandle(string affordance)
        {
            return affordance.ToLowerInvariant() == "sit" ||
                   affordance.ToLowerInvariant() == "stand";
        }

        public void Execute(InteractableObject target, string affordance, Action<bool, string> onComplete)
        {
            currentCallback = onComplete;

            switch (affordance.ToLowerInvariant())
            {
                case "sit":
                    ExecuteSit(target);
                    break;
                case "stand":
                    ExecuteStand();
                    break;
                default:
                    onComplete?.Invoke(false, $"SeatInteraction cannot handle '{affordance}'");
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

        private void ExecuteSit(InteractableObject seat)
        {
            if (seat == null)
            {
                currentCallback?.Invoke(false, "No seat target");
                return;
            }

            if (!seat.CanInteract(Affordances.Sit))
            {
                currentCallback?.Invoke(false, "Seat is occupied or unavailable");
                return;
            }

            currentSeat = seat;
            isExecuting = true;

            // Use controller if available
            if (controller != null)
            {
                controller.InteractWithObject(seat, Affordances.Sit);
                StartCoroutine(WaitForSitComplete());
            }
            else
            {
                // Manual sit - just teleport and mark occupied
                Transform sitPoint = seat.InteractionTransform ?? seat.transform;
                transform.position = sitPoint.position;
                transform.rotation = sitPoint.rotation;
                seat.SetOccupied(true);
                CompleteSit(true);
            }
        }

        private System.Collections.IEnumerator WaitForSitComplete()
        {
            float timeout = 10f;
            float elapsed = 0f;

            // Wait for controller to enter sitting state
            while (elapsed < timeout && !controller.IsSitting)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (controller.IsSitting)
            {
                // Wait for settle time
                yield return new WaitForSeconds(sitSettleTime);
                CompleteSit(true);
            }
            else
            {
                CompleteSit(false, "Failed to sit - timeout");
            }
        }

        private void CompleteSit(bool success, string message = null)
        {
            isExecuting = false;

            if (success && currentSeat != null)
            {
                currentSeat.SetOccupied(true);
            }

            currentCallback?.Invoke(success, message);
            currentCallback = null;
        }

        private void ExecuteStand()
        {
            isExecuting = true;

            if (controller != null)
            {
                controller.ForceStandUp();
                StartCoroutine(WaitForStandComplete());
            }
            else
            {
                // Manual stand
                if (currentSeat != null)
                {
                    currentSeat.SetOccupied(false);
                    currentSeat = null;
                }
                CompleteStand(true);
            }
        }

        private System.Collections.IEnumerator WaitForStandComplete()
        {
            float timeout = 5f;
            float elapsed = 0f;

            // Wait for standing up animation to complete
            while (elapsed < timeout && controller.IsStandingUp)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Release the seat
            if (currentSeat != null)
            {
                currentSeat.SetOccupied(false);
                currentSeat = null;
            }

            CompleteStand(!controller.IsSitting);
        }

        private void CompleteStand(bool success, string message = null)
        {
            isExecuting = false;
            currentCallback?.Invoke(success, message ?? (success ? null : "Failed to stand"));
            currentCallback = null;
        }
    }
}

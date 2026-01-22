using System;
using System.Collections.Generic;
using UnityEngine;

namespace Golem
{
    /// <summary>
    /// Executes specific interactions based on affordance type.
    /// Routes affordances to the appropriate interaction handler.
    /// </summary>
    public class InteractionExecutor : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The GolemAgent this executor serves.")]
        public GolemAgent agent;

        [Tooltip("The PointClickController for executing interactions.")]
        public PointClickController controller;

        [Tooltip("World memory for recording interaction outcomes.")]
        public WorldMemory memory;

        [Header("Settings")]
        [Tooltip("Default interaction timeout in seconds.")]
        public float defaultTimeout = 30f;

        // Registered interaction handlers
        private Dictionary<string, IInteractionHandler> handlers = new Dictionary<string, IInteractionHandler>();

        // Current interaction state
        private InteractableObject currentTarget;
        private string currentAffordance;
        private float interactionStartTime;
        private bool isExecuting;

        // Events
        public event Action<InteractableObject, string> OnInteractionStarted;
        public event Action<InteractableObject, string, bool> OnInteractionCompleted;
        public event Action<InteractableObject, string, string> OnInteractionFailed;

        private void Awake()
        {
            // Auto-find references
            if (agent == null)
                agent = GetComponent<GolemAgent>();

            if (controller == null)
                controller = GetComponent<PointClickController>();

            if (memory == null)
                memory = GetComponent<WorldMemory>();

            // Register built-in handlers
            RegisterBuiltInHandlers();
        }

        private void RegisterBuiltInHandlers()
        {
            // These will be populated by the specific interaction scripts
            // when they initialize
        }

        /// <summary>
        /// Register a custom interaction handler for an affordance.
        /// </summary>
        public void RegisterHandler(string affordance, IInteractionHandler handler)
        {
            handlers[affordance.ToLowerInvariant()] = handler;
            Debug.Log($"InteractionExecutor: Registered handler for '{affordance}'");
        }

        /// <summary>
        /// Unregister an interaction handler.
        /// </summary>
        public void UnregisterHandler(string affordance)
        {
            handlers.Remove(affordance.ToLowerInvariant());
        }

        /// <summary>
        /// Check if we can execute an interaction with the target.
        /// </summary>
        public bool CanExecute(InteractableObject target, string affordance)
        {
            if (target == null) return false;
            if (isExecuting) return false;
            if (!target.CanInteract(affordance)) return false;

            string key = affordance.ToLowerInvariant();

            // Check if we have a handler or if the controller can handle it
            if (handlers.ContainsKey(key)) return true;

            // Check built-in controller affordances
            return CanControllerHandle(affordance);
        }

        /// <summary>
        /// Execute an interaction with the target using the specified affordance.
        /// </summary>
        public void Execute(InteractableObject target, string affordance)
        {
            if (!CanExecute(target, affordance))
            {
                Debug.LogWarning($"InteractionExecutor: Cannot execute '{affordance}' on {target?.displayName}");
                OnInteractionFailed?.Invoke(target, affordance, "Cannot execute interaction");
                return;
            }

            currentTarget = target;
            currentAffordance = affordance;
            interactionStartTime = Time.time;
            isExecuting = true;

            OnInteractionStarted?.Invoke(target, affordance);

            string key = affordance.ToLowerInvariant();

            // Try custom handler first
            if (handlers.TryGetValue(key, out IInteractionHandler handler))
            {
                handler.Execute(target, affordance, OnHandlerComplete);
                return;
            }

            // Fall back to built-in controller handling
            ExecuteViaController(target, affordance);
        }

        private bool CanControllerHandle(string affordance)
        {
            if (controller == null) return false;

            return affordance.ToLowerInvariant() switch
            {
                "sit" => true,
                "stand" => true,
                "play" => true,
                "examine" or "look" => true,
                "lean" => true,
                _ => false
            };
        }

        private void ExecuteViaController(InteractableObject target, string affordance)
        {
            if (controller == null)
            {
                CompleteInteraction(false, "No controller available");
                return;
            }

            // Route to controller's existing methods
            switch (affordance.ToLowerInvariant())
            {
                case "sit":
                case "play":
                case "examine":
                case "look":
                case "lean":
                    controller.InteractWithObject(target, affordance);
                    // Controller will handle the interaction
                    // We'll monitor for completion
                    StartCoroutine(MonitorControllerInteraction());
                    break;

                case "stand":
                    controller.ForceStandUp();
                    CompleteInteraction(true);
                    break;

                default:
                    CompleteInteraction(false, $"Unknown affordance: {affordance}");
                    break;
            }
        }

        private System.Collections.IEnumerator MonitorControllerInteraction()
        {
            // Wait for controller to finish or timeout
            float timeout = defaultTimeout;

            while (isExecuting && Time.time - interactionStartTime < timeout)
            {
                // Check if controller is done with the interaction
                if (controller != null && !controller.IsStandingUp)
                {
                    // Check if we've entered an interaction state
                    if (controller.IsSitting || controller.IsLeaning ||
                        controller.IsLooking || controller.IsPlayingArcade ||
                        controller.IsPlayingClaw)
                    {
                        CompleteInteraction(true);
                        yield break;
                    }
                }

                yield return new WaitForSeconds(0.1f);
            }

            if (isExecuting)
            {
                CompleteInteraction(false, "Interaction timed out");
            }
        }

        private void OnHandlerComplete(bool success, string message)
        {
            CompleteInteraction(success, message);
        }

        private void CompleteInteraction(bool success, string message = null)
        {
            if (!isExecuting) return;

            isExecuting = false;

            // Record in memory
            if (memory != null && currentTarget != null)
            {
                memory.RecordAffordanceAttempt(currentTarget.objectType, currentAffordance, success);
            }

            if (success)
            {
                OnInteractionCompleted?.Invoke(currentTarget, currentAffordance, true);
            }
            else
            {
                OnInteractionFailed?.Invoke(currentTarget, currentAffordance, message ?? "Unknown failure");
                OnInteractionCompleted?.Invoke(currentTarget, currentAffordance, false);
            }

            currentTarget = null;
            currentAffordance = null;
        }

        /// <summary>
        /// Cancel the current interaction.
        /// </summary>
        public void CancelInteraction()
        {
            if (!isExecuting) return;

            if (controller != null)
            {
                controller.CancelInteractions();
            }

            CompleteInteraction(false, "Interaction cancelled");
        }

        /// <summary>
        /// Check if currently executing an interaction.
        /// </summary>
        public bool IsExecuting => isExecuting;

        /// <summary>
        /// Get the current interaction target.
        /// </summary>
        public InteractableObject CurrentTarget => currentTarget;

        /// <summary>
        /// Get the current affordance being executed.
        /// </summary>
        public string CurrentAffordance => currentAffordance;
    }

    /// <summary>
    /// Interface for custom interaction handlers.
    /// Implement this to add new interaction types.
    /// </summary>
    public interface IInteractionHandler
    {
        /// <summary>
        /// Execute the interaction.
        /// </summary>
        /// <param name="target">The object to interact with.</param>
        /// <param name="affordance">The affordance to use.</param>
        /// <param name="onComplete">Callback when interaction completes (success, message).</param>
        void Execute(InteractableObject target, string affordance, Action<bool, string> onComplete);

        /// <summary>
        /// Check if this handler can handle the given affordance.
        /// </summary>
        bool CanHandle(string affordance);

        /// <summary>
        /// Cancel the current interaction if possible.
        /// </summary>
        void Cancel();
    }
}

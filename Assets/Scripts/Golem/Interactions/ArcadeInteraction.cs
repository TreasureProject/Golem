using System;
using UnityEngine;

namespace Golem.Interactions
{
    /// <summary>
    /// Handles play interactions with arcade machines and similar game objects.
    /// </summary>
    public class ArcadeInteraction : MonoBehaviour, IInteractionHandler
    {
        [Header("References")]
        public InteractionExecutor executor;
        public PointClickController controller;

        [Header("Settings")]
        [Tooltip("Minimum play time before allowing exit.")]
        public float minPlayTime = 5f;

        [Tooltip("Maximum play time before auto-exit.")]
        public float maxPlayTime = 300f;

        private Action<bool, string> currentCallback;
        private InteractableObject currentMachine;
        private float playStartTime;
        private bool isExecuting;

        private void Awake()
        {
            if (executor == null)
                executor = GetComponent<InteractionExecutor>();

            if (controller == null)
                controller = GetComponent<PointClickController>();
        }

        private void Start()
        {
            if (executor != null)
            {
                executor.RegisterHandler(Affordances.Play, this);
            }
        }

        private void OnDestroy()
        {
            if (executor != null)
            {
                executor.UnregisterHandler(Affordances.Play);
            }
        }

        public bool CanHandle(string affordance)
        {
            return affordance.ToLowerInvariant() == "play";
        }

        public void Execute(InteractableObject target, string affordance, Action<bool, string> onComplete)
        {
            if (target == null)
            {
                onComplete?.Invoke(false, "No target specified");
                return;
            }

            if (!target.CanInteract(Affordances.Play))
            {
                onComplete?.Invoke(false, "Machine is occupied or unavailable");
                return;
            }

            currentCallback = onComplete;
            currentMachine = target;
            isExecuting = true;
            playStartTime = Time.time;

            // Use controller if available
            if (controller != null)
            {
                controller.InteractWithObject(target, Affordances.Play);
                StartCoroutine(MonitorPlay());
            }
            else
            {
                // Manual play - just mark as occupied
                target.SetOccupied(true);
                StartCoroutine(SimulatePlay());
            }
        }

        public void Cancel()
        {
            if (isExecuting)
            {
                StopAllCoroutines();
                StopPlaying();
                currentCallback?.Invoke(false, "Cancelled");
                currentCallback = null;
            }
        }

        private System.Collections.IEnumerator MonitorPlay()
        {
            float timeout = 10f;
            float elapsed = 0f;

            // Wait for controller to enter playing state
            while (elapsed < timeout && !IsPlaying())
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (IsPlaying())
            {
                // Successfully started playing
                // Now wait for play to end (by user action, AI decision, or timeout)
                while (IsPlaying() && Time.time - playStartTime < maxPlayTime)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                CompletePlay(true);
            }
            else
            {
                CompletePlay(false, "Failed to start playing - timeout");
            }
        }

        private System.Collections.IEnumerator SimulatePlay()
        {
            // Simulate playing for min time
            yield return new WaitForSeconds(minPlayTime);
            CompletePlay(true);
        }

        private bool IsPlaying()
        {
            if (controller == null) return false;
            return controller.IsPlayingArcade || controller.IsPlayingClaw;
        }

        private void StopPlaying()
        {
            if (controller != null)
            {
                controller.CancelInteractions();
            }

            if (currentMachine != null)
            {
                currentMachine.SetOccupied(false);
                currentMachine = null;
            }

            isExecuting = false;
        }

        private void CompletePlay(bool success, string message = null)
        {
            isExecuting = false;

            if (currentMachine != null)
            {
                currentMachine.SetOccupied(false);
                currentMachine = null;
            }

            currentCallback?.Invoke(success, message);
            currentCallback = null;
        }

        /// <summary>
        /// End the current play session.
        /// Call this when the AI decides to stop playing.
        /// </summary>
        public void EndPlay()
        {
            if (isExecuting && Time.time - playStartTime >= minPlayTime)
            {
                StopPlaying();
                currentCallback?.Invoke(true, "Play session ended");
                currentCallback = null;
            }
        }
    }
}

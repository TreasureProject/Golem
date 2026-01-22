using System;
using UnityEngine;

namespace Golem
{
    /// <summary>
    /// The foundation component that makes any object discoverable and interactive by Golem agents.
    /// Attach this to any GameObject you want agents to be able to find and interact with.
    /// Objects describe themselves - agents discover what's possible at runtime.
    /// </summary>
    public class InteractableObject : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("The type of object (e.g., 'seat', 'door', 'arcade'). Use ObjectTypes constants.")]
        public string objectType = ObjectTypes.Seat;

        [Tooltip("Human-readable name for this object (e.g., 'Red Chair', 'Main Entrance').")]
        public string displayName;

        [Tooltip("Description of the object for agent context.")]
        [TextArea(2, 4)]
        public string description;

        [Header("Affordances")]
        [Tooltip("What actions can be performed on this object (e.g., 'sit', 'examine', 'play').")]
        public string[] affordances = new string[] { Affordances.Use };

        [Header("Interaction")]
        [Tooltip("Where the agent should stand/position to interact. If null, uses object transform.")]
        public Transform interactionPoint;

        [Tooltip("How close the agent needs to be to interact (meters).")]
        public float interactionRadius = 2f;

        [Tooltip("Should the agent face a specific direction when interacting? Uses interactionPoint's forward if set.")]
        public bool useInteractionRotation = true;

        [Header("State")]
        [Tooltip("Is this object currently being used by someone?")]
        public bool isOccupied;

        [Tooltip("Is this object available for interaction?")]
        public bool isEnabled = true;

        [Tooltip("Optional: Another object to look at after interacting with this one.")]
        public Transform lookAtTarget;

        // Events for state changes
        public event Action<InteractableObject> OnOccupied;
        public event Action<InteractableObject> OnVacated;
        public event Action<InteractableObject, string> OnInteractionStarted;
        public event Action<InteractableObject, string> OnInteractionEnded;

        // Unique identifier for this instance
        private string uniqueId;
        public string UniqueId
        {
            get
            {
                if (string.IsNullOrEmpty(uniqueId))
                    uniqueId = $"{objectType}_{GetInstanceID()}";
                return uniqueId;
            }
        }

        /// <summary>
        /// Gets the position where an agent should go to interact with this object.
        /// </summary>
        public Vector3 InteractionPosition => interactionPoint != null ? interactionPoint.position : transform.position;

        /// <summary>
        /// Gets the rotation the agent should have when interacting.
        /// </summary>
        public Quaternion InteractionRotation => interactionPoint != null ? interactionPoint.rotation : transform.rotation;

        /// <summary>
        /// Gets the Transform to use for interaction (interactionPoint if set, otherwise this transform).
        /// </summary>
        public Transform InteractionTransform => interactionPoint != null ? interactionPoint : transform;

        /// <summary>
        /// Check if this object supports a specific affordance.
        /// </summary>
        public bool HasAffordance(string affordance)
        {
            if (affordances == null) return false;
            foreach (var a in affordances)
            {
                if (string.Equals(a, affordance, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if this object can currently be interacted with.
        /// </summary>
        public bool CanInteract()
        {
            return isEnabled && !isOccupied;
        }

        /// <summary>
        /// Check if this object can be interacted with using a specific affordance.
        /// </summary>
        public bool CanInteract(string affordance)
        {
            return CanInteract() && HasAffordance(affordance);
        }

        /// <summary>
        /// Mark the object as occupied by an agent.
        /// </summary>
        public void SetOccupied(bool occupied)
        {
            if (isOccupied == occupied) return;
            isOccupied = occupied;

            if (occupied)
                OnOccupied?.Invoke(this);
            else
                OnVacated?.Invoke(this);
        }

        /// <summary>
        /// Called when an interaction begins.
        /// </summary>
        public void BeginInteraction(string affordance)
        {
            OnInteractionStarted?.Invoke(this, affordance);
        }

        /// <summary>
        /// Called when an interaction ends.
        /// </summary>
        public void EndInteraction(string affordance)
        {
            OnInteractionEnded?.Invoke(this, affordance);
        }

        /// <summary>
        /// Get distance from a world position to the interaction point.
        /// </summary>
        public float GetDistanceFrom(Vector3 position)
        {
            return Vector3.Distance(position, InteractionPosition);
        }

        /// <summary>
        /// Check if a position is within interaction range.
        /// </summary>
        public bool IsInRange(Vector3 position)
        {
            return GetDistanceFrom(position) <= interactionRadius;
        }

        private void Awake()
        {
            // Auto-find InteractionSpot child if not assigned (for backwards compatibility)
            if (interactionPoint == null)
            {
                var spot = transform.Find("InteractionSpot");
                if (spot != null)
                    interactionPoint = spot;
            }

            // Auto-generate display name if not set
            if (string.IsNullOrEmpty(displayName))
                displayName = gameObject.name;
        }

        private void OnValidate()
        {
            // Auto-generate display name in editor
            if (string.IsNullOrEmpty(displayName))
                displayName = gameObject.name;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw interaction radius
            Gizmos.color = isEnabled ? (isOccupied ? Color.yellow : Color.green) : Color.red;
            Gizmos.DrawWireSphere(InteractionPosition, interactionRadius);

            // Draw interaction point
            if (interactionPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(interactionPoint.position, 0.1f);

                // Draw facing direction
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(interactionPoint.position, interactionPoint.forward * 0.5f);
            }

            // Draw look-at target line
            if (lookAtTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(InteractionPosition, lookAtTarget.position);
            }
        }
#endif
    }
}

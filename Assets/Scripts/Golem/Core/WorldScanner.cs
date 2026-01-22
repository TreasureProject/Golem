using System;
using System.Collections.Generic;
using UnityEngine;

namespace Golem
{
    /// <summary>
    /// The agent's "eyes" - discovers InteractableObjects in the environment.
    /// Attach to the agent character to enable world discovery.
    /// </summary>
    public class WorldScanner : MonoBehaviour
    {
        [Header("Scan Settings")]
        [Tooltip("How far to scan for objects (meters).")]
        public float scanRadius = 15f;

        [Tooltip("How often to automatically scan (seconds). Set to 0 to disable auto-scan.")]
        public float scanInterval = 1f;

        [Tooltip("Layer mask for interactable objects.")]
        public LayerMask interactableLayers = ~0;

        [Tooltip("Perform visibility check (raycast) to filter occluded objects.")]
        public bool checkVisibility = false;

        [Tooltip("Height offset for visibility raycast origin.")]
        public float visibilityRayHeight = 1.5f;

        [Header("Results")]
        [Tooltip("All objects currently within scan radius.")]
        public List<InteractableObject> nearbyObjects = new List<InteractableObject>();

        [Tooltip("Objects that passed visibility check (if enabled).")]
        public List<InteractableObject> visibleObjects = new List<InteractableObject>();

        // Events
        public event Action<InteractableObject> OnObjectDiscovered;
        public event Action<InteractableObject> OnObjectLost;
        public event Action<List<InteractableObject>> OnScanComplete;

        // Internal tracking
        private HashSet<InteractableObject> previouslyNearby = new HashSet<InteractableObject>();
        private float lastScanTime = 0f;
        private Collider[] scanBuffer = new Collider[100];

        private void Update()
        {
            if (scanInterval > 0 && Time.time - lastScanTime >= scanInterval)
            {
                ScanNow();
            }
        }

        /// <summary>
        /// Performs an immediate scan of the environment.
        /// </summary>
        public void ScanNow()
        {
            lastScanTime = Time.time;

            // Clear previous results
            nearbyObjects.Clear();
            visibleObjects.Clear();

            // Perform overlap sphere
            int count = Physics.OverlapSphereNonAlloc(transform.position, scanRadius, scanBuffer, interactableLayers);

            HashSet<InteractableObject> currentlyNearby = new HashSet<InteractableObject>();

            for (int i = 0; i < count; i++)
            {
                var interactable = scanBuffer[i].GetComponentInParent<InteractableObject>();
                if (interactable != null && interactable.isEnabled)
                {
                    // Avoid duplicates (multiple colliders on same object)
                    if (currentlyNearby.Contains(interactable))
                        continue;

                    currentlyNearby.Add(interactable);
                    nearbyObjects.Add(interactable);

                    // Check visibility if enabled
                    if (checkVisibility)
                    {
                        if (IsVisible(interactable))
                        {
                            visibleObjects.Add(interactable);
                        }
                    }
                    else
                    {
                        visibleObjects.Add(interactable);
                    }

                    // Fire discovery event for new objects
                    if (!previouslyNearby.Contains(interactable))
                    {
                        OnObjectDiscovered?.Invoke(interactable);
                    }
                }
            }

            // Sort by distance
            nearbyObjects.Sort((a, b) =>
                Vector3.Distance(transform.position, a.InteractionPosition)
                .CompareTo(Vector3.Distance(transform.position, b.InteractionPosition)));

            visibleObjects.Sort((a, b) =>
                Vector3.Distance(transform.position, a.InteractionPosition)
                .CompareTo(Vector3.Distance(transform.position, b.InteractionPosition)));

            // Fire lost event for objects no longer nearby
            foreach (var obj in previouslyNearby)
            {
                if (obj != null && !currentlyNearby.Contains(obj))
                {
                    OnObjectLost?.Invoke(obj);
                }
            }

            previouslyNearby = currentlyNearby;

            OnScanComplete?.Invoke(nearbyObjects);
        }

        /// <summary>
        /// Checks if an object is visible (not occluded by geometry).
        /// </summary>
        private bool IsVisible(InteractableObject obj)
        {
            Vector3 origin = transform.position + Vector3.up * visibilityRayHeight;
            Vector3 target = obj.InteractionPosition + Vector3.up * 0.5f;
            Vector3 direction = target - origin;
            float distance = direction.magnitude;

            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance))
            {
                // Check if we hit the target object or its parent
                var hitInteractable = hit.collider.GetComponentInParent<InteractableObject>();
                return hitInteractable == obj;
            }

            return true; // No obstruction
        }

        /// <summary>
        /// Gets the nearest object of a specific type.
        /// </summary>
        public InteractableObject GetNearest(string objectType)
        {
            foreach (var obj in nearbyObjects)
            {
                if (string.Equals(obj.objectType, objectType, StringComparison.OrdinalIgnoreCase))
                    return obj;
            }
            return null;
        }

        /// <summary>
        /// Gets the nearest object with a specific affordance.
        /// </summary>
        public InteractableObject GetNearestWithAffordance(string affordance)
        {
            foreach (var obj in nearbyObjects)
            {
                if (obj.HasAffordance(affordance) && obj.CanInteract())
                    return obj;
            }
            return null;
        }

        /// <summary>
        /// Gets all objects of a specific type, sorted by distance.
        /// </summary>
        public List<InteractableObject> GetAllOfType(string objectType)
        {
            var results = new List<InteractableObject>();
            foreach (var obj in nearbyObjects)
            {
                if (string.Equals(obj.objectType, objectType, StringComparison.OrdinalIgnoreCase))
                    results.Add(obj);
            }
            return results;
        }

        /// <summary>
        /// Gets all objects with a specific affordance, sorted by distance.
        /// </summary>
        public List<InteractableObject> GetAllWithAffordance(string affordance)
        {
            var results = new List<InteractableObject>();
            foreach (var obj in nearbyObjects)
            {
                if (obj.HasAffordance(affordance))
                    results.Add(obj);
            }
            return results;
        }

        /// <summary>
        /// Gets all objects that can currently be interacted with (enabled and not occupied).
        /// </summary>
        public List<InteractableObject> GetAllAvailable()
        {
            var results = new List<InteractableObject>();
            foreach (var obj in nearbyObjects)
            {
                if (obj.CanInteract())
                    results.Add(obj);
            }
            return results;
        }

        /// <summary>
        /// Finds an object by its unique ID.
        /// </summary>
        public InteractableObject FindById(string uniqueId)
        {
            foreach (var obj in nearbyObjects)
            {
                if (obj.UniqueId == uniqueId)
                    return obj;
            }
            return null;
        }

        /// <summary>
        /// Finds an object by display name (case-insensitive partial match).
        /// </summary>
        public InteractableObject FindByName(string name)
        {
            string lowerName = name.ToLower();
            foreach (var obj in nearbyObjects)
            {
                if (obj.displayName.ToLower().Contains(lowerName))
                    return obj;
            }
            return null;
        }

        /// <summary>
        /// Gets the distance to an interactable object.
        /// </summary>
        public float GetDistanceTo(InteractableObject obj)
        {
            if (obj == null) return float.MaxValue;
            return Vector3.Distance(transform.position, obj.InteractionPosition);
        }

        /// <summary>
        /// Checks if any object with the given affordance is in range.
        /// </summary>
        public bool HasNearbyWithAffordance(string affordance)
        {
            return GetNearestWithAffordance(affordance) != null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw scan radius
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(transform.position, scanRadius);

            // Draw lines to nearby objects
            foreach (var obj in nearbyObjects)
            {
                if (obj == null) continue;
                Gizmos.color = obj.CanInteract() ? Color.green : Color.yellow;
                Gizmos.DrawLine(transform.position + Vector3.up, obj.InteractionPosition);
            }
        }
#endif
    }
}

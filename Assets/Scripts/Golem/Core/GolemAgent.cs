using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Golem
{
    /// <summary>
    /// Central agent controller that ties all Golem components together.
    /// This is the main entry point for AI backends to control a character.
    /// </summary>
    public class GolemAgent : MonoBehaviour
    {
        [Header("Personality")]
        [Tooltip("Character's personality profile. Affects all behavior systems.")]
        public PersonalityProfile personality;

        [Tooltip("Use a preset if no personality is assigned.")]
        public string defaultPreset = "Balanced";

        [Header("Core Components")]
        [Tooltip("WorldScanner for discovering nearby objects. Auto-found if not assigned.")]
        public WorldScanner scanner;

        [Tooltip("WorldMemory for persistent knowledge. Auto-found if not assigned.")]
        public WorldMemory memory;

        [Tooltip("PointClickController for movement and interactions. Auto-found if not assigned.")]
        public PointClickController controller;

        [Header("Movement")]
        [Tooltip("NavMeshAgent for pathfinding. Auto-found if not assigned.")]
        public NavMeshAgent navAgent;

        [Tooltip("Animator for character animations. Auto-found if not assigned.")]
        public Animator animator;

        [Header("State")]
        [Tooltip("Current activity the agent is performing.")]
        public string currentActivity = "idle";

        [Tooltip("Current target object the agent is interacting with.")]
        public InteractableObject currentTarget;

        // Events
        public event Action<string> OnActivityChanged;
        public event Action<InteractableObject, string> OnInteractionStarted;
        public event Action<InteractableObject, string> OnInteractionEnded;
        public event Action<Vector3> OnMovementStarted;
        public event Action OnMovementCompleted;

        // State tracking
        private string previousActivity = "";
        private bool isMoving = false;
        private Vector3 moveDestination;

        private void Awake()
        {
            // Initialize personality from preset if none assigned
            if (personality == null)
            {
                personality = PersonalityPresets.FromName(defaultPreset);
                Debug.Log($"GolemAgent: Using personality preset '{defaultPreset}'");
            }

            // Auto-find components if not assigned
            if (scanner == null)
                scanner = GetComponent<WorldScanner>();
            if (scanner == null)
                scanner = gameObject.AddComponent<WorldScanner>();

            if (memory == null)
                memory = GetComponent<WorldMemory>();
            if (memory == null)
                memory = gameObject.AddComponent<WorldMemory>();

            // Share personality with memory for decay calculations
            if (memory != null)
            {
                memory.personality = personality;
            }

            if (controller == null)
                controller = GetComponent<PointClickController>();

            if (navAgent == null)
                navAgent = GetComponent<NavMeshAgent>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            // Subscribe to scanner events for memory
            if (scanner != null)
            {
                scanner.OnObjectDiscovered += OnObjectDiscovered;
            }

            // Subscribe to controller events
            if (controller != null)
            {
                controller.OnFinishedStanding += OnControllerFinishedStanding;
            }
        }

        private void OnDestroy()
        {
            if (scanner != null)
            {
                scanner.OnObjectDiscovered -= OnObjectDiscovered;
            }

            if (controller != null)
            {
                controller.OnFinishedStanding -= OnControllerFinishedStanding;
            }
        }

        private void OnObjectDiscovered(InteractableObject obj)
        {
            // Remember discovered objects in persistent memory
            if (memory != null)
            {
                memory.RememberObject(obj);
            }
        }

        private void Update()
        {
            UpdateActivity();
            CheckMovementCompletion();
        }

        private void UpdateActivity()
        {
            string newActivity = DetermineCurrentActivity();
            if (newActivity != currentActivity)
            {
                previousActivity = currentActivity;
                currentActivity = newActivity;
                OnActivityChanged?.Invoke(currentActivity);
            }
        }

        private string DetermineCurrentActivity()
        {
            if (controller == null) return "idle";

            if (controller.IsPlayingArcade) return "playing_arcade";
            if (controller.IsPlayingClaw) return "playing_claw";
            if (controller.IsSitting) return "sitting";
            if (controller.IsLeaning) return "leaning";
            if (controller.IsLooking) return "examining";
            if (controller.IsStandingUp) return "standing_up";

            if (navAgent != null && navAgent.enabled && navAgent.hasPath &&
                navAgent.remainingDistance > navAgent.stoppingDistance)
            {
                return "walking";
            }

            return "idle";
        }

        private void CheckMovementCompletion()
        {
            if (!isMoving) return;

            if (navAgent == null || !navAgent.enabled) return;

            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                isMoving = false;
                OnMovementCompleted?.Invoke();
            }
        }

        private void OnControllerFinishedStanding()
        {
            // Agent finished standing up, ready for next action
        }

        #region Public API - High Level Actions

        /// <summary>
        /// Move the agent to a world position.
        /// </summary>
        public void MoveTo(Vector3 position)
        {
            if (controller != null)
            {
                isMoving = true;
                moveDestination = position;
                OnMovementStarted?.Invoke(position);
                controller.MoveToPointPublic(position);
            }
        }

        /// <summary>
        /// Move to and interact with an object using the specified affordance.
        /// </summary>
        public void InteractWith(InteractableObject target, string affordance)
        {
            if (target == null)
            {
                Debug.LogWarning("GolemAgent: Cannot interact with null target");
                return;
            }

            if (!target.CanInteract(affordance))
            {
                Debug.LogWarning($"GolemAgent: Cannot interact with {target.displayName} using '{affordance}'");
                return;
            }

            currentTarget = target;
            OnInteractionStarted?.Invoke(target, affordance);

            if (controller != null)
            {
                controller.InteractWithObject(target, affordance);
            }
        }

        /// <summary>
        /// Move to the nearest object with the specified affordance and interact.
        /// </summary>
        public bool InteractWithNearest(string affordance)
        {
            if (scanner == null)
            {
                Debug.LogWarning("GolemAgent: No scanner attached, cannot find objects");
                return false;
            }

            var target = scanner.GetNearestWithAffordance(affordance);
            if (target == null)
            {
                Debug.Log($"GolemAgent: No nearby object with affordance '{affordance}'");
                return false;
            }

            InteractWith(target, affordance);
            return true;
        }

        /// <summary>
        /// Move to the nearest object of a specific type.
        /// </summary>
        public bool MoveToNearest(string objectType)
        {
            if (scanner == null) return false;

            var target = scanner.GetNearest(objectType);
            if (target == null) return false;

            MoveTo(target.InteractionPosition);
            return true;
        }

        /// <summary>
        /// Sit at the nearest available seat.
        /// </summary>
        public bool SitAtNearestSeat()
        {
            return InteractWithNearest(Affordances.Sit);
        }

        /// <summary>
        /// Stand up from current seated/interaction state.
        /// </summary>
        public void StandUp()
        {
            if (controller != null)
            {
                controller.ForceStandUp();
                controller.CancelInteractions();

                if (currentTarget != null)
                {
                    OnInteractionEnded?.Invoke(currentTarget, currentActivity);
                    currentTarget = null;
                }
            }
        }

        /// <summary>
        /// Cancel all current interactions and stop movement.
        /// </summary>
        public void Stop()
        {
            if (controller != null)
            {
                controller.CancelInteractions();
            }

            if (navAgent != null && navAgent.enabled)
            {
                navAgent.ResetPath();
            }

            isMoving = false;

            if (currentTarget != null)
            {
                OnInteractionEnded?.Invoke(currentTarget, currentActivity);
                currentTarget = null;
            }
        }

        /// <summary>
        /// Explore in a random direction within the scan radius.
        /// Exploration distance is modulated by personality curiosity trait.
        /// </summary>
        public void ExploreRandom()
        {
            // Exploration radius scales with curiosity (0.5x to 1.5x base radius)
            float explorationRadius = scanner.scanRadius * (0.5f + personality.curiosity);

            Vector3 randomDir = UnityEngine.Random.insideUnitSphere * explorationRadius;
            randomDir.y = 0;
            Vector3 targetPos = transform.position + randomDir;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, explorationRadius, NavMesh.AllAreas))
            {
                MoveTo(hit.position);
            }
        }

        /// <summary>
        /// Check if agent should explore based on personality.
        /// Use this for AI decision making.
        /// </summary>
        public bool ShouldExplore()
        {
            return UnityEngine.Random.value < personality.ExplorationChance;
        }

        /// <summary>
        /// Get the agent's personality profile.
        /// </summary>
        public PersonalityProfile Personality => personality;

        /// <summary>
        /// Get the agent's memory system.
        /// </summary>
        public WorldMemory Memory => memory;

        /// <summary>
        /// Check if the agent has learned that an affordance works on an object type.
        /// </summary>
        public bool HasLearnedAffordance(string objectType, string affordance)
        {
            return memory != null && memory.HasLearnedAffordance(objectType, affordance);
        }

        /// <summary>
        /// Get confidence that an affordance works on an object type (0-1).
        /// </summary>
        public float GetAffordanceConfidence(string objectType, string affordance)
        {
            return memory != null ? memory.GetAffordanceConfidence(objectType, affordance) : 0.5f;
        }

        /// <summary>
        /// Record the result of an interaction attempt.
        /// </summary>
        public void RecordInteractionResult(string objectType, string affordance, bool succeeded)
        {
            if (memory != null)
            {
                memory.RecordAffordanceAttempt(objectType, affordance, succeeded);
            }
        }

        /// <summary>
        /// Force an immediate scan of the environment.
        /// </summary>
        public void Scan()
        {
            if (scanner != null)
            {
                scanner.ScanNow();
            }
        }

        #endregion

        #region State Queries

        /// <summary>
        /// Check if the agent is currently idle (not moving or interacting).
        /// </summary>
        public bool IsIdle => currentActivity == "idle";

        /// <summary>
        /// Check if the agent is currently walking.
        /// </summary>
        public bool IsWalking => currentActivity == "walking";

        /// <summary>
        /// Check if the agent is in any interaction state.
        /// </summary>
        public bool IsInteracting => controller != null &&
            (controller.IsSitting || controller.IsLooking || controller.IsLeaning ||
             controller.IsPlayingArcade || controller.IsPlayingClaw);

        /// <summary>
        /// Get all nearby objects the agent can interact with.
        /// </summary>
        public List<InteractableObject> GetNearbyObjects()
        {
            return scanner != null ? scanner.nearbyObjects : new List<InteractableObject>();
        }

        /// <summary>
        /// Get all available actions based on nearby objects.
        /// </summary>
        public List<string> GetAvailableActions()
        {
            var actions = new List<string>();

            if (IsInteracting)
            {
                actions.Add("stand_up");
            }
            else
            {
                actions.Add("explore");

                if (scanner != null)
                {
                    foreach (var obj in scanner.nearbyObjects)
                    {
                        if (!obj.CanInteract()) continue;

                        foreach (var affordance in obj.affordances)
                        {
                            string action = $"{affordance} at {obj.displayName}";
                            if (!actions.Contains(action))
                                actions.Add(action);
                        }
                    }
                }
            }

            return actions;
        }

        #endregion

        #region World State for AI Backend

        /// <summary>
        /// Generates a world state report for AI backends.
        /// </summary>
        public WorldStateReport GenerateWorldState()
        {
            var report = new WorldStateReport
            {
                agentPosition = transform.position,
                agentRotation = transform.eulerAngles,
                agentActivity = currentActivity,
                isInteracting = IsInteracting,
                nearbyObjects = new List<ObjectReport>(),
                availableActions = GetAvailableActions(),
                personality = new PersonalityReport
                {
                    curiosity = personality.curiosity,
                    memoryRetention = personality.memoryRetention,
                    sociability = personality.sociability,
                    caution = personality.caution,
                    routinePreference = personality.routinePreference,
                    adaptability = personality.adaptability,
                    explorationChance = personality.ExplorationChance
                },
                memoryStats = new MemoryStats
                {
                    knownObjectCount = memory != null ? memory.knownObjectCount : 0,
                    learnedAffordanceCount = memory != null ? memory.knownAffordanceCount : 0,
                    visitedZoneCount = memory != null ? memory.knownZoneCount : 0
                }
            };

            if (scanner != null)
            {
                foreach (var obj in scanner.nearbyObjects)
                {
                    report.nearbyObjects.Add(new ObjectReport
                    {
                        id = obj.UniqueId,
                        type = obj.objectType,
                        name = obj.displayName,
                        description = obj.description,
                        affordances = obj.affordances,
                        distance = scanner.GetDistanceTo(obj),
                        isOccupied = obj.isOccupied,
                        canInteract = obj.CanInteract()
                    });
                }
            }

            return report;
        }

        #endregion
    }

    /// <summary>
    /// Report of current world state for AI backends.
    /// </summary>
    [Serializable]
    public class WorldStateReport
    {
        public Vector3 agentPosition;
        public Vector3 agentRotation;
        public string agentActivity;
        public bool isInteracting;
        public List<ObjectReport> nearbyObjects;
        public List<string> availableActions;
        public PersonalityReport personality;
        public MemoryStats memoryStats;
    }

    /// <summary>
    /// Statistics about agent's memory state.
    /// </summary>
    [Serializable]
    public class MemoryStats
    {
        public int knownObjectCount;
        public int learnedAffordanceCount;
        public int visitedZoneCount;
    }

    /// <summary>
    /// Report of agent's personality for AI backends.
    /// </summary>
    [Serializable]
    public class PersonalityReport
    {
        public float curiosity;
        public float memoryRetention;
        public float sociability;
        public float caution;
        public float routinePreference;
        public float adaptability;
        public float explorationChance;
    }

    /// <summary>
    /// Report of a single object for AI backends.
    /// </summary>
    [Serializable]
    public class ObjectReport
    {
        public string id;
        public string type;
        public string name;
        public string description;
        public string[] affordances;
        public float distance;
        public bool isOccupied;
        public bool canInteract;
    }
}

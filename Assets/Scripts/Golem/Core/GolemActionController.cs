using System;
using System.Collections.Generic;
using UnityEngine;

namespace Golem
{
    /// <summary>
    /// Handles incoming actions from AI backends via WebSocket.
    /// Replaces the hardcoded CelesteActionController with a generic system.
    /// </summary>
    public class GolemActionController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The GolemAgent to control.")]
        public GolemAgent agent;

        [Tooltip("Optional: CFConnector for WebSocket communication.")]
        public CFConnector connector;

        [Header("Camera")]
        [Tooltip("Optional camera controller for camera angle changes.")]
        public GameObject cameraRoot;

        // Action queue for sequential execution
        private Queue<GolemAction> actionQueue = new Queue<GolemAction>();
        private bool isProcessingAction = false;

        private void Awake()
        {
            if (agent == null)
                agent = GetComponent<GolemAgent>();
            if (agent == null)
                agent = FindObjectOfType<GolemAgent>();

            if (connector == null)
                connector = CFConnector.instance;
        }

        private void Start()
        {
            // Subscribe to CFConnector events
            if (connector != null)
            {
                SubscribeToConnector();
            }
        }

        private void OnDestroy()
        {
            if (connector != null)
            {
                UnsubscribeFromConnector();
            }
        }

        private void SubscribeToConnector()
        {
            // Subscribe to the generic action event
            // The connector should send golem_action messages
            connector.OnCelesteAction += HandleLegacyAction;

            Debug.Log("GolemActionController: Subscribed to CFConnector");
        }

        private void UnsubscribeFromConnector()
        {
            connector.OnCelesteAction -= HandleLegacyAction;
        }

        /// <summary>
        /// Handles legacy celeste_action format for backwards compatibility.
        /// </summary>
        private void HandleLegacyAction(CelesteActionData actionData)
        {
            if (actionData?.action == null) return;

            var action = new GolemAction
            {
                type = actionData.action.type,
                parameters = actionData.action.parameters ?? new Dictionary<string, object>()
            };

            HandleAction(action);
        }

        /// <summary>
        /// Main action handler - routes actions to appropriate methods.
        /// </summary>
        public void HandleAction(GolemAction action)
        {
            if (action == null) return;

            Debug.Log($"GolemActionController: Handling action '{action.type}'");

            switch (action.type?.ToLower())
            {
                // Generic actions
                case "interact":
                    HandleInteract(action);
                    break;
                case "moveto":
                case "move_to":
                    HandleMoveTo(action);
                    break;
                case "explore":
                    HandleExplore(action);
                    break;
                case "scan":
                    HandleScan();
                    break;
                case "stop":
                    HandleStop();
                    break;

                // Affordance-based shortcuts
                case "sit":
                    HandleSit(action);
                    break;
                case "standup":
                case "stand_up":
                case "stand":
                    HandleStandUp();
                    break;
                case "examine":
                case "look":
                    HandleExamine(action);
                    break;
                case "play":
                    HandlePlay(action);
                    break;

                // Legacy action support
                case "movetolocation":
                    HandleMoveToLocation(action);
                    break;
                case "sitatchairstickController":
                case "sitatchair":
                    HandleSitAtChair(action);
                    break;
                case "examinemenu":
                    HandleExamineMenu();
                    break;
                case "playarcadegame":
                    HandlePlayArcade();
                    break;
                case "changecameraangle":
                    HandleChangeCameraAngle(action);
                    break;
                case "walkaround":
                    HandleWalkAround();
                    break;
                case "idle":
                    HandleIdle();
                    break;

                default:
                    Debug.LogWarning($"GolemActionController: Unknown action type '{action.type}'");
                    break;
            }
        }

        #region Generic Action Handlers

        private void HandleInteract(GolemAction action)
        {
            string targetType = GetParam<string>(action, "targetType") ?? GetParam<string>(action, "target");
            string targetId = GetParam<string>(action, "targetId");
            string affordance = GetParam<string>(action, "affordance") ?? Affordances.Use;

            InteractableObject target = null;

            // Find target by ID first, then by type
            if (!string.IsNullOrEmpty(targetId))
            {
                target = agent.scanner.FindById(targetId);
            }

            if (target == null && !string.IsNullOrEmpty(targetType))
            {
                target = agent.scanner.GetNearest(targetType);
            }

            // If still no target, try finding by affordance
            if (target == null)
            {
                target = agent.scanner.GetNearestWithAffordance(affordance);
            }

            if (target != null)
            {
                agent.InteractWith(target, affordance);
            }
            else
            {
                Debug.LogWarning($"GolemActionController: No target found for interact action");
            }
        }

        private void HandleMoveTo(GolemAction action)
        {
            string targetId = GetParam<string>(action, "targetId");
            string targetName = GetParam<string>(action, "targetName") ?? GetParam<string>(action, "target");

            InteractableObject target = null;

            if (!string.IsNullOrEmpty(targetId))
            {
                target = agent.scanner.FindById(targetId);
            }
            else if (!string.IsNullOrEmpty(targetName))
            {
                target = agent.scanner.FindByName(targetName);
            }

            if (target != null)
            {
                agent.MoveTo(target.InteractionPosition);
            }
            else
            {
                // Try to parse position coordinates
                float x = GetParam<float>(action, "x");
                float y = GetParam<float>(action, "y");
                float z = GetParam<float>(action, "z");

                if (x != 0 || y != 0 || z != 0)
                {
                    agent.MoveTo(new Vector3(x, y, z));
                }
                else
                {
                    Debug.LogWarning("GolemActionController: No valid target for moveTo action");
                }
            }
        }

        private void HandleExplore(GolemAction action)
        {
            agent.ExploreRandom();
        }

        private void HandleScan()
        {
            agent.Scan();
        }

        private void HandleStop()
        {
            agent.Stop();
        }

        #endregion

        #region Affordance-Based Handlers

        private void HandleSit(GolemAction action)
        {
            string targetId = GetParam<string>(action, "targetId");

            if (!string.IsNullOrEmpty(targetId))
            {
                var target = agent.scanner.FindById(targetId);
                if (target != null)
                {
                    agent.InteractWith(target, Affordances.Sit);
                    return;
                }
            }

            // Sit at nearest available seat
            agent.SitAtNearestSeat();
        }

        private void HandleStandUp()
        {
            agent.StandUp();
        }

        private void HandleExamine(GolemAction action)
        {
            string targetId = GetParam<string>(action, "targetId");

            InteractableObject target = null;

            if (!string.IsNullOrEmpty(targetId))
            {
                target = agent.scanner.FindById(targetId);
            }

            if (target == null)
            {
                target = agent.scanner.GetNearestWithAffordance(Affordances.Examine);
            }

            if (target != null)
            {
                agent.InteractWith(target, Affordances.Examine);
            }
        }

        private void HandlePlay(GolemAction action)
        {
            string targetId = GetParam<string>(action, "targetId");

            InteractableObject target = null;

            if (!string.IsNullOrEmpty(targetId))
            {
                target = agent.scanner.FindById(targetId);
            }

            if (target == null)
            {
                target = agent.scanner.GetNearestWithAffordance(Affordances.Play);
            }

            if (target != null)
            {
                agent.InteractWith(target, Affordances.Play);
            }
        }

        #endregion

        #region Legacy Action Handlers (Backwards Compatibility)

        private void HandleMoveToLocation(GolemAction action)
        {
            string location = GetParam<string>(action, "location");
            if (string.IsNullOrEmpty(location)) return;

            // Try to find a waypoint or zone with matching name
            var target = agent.scanner.FindByName(location);
            if (target != null)
            {
                agent.MoveTo(target.InteractionPosition);
            }
            else
            {
                // Fallback: explore in general direction
                agent.ExploreRandom();
                Debug.Log($"GolemActionController: Location '{location}' not found, exploring randomly");
            }
        }

        private void HandleSitAtChair(GolemAction action)
        {
            agent.SitAtNearestSeat();
        }

        private void HandleExamineMenu()
        {
            agent.InteractWithNearest(Affordances.Examine);
        }

        private void HandlePlayArcade()
        {
            agent.InteractWithNearest(Affordances.Play);
        }

        private void HandleChangeCameraAngle(GolemAction action)
        {
            string angle = GetParam<string>(action, "angle");
            if (string.IsNullOrEmpty(angle)) return;

            if (cameraRoot == null)
                cameraRoot = Camera.main?.gameObject;

            if (cameraRoot != null)
            {
                cameraRoot.SendMessage("ChangeStateByName", angle, SendMessageOptions.DontRequireReceiver);
            }
        }

        private void HandleWalkAround()
        {
            agent.ExploreRandom();
        }

        private void HandleIdle()
        {
            agent.StandUp();
        }

        #endregion

        #region Helpers

        private T GetParam<T>(GolemAction action, string key)
        {
            if (action.parameters == null || !action.parameters.ContainsKey(key))
                return default;

            var value = action.parameters[key];
            if (value == null) return default;

            try
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)value.ToString();

                if (typeof(T) == typeof(float))
                {
                    if (value is double d) return (T)(object)(float)d;
                    if (value is long l) return (T)(object)(float)l;
                    if (value is int i) return (T)(object)(float)i;
                    if (float.TryParse(value.ToString(), out float f)) return (T)(object)f;
                }

                if (typeof(T) == typeof(int))
                {
                    if (value is long l) return (T)(object)(int)l;
                    if (value is double d) return (T)(object)(int)d;
                    if (int.TryParse(value.ToString(), out int i)) return (T)(object)i;
                }

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        #endregion
    }

    /// <summary>
    /// Generic action data structure for Golem.
    /// </summary>
    [Serializable]
    public class GolemAction
    {
        public string type;
        public Dictionary<string, object> parameters = new Dictionary<string, object>();
    }
}

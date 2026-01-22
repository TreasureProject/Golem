using System;
using System.Collections.Generic;
using UnityEngine;

namespace Golem
{
    /// <summary>
    /// Reports world state to the AI backend periodically and on significant changes.
    /// This provides the AI with current context about the agent and environment.
    /// </summary>
    public class GolemStateReporter : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The GolemAgent to report state for.")]
        public GolemAgent agent;

        [Tooltip("CFConnector for sending state updates.")]
        public CFConnector connector;

        [Header("Settings")]
        [Tooltip("How often to send periodic state updates (seconds). Set to 0 to disable.")]
        public float reportInterval = 2f;

        [Tooltip("Send update immediately when activity changes.")]
        public bool reportOnActivityChange = true;

        [Tooltip("Send update when new objects are discovered.")]
        public bool reportOnDiscovery = true;

        // State tracking
        private float lastReportTime = 0f;
        private string lastReportedActivity = "";
        private int lastReportedObjectCount = 0;

        private void Awake()
        {
            if (agent == null)
                agent = GetComponent<GolemAgent>();
            if (agent == null)
                agent = GetComponentInParent<GolemAgent>();
            if (agent == null)
                agent = FindObjectOfType<GolemAgent>();

            if (connector == null)
                connector = CFConnector.instance;
        }

        private void Start()
        {
            // Subscribe to agent events
            if (agent != null)
            {
                agent.OnActivityChanged += OnAgentActivityChanged;
            }

            // Subscribe to scanner events
            if (agent?.scanner != null)
            {
                agent.scanner.OnObjectDiscovered += OnObjectDiscovered;
            }
        }

        private void OnDestroy()
        {
            if (agent != null)
            {
                agent.OnActivityChanged -= OnAgentActivityChanged;
            }

            if (agent?.scanner != null)
            {
                agent.scanner.OnObjectDiscovered -= OnObjectDiscovered;
            }
        }

        private void Update()
        {
            if (reportInterval > 0 && Time.time - lastReportTime >= reportInterval)
            {
                SendStateUpdate();
            }
        }

        private void OnAgentActivityChanged(string newActivity)
        {
            if (reportOnActivityChange)
            {
                SendStateUpdate();
            }
        }

        private void OnObjectDiscovered(InteractableObject obj)
        {
            if (reportOnDiscovery)
            {
                // Debounce to avoid spamming when many objects discovered at once
                if (Time.time - lastReportTime >= 0.5f)
                {
                    SendStateUpdate();
                }
            }
        }

        /// <summary>
        /// Sends a state update to the backend immediately.
        /// </summary>
        public void SendStateUpdate()
        {
            if (connector == null || agent == null) return;

            lastReportTime = Time.time;

            try
            {
                var state = agent.GenerateWorldState();

                // Convert to dictionary for JSON serialization
                var stateDict = new Dictionary<string, object>
                {
                    { "agentPosition", new Dictionary<string, float>
                        {
                            { "x", state.agentPosition.x },
                            { "y", state.agentPosition.y },
                            { "z", state.agentPosition.z }
                        }
                    },
                    { "agentRotation", new Dictionary<string, float>
                        {
                            { "x", state.agentRotation.x },
                            { "y", state.agentRotation.y },
                            { "z", state.agentRotation.z }
                        }
                    },
                    { "agentActivity", state.agentActivity },
                    { "isInteracting", state.isInteracting },
                    { "nearbyObjects", ConvertObjectReports(state.nearbyObjects) },
                    { "availableActions", state.availableActions }
                };

                // Send via RPC (fire and forget)
                connector.SendRpcFireAndForget("golemWorldState", new object[] { stateDict });

                lastReportedActivity = state.agentActivity;
                lastReportedObjectCount = state.nearbyObjects.Count;

                Debug.Log($"GolemStateReporter: Sent state update (activity={state.agentActivity}, objects={state.nearbyObjects.Count})");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"GolemStateReporter: Failed to send state update: {e.Message}");
            }
        }

        /// <summary>
        /// Sends a specific event to the backend.
        /// </summary>
        public void SendEvent(string eventType, Dictionary<string, object> data = null)
        {
            if (connector == null) return;

            try
            {
                var eventData = new Dictionary<string, object>
                {
                    { "type", eventType },
                    { "timestamp", DateTime.UtcNow.ToString("o") },
                    { "data", data ?? new Dictionary<string, object>() }
                };

                connector.SendRpcFireAndForget("golemEvent", new object[] { eventData });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"GolemStateReporter: Failed to send event: {e.Message}");
            }
        }

        /// <summary>
        /// Sends an interaction started event.
        /// </summary>
        public void ReportInteractionStarted(InteractableObject target, string affordance)
        {
            SendEvent("interactionStarted", new Dictionary<string, object>
            {
                { "targetId", target?.UniqueId },
                { "targetName", target?.displayName },
                { "affordance", affordance }
            });
        }

        /// <summary>
        /// Sends an interaction ended event.
        /// </summary>
        public void ReportInteractionEnded(InteractableObject target, string affordance)
        {
            SendEvent("interactionEnded", new Dictionary<string, object>
            {
                { "targetId", target?.UniqueId },
                { "targetName", target?.displayName },
                { "affordance", affordance }
            });
        }

        private List<Dictionary<string, object>> ConvertObjectReports(List<ObjectReport> reports)
        {
            var result = new List<Dictionary<string, object>>();

            foreach (var report in reports)
            {
                result.Add(new Dictionary<string, object>
                {
                    { "id", report.id },
                    { "type", report.type },
                    { "name", report.name },
                    { "description", report.description },
                    { "affordances", report.affordances },
                    { "distance", report.distance },
                    { "isOccupied", report.isOccupied },
                    { "canInteract", report.canInteract }
                });
            }

            return result;
        }
    }
}

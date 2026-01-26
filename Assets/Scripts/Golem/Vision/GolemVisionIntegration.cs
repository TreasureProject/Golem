using System;
using System.Collections.Generic;
using UnityEngine;

namespace Golem.Vision
{
    /// <summary>
    /// Integrates the Vision system with GolemAgent.
    /// Adds visual perception capabilities without modifying the core GolemAgent.
    /// Attach this component alongside GolemAgent to enable visual perception.
    /// </summary>
    [RequireComponent(typeof(GolemAgent))]
    public class GolemVisionIntegration : MonoBehaviour
    {
        [Header("Configuration")]
        public VisionConfig config;

        [Header("Auto-Setup")]
        [Tooltip("Automatically create Vision components if not found.")]
        public bool autoSetupComponents = true;

        [Header("Integration Settings")]
        [Tooltip("Capture before-action frames for verification.")]
        public bool enableActionVerification = true;

        [Tooltip("Include visual objects in world state reports.")]
        public bool includeVisualInWorldState = true;

        [Tooltip("Auto-scan when entering new areas.")]
        public bool autoScanOnZoneChange = true;

        // Components
        private GolemAgent agent;
        private VisualPerceptionManager visualPerception;
        private PerceptionFuser perceptionFuser;
        private FrameCaptureService captureService;
        private VLMClient vlmClient;
        private VisualObjectCache cache;
        private HallucinationDetector hallucinationDetector;
        private ActionVerifier actionVerifier;

        // State
        private string lastZone = "";

        public VisualPerceptionManager VisualPerception => visualPerception;
        public PerceptionFuser PerceptionFuser => perceptionFuser;
        public bool IsVisualPerceptionEnabled => config != null && config.enabled;

        public event Action<VisualScanResult> OnVisualScanComplete;
        public event Action<FusedPerceptionResult> OnPerceptionFused;

        private void Awake()
        {
            agent = GetComponent<GolemAgent>();

            if (autoSetupComponents)
            {
                SetupVisionComponents();
            }
            else
            {
                FindExistingComponents();
            }

            SubscribeToAgentEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromAgentEvents();
        }

        private void SetupVisionComponents()
        {
            // Create or find each component
            captureService = GetComponent<FrameCaptureService>();
            if (captureService == null)
                captureService = gameObject.AddComponent<FrameCaptureService>();
            captureService.config = config;
            captureService.targetCamera = Camera.main;

            vlmClient = GetComponent<VLMClient>();
            if (vlmClient == null)
                vlmClient = gameObject.AddComponent<VLMClient>();
            vlmClient.config = config;

            cache = GetComponent<VisualObjectCache>();
            if (cache == null)
                cache = gameObject.AddComponent<VisualObjectCache>();
            cache.config = config;

            hallucinationDetector = GetComponent<HallucinationDetector>();
            if (hallucinationDetector == null)
                hallucinationDetector = gameObject.AddComponent<HallucinationDetector>();
            hallucinationDetector.config = config;
            hallucinationDetector.worldScanner = agent.scanner;

            actionVerifier = GetComponent<ActionVerifier>();
            if (actionVerifier == null)
                actionVerifier = gameObject.AddComponent<ActionVerifier>();
            actionVerifier.config = config;

            visualPerception = GetComponent<VisualPerceptionManager>();
            if (visualPerception == null)
                visualPerception = gameObject.AddComponent<VisualPerceptionManager>();
            visualPerception.config = config;
            visualPerception.captureService = captureService;
            visualPerception.vlmClient = vlmClient;
            visualPerception.cache = cache;
            visualPerception.hallucinationDetector = hallucinationDetector;
            visualPerception.actionVerifier = actionVerifier;
            visualPerception.agentTransform = transform;

            perceptionFuser = GetComponent<PerceptionFuser>();
            if (perceptionFuser == null)
                perceptionFuser = gameObject.AddComponent<PerceptionFuser>();
            perceptionFuser.config = config;
            perceptionFuser.visualPerception = visualPerception;

            // Subscribe to vision events
            visualPerception.OnScanComplete += HandleVisualScanComplete;
            perceptionFuser.OnFusionComplete += HandlePerceptionFused;

            Debug.Log("[GolemVision] Vision components initialized");
        }

        private void FindExistingComponents()
        {
            captureService = GetComponent<FrameCaptureService>();
            vlmClient = GetComponent<VLMClient>();
            cache = GetComponent<VisualObjectCache>();
            hallucinationDetector = GetComponent<HallucinationDetector>();
            actionVerifier = GetComponent<ActionVerifier>();
            visualPerception = GetComponent<VisualPerceptionManager>();
            perceptionFuser = GetComponent<PerceptionFuser>();

            if (visualPerception != null)
            {
                visualPerception.OnScanComplete += HandleVisualScanComplete;
            }

            if (perceptionFuser != null)
            {
                perceptionFuser.OnFusionComplete += HandlePerceptionFused;
            }
        }

        private void SubscribeToAgentEvents()
        {
            if (agent == null) return;

            agent.OnInteractionStarted += HandleInteractionStarted;
            agent.OnInteractionEnded += HandleInteractionEnded;
            agent.OnMovementCompleted += HandleMovementCompleted;
        }

        private void UnsubscribeFromAgentEvents()
        {
            if (agent == null) return;

            agent.OnInteractionStarted -= HandleInteractionStarted;
            agent.OnInteractionEnded -= HandleInteractionEnded;
            agent.OnMovementCompleted -= HandleMovementCompleted;

            if (visualPerception != null)
            {
                visualPerception.OnScanComplete -= HandleVisualScanComplete;
            }

            if (perceptionFuser != null)
            {
                perceptionFuser.OnFusionComplete -= HandlePerceptionFused;
            }
        }

        private void HandleInteractionStarted(InteractableObject target, string affordance)
        {
            if (!enableActionVerification || visualPerception == null)
                return;

            // Capture before-action frame
            visualPerception.CaptureBeforeAction(affordance, target.UniqueId);
        }

        private void HandleInteractionEnded(InteractableObject target, string affordance)
        {
            if (!enableActionVerification || visualPerception == null)
                return;

            // Verify action outcome
            visualPerception.VerifyAction(
                affordance,
                target.UniqueId,
                $"Agent should have completed {affordance} on {target.displayName}",
                result =>
                {
                    // Record result in memory
                    if (agent.Memory != null)
                    {
                        agent.RecordInteractionResult(target.objectType, affordance, result.success);
                    }

                    Debug.Log($"[GolemVision] Action verification: {affordance} on {target.displayName} - " +
                              $"Success: {result.success}, Confidence: {result.confidence:F2}");
                }
            );

            // Notify visual perception of interaction completion
            visualPerception.OnInteractionComplete(affordance, target.UniqueId, true);
        }

        private void HandleMovementCompleted()
        {
            // Check if we entered a new zone
            string currentZone = GetCurrentZone();
            if (currentZone != lastZone)
            {
                lastZone = currentZone;

                if (autoScanOnZoneChange && visualPerception != null)
                {
                    visualPerception.OnZoneEntered(currentZone);
                }
            }
        }

        private void HandleVisualScanComplete(VisualScanResult result)
        {
            if (result.success)
            {
                Debug.Log($"[GolemVision] Visual scan complete: {result.objects.Count} objects, " +
                          $"'{result.sceneDescription}'");
            }

            OnVisualScanComplete?.Invoke(result);

            // Trigger fusion with structured data
            if (includeVisualInWorldState && perceptionFuser != null)
            {
                var structuredObjects = GetStructuredObjects();
                perceptionFuser.Fuse(structuredObjects, result);
            }
        }

        private void HandlePerceptionFused(FusedPerceptionResult result)
        {
            Debug.Log($"[GolemVision] Perception fused: {result.TotalCount} objects " +
                      $"(structured: {result.structuredCount}, visual-only: {result.visualOnlyCount}, " +
                      $"cross-validated: {result.crossValidatedCount})");

            OnPerceptionFused?.Invoke(result);
        }

        private string GetCurrentZone()
        {
            // Simple zone detection based on position
            // Could be enhanced to use trigger zones or areas
            Vector3 pos = transform.position;
            return $"zone_{Mathf.FloorToInt(pos.x / 10)}_{Mathf.FloorToInt(pos.z / 10)}";
        }

        private List<StructuredObjectData> GetStructuredObjects()
        {
            var result = new List<StructuredObjectData>();

            if (agent.scanner == null || agent.scanner.nearbyObjects == null)
                return result;

            foreach (var obj in agent.scanner.nearbyObjects)
            {
                result.Add(new StructuredObjectData
                {
                    uniqueId = obj.UniqueId,
                    displayName = obj.displayName,
                    objectType = obj.objectType,
                    position = obj.transform.position,
                    interactionPosition = obj.InteractionPosition,
                    affordances = new List<string>(obj.affordances),
                    isInteractable = obj.CanInteract(),
                    currentState = obj.isOccupied ? "occupied" : "available"
                });
            }

            return result;
        }

        #region Public API

        /// <summary>
        /// Request a visual scan of the current scene.
        /// </summary>
        public void RequestVisualScan(Action<VisualScanResult> callback = null)
        {
            if (visualPerception != null)
            {
                visualPerception.RequestScan(callback);
            }
            else
            {
                callback?.Invoke(new VisualScanResult
                {
                    success = false,
                    errorMessage = "Visual perception not available"
                });
            }
        }

        /// <summary>
        /// Get fused perception data (structured + visual).
        /// </summary>
        public FusedPerceptionResult GetFusedPerception()
        {
            if (perceptionFuser == null)
                return null;

            var structuredObjects = GetStructuredObjects();
            var visualResult = visualPerception?.LastScanResult;

            return perceptionFuser.Fuse(structuredObjects, visualResult);
        }

        /// <summary>
        /// Generate enhanced world state with visual perception.
        /// </summary>
        public EnhancedWorldStateReport GenerateEnhancedWorldState()
        {
            var baseReport = agent.GenerateWorldState();
            var fusedPerception = GetFusedPerception();

            return new EnhancedWorldStateReport
            {
                // Base report data
                agentPosition = baseReport.agentPosition,
                agentRotation = baseReport.agentRotation,
                agentActivity = baseReport.agentActivity,
                isInteracting = baseReport.isInteracting,
                nearbyObjects = baseReport.nearbyObjects,
                availableActions = baseReport.availableActions,
                personality = baseReport.personality,
                memoryStats = baseReport.memoryStats,

                // Visual perception data
                hasVisualPerception = fusedPerception != null,
                sceneDescription = fusedPerception?.sceneDescription,
                suggestedActions = fusedPerception?.suggestedActions,
                visualObjects = ConvertToVisualReports(fusedPerception),
                perceptionStats = new PerceptionStats
                {
                    structuredObjectCount = fusedPerception?.structuredCount ?? 0,
                    visualOnlyObjectCount = fusedPerception?.visualOnlyCount ?? 0,
                    crossValidatedCount = fusedPerception?.crossValidatedCount ?? 0,
                    cacheHitRate = cache?.GetStats().hitRate ?? 0f
                }
            };
        }

        private List<VisualObjectReport> ConvertToVisualReports(FusedPerceptionResult fusedResult)
        {
            var result = new List<VisualObjectReport>();

            if (fusedResult?.objects == null)
                return result;

            foreach (var fused in fusedResult.objects)
            {
                // Only include visual-only objects (structured objects are already in nearbyObjects)
                if (fused.source == PerceptionSource.VisualOnly)
                {
                    result.Add(new VisualObjectReport
                    {
                        id = fused.id,
                        name = fused.name,
                        type = fused.type,
                        description = fused.description,
                        inferredAffordances = fused.affordances,
                        relativePosition = fused.relativePosition,
                        state = fused.visualState,
                        confidence = fused.confidence,
                        estimatedPosition = fused.position
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Check if visual perception is available and ready.
        /// </summary>
        public bool IsReady => visualPerception != null && visualPerception.CanProcessRequests();

        /// <summary>
        /// Get vision system statistics.
        /// </summary>
        public VisionSystemStats GetStats()
        {
            return new VisionSystemStats
            {
                isEnabled = config?.enabled ?? false,
                isProcessing = visualPerception?.IsProcessing ?? false,
                cacheStats = cache?.GetStats(),
                hallucinationStats = hallucinationDetector?.GetStats() ?? (0, 0, 0f),
                verificationStats = actionVerifier?.GetStats(),
                vlmStats = vlmClient?.Stats
            };
        }

        #endregion
    }

    /// <summary>
    /// Enhanced world state report including visual perception.
    /// </summary>
    [Serializable]
    public class EnhancedWorldStateReport : WorldStateReport
    {
        // Visual perception additions
        public bool hasVisualPerception;
        public string sceneDescription;
        public List<string> suggestedActions;
        public List<VisualObjectReport> visualObjects;
        public PerceptionStats perceptionStats;
    }

    /// <summary>
    /// Statistics about perception systems.
    /// </summary>
    [Serializable]
    public class PerceptionStats
    {
        public int structuredObjectCount;
        public int visualOnlyObjectCount;
        public int crossValidatedCount;
        public float cacheHitRate;
    }

    /// <summary>
    /// Overall vision system statistics.
    /// </summary>
    [Serializable]
    public class VisionSystemStats
    {
        public bool isEnabled;
        public bool isProcessing;
        public CacheStats cacheStats;
        public (int total, int hallucinations, float rate) hallucinationStats;
        public ActionVerificationStats verificationStats;
        public VisualPerceptionStats vlmStats;
    }
}

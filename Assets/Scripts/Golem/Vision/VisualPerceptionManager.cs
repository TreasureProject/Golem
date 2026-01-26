using System;
using System.Collections.Generic;
using UnityEngine;

namespace Golem.Vision
{
    /// <summary>
    /// Central manager for the visual perception pipeline.
    /// Orchestrates frame capture, VLM analysis, caching, and hallucination detection.
    /// Implements trigger-based capture for efficient VLM usage.
    /// </summary>
    public class VisualPerceptionManager : MonoBehaviour
    {
        [Header("Configuration")]
        public VisionConfig config;

        [Header("Components")]
        public FrameCaptureService captureService;
        public VLMClient vlmClient;
        public VisualObjectCache cache;
        public HallucinationDetector hallucinationDetector;
        public ActionVerifier actionVerifier;

        [Header("Agent Reference")]
        [Tooltip("The agent transform for position tracking.")]
        public Transform agentTransform;

        [Header("Trigger Settings")]
        [Tooltip("Minimum distance moved before triggering a new scan.")]
        public float movementTriggerDistance = 2f;

        [Tooltip("Minimum angle rotated before triggering a new scan.")]
        public float rotationTriggerAngle = 45f;

        [Tooltip("Delay after interaction before verification scan.")]
        public float verificationDelay = 0.5f;

        private bool isProcessing;
        private VisualScanResult lastScanResult;
        private Vector3 lastScanPosition;
        private Vector3 lastScanForward;
        private float lastScanTime;
        private Queue<ScanRequest> pendingScanRequests = new Queue<ScanRequest>();

        public bool IsProcessing => isProcessing;
        public VisualScanResult LastScanResult => lastScanResult;

        public event Action<VisualScanResult> OnScanComplete;
        public event Action<ActionVerificationResult> OnVerificationComplete;
        public event Action<string> OnError;

        private class ScanRequest
        {
            public VLMRequestType type;
            public Action<VisualScanResult> callback;
            public string actionName;
            public string targetId;
            public string expectedOutcome;
            public CaptureResult beforeCapture;
        }

        /// <summary>
        /// Check if the pipeline can process new requests.
        /// </summary>
        public bool CanProcessRequests()
        {
            if (config == null || !config.enabled)
                return false;

            if (vlmClient == null || !vlmClient.CanAcceptRequests())
                return false;

            return true;
        }

        /// <summary>
        /// Request a visual scan of the current scene.
        /// Uses caching to avoid redundant scans.
        /// </summary>
        public void RequestScan(Action<VisualScanResult> callback = null)
        {
            if (!CanProcessRequests())
            {
                callback?.Invoke(CreateErrorResult("Visual perception not available"));
                return;
            }

            Vector3 position = GetAgentPosition();
            Vector3 forward = GetAgentForward();

            // Check cache first
            if (cache != null && cache.TryGetCached(position, forward, out VisualScanResult cached))
            {
                Debug.Log("[VisualPerception] Using cached scan result");
                callback?.Invoke(cached);
                OnScanComplete?.Invoke(cached);
                return;
            }

            // Queue new scan request
            pendingScanRequests.Enqueue(new ScanRequest
            {
                type = VLMRequestType.SceneUnderstanding,
                callback = callback
            });

            ProcessNextRequest();
        }

        /// <summary>
        /// Force a new scan, bypassing cache.
        /// </summary>
        public void ForceNewScan(Action<VisualScanResult> callback = null)
        {
            if (!CanProcessRequests())
            {
                callback?.Invoke(CreateErrorResult("Visual perception not available"));
                return;
            }

            pendingScanRequests.Enqueue(new ScanRequest
            {
                type = VLMRequestType.SceneUnderstanding,
                callback = callback
            });

            ProcessNextRequest();
        }

        /// <summary>
        /// Capture a "before" frame for action verification.
        /// </summary>
        public void CaptureBeforeAction(string actionName, string targetId)
        {
            if (captureService == null || actionVerifier == null)
                return;

            if (!actionVerifier.RequiresVerification(actionName))
                return;

            captureService.CaptureFrameAsync(result =>
            {
                if (result.success)
                {
                    actionVerifier.SetBeforeCapture($"{actionName}_{targetId}", result);
                    Debug.Log($"[VisualPerception] Captured before-action frame for {actionName}");
                }
            });
        }

        /// <summary>
        /// Verify an action by comparing before/after frames.
        /// </summary>
        public void VerifyAction(string actionName, string targetId, string expectedOutcome, Action<ActionVerificationResult> callback = null)
        {
            if (!CanProcessRequests() || actionVerifier == null)
            {
                callback?.Invoke(new ActionVerificationResult
                {
                    success = false,
                    confidence = 0f,
                    failureReason = "Verification not available"
                });
                return;
            }

            string actionKey = $"{actionName}_{targetId}";

            // Delay to allow action to complete visually
            StartCoroutine(DelayedVerification(actionKey, actionName, targetId, expectedOutcome, callback));
        }

        private System.Collections.IEnumerator DelayedVerification(
            string actionKey,
            string actionName,
            string targetId,
            string expectedOutcome,
            Action<ActionVerificationResult> callback)
        {
            yield return new WaitForSeconds(verificationDelay);

            // Capture after frame
            captureService.CaptureFrameAsync(afterResult =>
            {
                if (!afterResult.success)
                {
                    var errorResult = new ActionVerificationResult
                    {
                        success = false,
                        confidence = 0f,
                        failureReason = "Failed to capture after-action frame"
                    };
                    callback?.Invoke(errorResult);
                    OnVerificationComplete?.Invoke(errorResult);
                    return;
                }

                actionVerifier.SetAfterCapture(actionKey, afterResult);

                // Get before capture
                if (!actionVerifier.HasPendingVerification(actionKey))
                {
                    // No before capture, use after-only verification
                    vlmClient.RequestSceneUnderstanding(afterResult.imageBase64, response =>
                    {
                        var result = new ActionVerificationResult
                        {
                            success = response.success,
                            confidence = 0.5f, // Lower confidence without before/after comparison
                            observedChange = response.sceneResult?.sceneDescription ?? "Unable to verify",
                            actionType = actionName,
                            targetId = targetId
                        };

                        actionVerifier.RecordVerificationResult(result.success);
                        callback?.Invoke(result);
                        OnVerificationComplete?.Invoke(result);
                    });
                    return;
                }

                // Full before/after verification
                // Note: beforeCapture would need to be retrieved from actionVerifier
                vlmClient.RequestActionVerification(
                    "", // Would need to store before image base64
                    afterResult.imageBase64,
                    actionName,
                    targetId,
                    expectedOutcome,
                    response =>
                    {
                        var result = response.verificationResult ?? new ActionVerificationResult
                        {
                            success = false,
                            confidence = 0f,
                            failureReason = "Failed to parse verification response"
                        };

                        result.actionType = actionName;
                        result.targetId = targetId;

                        actionVerifier.RecordVerificationResult(result.success);
                        actionVerifier.ClearPendingVerification(actionKey);

                        callback?.Invoke(result);
                        OnVerificationComplete?.Invoke(result);
                    }
                );
            });
        }

        /// <summary>
        /// Check if agent has moved enough to warrant a new scan.
        /// </summary>
        public bool ShouldTriggerScan()
        {
            if (lastScanResult == null)
                return true;

            Vector3 position = GetAgentPosition();
            Vector3 forward = GetAgentForward();

            float distance = Vector3.Distance(position, lastScanPosition);
            float angle = Vector3.Angle(forward, lastScanForward);

            return distance >= movementTriggerDistance || angle >= rotationTriggerAngle;
        }

        /// <summary>
        /// Notify that the agent entered a new zone/area.
        /// </summary>
        public void OnZoneEntered(string zoneName)
        {
            Debug.Log($"[VisualPerception] Entered zone: {zoneName}");

            // Invalidate nearby cache entries
            cache?.InvalidateNear(GetAgentPosition(), movementTriggerDistance * 2);

            // Trigger new scan
            ForceNewScan();
        }

        /// <summary>
        /// Notify that an interaction completed.
        /// </summary>
        public void OnInteractionComplete(string actionName, string targetId, bool success)
        {
            Debug.Log($"[VisualPerception] Interaction complete: {actionName} on {targetId} (success: {success})");

            // Invalidate cache near interaction
            cache?.InvalidateNear(GetAgentPosition(), 5f);
        }

        private void ProcessNextRequest()
        {
            if (isProcessing || pendingScanRequests.Count == 0)
                return;

            isProcessing = true;
            var request = pendingScanRequests.Dequeue();

            // Capture frame
            captureService.CaptureFrameAsync(captureResult =>
            {
                if (!captureResult.success)
                {
                    isProcessing = false;
                    var errorResult = CreateErrorResult(captureResult.errorMessage);
                    request.callback?.Invoke(errorResult);
                    OnError?.Invoke(captureResult.errorMessage);
                    ProcessNextRequest();
                    return;
                }

                // Send to VLM
                vlmClient.RequestSceneUnderstanding(captureResult.imageBase64, vlmResponse =>
                {
                    VisualScanResult result;

                    if (vlmResponse.success && vlmResponse.sceneResult != null)
                    {
                        result = new VisualScanResult
                        {
                            scanId = Guid.NewGuid().ToString("N").Substring(0, 8),
                            success = true,
                            agentPosition = GetAgentPosition(),
                            agentForward = GetAgentForward(),
                            scanTime = Time.time,
                            requestDuration = vlmResponse.processingTime,
                            sceneDescription = vlmResponse.sceneResult.sceneDescription,
                            suggestedActions = vlmResponse.sceneResult.suggestedActions,
                            objects = vlmResponse.sceneResult.objects
                        };

                        // Filter hallucinations
                        if (hallucinationDetector != null)
                        {
                            result = hallucinationDetector.FilterScanResult(result);
                        }

                        // Cache result
                        if (cache != null)
                        {
                            cache.Store(result.agentPosition, result.agentForward, result, captureResult.ImageHash);
                        }

                        lastScanResult = result;
                        lastScanPosition = result.agentPosition;
                        lastScanForward = result.agentForward;
                        lastScanTime = Time.time;
                    }
                    else
                    {
                        result = CreateErrorResult(vlmResponse.errorMessage ?? "VLM request failed");
                    }

                    isProcessing = false;
                    request.callback?.Invoke(result);
                    OnScanComplete?.Invoke(result);
                    ProcessNextRequest();
                });
            });
        }

        private VisualScanResult CreateErrorResult(string errorMessage)
        {
            return new VisualScanResult
            {
                scanId = Guid.NewGuid().ToString("N").Substring(0, 8),
                success = false,
                errorMessage = errorMessage,
                agentPosition = GetAgentPosition(),
                agentForward = GetAgentForward(),
                scanTime = Time.time
            };
        }

        private Vector3 GetAgentPosition()
        {
            return agentTransform != null ? agentTransform.position : transform.position;
        }

        private Vector3 GetAgentForward()
        {
            return agentTransform != null ? agentTransform.forward : transform.forward;
        }

        private void Awake()
        {
            // Auto-find components if not assigned
            if (captureService == null)
                captureService = GetComponent<FrameCaptureService>();
            if (vlmClient == null)
                vlmClient = GetComponent<VLMClient>();
            if (cache == null)
                cache = GetComponent<VisualObjectCache>();
            if (hallucinationDetector == null)
                hallucinationDetector = GetComponent<HallucinationDetector>();
            if (actionVerifier == null)
                actionVerifier = GetComponent<ActionVerifier>();
        }

        private void Update()
        {
            // Auto-trigger scan when agent moves significantly
            if (config != null && config.enabled && ShouldTriggerScan() && !isProcessing)
            {
                // Only auto-scan if there are no pending requests
                if (pendingScanRequests.Count == 0)
                {
                    // Check if enough time has passed since last scan
                    if (Time.time - lastScanTime > config.cacheTTL * 0.5f)
                    {
                        RequestScan();
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Golem.Vision
{
    /// <summary>
    /// Result of a visual scene scan.
    /// </summary>
    [Serializable]
    public class VisualScanResult
    {
        public string scanId;
        public bool success;
        public string errorMessage;
        public Vector3 agentPosition;
        public Vector3 agentForward;
        public float scanTime;
        public float requestDuration;
        public string sceneDescription;
        public List<string> suggestedActions = new List<string>();
        public List<VisualObjectReport> objects = new List<VisualObjectReport>();
    }

    /// <summary>
    /// Report of a visually detected object.
    /// </summary>
    [Serializable]
    public class VisualObjectReport
    {
        public string id;
        public string name;
        public string type;
        public string description;
        public string[] inferredAffordances;
        public string relativePosition;
        public string state;
        public float confidence;
        public bool matchedStructured;
        public string matchedObjectId;
        public Vector3 estimatedPosition;
        public float observationTime;
    }

    /// <summary>
    /// Request to the VLM API.
    /// </summary>
    [Serializable]
    public class VLMRequest
    {
        public string requestId;
        public VLMRequestType requestType;
        public string prompt;
        public string imageBase64;
        public float createdTime;
        public Dictionary<string, string> promptVariables;

        public VLMRequest()
        {
            requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            createdTime = Time.time;
            promptVariables = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Type of VLM request.
    /// </summary>
    public enum VLMRequestType
    {
        SceneUnderstanding,
        ActionVerification,
        AffordanceDiscovery
    }

    /// <summary>
    /// Response from the VLM API.
    /// </summary>
    [Serializable]
    public class VLMResponse
    {
        public string requestId;
        public bool success;
        public string errorMessage;
        public string rawContent;
        public float processingTime;
        public float estimatedCost;
        public int tokensUsed;
        public SceneUnderstandingResult sceneResult;
        public ActionVerificationResult verificationResult;
    }

    /// <summary>
    /// Parsed result of scene understanding request.
    /// </summary>
    [Serializable]
    public class SceneUnderstandingResult
    {
        public string sceneDescription;
        public List<string> suggestedActions = new List<string>();
        public List<VisualObjectReport> objects = new List<VisualObjectReport>();
    }

    /// <summary>
    /// Result of action verification.
    /// </summary>
    [Serializable]
    public class ActionVerificationResult
    {
        public bool success;
        public float confidence;
        public string observedChange = "";
        public string failureReason = "";
        public string actionType;
        public string targetId;
        public string objectType;
        public string affordance;
        public float verificationTime;
    }

    /// <summary>
    /// Request for action verification.
    /// </summary>
    [Serializable]
    public class ActionVerificationRequest
    {
        public string requestId;
        public string actionName;
        public string targetId;
        public string expectedOutcome;
        public float createdTime;

        public ActionVerificationRequest(string action, string target)
        {
            requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            actionName = action;
            targetId = target;
            createdTime = Time.time;
        }
    }

    /// <summary>
    /// Statistics for action verification.
    /// </summary>
    [Serializable]
    public class ActionVerificationStats
    {
        public int totalVerifications;
        public int successfulVerifications;
        public int failedVerifications;
        public float averageConfidence;

        public float SuccessRate => totalVerifications > 0
            ? (float)successfulVerifications / totalVerifications
            : 0f;
    }

    /// <summary>
    /// Result of a frame capture.
    /// </summary>
    [Serializable]
    public class CaptureResult
    {
        public bool success;
        public string errorMessage;
        public string imageBase64;
        public byte[] imageBytes;
        public int width;
        public int height;
        public CaptureMode captureMode;
        public float captureTime;

        private string cachedHash;

        public string ImageHash
        {
            get
            {
                if (cachedHash == null)
                {
                    cachedHash = FrameCaptureService.ComputeImageHash(imageBytes);
                }
                return cachedHash;
            }
        }
    }

    /// <summary>
    /// Statistics for visual perception.
    /// </summary>
    [Serializable]
    public class VisualPerceptionStats
    {
        public int totalRequests;
        public int successfulRequests;
        public int failedRequests;
        public float totalCost;
        public float currentHourCost;
        public float currentHourStartTime;
        public float averageRequestTime;

        public float SuccessRate => totalRequests > 0
            ? (float)successfulRequests / totalRequests
            : 0f;

        public void ResetHourlyIfNeeded(float currentTime)
        {
            if (currentTime - currentHourStartTime >= 3600f)
            {
                currentHourCost = 0f;
                currentHourStartTime = currentTime;
            }
        }
    }

    /// <summary>
    /// Statistics for the visual object cache.
    /// </summary>
    [Serializable]
    public class CacheStats
    {
        public int count;
        public int hits;
        public int misses;

        public float hitRate => (hits + misses) > 0
            ? (float)hits / (hits + misses)
            : 0f;
    }
}

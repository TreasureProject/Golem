using UnityEngine;

namespace Golem.Vision
{
    /// <summary>
    /// ScriptableObject containing all configuration settings for the Vision system.
    /// </summary>
    [CreateAssetMenu(fileName = "VisionConfig", menuName = "Golem/Vision Config")]
    public class VisionConfig : ScriptableObject
    {
        [Header("General Settings")]
        [Tooltip("Enable/disable the visual perception system.")]
        public bool enabled = true;

        [Header("VLM Provider Settings")]
        [Tooltip("The VLM provider to use.")]
        public VLMProvider provider = VLMProvider.OpenAI;

        [Tooltip("API key for the VLM provider.")]
        public string apiKey = "";

        [Tooltip("Custom base URL (overrides provider default if set).")]
        public string customBaseUrl = "";

        [Tooltip("Model name to use for VLM requests.")]
        public string modelName = "gpt-4o";

        [Header("Capture Settings")]
        [Tooltip("Width of captured frames in pixels.")]
        [Range(64, 1024)]
        public int captureWidth = 256;

        [Tooltip("Height of captured frames in pixels.")]
        [Range(64, 1024)]
        public int captureHeight = 256;

        [Tooltip("JPEG quality for image compression (0-100).")]
        [Range(50, 100)]
        public int jpegQuality = 75;

        [Tooltip("Default capture mode.")]
        public CaptureMode captureMode = CaptureMode.AgentPOV;

        [Header("Request Settings")]
        [Tooltip("Request timeout in seconds.")]
        public float requestTimeout = 30f;

        [Tooltip("Maximum number of retries on failure.")]
        [Range(0, 5)]
        public int maxRetries = 2;

        [Tooltip("Delay between retries in seconds.")]
        public float retryDelay = 1f;

        [Header("Cache Settings")]
        [Tooltip("Enable caching of VLM responses.")]
        public bool enableCache = true;

        [Tooltip("Cache time-to-live in seconds.")]
        public float cacheTTL = 60f;

        [Tooltip("Distance threshold for cache invalidation (meters).")]
        public float cacheInvalidationDistance = 2f;

        [Tooltip("Angle threshold for cache invalidation (degrees).")]
        public float cacheInvalidationAngle = 30f;

        [Tooltip("Maximum number of cache entries.")]
        public int maxCacheEntries = 50;

        [Header("Quality Settings")]
        [Tooltip("Minimum confidence threshold for visual objects.")]
        [Range(0f, 1f)]
        public float minVisualConfidence = 0.6f;

        [Tooltip("Minimum confidence for action verification.")]
        [Range(0f, 1f)]
        public float minVerificationConfidence = 0.7f;

        [Header("Cost Management")]
        [Tooltip("Maximum cost per hour in dollars.")]
        public float maxCostPerHour = 1f;

        [Tooltip("Pause requests when budget exceeded.")]
        public bool pauseOnBudgetExceeded = true;

        [Header("Debug Settings")]
        [Tooltip("Log VLM responses to console.")]
        public bool logVLMResponses = false;

        [Tooltip("Show debug visualization.")]
        public bool showDebugVisualization = false;

        /// <summary>
        /// Get the base URL for the configured provider.
        /// </summary>
        public string GetBaseUrl()
        {
            if (!string.IsNullOrEmpty(customBaseUrl))
                return customBaseUrl;

            switch (provider)
            {
                case VLMProvider.OpenAI:
                    return "https://api.openai.com/v1";
                case VLMProvider.Anthropic:
                    return "https://api.anthropic.com/v1";
                case VLMProvider.Ollama:
                    return "http://localhost:11434/api";
                default:
                    return "https://api.openai.com/v1";
            }
        }

        /// <summary>
        /// Validate the configuration.
        /// </summary>
        public bool Validate()
        {
            if (!enabled)
                return true;

            // Ollama doesn't require an API key
            if (provider == VLMProvider.Ollama)
                return true;

            return !string.IsNullOrEmpty(apiKey);
        }
    }

    /// <summary>
    /// VLM provider options.
    /// </summary>
    public enum VLMProvider
    {
        OpenAI,
        Anthropic,
        Ollama
    }

    /// <summary>
    /// Frame capture mode options.
    /// </summary>
    public enum CaptureMode
    {
        AgentPOV,
        ThirdPerson,
        Overhead,
        Multiple
    }
}

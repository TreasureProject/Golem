using System;
using System.Collections.Generic;
using UnityEngine;

namespace Golem.Vision.Tests
{
    /// <summary>
    /// Test utilities and mock objects for Vision system testing.
    /// </summary>
    public static class TestUtilities
    {
        /// <summary>
        /// Create a test VisionConfig with default settings.
        /// </summary>
        public static VisionConfig CreateTestConfig()
        {
            var config = ScriptableObject.CreateInstance<VisionConfig>();
            config.enabled = true;
            config.captureWidth = 256;
            config.captureHeight = 256;
            config.jpegQuality = 75;
            config.requestTimeout = 5f;
            config.maxRetries = 1;
            config.enableCache = true;
            config.cacheTTL = 30f;
            config.cacheInvalidationDistance = 2f;
            config.cacheInvalidationAngle = 30f;
            config.maxCacheEntries = 10;
            config.minVisualConfidence = 0.6f;
            config.logVLMResponses = false;
            return config;
        }

        /// <summary>
        /// Create a mock VisualScanResult for testing.
        /// </summary>
        public static VisualScanResult CreateMockScanResult(
            int objectCount = 3,
            bool success = true,
            Vector3? position = null,
            Vector3? forward = null)
        {
            var result = new VisualScanResult
            {
                scanId = Guid.NewGuid().ToString("N").Substring(0, 8),
                success = success,
                agentPosition = position ?? Vector3.zero,
                agentForward = forward ?? Vector3.forward,
                scanTime = Time.time,
                requestDuration = 1.5f,
                sceneDescription = "Test scene with objects",
                suggestedActions = new List<string> { "examine chair", "sit on bench" }
            };

            for (int i = 0; i < objectCount; i++)
            {
                result.objects.Add(CreateMockVisualObject($"object_{i}", 0.7f + i * 0.1f));
            }

            return result;
        }

        /// <summary>
        /// Create a mock VisualObjectReport for testing.
        /// </summary>
        public static VisualObjectReport CreateMockVisualObject(
            string name = "TestObject",
            float confidence = 0.8f,
            string type = "seat",
            string[] affordances = null)
        {
            return new VisualObjectReport
            {
                id = Guid.NewGuid().ToString("N").Substring(0, 8),
                name = name,
                type = type,
                description = $"A test {type}",
                inferredAffordances = affordances ?? new[] { "sit", "examine" },
                relativePosition = "center",
                state = "available",
                confidence = confidence,
                matchedStructured = false,
                observationTime = Time.time
            };
        }

        /// <summary>
        /// Create a mock VLMResponse for testing.
        /// </summary>
        public static VLMResponse CreateMockVLMResponse(
            bool success = true,
            int objectCount = 2,
            string errorMessage = null)
        {
            var response = new VLMResponse
            {
                requestId = Guid.NewGuid().ToString("N").Substring(0, 8),
                success = success,
                errorMessage = errorMessage,
                processingTime = 1.2f,
                estimatedCost = 0.01f,
                tokensUsed = 500
            };

            if (success)
            {
                response.sceneResult = new SceneUnderstandingResult
                {
                    sceneDescription = "A room with furniture",
                    suggestedActions = new List<string> { "sit on chair", "examine table" }
                };

                for (int i = 0; i < objectCount; i++)
                {
                    response.sceneResult.objects.Add(CreateMockVisualObject($"detected_{i}"));
                }
            }

            return response;
        }

        /// <summary>
        /// Create a mock ActionVerificationResult for testing.
        /// </summary>
        public static ActionVerificationResult CreateMockVerificationResult(
            bool success = true,
            float confidence = 0.9f,
            string actionType = "sit",
            string targetId = "chair_01")
        {
            return new ActionVerificationResult
            {
                success = success,
                confidence = confidence,
                observedChange = success ? "Agent is now seated" : "No change detected",
                failureReason = success ? "" : "Action did not complete",
                actionType = actionType,
                targetId = targetId,
                objectType = "seat",
                affordance = "sit"
            };
        }

        /// <summary>
        /// Create mock JSON response for scene understanding.
        /// </summary>
        public static string CreateMockSceneJson(int objectCount = 2, float baseConfidence = 0.8f)
        {
            var objects = new List<string>();
            for (int i = 0; i < objectCount; i++)
            {
                objects.Add($@"{{
                    ""name"": ""Object{i}"",
                    ""type"": ""seat"",
                    ""description"": ""A comfortable seat"",
                    ""affordances"": [""sit"", ""examine""],
                    ""position"": {{ ""relative"": ""center"" }},
                    ""state"": ""available"",
                    ""confidence"": {(baseConfidence + i * 0.05f):F2}
                }}");
            }

            return $@"{{
                ""objects"": [{string.Join(",", objects)}],
                ""scene_description"": ""A test room"",
                ""suggested_actions"": [""sit on chair"", ""look around""]
            }}";
        }

        /// <summary>
        /// Create mock JSON response for action verification.
        /// </summary>
        public static string CreateMockVerificationJson(bool success = true, float confidence = 0.9f)
        {
            return $@"{{
                ""success"": {success.ToString().ToLower()},
                ""confidence"": {confidence:F2},
                ""observed_change"": ""{(success ? "Action completed successfully" : "No visible change")}"",
                ""failure_reason"": ""{(success ? "" : "Target state unchanged")}""
            }}";
        }

        /// <summary>
        /// Generate a simple test image as base64.
        /// </summary>
        public static string CreateMockImageBase64()
        {
            // Create a small 4x4 test image
            var texture = new Texture2D(4, 4, TextureFormat.RGB24, false);
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    texture.SetPixel(x, y, new Color(x / 4f, y / 4f, 0.5f));
                }
            }
            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);

            return Convert.ToBase64String(bytes);
        }
    }

    /// <summary>
    /// Mock VLM client for testing without actual API calls.
    /// </summary>
    public class MockVLMClient
    {
        public bool ShouldSucceed { get; set; } = true;
        public float ResponseDelay { get; set; } = 0.1f;
        public int ObjectCount { get; set; } = 2;
        public string ErrorMessage { get; set; } = "Mock error";

        public int RequestCount { get; private set; }
        public List<string> ReceivedPrompts { get; } = new List<string>();

        public void Reset()
        {
            RequestCount = 0;
            ReceivedPrompts.Clear();
        }

        public VLMResponse ProcessRequest(VLMRequest request)
        {
            RequestCount++;
            ReceivedPrompts.Add(request.prompt);

            if (!ShouldSucceed)
            {
                return new VLMResponse
                {
                    requestId = request.requestId,
                    success = false,
                    errorMessage = ErrorMessage
                };
            }

            return TestUtilities.CreateMockVLMResponse(true, ObjectCount);
        }
    }

    /// <summary>
    /// Test data for hallucination detection tests.
    /// </summary>
    public static class HallucinationTestData
    {
        public static VisualObjectReport CreateHallucination_LowConfidence()
        {
            return new VisualObjectReport
            {
                id = "hall_1",
                name = "Mysterious Object",
                type = "unknown",
                confidence = 0.3f, // Below threshold
                inferredAffordances = new[] { "examine" }
            };
        }

        public static VisualObjectReport CreateHallucination_InvalidAffordance()
        {
            return new VisualObjectReport
            {
                id = "hall_2",
                name = "Wall",
                type = "wall",
                confidence = 0.9f,
                inferredAffordances = new[] { "sit", "walk_through" } // Walls shouldn't be sittable
            };
        }

        public static VisualObjectReport CreateHallucination_ImpossibleObject()
        {
            return new VisualObjectReport
            {
                id = "hall_3",
                name = "Flying Chair",
                type = "seat",
                confidence = 0.7f,
                inferredAffordances = new[] { "sit" },
                estimatedPosition = new Vector3(0, 100, 0) // Impossibly high
            };
        }

        public static VisualObjectReport CreateValidObject()
        {
            return new VisualObjectReport
            {
                id = "valid_1",
                name = "Red Chair",
                type = "seat",
                confidence = 0.85f,
                inferredAffordances = new[] { "sit", "examine" },
                estimatedPosition = new Vector3(2, 0, 3)
            };
        }
    }
}

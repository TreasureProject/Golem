using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Golem.Vision.Tests
{
    /// <summary>
    /// PlayMode integration tests for the Vision system.
    /// These tests run in the Unity game loop with real GameObjects and cameras.
    /// </summary>
    [TestFixture]
    [Category("PlayMode")]
    public class VisionPlayModeTests
    {
        private GameObject testCamera;
        private GameObject testScene;
        private VisionConfig config;

        [SetUp]
        public void SetUp()
        {
            // Create a simple test scene
            testScene = new GameObject("TestScene");

            // Create camera
            testCamera = new GameObject("TestCamera");
            testCamera.transform.SetParent(testScene.transform);
            var camera = testCamera.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.blue;
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            // Create some test objects
            CreateTestObject("RedCube", Color.red, new Vector3(-2, 0, 5));
            CreateTestObject("GreenCube", Color.green, new Vector3(0, 0, 5));
            CreateTestObject("BlueCube", Color.blue, new Vector3(2, 0, 5));

            // Create config
            config = ScriptableObject.CreateInstance<VisionConfig>();
            config.enabled = true;
            config.captureWidth = 256;
            config.captureHeight = 256;
            config.jpegQuality = 75;
            config.minVisualConfidence = 0.5f;
        }

        [TearDown]
        public void TearDown()
        {
            if (testScene != null)
                Object.Destroy(testScene);
            if (config != null)
                Object.DestroyImmediate(config);
        }

        private GameObject CreateTestObject(string name, Color color, Vector3 position)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(testScene.transform);
            obj.transform.position = position;

            var renderer = obj.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;

            return obj;
        }

        #region Frame Capture Tests

        [UnityTest]
        public IEnumerator FrameCaptureService_CanCaptureFrame()
        {
            var captureGO = new GameObject("FrameCaptureService");
            captureGO.transform.SetParent(testScene.transform);
            var captureService = captureGO.AddComponent<FrameCaptureService>();

            // Set up via serialized fields
            captureService.config = config;
            captureService.targetCamera = testCamera.GetComponent<Camera>();

            // Wait a frame for rendering
            yield return null;

            CaptureResult result = null;
            bool captureComplete = false;

            captureService.CaptureFrameAsync((r) =>
            {
                result = r;
                captureComplete = true;
            });

            // Wait for capture (max 5 seconds)
            float timeout = 5f;
            float elapsed = 0f;
            while (!captureComplete && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(captureComplete, "Capture should complete within timeout");
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.success, $"Capture should succeed: {result.errorMessage}");
            Assert.IsFalse(string.IsNullOrEmpty(result.imageBase64), "Should have base64 image data");
            Assert.AreEqual(config.captureWidth, result.width);
            Assert.AreEqual(config.captureHeight, result.height);

            Debug.Log($"[PlayModeTest] Captured frame: {result.imageBase64.Length} bytes base64");
        }

        [UnityTest]
        public IEnumerator FrameCaptureService_CapturedImageIsValid()
        {
            var captureGO = new GameObject("FrameCaptureService");
            captureGO.transform.SetParent(testScene.transform);
            var captureService = captureGO.AddComponent<FrameCaptureService>();
            captureService.config = config;
            captureService.targetCamera = testCamera.GetComponent<Camera>();

            yield return null;

            CaptureResult result = null;
            bool captureComplete = false;

            captureService.CaptureFrameAsync((r) =>
            {
                result = r;
                captureComplete = true;
            });

            while (!captureComplete)
                yield return null;

            // Decode the base64 and verify it's a valid image
            byte[] imageBytes = System.Convert.FromBase64String(result.imageBase64);
            Assert.IsTrue(imageBytes.Length > 0, "Image bytes should not be empty");

            // JPEG starts with FFD8
            Assert.AreEqual(0xFF, imageBytes[0], "Should be valid JPEG (first byte)");
            Assert.AreEqual(0xD8, imageBytes[1], "Should be valid JPEG (second byte)");

            // Try to load as texture
            var texture = new Texture2D(2, 2);
            bool loaded = texture.LoadImage(imageBytes);
            Assert.IsTrue(loaded, "Should be able to load as texture");
            Assert.AreEqual(config.captureWidth, texture.width);
            Assert.AreEqual(config.captureHeight, texture.height);

            Object.Destroy(texture);
        }

        [UnityTest]
        public IEnumerator FrameCaptureService_MultipleCapturesWork()
        {
            var captureGO = new GameObject("FrameCaptureService");
            captureGO.transform.SetParent(testScene.transform);
            var captureService = captureGO.AddComponent<FrameCaptureService>();
            captureService.config = config;
            captureService.targetCamera = testCamera.GetComponent<Camera>();

            yield return null;

            int captureCount = 0;
            int targetCaptures = 3;

            for (int i = 0; i < targetCaptures; i++)
            {
                bool captureComplete = false;
                captureService.CaptureFrameAsync((r) =>
                {
                    if (r.success) captureCount++;
                    captureComplete = true;
                });

                while (!captureComplete)
                    yield return null;

                // Small delay between captures
                yield return new WaitForSeconds(0.1f);
            }

            Assert.AreEqual(targetCaptures, captureCount, "All captures should succeed");
        }

        #endregion

        #region Visual Object Cache Tests

        [UnityTest]
        public IEnumerator VisualObjectCache_CanCacheAndRetrieve()
        {
            var cacheGO = new GameObject("VisualObjectCache");
            cacheGO.transform.SetParent(testScene.transform);
            var cache = cacheGO.AddComponent<VisualObjectCache>();
            cache.config = config;

            yield return null;

            Vector3 testPosition = new Vector3(1, 0, 1);
            Vector3 testForward = Vector3.forward;

            // Create a mock scan result
            var scanResult = new VisualScanResult
            {
                success = true,
                scanId = "test_scan_001",
                agentPosition = testPosition,
                agentForward = testForward,
                sceneDescription = "Test scene with colored cubes",
                objects = new List<VisualObjectReport>
                {
                    new VisualObjectReport
                    {
                        id = "obj_001",
                        name = "RedCube",
                        type = "cube",
                        confidence = 0.95f,
                        relativePosition = "left"
                    }
                }
            };

            // Cache the result
            cache.Store(testPosition, testForward, scanResult);

            // Verify cached
            VisualScanResult cachedResult;
            bool found = cache.TryGetCached(testPosition, testForward, out cachedResult);
            Assert.IsTrue(found, "Should retrieve cached result");
            Assert.AreEqual("test_scan_001", cachedResult.scanId);
            Assert.AreEqual(1, cachedResult.objects.Count);
        }

        [UnityTest]
        public IEnumerator VisualObjectCache_InvalidatesOnDistance()
        {
            var cacheGO = new GameObject("VisualObjectCache");
            cacheGO.transform.SetParent(testScene.transform);
            var cache = cacheGO.AddComponent<VisualObjectCache>();
            cache.config = config;
            config.cacheInvalidationDistance = 2f;

            yield return null;

            Vector3 testPosition = Vector3.zero;
            Vector3 testForward = Vector3.forward;

            var scanResult = new VisualScanResult
            {
                success = true,
                scanId = "test_distance",
                agentPosition = testPosition,
                agentForward = testForward
            };

            cache.Store(testPosition, testForward, scanResult);

            // Query from same position - should hit
            VisualScanResult result1;
            bool found1 = cache.TryGetCached(testPosition, testForward, out result1);
            Assert.IsTrue(found1, "Should hit cache at same position");

            // Query from far position - should miss
            var farPosition = new Vector3(10, 0, 10);
            VisualScanResult result2;
            bool found2 = cache.TryGetCached(farPosition, testForward, out result2);
            Assert.IsFalse(found2, "Should miss cache at far position");
        }

        #endregion

        #region Hallucination Detector Tests

        [UnityTest]
        public IEnumerator HallucinationDetector_FiltersLowConfidence()
        {
            var detectorGO = new GameObject("HallucinationDetector");
            detectorGO.transform.SetParent(testScene.transform);
            var detector = detectorGO.AddComponent<HallucinationDetector>();
            detector.config = config;
            config.minVisualConfidence = 0.6f;

            yield return null;

            var scanResult = new VisualScanResult
            {
                success = true,
                scanId = "test_filter",
                objects = new List<VisualObjectReport>
                {
                    new VisualObjectReport
                    {
                        id = "high_conf",
                        name = "Chair",
                        type = "seat",
                        confidence = 0.9f
                    },
                    new VisualObjectReport
                    {
                        id = "low_conf",
                        name = "Maybe something",
                        type = "unknown",
                        confidence = 0.3f
                    }
                }
            };

            var filtered = detector.FilterScanResult(scanResult);

            Assert.AreEqual(1, filtered.objects.Count, "Should filter out low confidence object");
            Assert.AreEqual("high_conf", filtered.objects[0].id);
        }

        #endregion

        #region VLM Client Tests

        [UnityTest]
        public IEnumerator VLMClient_CanBeConfigured()
        {
            var clientGO = new GameObject("VLMClient");
            clientGO.transform.SetParent(testScene.transform);
            var client = clientGO.AddComponent<VLMClient>();
            client.config = config;

            config.provider = VLMProvider.OpenAI;
            config.modelName = "gpt-4o";

            yield return null;

            Assert.IsNotNull(client);
            Assert.AreEqual(VLMProvider.OpenAI, config.provider);
        }

        #endregion

        #region Live API Integration (if keys available)

        [UnityTest]
        [Category("LiveAPI")]
        public IEnumerator LiveAPI_FullPipeline_CaptureAndAnalyze()
        {
            // Load API key
            string apiKey = LoadOpenAIKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                Assert.Ignore("OpenAI API key not configured. Skipping live pipeline test.");
                yield break;
            }

            // Configure for live API
            config.provider = VLMProvider.OpenAI;
            config.apiKey = apiKey;
            config.modelName = "gpt-4o-mini";

            var pipelineGO = new GameObject("LiveVisionPipeline");
            pipelineGO.transform.SetParent(testScene.transform);

            var captureService = pipelineGO.AddComponent<FrameCaptureService>();
            captureService.config = config;
            captureService.targetCamera = testCamera.GetComponent<Camera>();

            var vlmClient = pipelineGO.AddComponent<VLMClient>();
            vlmClient.config = config;

            yield return null;

            // First, capture a frame
            CaptureResult captureResult = null;
            bool captureComplete = false;

            captureService.CaptureFrameAsync((r) =>
            {
                captureResult = r;
                captureComplete = true;
            });

            while (!captureComplete)
                yield return null;

            Assert.IsTrue(captureResult.success, "Capture should succeed");

            // Now send to VLM
            VLMResponse vlmResponse = null;
            bool vlmComplete = false;

            vlmClient.RequestSceneUnderstanding(captureResult.imageBase64, (response) =>
            {
                vlmResponse = response;
                vlmComplete = true;
            });

            // Wait for VLM response (max 60 seconds)
            float timeout = 60f;
            float elapsed = 0f;
            while (!vlmComplete && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(vlmComplete, "VLM request should complete within timeout");
            Assert.IsNotNull(vlmResponse, "VLM response should not be null");
            Assert.IsTrue(vlmResponse.success, $"VLM request should succeed: {vlmResponse.errorMessage}");

            Debug.Log($"[PlayModeTest] Live VLM response received");
            Debug.Log($"[PlayModeTest] Raw response: {vlmResponse.rawContent?.Substring(0, System.Math.Min(500, vlmResponse.rawContent?.Length ?? 0))}...");
        }

        [UnityTest]
        [Category("LiveAPI")]
        public IEnumerator LiveAPI_CaptureRealScene_SendToVLM()
        {
            string apiKey = LoadOpenAIKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                Assert.Ignore("OpenAI API key not configured. Skipping live scene test.");
                yield break;
            }

            config.provider = VLMProvider.OpenAI;
            config.apiKey = apiKey;
            config.modelName = "gpt-4o-mini";
            config.captureWidth = 512;
            config.captureHeight = 512;

            var pipelineGO = new GameObject("LiveSceneTest");
            pipelineGO.transform.SetParent(testScene.transform);

            var captureService = pipelineGO.AddComponent<FrameCaptureService>();
            captureService.config = config;
            captureService.targetCamera = testCamera.GetComponent<Camera>();

            var vlmClient = pipelineGO.AddComponent<VLMClient>();
            vlmClient.config = config;

            yield return null;
            yield return null; // Extra frame for rendering

            // Capture
            CaptureResult capture = null;
            bool captured = false;
            captureService.CaptureFrameAsync((r) => { capture = r; captured = true; });

            while (!captured) yield return null;

            Assert.IsTrue(capture.success);
            Debug.Log($"[LiveTest] Captured {capture.width}x{capture.height} image");

            // Send to VLM with specific prompt
            VLMResponse response = null;
            bool responded = false;

            vlmClient.RequestSceneUnderstanding(capture.imageBase64, (r) =>
            {
                response = r;
                responded = true;
            });

            float timeout = 60f;
            float elapsed = 0f;
            while (!responded && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(responded, "Should get VLM response");
            Assert.IsTrue(response.success, $"VLM should succeed: {response.errorMessage}");

            // Log what the VLM saw
            Debug.Log($"[LiveTest] VLM Analysis Complete");
            if (response.sceneResult != null)
            {
                Debug.Log($"[LiveTest] Scene: {response.sceneResult.sceneDescription}");
                Debug.Log($"[LiveTest] Objects found: {response.sceneResult.objects?.Count ?? 0}");
                foreach (var obj in response.sceneResult.objects ?? new List<VisualObjectReport>())
                {
                    Debug.Log($"[LiveTest]   - {obj.name} ({obj.type}): {obj.confidence:P0}");
                }
            }
        }

        private string LoadOpenAIKey()
        {
            string envPath = Path.Combine(Application.dataPath, "..", ".env.local");
            if (File.Exists(envPath))
            {
                string[] lines = File.ReadAllLines(envPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("OPENAI_API_KEY="))
                    {
                        string key = line.Substring("OPENAI_API_KEY=".Length).Trim();
                        if (key.StartsWith("sk-"))
                            return key;
                    }
                }
            }
            return System.Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        #endregion
    }
}

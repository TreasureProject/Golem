using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Golem.Vision.Tests
{
    /// <summary>
    /// Integration tests for the Visual Perception pipeline.
    /// Tests component interactions, data flow, and graceful degradation.
    /// Note: Does not make real API calls - uses mocked responses.
    /// </summary>
    [TestFixture]
    public class VisualPerceptionIntegrationTests
    {
        private GameObject testRoot;
        private VisualPerceptionManager perceptionManager;
        private FrameCaptureService captureService;
        private VLMClient vlmClient;
        private VisualObjectCache cache;
        private HallucinationDetector hallucinationDetector;
        private VisionConfig config;
        private VLMPromptTemplates templates;

        [SetUp]
        public void SetUp()
        {
            // Create test hierarchy
            testRoot = new GameObject("TestVisualPerceptionRoot");

            // Create config
            config = TestUtilities.CreateTestConfig();
            config.enabled = true;
            config.apiKey = "test-key";

            // Create templates
            templates = ScriptableObject.CreateInstance<VLMPromptTemplates>();

            // Create components in order
            captureService = testRoot.AddComponent<FrameCaptureService>();
            captureService.config = config;

            cache = testRoot.AddComponent<VisualObjectCache>();
            cache.config = config;

            vlmClient = testRoot.AddComponent<VLMClient>();
            vlmClient.config = config;
            vlmClient.promptTemplates = templates;

            hallucinationDetector = testRoot.AddComponent<HallucinationDetector>();
            hallucinationDetector.config = config;

            perceptionManager = testRoot.AddComponent<VisualPerceptionManager>();
            perceptionManager.config = config;
            perceptionManager.captureService = captureService;
            perceptionManager.vlmClient = vlmClient;
            perceptionManager.cache = cache;
            perceptionManager.hallucinationDetector = hallucinationDetector;
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
            if (config != null)
            {
                Object.DestroyImmediate(config);
            }
            if (templates != null)
            {
                Object.DestroyImmediate(templates);
            }
        }

        #region Pipeline Integration Tests

        [Test]
        public void Pipeline_AllComponentsInitialized()
        {
            // Assert - All components should be properly linked
            Assert.IsNotNull(perceptionManager.captureService, "CaptureService should be linked");
            Assert.IsNotNull(perceptionManager.vlmClient, "VLMClient should be linked");
            Assert.IsNotNull(perceptionManager.cache, "Cache should be linked");
            Assert.IsNotNull(perceptionManager.hallucinationDetector, "HallucinationDetector should be linked");
        }

        [Test]
        public void Pipeline_ConfigSharedAcrossComponents()
        {
            // Assert - All components should share the same config
            Assert.AreSame(config, captureService.config);
            Assert.AreSame(config, cache.config);
            Assert.AreSame(config, vlmClient.config);
            Assert.AreSame(config, hallucinationDetector.config);
            Assert.AreSame(config, perceptionManager.config);
        }

        [Test]
        public void Pipeline_DisabledWhenConfigDisabled()
        {
            // Arrange
            config.enabled = false;

            // Act
            bool canProcess = perceptionManager.CanProcessRequests();

            // Assert
            Assert.IsFalse(canProcess, "Pipeline should be disabled when config is disabled");
        }

        [Test]
        public void Pipeline_EnabledWithValidConfig()
        {
            // Arrange
            config.enabled = true;
            config.apiKey = "test-key";

            // Act
            bool canProcess = perceptionManager.CanProcessRequests();

            // Assert
            Assert.IsTrue(canProcess, "Pipeline should be enabled with valid config");
        }

        #endregion

        #region Cache Integration Tests

        [Test]
        public void CacheIntegration_StoredResultsRetrievable()
        {
            // Arrange
            var position = new Vector3(5, 0, 5);
            var forward = Vector3.forward;
            var mockResult = TestUtilities.CreateMockScanResult(3, true, position, forward);

            // Act
            cache.Store(position, forward, mockResult);
            bool found = cache.TryGetCached(position, forward, out VisualScanResult retrieved);

            // Assert
            Assert.IsTrue(found, "Should retrieve stored result");
            Assert.AreEqual(mockResult.scanId, retrieved.scanId);
            Assert.AreEqual(mockResult.objects.Count, retrieved.objects.Count);
        }

        [Test]
        public void CacheIntegration_InvalidationClearsResults()
        {
            // Arrange
            var position = Vector3.zero;
            var forward = Vector3.forward;
            cache.Store(position, forward, TestUtilities.CreateMockScanResult());

            // Act
            cache.InvalidateAll();
            bool found = cache.TryGetCached(position, forward, out _);

            // Assert
            Assert.IsFalse(found, "Should not find result after invalidation");
        }

        [Test]
        public void CacheIntegration_ImageHashLookupWorks()
        {
            // Arrange
            var position = Vector3.zero;
            var forward = Vector3.forward;
            var mockResult = TestUtilities.CreateMockScanResult();
            string imageHash = "INTEGRATION_TEST_HASH";
            cache.Store(position, forward, mockResult, imageHash);

            // Act
            bool foundByPosition = cache.TryGetCached(position, forward, out _);
            bool foundByHash = cache.TryGetByImageHash(imageHash, out _);

            // Assert
            Assert.IsTrue(foundByPosition, "Should find by position");
            Assert.IsTrue(foundByHash, "Should find by image hash");
        }

        #endregion

        #region Hallucination Detection Integration Tests

        [Test]
        public void HallucinationIntegration_FiltersScanResults()
        {
            // Arrange
            var scanResult = new VisualScanResult
            {
                success = true,
                objects = new List<VisualObjectReport>
                {
                    HallucinationTestData.CreateValidObject(),
                    HallucinationTestData.CreateHallucination_LowConfidence(),
                    HallucinationTestData.CreateValidObject(),
                    HallucinationTestData.CreateHallucination_InvalidAffordance()
                }
            };

            // Act
            var filtered = hallucinationDetector.FilterScanResult(scanResult);

            // Assert
            Assert.AreEqual(2, filtered.objects.Count, "Should filter out hallucinations");
        }

        [Test]
        public void HallucinationIntegration_UpdatesStatistics()
        {
            // Arrange
            var objects = new List<VisualObjectReport>
            {
                HallucinationTestData.CreateValidObject(),
                HallucinationTestData.CreateHallucination_LowConfidence()
            };

            // Act
            hallucinationDetector.FilterHallucinations(objects);
            var stats = hallucinationDetector.GetStats();

            // Assert
            Assert.AreEqual(2, stats.total, "Should track all checked objects");
            Assert.AreEqual(1, stats.hallucinations, "Should track detected hallucinations");
        }

        [Test]
        public void HallucinationIntegration_AdjustsConfidenceScores()
        {
            // Arrange
            var obj = new VisualObjectReport
            {
                id = "test_1",
                name = "Test Chair",
                type = "seat",
                confidence = 0.75f,
                inferredAffordances = new[] { "sit" },
                matchedStructured = true // Should boost confidence
            };

            // Act
            var result = hallucinationDetector.CheckObject(obj);

            // Assert
            Assert.Greater(result.confidenceScore, 0.75f,
                "Confidence should be boosted for matched structured objects");
        }

        #endregion

        #region VLM Client Integration Tests

        [Test]
        public void VLMClientIntegration_CanAcceptRequestsWithValidConfig()
        {
            // Arrange
            config.enabled = true;
            config.apiKey = "test-key";
            config.provider = VLMProvider.OpenAI;

            // Act
            bool canAccept = vlmClient.CanAcceptRequests();

            // Assert
            Assert.IsTrue(canAccept, "Should accept requests with valid config");
        }

        [Test]
        public void VLMClientIntegration_OllamaWorksWithoutApiKey()
        {
            // Arrange
            config.enabled = true;
            config.apiKey = "";
            config.provider = VLMProvider.Ollama;

            // Act
            bool canAccept = vlmClient.CanAcceptRequests();

            // Assert
            Assert.IsTrue(canAccept, "Ollama should work without API key");
        }

        [Test]
        public void VLMClientIntegration_TracksRequestStats()
        {
            // Arrange & Act
            var stats = vlmClient.Stats;

            // Assert
            Assert.AreEqual(0, stats.totalRequests, "Initial requests should be 0");
            Assert.AreEqual(0, stats.successfulRequests, "Initial successful should be 0");
            Assert.AreEqual(0f, stats.SuccessRate, "Initial success rate should be 0");
        }

        #endregion

        #region End-to-End Data Flow Tests

        [Test]
        public void DataFlow_MockScanResultContainsExpectedFields()
        {
            // Arrange
            var position = new Vector3(10, 0, 10);
            var forward = Vector3.forward;
            int objectCount = 3;

            // Act
            var result = TestUtilities.CreateMockScanResult(objectCount, true, position, forward);

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(objectCount, result.objects.Count);
            Assert.IsNotEmpty(result.scanId);
            Assert.IsNotEmpty(result.sceneDescription);
            Assert.IsNotNull(result.suggestedActions);

            foreach (var obj in result.objects)
            {
                Assert.IsNotEmpty(obj.id);
                Assert.IsNotEmpty(obj.name);
                Assert.Greater(obj.confidence, 0f);
                Assert.IsNotNull(obj.inferredAffordances);
            }
        }

        [Test]
        public void DataFlow_FilteredResultMaintainsStructure()
        {
            // Arrange
            var scanResult = new VisualScanResult
            {
                success = true,
                scanId = "test_scan_123",
                sceneDescription = "Test scene",
                suggestedActions = new List<string> { "explore", "interact" },
                objects = new List<VisualObjectReport>
                {
                    HallucinationTestData.CreateValidObject(),
                    HallucinationTestData.CreateHallucination_LowConfidence()
                }
            };

            // Act
            var filtered = hallucinationDetector.FilterScanResult(scanResult);

            // Assert
            Assert.AreEqual(scanResult.scanId, filtered.scanId, "Should preserve scan ID");
            Assert.AreEqual(scanResult.sceneDescription, filtered.sceneDescription, "Should preserve description");
            Assert.AreEqual(scanResult.suggestedActions, filtered.suggestedActions, "Should preserve actions");
            Assert.AreEqual(1, filtered.objects.Count, "Should filter to valid objects only");
        }

        [Test]
        public void DataFlow_CachedResultMatchesOriginal()
        {
            // Arrange
            var position = Vector3.zero;
            var forward = Vector3.forward;
            var original = TestUtilities.CreateMockScanResult(5, true, position, forward);
            cache.Store(position, forward, original);

            // Act
            cache.TryGetCached(position, forward, out VisualScanResult retrieved);

            // Assert
            Assert.AreEqual(original.scanId, retrieved.scanId);
            Assert.AreEqual(original.success, retrieved.success);
            Assert.AreEqual(original.objects.Count, retrieved.objects.Count);
            Assert.AreEqual(original.sceneDescription, retrieved.sceneDescription);
        }

        #endregion

        #region Graceful Degradation Tests

        [Test]
        public void GracefulDegradation_DisabledConfigStopsProcessing()
        {
            // Arrange
            config.enabled = false;

            // Assert
            Assert.IsFalse(perceptionManager.CanProcessRequests());
            Assert.IsFalse(vlmClient.CanAcceptRequests());
        }

        [Test]
        public void GracefulDegradation_MissingApiKeyStopsNonOllamaProviders()
        {
            // Arrange
            config.enabled = true;
            config.apiKey = "";
            config.provider = VLMProvider.OpenAI;

            // Assert
            Assert.IsFalse(vlmClient.CanAcceptRequests(),
                "OpenAI without API key should not accept requests");
        }

        [Test]
        public void GracefulDegradation_NullScanResultHandled()
        {
            // Act
            var filtered = hallucinationDetector.FilterScanResult(null);

            // Assert
            Assert.IsNull(filtered, "Should return null for null input");
        }

        [Test]
        public void GracefulDegradation_FailedScanResultPassedThrough()
        {
            // Arrange
            var failedResult = new VisualScanResult
            {
                success = false,
                errorMessage = "VLM API error"
            };

            // Act
            var filtered = hallucinationDetector.FilterScanResult(failedResult);

            // Assert
            Assert.IsFalse(filtered.success);
            Assert.AreEqual("VLM API error", filtered.errorMessage);
        }

        [Test]
        public void GracefulDegradation_EmptyObjectListHandled()
        {
            // Arrange
            var emptyResult = new VisualScanResult
            {
                success = true,
                objects = new List<VisualObjectReport>()
            };

            // Act
            var filtered = hallucinationDetector.FilterScanResult(emptyResult);

            // Assert
            Assert.IsTrue(filtered.success);
            Assert.AreEqual(0, filtered.objects.Count);
        }

        #endregion

        #region Configuration Propagation Tests

        [Test]
        public void ConfigPropagation_ConfidenceThresholdRespected()
        {
            // Arrange
            config.minVisualConfidence = 0.7f;
            hallucinationDetector.confidenceThreshold = config.minVisualConfidence;

            var lowConfObj = TestUtilities.CreateMockVisualObject("LowConf", 0.65f);
            var highConfObj = TestUtilities.CreateMockVisualObject("HighConf", 0.75f);

            // Act
            var lowResult = hallucinationDetector.CheckObject(lowConfObj);
            var highResult = hallucinationDetector.CheckObject(highConfObj);

            // Assert
            Assert.IsFalse(lowResult.isValid, "Object below threshold should be rejected");
            Assert.IsTrue(highResult.isValid, "Object above threshold should be accepted");
        }

        [Test]
        public void ConfigPropagation_CacheSettingsRespected()
        {
            // Arrange
            config.enableCache = false;

            // Act
            cache.Store(Vector3.zero, Vector3.forward, TestUtilities.CreateMockScanResult());
            var stats = cache.GetStats();

            // Assert
            Assert.AreEqual(0, stats.count, "Should not cache when disabled");
        }

        [Test]
        public void ConfigPropagation_ProviderUrlsCorrect()
        {
            // Test each provider
            config.customBaseUrl = "";

            config.provider = VLMProvider.OpenAI;
            Assert.AreEqual("https://api.openai.com/v1", config.GetBaseUrl());

            config.provider = VLMProvider.Anthropic;
            Assert.AreEqual("https://api.anthropic.com/v1", config.GetBaseUrl());

            config.provider = VLMProvider.Ollama;
            Assert.AreEqual("http://localhost:11434/api", config.GetBaseUrl());
        }

        [Test]
        public void ConfigPropagation_CustomUrlOverridesProvider()
        {
            // Arrange
            config.provider = VLMProvider.OpenAI;
            config.customBaseUrl = "https://my-custom-api.com/v1";

            // Act
            string url = config.GetBaseUrl();

            // Assert
            Assert.AreEqual("https://my-custom-api.com/v1", url);
        }

        #endregion

        #region Statistics Integration Tests

        [Test]
        public void StatisticsIntegration_CacheStatsAccurate()
        {
            // Arrange
            var position = Vector3.zero;
            var forward = Vector3.forward;
            cache.Store(position, forward, TestUtilities.CreateMockScanResult());

            // Act - 2 hits, 1 miss
            cache.TryGetCached(position, forward, out _);
            cache.TryGetCached(position, forward, out _);
            cache.TryGetCached(new Vector3(100, 0, 0), forward, out _);

            var stats = cache.GetStats();

            // Assert
            Assert.AreEqual(2, stats.hits);
            Assert.AreEqual(1, stats.misses);
            Assert.AreEqual(2f / 3f, stats.hitRate, 0.01f);
        }

        [Test]
        public void StatisticsIntegration_HallucinationStatsAccurate()
        {
            // Arrange
            var objects = new List<VisualObjectReport>
            {
                HallucinationTestData.CreateValidObject(),
                HallucinationTestData.CreateValidObject(),
                HallucinationTestData.CreateHallucination_LowConfidence()
            };

            // Act
            hallucinationDetector.FilterHallucinations(objects);
            var stats = hallucinationDetector.GetStats();

            // Assert
            Assert.AreEqual(3, stats.total);
            Assert.AreEqual(1, stats.hallucinations);
            Assert.AreEqual(1f / 3f, stats.rate, 0.01f);
        }

        [Test]
        public void StatisticsIntegration_ResetClearsAllStats()
        {
            // Arrange - Generate some stats
            var position = Vector3.zero;
            var forward = Vector3.forward;
            cache.Store(position, forward, TestUtilities.CreateMockScanResult());
            cache.TryGetCached(position, forward, out _);

            hallucinationDetector.CheckObject(HallucinationTestData.CreateValidObject());
            hallucinationDetector.CheckObject(HallucinationTestData.CreateHallucination_LowConfidence());

            // Act
            cache.ResetStats();
            hallucinationDetector.ResetStats();

            // Assert
            var cacheStats = cache.GetStats();
            var hallucinationStats = hallucinationDetector.GetStats();

            Assert.AreEqual(0, cacheStats.hits);
            Assert.AreEqual(0, cacheStats.misses);
            Assert.AreEqual(0, hallucinationStats.total);
            Assert.AreEqual(0, hallucinationStats.hallucinations);
        }

        #endregion
    }
}

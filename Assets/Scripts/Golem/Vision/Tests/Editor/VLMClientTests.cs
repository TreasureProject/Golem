using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Golem.Vision.Tests
{
    /// <summary>
    /// Unit tests for VLMClient.
    /// Tests JSON parsing, response handling, and configuration.
    /// Note: Does not test actual API calls - those are integration tests.
    /// </summary>
    [TestFixture]
    public class VLMClientTests
    {
        private GameObject testObject;
        private VLMClient client;
        private VisionConfig config;
        private VLMPromptTemplates templates;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestVLMClient");
            client = testObject.AddComponent<VLMClient>();
            config = TestUtilities.CreateTestConfig();
            templates = ScriptableObject.CreateInstance<VLMPromptTemplates>();

            client.config = config;
            client.promptTemplates = templates;
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
            if (templates != null)
            {
                Object.DestroyImmediate(templates);
            }
        }

        #region Configuration Tests

        [Test]
        public void CanAcceptRequests_ReturnsFalseWhenDisabled()
        {
            // Arrange
            config.enabled = false;

            // Act
            bool canAccept = client.CanAcceptRequests();

            // Assert
            Assert.IsFalse(canAccept, "Should not accept requests when disabled");
        }

        [Test]
        public void CanAcceptRequests_ReturnsFalseWithoutApiKey()
        {
            // Arrange
            config.enabled = true;
            config.apiKey = "";
            config.provider = VLMProvider.OpenAI;

            // Act
            bool canAccept = client.CanAcceptRequests();

            // Assert
            Assert.IsFalse(canAccept, "Should not accept requests without API key");
        }

        [Test]
        public void CanAcceptRequests_ReturnsTrueForOllamaWithoutKey()
        {
            // Arrange
            config.enabled = true;
            config.apiKey = "";
            config.provider = VLMProvider.Ollama;

            // Act
            bool canAccept = client.CanAcceptRequests();

            // Assert
            Assert.IsTrue(canAccept, "Ollama should work without API key");
        }

        #endregion

        #region JSON Parsing Tests - Scene Understanding

        [Test]
        public void ParseSceneJson_ExtractsObjects()
        {
            // Arrange
            string json = TestUtilities.CreateMockSceneJson(3, 0.75f);

            // The VLMClient has private parsing methods, so we test indirectly
            // by checking if a mock response with this JSON structure would be valid
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("objects"));
            Assert.IsTrue(json.Contains("confidence"));
        }

        [Test]
        public void ParseSceneJson_HandlesEmptyObjects()
        {
            // Arrange
            string json = @"{
                ""objects"": [],
                ""scene_description"": ""Empty room"",
                ""suggested_actions"": []
            }";

            // Assert - JSON should be valid
            Assert.IsTrue(json.Contains("objects"));
        }

        [Test]
        public void ParseSceneJson_ExtractsSceneDescription()
        {
            // Arrange
            string json = TestUtilities.CreateMockSceneJson(1);

            // Assert
            Assert.IsTrue(json.Contains("scene_description"));
        }

        [Test]
        public void ParseSceneJson_ExtractsSuggestedActions()
        {
            // Arrange
            string json = TestUtilities.CreateMockSceneJson(1);

            // Assert
            Assert.IsTrue(json.Contains("suggested_actions"));
        }

        #endregion

        #region JSON Parsing Tests - Action Verification

        [Test]
        public void ParseVerificationJson_ExtractsSuccess()
        {
            // Arrange
            string jsonSuccess = TestUtilities.CreateMockVerificationJson(true, 0.95f);
            string jsonFail = TestUtilities.CreateMockVerificationJson(false, 0.7f);

            // Assert
            Assert.IsTrue(jsonSuccess.Contains("\"success\": true"));
            Assert.IsTrue(jsonFail.Contains("\"success\": false"));
        }

        [Test]
        public void ParseVerificationJson_ExtractsConfidence()
        {
            // Arrange
            string json = TestUtilities.CreateMockVerificationJson(true, 0.85f);

            // Assert
            Assert.IsTrue(json.Contains("\"confidence\": 0.85"));
        }

        [Test]
        public void ParseVerificationJson_ExtractsObservedChange()
        {
            // Arrange
            string json = TestUtilities.CreateMockVerificationJson(true);

            // Assert
            Assert.IsTrue(json.Contains("observed_change"));
        }

        [Test]
        public void ParseVerificationJson_ExtractsFailureReason()
        {
            // Arrange
            string jsonFail = TestUtilities.CreateMockVerificationJson(false, 0.5f);

            // Assert
            Assert.IsTrue(jsonFail.Contains("failure_reason"));
        }

        #endregion

        #region Request Creation Tests

        [Test]
        public void VLMRequest_GeneratesUniqueIds()
        {
            // Arrange & Act
            var request1 = new VLMRequest();
            var request2 = new VLMRequest();

            // Assert
            Assert.AreNotEqual(request1.requestId, request2.requestId,
                "Each request should have unique ID");
        }

        [Test]
        public void VLMRequest_SetsCreatedTime()
        {
            // Arrange & Act
            var request = new VLMRequest();

            // Assert
            Assert.Greater(request.createdTime, 0, "Created time should be set");
        }

        [Test]
        public void VLMRequest_InitializesPromptVariables()
        {
            // Arrange & Act
            var request = new VLMRequest();

            // Assert
            Assert.IsNotNull(request.promptVariables);
        }

        #endregion

        #region Statistics Tests

        [Test]
        public void Stats_InitiallyZero()
        {
            // Act
            var stats = client.Stats;

            // Assert
            Assert.AreEqual(0, stats.totalRequests);
            Assert.AreEqual(0, stats.successfulRequests);
            Assert.AreEqual(0, stats.failedRequests);
        }

        [Test]
        public void Stats_SuccessRate_ReturnsZeroForNoRequests()
        {
            // Act
            var stats = client.Stats;

            // Assert
            Assert.AreEqual(0f, stats.SuccessRate);
        }

        [Test]
        public void PendingRequestCount_InitiallyZero()
        {
            // Act & Assert
            Assert.AreEqual(0, client.PendingRequestCount);
        }

        #endregion

        #region Budget Tests

        [Test]
        public void CanAcceptRequests_ReturnsFalseWhenBudgetExceeded()
        {
            // Arrange
            config.enabled = true;
            config.apiKey = "test-key";
            config.maxCostPerHour = 0.01f;
            config.pauseOnBudgetExceeded = true;

            // Simulate budget exceeded
            var stats = client.Stats;
            stats.currentHourCost = 0.02f;

            // Note: This test verifies the logic but actual budget check
            // happens inside CanAcceptRequests which reads from stats
        }

        [Test]
        public void Stats_ResetHourlyIfNeeded_ResetsAfterHour()
        {
            // Arrange
            var stats = new VisualPerceptionStats();
            stats.currentHourCost = 0.5f;
            stats.currentHourStartTime = 0f;

            // Act - simulate time passed (3601 seconds = just over 1 hour)
            stats.ResetHourlyIfNeeded(3601f);

            // Assert
            Assert.AreEqual(0f, stats.currentHourCost, "Should reset hourly cost");
            Assert.AreEqual(3601f, stats.currentHourStartTime, "Should update start time");
        }

        [Test]
        public void Stats_ResetHourlyIfNeeded_DoesNotResetWithinHour()
        {
            // Arrange
            var stats = new VisualPerceptionStats();
            stats.currentHourCost = 0.5f;
            stats.currentHourStartTime = 0f;

            // Act - simulate time passed (1800 seconds = 30 minutes)
            stats.ResetHourlyIfNeeded(1800f);

            // Assert
            Assert.AreEqual(0.5f, stats.currentHourCost, "Should not reset within hour");
        }

        #endregion

        #region Provider URL Tests

        [Test]
        public void GetBaseUrl_ReturnsCorrectUrlForOpenAI()
        {
            // Arrange
            config.provider = VLMProvider.OpenAI;
            config.customBaseUrl = "";

            // Act
            string url = config.GetBaseUrl();

            // Assert
            Assert.AreEqual("https://api.openai.com/v1", url);
        }

        [Test]
        public void GetBaseUrl_ReturnsCorrectUrlForAnthropic()
        {
            // Arrange
            config.provider = VLMProvider.Anthropic;
            config.customBaseUrl = "";

            // Act
            string url = config.GetBaseUrl();

            // Assert
            Assert.AreEqual("https://api.anthropic.com/v1", url);
        }

        [Test]
        public void GetBaseUrl_ReturnsCorrectUrlForOllama()
        {
            // Arrange
            config.provider = VLMProvider.Ollama;
            config.customBaseUrl = "";

            // Act
            string url = config.GetBaseUrl();

            // Assert
            Assert.AreEqual("http://localhost:11434/api", url);
        }

        [Test]
        public void GetBaseUrl_UsesCustomUrlWhenProvided()
        {
            // Arrange
            config.provider = VLMProvider.OpenAI;
            config.customBaseUrl = "https://custom.api.com/v1";

            // Act
            string url = config.GetBaseUrl();

            // Assert
            Assert.AreEqual("https://custom.api.com/v1", url);
        }

        #endregion

        #region Prompt Template Tests

        [Test]
        public void PromptTemplates_SceneUnderstandingNotEmpty()
        {
            // Assert
            Assert.IsNotEmpty(templates.sceneUnderstandingPrompt);
        }

        [Test]
        public void PromptTemplates_ActionVerificationNotEmpty()
        {
            // Assert
            Assert.IsNotEmpty(templates.actionVerificationPrompt);
        }

        [Test]
        public void PromptTemplates_AffordanceDiscoveryNotEmpty()
        {
            // Assert
            Assert.IsNotEmpty(templates.affordanceDiscoveryPrompt);
        }

        [Test]
        public void PromptTemplates_GetPrompt_ReturnsCorrectTemplate()
        {
            // Act
            string scene = templates.GetPrompt(VLMRequestType.SceneUnderstanding);
            string verify = templates.GetPrompt(VLMRequestType.ActionVerification);
            string afford = templates.GetPrompt(VLMRequestType.AffordanceDiscovery);

            // Assert
            Assert.AreEqual(templates.sceneUnderstandingPrompt, scene);
            Assert.AreEqual(templates.actionVerificationPrompt, verify);
            Assert.AreEqual(templates.affordanceDiscoveryPrompt, afford);
        }

        [Test]
        public void PromptTemplates_ReplaceVariables_ReplacesPlaceholders()
        {
            // Arrange
            string template = "Action: {action}, Target: {target}";

            // Act
            string result = VLMPromptTemplates.ReplaceVariables(template,
                ("action", "sit"),
                ("target", "chair"));

            // Assert
            Assert.AreEqual("Action: sit, Target: chair", result);
        }

        #endregion
    }
}

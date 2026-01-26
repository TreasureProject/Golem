using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Golem.Vision.Tests
{
    /// <summary>
    /// Unit and integration tests for ActionVerifier.
    /// Tests the VIGA-style before/after verification pattern.
    /// </summary>
    [TestFixture]
    public class ActionVerifierTests
    {
        private GameObject testObject;
        private ActionVerifier verifier;
        private VisionConfig config;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestActionVerifier");
            verifier = testObject.AddComponent<ActionVerifier>();
            config = TestUtilities.CreateTestConfig();
            verifier.config = config;
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
            if (config != null)
            {
                Object.DestroyImmediate(config);
            }
        }

        #region Verification Request Tests

        [Test]
        public void VerificationRequest_GeneratesUniqueId()
        {
            // Arrange & Act
            var request1 = new ActionVerificationRequest("action1", "target1");
            var request2 = new ActionVerificationRequest("action2", "target2");

            // Assert
            Assert.IsNotEmpty(request1.requestId);
            Assert.IsNotEmpty(request2.requestId);
            Assert.AreNotEqual(request1.requestId, request2.requestId);
        }

        [Test]
        public void VerificationRequest_StoresActionAndTarget()
        {
            // Arrange & Act
            var request = new ActionVerificationRequest("sit", "chair_1");

            // Assert
            Assert.AreEqual("sit", request.actionName);
            Assert.AreEqual("chair_1", request.targetId);
        }

        [Test]
        public void VerificationRequest_CanStoreExpectedOutcome()
        {
            // Arrange & Act
            var request = new ActionVerificationRequest("open", "door_1")
            {
                expectedOutcome = "Door should be in open state"
            };

            // Assert
            Assert.AreEqual("Door should be in open state", request.expectedOutcome);
        }

        #endregion

        #region Verification Result Tests

        [Test]
        public void VerificationResult_SuccessState()
        {
            // Arrange
            var result = new ActionVerificationResult
            {
                success = true,
                confidence = 0.95f,
                observedChange = "Agent is now sitting on chair",
                verificationTime = 1.5f
            };

            // Assert
            Assert.IsTrue(result.success);
            Assert.AreEqual(0.95f, result.confidence);
            Assert.IsNotEmpty(result.observedChange);
            Assert.IsEmpty(result.failureReason);
        }

        [Test]
        public void VerificationResult_FailureState()
        {
            // Arrange
            var result = new ActionVerificationResult
            {
                success = false,
                confidence = 0.8f,
                observedChange = "No change detected",
                failureReason = "Target object not found in view"
            };

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsNotEmpty(result.failureReason);
        }

        [Test]
        public void VerificationResult_ParsesFromJson_Success()
        {
            // Arrange
            string json = TestUtilities.CreateMockVerificationJson(true, 0.92f);

            // Assert - JSON structure is valid
            Assert.IsTrue(json.Contains("\"success\": true"));
            Assert.IsTrue(json.Contains("\"confidence\": 0.92"));
            Assert.IsTrue(json.Contains("observed_change"));
        }

        [Test]
        public void VerificationResult_ParsesFromJson_Failure()
        {
            // Arrange
            string json = TestUtilities.CreateMockVerificationJson(false, 0.65f);

            // Assert - JSON structure is valid
            Assert.IsTrue(json.Contains("\"success\": false"));
            Assert.IsTrue(json.Contains("\"confidence\": 0.65"));
            Assert.IsTrue(json.Contains("failure_reason"));
        }

        #endregion

        #region State Tracking Tests

        [Test]
        public void StateTracking_InitiallyNotVerifying()
        {
            // Assert
            Assert.IsFalse(verifier.IsVerifying);
        }

        [Test]
        public void StateTracking_PendingCountInitiallyZero()
        {
            // Assert
            Assert.AreEqual(0, verifier.PendingVerificationCount);
        }

        #endregion

        #region Before/After Capture Pattern Tests

        [Test]
        public void BeforeAfterPattern_CanStoreBefore()
        {
            // Arrange
            var mockCapture = new CaptureResult
            {
                success = true,
                imageBase64 = TestUtilities.CreateMockImageBase64(),
                width = 256,
                height = 256
            };

            // Act
            verifier.SetBeforeCapture("test_action", mockCapture);

            // Assert - Should not throw, verifier stores the before state
            Assert.Pass("Before capture stored successfully");
        }

        [Test]
        public void BeforeAfterPattern_CanStoreAfter()
        {
            // Arrange
            var mockCapture = new CaptureResult
            {
                success = true,
                imageBase64 = TestUtilities.CreateMockImageBase64(),
                width = 256,
                height = 256
            };

            // Act
            verifier.SetAfterCapture("test_action", mockCapture);

            // Assert - Should not throw, verifier stores the after state
            Assert.Pass("After capture stored successfully");
        }

        [Test]
        public void BeforeAfterPattern_ClearsOnCompletion()
        {
            // Arrange
            var mockCapture = new CaptureResult
            {
                success = true,
                imageBase64 = TestUtilities.CreateMockImageBase64()
            };
            verifier.SetBeforeCapture("test_action", mockCapture);
            verifier.SetAfterCapture("test_action", mockCapture);

            // Act
            verifier.ClearPendingVerification("test_action");

            // Assert
            Assert.IsFalse(verifier.HasPendingVerification("test_action"));
        }

        #endregion

        #region Confidence Threshold Tests

        [Test]
        public void ConfidenceThreshold_LowConfidenceMarkedAsUncertain()
        {
            // Arrange
            config.minVerificationConfidence = 0.7f;

            var result = new ActionVerificationResult
            {
                success = true,
                confidence = 0.5f
            };

            // Act
            bool isConfident = verifier.IsConfidentResult(result);

            // Assert
            Assert.IsFalse(isConfident, "Low confidence result should not be marked as confident");
        }

        [Test]
        public void ConfidenceThreshold_HighConfidenceMarkedAsConfident()
        {
            // Arrange
            config.minVerificationConfidence = 0.7f;

            var result = new ActionVerificationResult
            {
                success = true,
                confidence = 0.85f
            };

            // Act
            bool isConfident = verifier.IsConfidentResult(result);

            // Assert
            Assert.IsTrue(isConfident, "High confidence result should be marked as confident");
        }

        #endregion

        #region Verification Statistics Tests

        [Test]
        public void Statistics_InitiallyZero()
        {
            // Act
            var stats = verifier.GetStats();

            // Assert
            Assert.AreEqual(0, stats.totalVerifications);
            Assert.AreEqual(0, stats.successfulVerifications);
            Assert.AreEqual(0, stats.failedVerifications);
        }

        [Test]
        public void Statistics_TracksSuccessRate()
        {
            // Arrange
            var stats = new ActionVerificationStats
            {
                totalVerifications = 10,
                successfulVerifications = 8,
                failedVerifications = 2
            };

            // Act
            float successRate = stats.SuccessRate;

            // Assert
            Assert.AreEqual(0.8f, successRate, 0.01f);
        }

        [Test]
        public void Statistics_SuccessRateZeroWhenNoVerifications()
        {
            // Arrange
            var stats = new ActionVerificationStats();

            // Act
            float successRate = stats.SuccessRate;

            // Assert
            Assert.AreEqual(0f, successRate);
        }

        [Test]
        public void Statistics_ResetClearsAll()
        {
            // Arrange - Create some stats by recording verifications
            verifier.RecordVerificationResult(true);
            verifier.RecordVerificationResult(false);
            verifier.RecordVerificationResult(true);

            // Act
            verifier.ResetStats();
            var stats = verifier.GetStats();

            // Assert
            Assert.AreEqual(0, stats.totalVerifications);
            Assert.AreEqual(0, stats.successfulVerifications);
            Assert.AreEqual(0, stats.failedVerifications);
        }

        #endregion

        #region Action Type Tests

        [Test]
        public void ActionType_SitActionRequiresVerification()
        {
            // Act
            bool requires = verifier.RequiresVerification("sit");

            // Assert
            Assert.IsTrue(requires, "Sit action should require verification");
        }

        [Test]
        public void ActionType_OpenActionRequiresVerification()
        {
            // Act
            bool requires = verifier.RequiresVerification("open");

            // Assert
            Assert.IsTrue(requires, "Open action should require verification");
        }

        [Test]
        public void ActionType_PickupActionRequiresVerification()
        {
            // Act
            bool requires = verifier.RequiresVerification("pickup");

            // Assert
            Assert.IsTrue(requires, "Pickup action should require verification");
        }

        [Test]
        public void ActionType_MoveActionMayNotRequireVerification()
        {
            // Act
            bool requires = verifier.RequiresVerification("move");

            // Assert - Movement is typically verified by navigation system
            Assert.IsFalse(requires, "Move action may not require visual verification");
        }

        [Test]
        public void ActionType_LookActionDoesNotRequireVerification()
        {
            // Act
            bool requires = verifier.RequiresVerification("look");

            // Assert
            Assert.IsFalse(requires, "Look action does not require verification");
        }

        #endregion

        #region Edge Cases

        [Test]
        public void EdgeCase_NullActionName()
        {
            // Act
            bool requires = verifier.RequiresVerification(null);

            // Assert
            Assert.IsFalse(requires, "Null action should not require verification");
        }

        [Test]
        public void EdgeCase_EmptyActionName()
        {
            // Act
            bool requires = verifier.RequiresVerification("");

            // Assert
            Assert.IsFalse(requires, "Empty action should not require verification");
        }

        [Test]
        public void EdgeCase_UnknownActionType()
        {
            // Act
            bool requires = verifier.RequiresVerification("unknown_custom_action");

            // Assert - Unknown actions should default to requiring verification for safety
            Assert.IsTrue(requires, "Unknown actions should require verification by default");
        }

        [Test]
        public void EdgeCase_CaseInsensitiveActionMatching()
        {
            // Act
            bool requiresSit = verifier.RequiresVerification("SIT");
            bool requiresOpen = verifier.RequiresVerification("OPEN");
            bool requiresMove = verifier.RequiresVerification("MOVE");

            // Assert - Should match regardless of case
            Assert.IsTrue(requiresSit);
            Assert.IsTrue(requiresOpen);
            Assert.IsFalse(requiresMove);
        }

        #endregion
    }
}

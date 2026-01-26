using NUnit.Framework;
using UnityEngine;

namespace Golem.Vision.Tests
{
    /// <summary>
    /// Unit tests for FrameCaptureService.
    /// Tests configuration, resource management, and capture result handling.
    /// Note: Actual capture tests require a camera and are in integration tests.
    /// </summary>
    [TestFixture]
    public class FrameCaptureServiceTests
    {
        private GameObject testObject;
        private FrameCaptureService captureService;
        private VisionConfig config;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestCaptureService");
            captureService = testObject.AddComponent<FrameCaptureService>();
            config = TestUtilities.CreateTestConfig();
            captureService.config = config;
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        #region Configuration Tests

        [Test]
        public void Config_DefaultResolution()
        {
            // Assert
            Assert.AreEqual(256, config.captureWidth);
            Assert.AreEqual(256, config.captureHeight);
        }

        [Test]
        public void Config_JpegQuality_WithinValidRange()
        {
            // Assert
            Assert.GreaterOrEqual(config.jpegQuality, 50);
            Assert.LessOrEqual(config.jpegQuality, 100);
        }

        #endregion

        #region State Tests

        [Test]
        public void IsCapturing_InitiallyFalse()
        {
            // Assert
            Assert.IsFalse(captureService.IsCapturing);
        }

        #endregion

        #region CaptureResult Tests

        [Test]
        public void CaptureResult_Success_HasRequiredFields()
        {
            // Arrange
            var result = new CaptureResult
            {
                success = true,
                imageBase64 = TestUtilities.CreateMockImageBase64(),
                imageBytes = new byte[] { 1, 2, 3 },
                width = 256,
                height = 256,
                captureMode = CaptureMode.AgentPOV,
                captureTime = Time.time
            };

            // Assert
            Assert.IsTrue(result.success);
            Assert.IsNotEmpty(result.imageBase64);
            Assert.IsNotNull(result.imageBytes);
            Assert.Greater(result.width, 0);
            Assert.Greater(result.height, 0);
        }

        [Test]
        public void CaptureResult_Failure_HasErrorMessage()
        {
            // Arrange
            var result = new CaptureResult
            {
                success = false,
                errorMessage = "No camera available"
            };

            // Assert
            Assert.IsFalse(result.success);
            Assert.IsNotEmpty(result.errorMessage);
        }

        [Test]
        public void CaptureResult_ImageHash_IsConsistent()
        {
            // Arrange
            byte[] imageData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var result1 = new CaptureResult { imageBytes = imageData };
            var result2 = new CaptureResult { imageBytes = imageData };

            // Act
            string hash1 = result1.ImageHash;
            string hash2 = result2.ImageHash;

            // Assert
            Assert.AreEqual(hash1, hash2, "Same image data should produce same hash");
        }

        [Test]
        public void CaptureResult_ImageHash_DiffersForDifferentImages()
        {
            // Arrange
            var result1 = new CaptureResult { imageBytes = new byte[] { 1, 2, 3, 4 } };
            var result2 = new CaptureResult { imageBytes = new byte[] { 5, 6, 7, 8 } };

            // Act
            string hash1 = result1.ImageHash;
            string hash2 = result2.ImageHash;

            // Assert
            Assert.AreNotEqual(hash1, hash2, "Different image data should produce different hash");
        }

        [Test]
        public void CaptureResult_ImageHash_HandlesNullBytes()
        {
            // Arrange
            var result = new CaptureResult { imageBytes = null };

            // Act
            string hash = result.ImageHash;

            // Assert
            Assert.AreEqual("", hash, "Null bytes should return empty hash");
        }

        [Test]
        public void CaptureResult_ImageHash_HandlesEmptyBytes()
        {
            // Arrange
            var result = new CaptureResult { imageBytes = new byte[0] };

            // Act
            string hash = result.ImageHash;

            // Assert
            Assert.AreEqual("", hash, "Empty bytes should return empty hash");
        }

        #endregion

        #region Image Hash Computation Tests

        [Test]
        public void ComputeImageHash_ReturnsConsistentValue()
        {
            // Arrange
            byte[] data = new byte[] { 10, 20, 30, 40, 50 };

            // Act
            string hash1 = FrameCaptureService.ComputeImageHash(data);
            string hash2 = FrameCaptureService.ComputeImageHash(data);

            // Assert
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void ComputeImageHash_ReturnsHexString()
        {
            // Arrange
            byte[] data = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            string hash = FrameCaptureService.ComputeImageHash(data);

            // Assert
            Assert.AreEqual(8, hash.Length, "Hash should be 8 character hex string");
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(hash, "^[0-9A-F]+$"),
                "Hash should be hexadecimal");
        }

        [Test]
        public void ComputeImageHash_NullReturnsEmpty()
        {
            // Act
            string hash = FrameCaptureService.ComputeImageHash(null);

            // Assert
            Assert.AreEqual("", hash);
        }

        #endregion

        #region Capture Mode Tests

        [Test]
        public void CaptureMode_AgentPOV_IsDefault()
        {
            // Assert
            Assert.AreEqual(CaptureMode.AgentPOV, config.captureMode);
        }

        [Test]
        public void CaptureMode_AllModesAreDefined()
        {
            // Assert - Verify all expected modes exist
            Assert.IsTrue(System.Enum.IsDefined(typeof(CaptureMode), CaptureMode.AgentPOV));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CaptureMode), CaptureMode.ThirdPerson));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CaptureMode), CaptureMode.Overhead));
            Assert.IsTrue(System.Enum.IsDefined(typeof(CaptureMode), CaptureMode.Multiple));
        }

        #endregion

        #region Resource Management Tests

        [Test]
        public void ReconfigureIfNeeded_HandlesConfigChange()
        {
            // Arrange - Initial config
            config.captureWidth = 256;
            config.captureHeight = 256;

            // Act - Change resolution
            config.captureWidth = 512;
            config.captureHeight = 512;

            // Assert - Method should exist and not throw
            Assert.DoesNotThrow(() => captureService.ReconfigureIfNeeded());
        }

        #endregion

        #region VisionConfig Validation Tests

        [Test]
        public void VisionConfig_Validate_ReturnsTrueForValidConfig()
        {
            // Arrange
            config.enabled = true;
            config.apiKey = "test-key";
            config.provider = VLMProvider.OpenAI;

            // Act
            bool valid = config.Validate();

            // Assert
            Assert.IsTrue(valid);
        }

        [Test]
        public void VisionConfig_Validate_ReturnsFalseWithoutApiKey()
        {
            // Arrange
            config.enabled = true;
            config.apiKey = "";
            config.provider = VLMProvider.OpenAI;

            // Act
            bool valid = config.Validate();

            // Assert
            Assert.IsFalse(valid);
        }

        [Test]
        public void VisionConfig_Validate_ReturnsTrueForOllamaWithoutKey()
        {
            // Arrange
            config.enabled = true;
            config.apiKey = "";
            config.provider = VLMProvider.Ollama;

            // Act
            bool valid = config.Validate();

            // Assert
            Assert.IsTrue(valid);
        }

        #endregion
    }
}

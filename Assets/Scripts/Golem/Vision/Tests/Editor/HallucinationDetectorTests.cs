using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Golem.Vision.Tests
{
    /// <summary>
    /// Unit tests for HallucinationDetector.
    /// Tests confidence thresholds, common sense rules, and filtering.
    /// </summary>
    [TestFixture]
    public class HallucinationDetectorTests
    {
        private GameObject testObject;
        private HallucinationDetector detector;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestDetector");
            detector = testObject.AddComponent<HallucinationDetector>();
            detector.confidenceThreshold = 0.6f;
            detector.maxValidHeight = 20f;
            detector.minValidHeight = -5f;
            detector.maxValidDistance = 50f;
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        #region Confidence Threshold Tests

        [Test]
        public void CheckObject_RejectsLowConfidenceObject()
        {
            // Arrange
            var obj = HallucinationTestData.CreateHallucination_LowConfidence();

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsFalse(result.isValid, "Low confidence object should be rejected");
            Assert.IsTrue(result.issues.Exists(i => i.Contains("Low confidence")),
                "Should report low confidence issue");
        }

        [Test]
        public void CheckObject_AcceptsHighConfidenceObject()
        {
            // Arrange
            var obj = HallucinationTestData.CreateValidObject();

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsTrue(result.isValid, "High confidence object should be accepted");
        }

        [Test]
        public void CheckObject_AcceptsObjectAtExactThreshold()
        {
            // Arrange
            var obj = TestUtilities.CreateMockVisualObject("ThresholdObj", 0.6f);

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsTrue(result.isValid, "Object at exact threshold should be accepted");
        }

        [Test]
        public void CheckObject_RejectsObjectJustBelowThreshold()
        {
            // Arrange
            var obj = TestUtilities.CreateMockVisualObject("BelowThreshold", 0.59f);

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsFalse(result.isValid, "Object just below threshold should be rejected");
        }

        #endregion

        #region Position Validity Tests

        [Test]
        public void CheckObject_RejectsObjectTooHigh()
        {
            // Arrange
            var obj = HallucinationTestData.CreateHallucination_ImpossibleObject();
            obj.estimatedPosition = new Vector3(0, 100, 0); // Way too high

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsFalse(result.isValid, "Object too high should be rejected");
            Assert.IsTrue(result.issues.Exists(i => i.Contains("too high")),
                "Should report position too high");
        }

        [Test]
        public void CheckObject_RejectsObjectTooLow()
        {
            // Arrange
            var obj = TestUtilities.CreateMockVisualObject("UndergroundObj", 0.8f);
            obj.estimatedPosition = new Vector3(0, -20, 0); // Below ground

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsFalse(result.isValid, "Object too low should be rejected");
            Assert.IsTrue(result.issues.Exists(i => i.Contains("too low")),
                "Should report position too low");
        }

        [Test]
        public void CheckObject_AcceptsObjectAtValidHeight()
        {
            // Arrange
            var obj = TestUtilities.CreateMockVisualObject("ValidHeightObj", 0.8f);
            obj.estimatedPosition = new Vector3(5, 2, 5); // Reasonable height

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsTrue(result.isValid, "Object at valid height should be accepted");
        }

        #endregion

        #region Common Sense Rules Tests

        [Test]
        public void CheckObject_RejectsWallWithSitAffordance()
        {
            // Arrange
            var obj = HallucinationTestData.CreateHallucination_InvalidAffordance();

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsFalse(result.isValid, "Wall with sit affordance should be rejected");
            Assert.IsTrue(result.issues.Exists(i => i.Contains("Common sense violation")),
                "Should report common sense violation");
        }

        [Test]
        public void CheckObject_RejectsBuildingWithPickupAffordance()
        {
            // Arrange
            var obj = new VisualObjectReport
            {
                id = "building_1",
                name = "Office Building",
                type = "building",
                confidence = 0.9f,
                inferredAffordances = new[] { "pickup", "examine" }
            };

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsFalse(result.isValid, "Building with pickup affordance should be rejected");
        }

        [Test]
        public void CheckObject_RejectsChairWithOpenAffordance()
        {
            // Arrange
            var obj = new VisualObjectReport
            {
                id = "chair_1",
                name = "Office Chair",
                type = "chair",
                confidence = 0.85f,
                inferredAffordances = new[] { "sit", "open" } // Open doesn't make sense for chair
            };

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsFalse(result.isValid, "Chair with open affordance should be rejected");
        }

        [Test]
        public void CheckObject_AcceptsChairWithValidAffordances()
        {
            // Arrange
            var obj = new VisualObjectReport
            {
                id = "chair_2",
                name = "Wooden Chair",
                type = "seat",
                confidence = 0.8f,
                inferredAffordances = new[] { "sit", "examine" }
            };

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsTrue(result.isValid, "Chair with valid affordances should be accepted");
        }

        [Test]
        public void CheckObject_AcceptsDoorWithOpenAffordance()
        {
            // Arrange
            var obj = new VisualObjectReport
            {
                id = "door_1",
                name = "Front Door",
                type = "door",
                confidence = 0.9f,
                inferredAffordances = new[] { "open", "close", "examine" }
            };

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsTrue(result.isValid, "Door with open affordance should be accepted");
        }

        #endregion

        #region Filtering Tests

        [Test]
        public void FilterHallucinations_RemovesInvalidObjects()
        {
            // Arrange
            var objects = new List<VisualObjectReport>
            {
                HallucinationTestData.CreateValidObject(),
                HallucinationTestData.CreateHallucination_LowConfidence(),
                HallucinationTestData.CreateValidObject(),
                HallucinationTestData.CreateHallucination_InvalidAffordance()
            };

            // Act
            var filtered = detector.FilterHallucinations(objects);

            // Assert
            Assert.AreEqual(2, filtered.Count, "Should keep only valid objects");
        }

        [Test]
        public void FilterHallucinations_PreservesAllValidObjects()
        {
            // Arrange
            var objects = new List<VisualObjectReport>
            {
                HallucinationTestData.CreateValidObject(),
                HallucinationTestData.CreateValidObject(),
                HallucinationTestData.CreateValidObject()
            };

            // Act
            var filtered = detector.FilterHallucinations(objects);

            // Assert
            Assert.AreEqual(3, filtered.Count, "Should keep all valid objects");
        }

        [Test]
        public void FilterHallucinations_ReturnsEmptyForAllInvalid()
        {
            // Arrange
            var objects = new List<VisualObjectReport>
            {
                HallucinationTestData.CreateHallucination_LowConfidence(),
                HallucinationTestData.CreateHallucination_InvalidAffordance()
            };

            // Act
            var filtered = detector.FilterHallucinations(objects);

            // Assert
            Assert.AreEqual(0, filtered.Count, "Should return empty list when all invalid");
        }

        [Test]
        public void FilterScanResult_FiltersObjectsInResult()
        {
            // Arrange
            var scanResult = new VisualScanResult
            {
                success = true,
                objects = new List<VisualObjectReport>
                {
                    HallucinationTestData.CreateValidObject(),
                    HallucinationTestData.CreateHallucination_LowConfidence()
                }
            };

            // Act
            var filtered = detector.FilterScanResult(scanResult);

            // Assert
            Assert.AreEqual(1, filtered.objects.Count, "Should filter objects in scan result");
        }

        [Test]
        public void FilterScanResult_ReturnsNullForNullInput()
        {
            // Act
            var result = detector.FilterScanResult(null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void FilterScanResult_ReturnsUnchangedForFailedScan()
        {
            // Arrange
            var scanResult = new VisualScanResult
            {
                success = false,
                errorMessage = "Test error"
            };

            // Act
            var result = detector.FilterScanResult(scanResult);

            // Assert
            Assert.IsFalse(result.success);
            Assert.AreEqual("Test error", result.errorMessage);
        }

        #endregion

        #region Statistics Tests

        [Test]
        public void GetStats_TracksCheckedObjects()
        {
            // Arrange
            var valid = HallucinationTestData.CreateValidObject();
            var invalid = HallucinationTestData.CreateHallucination_LowConfidence();

            // Act
            detector.CheckObject(valid);
            detector.CheckObject(invalid);
            detector.CheckObject(valid);
            var stats = detector.GetStats();

            // Assert
            Assert.AreEqual(3, stats.total, "Should track total checked");
            Assert.AreEqual(1, stats.hallucinations, "Should track hallucinations detected");
        }

        [Test]
        public void ResetStats_ClearsStatistics()
        {
            // Arrange
            detector.CheckObject(HallucinationTestData.CreateValidObject());
            detector.CheckObject(HallucinationTestData.CreateHallucination_LowConfidence());

            // Act
            detector.ResetStats();
            var stats = detector.GetStats();

            // Assert
            Assert.AreEqual(0, stats.total);
            Assert.AreEqual(0, stats.hallucinations);
        }

        #endregion

        #region Confidence Adjustment Tests

        [Test]
        public void CheckObject_AdjustsConfidenceForIssues()
        {
            // Arrange
            var obj = new VisualObjectReport
            {
                id = "test_1",
                name = "Test Object",
                type = "seat",
                confidence = 0.8f,
                inferredAffordances = new[] { "sit" },
                estimatedPosition = new Vector3(0, 50, 0) // Too high - will add an issue
            };

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.Less(result.confidenceScore, 0.8f,
                "Confidence should be reduced due to issues");
        }

        [Test]
        public void CheckObject_BoostsConfidenceForMatchedStructured()
        {
            // Arrange
            var obj = new VisualObjectReport
            {
                id = "matched_1",
                name = "Matched Chair",
                type = "seat",
                confidence = 0.7f,
                inferredAffordances = new[] { "sit" },
                matchedStructured = true
            };

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.Greater(result.confidenceScore, 0.7f,
                "Confidence should be boosted for matched structured objects");
        }

        #endregion

        #region Edge Cases

        [Test]
        public void CheckObject_HandlesNullAffordances()
        {
            // Arrange
            var obj = new VisualObjectReport
            {
                id = "null_aff",
                name = "No Affordances",
                type = "unknown",
                confidence = 0.8f,
                inferredAffordances = null
            };

            // Act & Assert (should not throw)
            var result = detector.CheckObject(obj);
            Assert.IsTrue(result.isValid);
        }

        [Test]
        public void CheckObject_HandlesEmptyAffordances()
        {
            // Arrange
            var obj = new VisualObjectReport
            {
                id = "empty_aff",
                name = "Empty Affordances",
                type = "item",
                confidence = 0.75f,
                inferredAffordances = new string[0]
            };

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsTrue(result.isValid);
        }

        [Test]
        public void CheckObject_HandlesNullType()
        {
            // Arrange
            var obj = new VisualObjectReport
            {
                id = "null_type",
                name = "Unknown Type",
                type = null,
                confidence = 0.8f,
                inferredAffordances = new[] { "examine" }
            };

            // Act & Assert (should not throw)
            var result = detector.CheckObject(obj);
            Assert.IsTrue(result.isValid);
        }

        [Test]
        public void CheckObject_HandlesZeroPosition()
        {
            // Arrange - Zero position is valid (often means position unknown)
            var obj = new VisualObjectReport
            {
                id = "zero_pos",
                name = "Zero Position Object",
                type = "item",
                confidence = 0.8f,
                estimatedPosition = Vector3.zero
            };

            // Act
            var result = detector.CheckObject(obj);

            // Assert
            Assert.IsTrue(result.isValid, "Zero position should be treated as unknown, not invalid");
        }

        #endregion
    }
}

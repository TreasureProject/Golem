using System;
using System.Collections.Generic;
using UnityEngine;

namespace Golem.Vision
{
    /// <summary>
    /// Detects potential VLM hallucinations using confidence thresholds,
    /// cross-checks with structured data, and common sense rules.
    /// </summary>
    public class HallucinationDetector : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Vision configuration for threshold settings.")]
        public VisionConfig config;

        [Header("References")]
        [Tooltip("WorldScanner for cross-checking objects.")]
        public WorldScanner worldScanner;

        [Header("Detection Settings")]
        [Tooltip("Confidence threshold - objects below this are rejected.")]
        [Range(0f, 1f)]
        public float confidenceThreshold = 0.6f;

        [Tooltip("Maximum height for valid objects (meters).")]
        public float maxValidHeight = 20f;

        [Tooltip("Minimum height for valid objects (meters).")]
        public float minValidHeight = -5f;

        [Tooltip("Maximum distance from agent for valid objects.")]
        public float maxValidDistance = 50f;

        [Header("Common Sense Rules")]
        [Tooltip("Object types that cannot have 'sit' affordance.")]
        public string[] nonSittableTypes = { "wall", "ceiling", "floor", "sky", "water", "fire", "lava" };

        [Tooltip("Object types that cannot have 'pickup' affordance.")]
        public string[] nonPickupableTypes = { "wall", "building", "vehicle", "tree", "mountain", "door" };

        [Tooltip("Object types that cannot have 'open' affordance.")]
        public string[] nonOpenableTypes = { "chair", "table", "wall", "floor", "rock", "plant" };

        // Statistics
        private int totalChecked;
        private int hallucinationsDetected;

        /// <summary>
        /// Check a visual object for hallucination indicators.
        /// Returns a HallucinationCheckResult with details.
        /// </summary>
        public HallucinationCheckResult CheckObject(VisualObjectReport obj)
        {
            totalChecked++;
            var result = new HallucinationCheckResult
            {
                objectId = obj.id,
                objectName = obj.name,
                isValid = true,
                issues = new List<string>()
            };

            // Check 1: Confidence threshold
            if (obj.confidence < confidenceThreshold)
            {
                result.isValid = false;
                result.issues.Add($"Low confidence: {obj.confidence:F2} < {confidenceThreshold:F2}");
            }

            // Check 2: Position validity
            if (obj.estimatedPosition != Vector3.zero)
            {
                if (obj.estimatedPosition.y > maxValidHeight)
                {
                    result.isValid = false;
                    result.issues.Add($"Position too high: {obj.estimatedPosition.y:F1}m > {maxValidHeight}m");
                }

                if (obj.estimatedPosition.y < minValidHeight)
                {
                    result.isValid = false;
                    result.issues.Add($"Position too low: {obj.estimatedPosition.y:F1}m < {minValidHeight}m");
                }

                float distance = Vector3.Distance(transform.position, obj.estimatedPosition);
                if (distance > maxValidDistance)
                {
                    result.isValid = false;
                    result.issues.Add($"Object too far: {distance:F1}m > {maxValidDistance}m");
                }
            }

            // Check 3: Common sense affordance rules
            if (obj.inferredAffordances != null)
            {
                foreach (var affordance in obj.inferredAffordances)
                {
                    string violation = CheckAffordanceViolation(obj.type, affordance);
                    if (!string.IsNullOrEmpty(violation))
                    {
                        result.isValid = false;
                        result.issues.Add(violation);
                    }
                }
            }

            // Check 4: Cross-check with WorldScanner (if available)
            if (worldScanner != null && obj.matchedStructured == false)
            {
                bool foundInStructured = TryCrossCheckWithScanner(obj);
                if (!foundInStructured && obj.confidence < 0.8f)
                {
                    // Only flag as issue if confidence is not very high
                    result.issues.Add("Not found in WorldScanner (unverified visual-only object)");
                    // Don't mark as invalid - just a warning
                }
            }

            if (!result.isValid)
            {
                hallucinationsDetected++;
            }

            result.confidenceScore = CalculateAdjustedConfidence(obj, result);

            return result;
        }

        /// <summary>
        /// Filter a list of visual objects, removing likely hallucinations.
        /// </summary>
        public List<VisualObjectReport> FilterHallucinations(List<VisualObjectReport> objects)
        {
            var validObjects = new List<VisualObjectReport>();

            foreach (var obj in objects)
            {
                var checkResult = CheckObject(obj);
                if (checkResult.isValid)
                {
                    // Update object confidence with adjusted score
                    obj.confidence = checkResult.confidenceScore;
                    validObjects.Add(obj);
                }
            }

            return validObjects;
        }

        /// <summary>
        /// Check a visual scan result and filter hallucinations.
        /// Returns filtered result.
        /// </summary>
        public VisualScanResult FilterScanResult(VisualScanResult scanResult)
        {
            if (scanResult == null || !scanResult.success)
                return scanResult;

            scanResult.objects = FilterHallucinations(scanResult.objects);
            return scanResult;
        }

        private string CheckAffordanceViolation(string objectType, string affordance)
        {
            if (string.IsNullOrEmpty(objectType) || string.IsNullOrEmpty(affordance))
                return null;

            string typeLower = objectType.ToLower();
            string affordanceLower = affordance.ToLower();

            // Check non-sittable
            if (affordanceLower == "sit")
            {
                foreach (var nonSittable in nonSittableTypes)
                {
                    if (typeLower.Contains(nonSittable.ToLower()))
                    {
                        return $"Common sense violation: '{objectType}' cannot have 'sit' affordance";
                    }
                }
            }

            // Check non-pickupable
            if (affordanceLower == "pickup" || affordanceLower == "pick_up" || affordanceLower == "grab")
            {
                foreach (var nonPickup in nonPickupableTypes)
                {
                    if (typeLower.Contains(nonPickup.ToLower()))
                    {
                        return $"Common sense violation: '{objectType}' cannot have 'pickup' affordance";
                    }
                }
            }

            // Check non-openable
            if (affordanceLower == "open")
            {
                foreach (var nonOpen in nonOpenableTypes)
                {
                    if (typeLower.Contains(nonOpen.ToLower()))
                    {
                        return $"Common sense violation: '{objectType}' cannot have 'open' affordance";
                    }
                }
            }

            return null;
        }

        private bool TryCrossCheckWithScanner(VisualObjectReport visualObj)
        {
            if (worldScanner == null || worldScanner.nearbyObjects == null)
                return false;

            // Try to find a matching object in WorldScanner results
            foreach (var structuredObj in worldScanner.nearbyObjects)
            {
                // Match by name similarity
                if (IsNameSimilar(visualObj.name, structuredObj.displayName))
                {
                    visualObj.matchedStructured = true;
                    visualObj.matchedObjectId = structuredObj.UniqueId;
                    return true;
                }

                // Match by type and proximity (if we have position data)
                if (visualObj.estimatedPosition != Vector3.zero &&
                    visualObj.type == structuredObj.objectType)
                {
                    float distance = Vector3.Distance(visualObj.estimatedPosition,
                                                       structuredObj.InteractionPosition);
                    if (distance < 2f) // Within 2 meters
                    {
                        visualObj.matchedStructured = true;
                        visualObj.matchedObjectId = structuredObj.UniqueId;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsNameSimilar(string name1, string name2)
        {
            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                return false;

            // Simple similarity check - contains or case-insensitive match
            string n1 = name1.ToLower().Replace(" ", "").Replace("_", "");
            string n2 = name2.ToLower().Replace(" ", "").Replace("_", "");

            return n1.Contains(n2) || n2.Contains(n1) || n1 == n2;
        }

        private float CalculateAdjustedConfidence(VisualObjectReport obj, HallucinationCheckResult checkResult)
        {
            float confidence = obj.confidence;

            // Reduce confidence for each issue found
            confidence -= checkResult.issues.Count * 0.1f;

            // Boost confidence if matched with structured data
            if (obj.matchedStructured)
            {
                confidence += 0.15f;
            }

            // Clamp to valid range
            return Mathf.Clamp01(confidence);
        }

        /// <summary>
        /// Get detection statistics.
        /// </summary>
        public (int total, int hallucinations, float rate) GetStats()
        {
            float rate = totalChecked > 0 ? (float)hallucinationsDetected / totalChecked : 0f;
            return (totalChecked, hallucinationsDetected, rate);
        }

        /// <summary>
        /// Reset statistics.
        /// </summary>
        public void ResetStats()
        {
            totalChecked = 0;
            hallucinationsDetected = 0;
        }

        private void Awake()
        {
            if (config != null)
            {
                confidenceThreshold = config.minVisualConfidence;
            }

            if (worldScanner == null)
            {
                worldScanner = GetComponentInParent<WorldScanner>();
            }
        }
    }

    /// <summary>
    /// Result of checking an object for hallucination.
    /// </summary>
    [Serializable]
    public class HallucinationCheckResult
    {
        public string objectId;
        public string objectName;
        public bool isValid;
        public List<string> issues;
        public float confidenceScore;

        public bool HasIssues => issues != null && issues.Count > 0;

        public override string ToString()
        {
            if (isValid)
                return $"[VALID] {objectName} (confidence: {confidenceScore:F2})";
            else
                return $"[HALLUCINATION] {objectName}: {string.Join("; ", issues)}";
        }
    }
}

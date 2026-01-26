using System;
using System.Collections.Generic;
using UnityEngine;

namespace Golem.Vision
{
    /// <summary>
    /// Fuses structured perception (WorldScanner) with visual perception (VLM).
    /// Creates a unified view of the world combining both data sources.
    /// Structured perception remains primary; visual perception supplements and enhances.
    /// </summary>
    public class PerceptionFuser : MonoBehaviour
    {
        [Header("Configuration")]
        public VisionConfig config;

        [Header("References")]
        [Tooltip("Reference to VisualPerceptionManager.")]
        public VisualPerceptionManager visualPerception;

        [Header("Fusion Settings")]
        [Tooltip("Maximum distance for matching objects between sources.")]
        public float matchingDistanceThreshold = 2f;

        [Tooltip("Confidence boost when object found in both sources.")]
        [Range(0f, 0.3f)]
        public float crossValidationBoost = 0.15f;

        [Tooltip("Minimum confidence for visual-only objects.")]
        [Range(0f, 1f)]
        public float visualOnlyMinConfidence = 0.7f;

        private FusedPerceptionResult lastFusedResult;
        private float lastFusionTime;

        public FusedPerceptionResult LastFusedResult => lastFusedResult;

        public event Action<FusedPerceptionResult> OnFusionComplete;

        /// <summary>
        /// Fuse structured and visual perception data.
        /// </summary>
        public FusedPerceptionResult Fuse(
            List<StructuredObjectData> structuredObjects,
            VisualScanResult visualResult)
        {
            var result = new FusedPerceptionResult
            {
                fusionTime = Time.time,
                hasStructuredData = structuredObjects != null && structuredObjects.Count > 0,
                hasVisualData = visualResult != null && visualResult.success
            };

            // Start with structured objects as base
            var fusedObjects = new Dictionary<string, FusedObjectData>();

            if (structuredObjects != null)
            {
                foreach (var structured in structuredObjects)
                {
                    var fused = CreateFromStructured(structured);
                    fusedObjects[fused.id] = fused;
                }
            }

            // Merge visual objects
            if (visualResult != null && visualResult.success && visualResult.objects != null)
            {
                foreach (var visual in visualResult.objects)
                {
                    // Try to match with existing structured object
                    var matched = TryMatchStructured(visual, fusedObjects.Values);

                    if (matched != null)
                    {
                        // Enhance existing object with visual data
                        EnhanceWithVisual(matched, visual);
                    }
                    else if (visual.confidence >= visualOnlyMinConfidence)
                    {
                        // Add as visual-only object
                        var fused = CreateFromVisual(visual);
                        fusedObjects[fused.id] = fused;
                    }
                }

                result.sceneDescription = visualResult.sceneDescription;
                result.suggestedActions = visualResult.suggestedActions;
            }

            result.objects = new List<FusedObjectData>(fusedObjects.Values);
            result.structuredCount = CountBySource(result.objects, PerceptionSource.Structured);
            result.visualOnlyCount = CountBySource(result.objects, PerceptionSource.VisualOnly);
            result.crossValidatedCount = CountBySource(result.objects, PerceptionSource.CrossValidated);

            lastFusedResult = result;
            lastFusionTime = Time.time;

            OnFusionComplete?.Invoke(result);
            return result;
        }

        /// <summary>
        /// Quick fusion using last visual result.
        /// </summary>
        public FusedPerceptionResult FuseWithLastVisual(List<StructuredObjectData> structuredObjects)
        {
            var visualResult = visualPerception?.LastScanResult;
            return Fuse(structuredObjects, visualResult);
        }

        private FusedObjectData CreateFromStructured(StructuredObjectData structured)
        {
            return new FusedObjectData
            {
                id = structured.uniqueId,
                name = structured.displayName,
                type = structured.objectType,
                position = structured.position,
                affordances = structured.affordances?.ToArray() ?? new string[0],
                confidence = 1f, // Structured data is authoritative
                source = PerceptionSource.Structured,
                structuredData = structured,
                isInteractable = structured.isInteractable,
                currentState = structured.currentState
            };
        }

        private FusedObjectData CreateFromVisual(VisualObjectReport visual)
        {
            return new FusedObjectData
            {
                id = $"visual_{visual.id}",
                name = visual.name,
                type = visual.type,
                description = visual.description,
                position = visual.estimatedPosition,
                affordances = visual.inferredAffordances ?? new string[0],
                confidence = visual.confidence,
                source = PerceptionSource.VisualOnly,
                visualData = visual,
                relativePosition = visual.relativePosition,
                visualState = visual.state
            };
        }

        private FusedObjectData TryMatchStructured(
            VisualObjectReport visual,
            IEnumerable<FusedObjectData> existingObjects)
        {
            foreach (var existing in existingObjects)
            {
                if (existing.source != PerceptionSource.Structured)
                    continue;

                // Match by name similarity
                if (IsNameSimilar(visual.name, existing.name))
                    return existing;

                // Match by type and proximity
                if (visual.estimatedPosition != Vector3.zero &&
                    existing.position != Vector3.zero)
                {
                    float distance = Vector3.Distance(visual.estimatedPosition, existing.position);
                    if (distance <= matchingDistanceThreshold &&
                        IsTypeSimilar(visual.type, existing.type))
                    {
                        return existing;
                    }
                }
            }

            return null;
        }

        private void EnhanceWithVisual(FusedObjectData fused, VisualObjectReport visual)
        {
            // Update source to cross-validated
            fused.source = PerceptionSource.CrossValidated;
            fused.visualData = visual;

            // Boost confidence
            fused.confidence = Mathf.Min(1f, fused.confidence + crossValidationBoost);

            // Add visual description if missing
            if (string.IsNullOrEmpty(fused.description))
                fused.description = visual.description;

            // Merge affordances
            fused.affordances = MergeAffordances(fused.affordances, visual.inferredAffordances);

            // Add visual state info
            fused.visualState = visual.state;
            fused.relativePosition = visual.relativePosition;
        }

        private bool IsNameSimilar(string name1, string name2)
        {
            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                return false;

            string n1 = NormalizeName(name1);
            string n2 = NormalizeName(name2);

            return n1.Contains(n2) || n2.Contains(n1) || n1 == n2;
        }

        private bool IsTypeSimilar(string type1, string type2)
        {
            if (string.IsNullOrEmpty(type1) || string.IsNullOrEmpty(type2))
                return false;

            string t1 = type1.ToLower();
            string t2 = type2.ToLower();

            // Direct match
            if (t1 == t2)
                return true;

            // Common type mappings
            var typeGroups = new Dictionary<string, string[]>
            {
                { "seat", new[] { "chair", "bench", "stool", "sofa", "couch" } },
                { "door", new[] { "gate", "entrance", "exit" } },
                { "container", new[] { "box", "chest", "drawer", "cabinet" } },
                { "display", new[] { "screen", "monitor", "terminal" } }
            };

            foreach (var group in typeGroups)
            {
                bool t1InGroup = t1 == group.Key || Array.Exists(group.Value, v => t1.Contains(v));
                bool t2InGroup = t2 == group.Key || Array.Exists(group.Value, v => t2.Contains(v));

                if (t1InGroup && t2InGroup)
                    return true;
            }

            return false;
        }

        private string NormalizeName(string name)
        {
            return name.ToLower()
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "");
        }

        private string[] MergeAffordances(string[] existing, string[] additional)
        {
            if (additional == null || additional.Length == 0)
                return existing;

            var merged = new HashSet<string>(existing ?? new string[0]);

            foreach (var aff in additional)
            {
                if (!string.IsNullOrEmpty(aff))
                    merged.Add(aff.ToLower());
            }

            var result = new string[merged.Count];
            merged.CopyTo(result);
            return result;
        }

        private int CountBySource(List<FusedObjectData> objects, PerceptionSource source)
        {
            int count = 0;
            foreach (var obj in objects)
            {
                if (obj.source == source)
                    count++;
            }
            return count;
        }
    }

    /// <summary>
    /// Source of perception data.
    /// </summary>
    public enum PerceptionSource
    {
        Structured,      // From WorldScanner only
        VisualOnly,      // From VLM only
        CrossValidated   // Found in both sources
    }

    /// <summary>
    /// Structured object data from WorldScanner.
    /// </summary>
    [Serializable]
    public class StructuredObjectData
    {
        public string uniqueId;
        public string displayName;
        public string objectType;
        public Vector3 position;
        public Vector3 interactionPosition;
        public List<string> affordances;
        public bool isInteractable;
        public string currentState;
    }

    /// <summary>
    /// Fused object combining structured and visual data.
    /// </summary>
    [Serializable]
    public class FusedObjectData
    {
        public string id;
        public string name;
        public string type;
        public string description;
        public Vector3 position;
        public string[] affordances;
        public float confidence;
        public PerceptionSource source;

        // Structured data
        public StructuredObjectData structuredData;
        public bool isInteractable;
        public string currentState;

        // Visual data
        public VisualObjectReport visualData;
        public string relativePosition;
        public string visualState;

        public bool HasStructuredData => structuredData != null;
        public bool HasVisualData => visualData != null;
    }

    /// <summary>
    /// Result of perception fusion.
    /// </summary>
    [Serializable]
    public class FusedPerceptionResult
    {
        public float fusionTime;
        public bool hasStructuredData;
        public bool hasVisualData;
        public List<FusedObjectData> objects = new List<FusedObjectData>();
        public string sceneDescription;
        public List<string> suggestedActions;

        // Statistics
        public int structuredCount;
        public int visualOnlyCount;
        public int crossValidatedCount;

        public int TotalCount => objects?.Count ?? 0;

        public float CrossValidationRate =>
            TotalCount > 0 ? (float)crossValidatedCount / TotalCount : 0f;
    }
}

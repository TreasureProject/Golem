using System;
using UnityEngine;

namespace Golem
{
    /// <summary>
    /// Defines a character's personality traits that affect all behavior systems.
    /// Personality is portable - characters carry this profile across different worlds.
    ///
    /// All traits are 0-1 floats:
    /// - 0.0 = minimum expression of trait
    /// - 0.5 = average/balanced
    /// - 1.0 = maximum expression of trait
    /// </summary>
    [CreateAssetMenu(fileName = "Personality", menuName = "Golem/Personality Profile")]
    public class PersonalityProfile : ScriptableObject
    {
        [Header("Core Traits")]

        [Tooltip("0 = homebody (prefers familiar areas), 1 = explorer (seeks novelty)")]
        [Range(0f, 1f)]
        public float curiosity = 0.5f;

        [Tooltip("0 = forgetful (fast memory decay), 1 = perfect memory (slow decay)")]
        [Range(0f, 1f)]
        public float memoryRetention = 0.5f;

        [Tooltip("0 = loner (avoids interaction), 1 = social butterfly (seeks interaction)")]
        [Range(0f, 1f)]
        public float sociability = 0.5f;

        [Tooltip("0 = reckless (ignores danger), 1 = very cautious (careful approach)")]
        [Range(0f, 1f)]
        public float caution = 0.5f;

        [Tooltip("0 = spontaneous (varied behavior), 1 = creature of habit (repeats patterns)")]
        [Range(0f, 1f)]
        public float routinePreference = 0.5f;

        [Tooltip("0 = rigid (slow to learn), 1 = flexible (adapts quickly)")]
        [Range(0f, 1f)]
        public float adaptability = 0.5f;

        [Header("Derived Values (Read-Only in Inspector)")]
        [SerializeField, HideInInspector]
        private float _explorationChance;
        [SerializeField, HideInInspector]
        private float _memoryHalfLifeDays;
        [SerializeField, HideInInspector]
        private int _maxMemoryObjects;

        /// <summary>
        /// How likely the character is to explore vs stay in known areas.
        /// Derived from curiosity and caution.
        /// </summary>
        public float ExplorationChance => curiosity * (1f - caution * 0.5f);

        /// <summary>
        /// How many days until memories decay to half strength.
        /// Derived from memoryRetention (1-30 days).
        /// </summary>
        public float MemoryHalfLifeDays => 1f + 29f * memoryRetention;

        /// <summary>
        /// Maximum number of objects to keep in memory.
        /// Derived from memoryRetention (100-10000).
        /// </summary>
        public int MaxMemoryObjects => Mathf.RoundToInt(100f + 9900f * memoryRetention);

        /// <summary>
        /// Threshold below which memory edges are pruned.
        /// Derived from memoryRetention (0.05-0.30).
        /// </summary>
        public float MemoryPruneThreshold => 0.3f - 0.25f * memoryRetention;

        /// <summary>
        /// Random exploration chance (epsilon for exploration vs exploitation).
        /// Derived from curiosity (0.05-0.20).
        /// </summary>
        public float RandomExplorationEpsilon => 0.05f + curiosity * 0.15f;

        /// <summary>
        /// How much to weight routine/familiar behaviors.
        /// </summary>
        public float RoutineWeight => routinePreference;

        /// <summary>
        /// Learning rate multiplier based on adaptability.
        /// </summary>
        public float LearningRateMultiplier => 0.5f + adaptability * 1f;

        /// <summary>
        /// Approach distance multiplier based on caution.
        /// Cautious characters keep more distance.
        /// </summary>
        public float ApproachDistanceMultiplier => 1f + caution * 0.5f;

        /// <summary>
        /// Creates a clone of this personality profile.
        /// </summary>
        public PersonalityProfile Clone()
        {
            var clone = CreateInstance<PersonalityProfile>();
            clone.curiosity = curiosity;
            clone.memoryRetention = memoryRetention;
            clone.sociability = sociability;
            clone.caution = caution;
            clone.routinePreference = routinePreference;
            clone.adaptability = adaptability;
            return clone;
        }

        /// <summary>
        /// Adjusts a trait based on experience. Use for personality evolution.
        /// </summary>
        /// <param name="traitName">Name of the trait (curiosity, memoryRetention, etc.)</param>
        /// <param name="delta">Amount to adjust (-1 to 1, will be clamped)</param>
        public void AdjustTrait(string traitName, float delta)
        {
            switch (traitName.ToLowerInvariant())
            {
                case "curiosity":
                    curiosity = Mathf.Clamp01(curiosity + delta);
                    break;
                case "memoryretention":
                case "memory":
                    memoryRetention = Mathf.Clamp01(memoryRetention + delta);
                    break;
                case "sociability":
                case "social":
                    sociability = Mathf.Clamp01(sociability + delta);
                    break;
                case "caution":
                    caution = Mathf.Clamp01(caution + delta);
                    break;
                case "routinepreference":
                case "routine":
                    routinePreference = Mathf.Clamp01(routinePreference + delta);
                    break;
                case "adaptability":
                    adaptability = Mathf.Clamp01(adaptability + delta);
                    break;
                default:
                    Debug.LogWarning($"PersonalityProfile: Unknown trait '{traitName}'");
                    break;
            }
        }

        /// <summary>
        /// Serializes the personality to JSON for persistence.
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(new PersonalityData
            {
                curiosity = curiosity,
                memoryRetention = memoryRetention,
                sociability = sociability,
                caution = caution,
                routinePreference = routinePreference,
                adaptability = adaptability
            });
        }

        /// <summary>
        /// Loads personality from JSON.
        /// </summary>
        public void FromJson(string json)
        {
            var data = JsonUtility.FromJson<PersonalityData>(json);
            curiosity = data.curiosity;
            memoryRetention = data.memoryRetention;
            sociability = data.sociability;
            caution = data.caution;
            routinePreference = data.routinePreference;
            adaptability = data.adaptability;
        }

        private void OnValidate()
        {
            // Update derived values for inspector visibility
            _explorationChance = ExplorationChance;
            _memoryHalfLifeDays = MemoryHalfLifeDays;
            _maxMemoryObjects = MaxMemoryObjects;
        }

        [Serializable]
        private struct PersonalityData
        {
            public float curiosity;
            public float memoryRetention;
            public float sociability;
            public float caution;
            public float routinePreference;
            public float adaptability;
        }
    }
}

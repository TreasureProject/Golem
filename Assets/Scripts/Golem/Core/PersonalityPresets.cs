using UnityEngine;

namespace Golem
{
    /// <summary>
    /// Factory for creating common personality archetypes.
    /// Use these as starting points and customize as needed.
    /// </summary>
    public static class PersonalityPresets
    {
        /// <summary>
        /// Adventurous explorer who seeks out new places and experiences.
        /// High curiosity, low caution, good memory for discovered locations.
        /// </summary>
        public static PersonalityProfile CuriousExplorer()
        {
            var profile = ScriptableObject.CreateInstance<PersonalityProfile>();
            profile.curiosity = 0.9f;
            profile.memoryRetention = 0.7f;
            profile.sociability = 0.5f;
            profile.caution = 0.2f;
            profile.routinePreference = 0.2f;
            profile.adaptability = 0.8f;
            profile.name = "CuriousExplorer";
            return profile;
        }

        /// <summary>
        /// Prefers familiar surroundings and routines. Careful and observant.
        /// Low curiosity, high caution, strong routine preference.
        /// </summary>
        public static PersonalityProfile CautiousHomebody()
        {
            var profile = ScriptableObject.CreateInstance<PersonalityProfile>();
            profile.curiosity = 0.2f;
            profile.memoryRetention = 0.8f;
            profile.sociability = 0.4f;
            profile.caution = 0.9f;
            profile.routinePreference = 0.85f;
            profile.adaptability = 0.3f;
            profile.name = "CautiousHomebody";
            return profile;
        }

        /// <summary>
        /// Loves meeting new people and engaging in social activities.
        /// High sociability, moderate curiosity, very adaptable.
        /// </summary>
        public static PersonalityProfile SocialButterfly()
        {
            var profile = ScriptableObject.CreateInstance<PersonalityProfile>();
            profile.curiosity = 0.6f;
            profile.memoryRetention = 0.6f;
            profile.sociability = 0.95f;
            profile.caution = 0.4f;
            profile.routinePreference = 0.3f;
            profile.adaptability = 0.85f;
            profile.name = "SocialButterfly";
            return profile;
        }

        /// <summary>
        /// Devoted companion who stays close and remembers everything.
        /// High sociability and memory, moderate routine preference.
        /// </summary>
        public static PersonalityProfile LoyalCompanion()
        {
            var profile = ScriptableObject.CreateInstance<PersonalityProfile>();
            profile.curiosity = 0.5f;
            profile.memoryRetention = 0.9f;
            profile.sociability = 0.85f;
            profile.caution = 0.5f;
            profile.routinePreference = 0.7f;
            profile.adaptability = 0.6f;
            profile.name = "LoyalCompanion";
            return profile;
        }

        /// <summary>
        /// Unpredictable and spontaneous. Thrives on chaos and novelty.
        /// High curiosity, very low caution and routine preference.
        /// </summary>
        public static PersonalityProfile WildCard()
        {
            var profile = ScriptableObject.CreateInstance<PersonalityProfile>();
            profile.curiosity = 0.85f;
            profile.memoryRetention = 0.4f;
            profile.sociability = 0.6f;
            profile.caution = 0.1f;
            profile.routinePreference = 0.1f;
            profile.adaptability = 0.9f;
            profile.name = "WildCard";
            return profile;
        }

        /// <summary>
        /// Quiet observer who watches and remembers. Selective social interaction.
        /// High memory, low sociability, moderate caution.
        /// </summary>
        public static PersonalityProfile SilentObserver()
        {
            var profile = ScriptableObject.CreateInstance<PersonalityProfile>();
            profile.curiosity = 0.7f;
            profile.memoryRetention = 0.95f;
            profile.sociability = 0.2f;
            profile.caution = 0.6f;
            profile.routinePreference = 0.5f;
            profile.adaptability = 0.5f;
            profile.name = "SilentObserver";
            return profile;
        }

        /// <summary>
        /// Balanced personality with no extreme traits.
        /// Good starting point for customization.
        /// </summary>
        public static PersonalityProfile Balanced()
        {
            var profile = ScriptableObject.CreateInstance<PersonalityProfile>();
            profile.curiosity = 0.5f;
            profile.memoryRetention = 0.5f;
            profile.sociability = 0.5f;
            profile.caution = 0.5f;
            profile.routinePreference = 0.5f;
            profile.adaptability = 0.5f;
            profile.name = "Balanced";
            return profile;
        }

        /// <summary>
        /// Creates a personality from a preset name.
        /// </summary>
        /// <param name="presetName">Name of the preset (case-insensitive)</param>
        /// <returns>PersonalityProfile or Balanced if not found</returns>
        public static PersonalityProfile FromName(string presetName)
        {
            return presetName?.ToLowerInvariant() switch
            {
                "curiousexplorer" or "curious" or "explorer" => CuriousExplorer(),
                "cautioushomebody" or "cautious" or "homebody" => CautiousHomebody(),
                "socialbutterfly" or "social" or "butterfly" => SocialButterfly(),
                "loyalcompanion" or "loyal" or "companion" => LoyalCompanion(),
                "wildcard" or "wild" or "chaotic" => WildCard(),
                "silentobserver" or "silent" or "observer" => SilentObserver(),
                "balanced" or "default" or "neutral" => Balanced(),
                _ => Balanced()
            };
        }

        /// <summary>
        /// Creates a random personality with traits in reasonable ranges.
        /// </summary>
        public static PersonalityProfile Random()
        {
            var profile = ScriptableObject.CreateInstance<PersonalityProfile>();
            profile.curiosity = UnityEngine.Random.Range(0.2f, 0.9f);
            profile.memoryRetention = UnityEngine.Random.Range(0.3f, 0.9f);
            profile.sociability = UnityEngine.Random.Range(0.2f, 0.9f);
            profile.caution = UnityEngine.Random.Range(0.1f, 0.8f);
            profile.routinePreference = UnityEngine.Random.Range(0.2f, 0.8f);
            profile.adaptability = UnityEngine.Random.Range(0.3f, 0.9f);
            profile.name = "Random";
            return profile;
        }
    }
}

using UnityEngine;

namespace Golem.Vision
{
    /// <summary>
    /// ScriptableObject containing customizable VLM prompt templates.
    /// </summary>
    [CreateAssetMenu(fileName = "VLMPromptTemplates", menuName = "Golem/VLM Prompt Templates")]
    public class VLMPromptTemplates : ScriptableObject
    {
        [Header("Scene Understanding")]
        [TextArea(5, 15)]
        public string sceneUnderstandingPrompt = @"Analyze this game scene from an AI agent's perspective.
Identify all interactive objects and their properties.

Return JSON:
{
  ""objects"": [
    {
      ""name"": ""string"",
      ""type"": ""seat|door|arcade|display|container|item"",
      ""description"": ""brief description"",
      ""affordances"": [""sit"", ""examine"", ...],
      ""position"": { ""relative"": ""center|left|right|near|far"" },
      ""state"": ""open|closed|occupied|available"",
      ""confidence"": 0.0-1.0
    }
  ],
  ""scene_description"": ""brief overall description"",
  ""suggested_actions"": [""action 1"", ""action 2""]
}";

        [Header("Action Verification")]
        [TextArea(5, 15)]
        public string actionVerificationPrompt = @"Previous action: {action}
Target: {target}
Expected outcome: {expected}

Compare the before and after images. Did the action succeed?
Return JSON:
{
  ""success"": true|false,
  ""confidence"": 0.0-1.0,
  ""observed_change"": ""description of what changed"",
  ""failure_reason"": ""if failed, why?""
}";

        [Header("Affordance Discovery")]
        [TextArea(5, 15)]
        public string affordanceDiscoveryPrompt = @"Examine this object in the scene.
Identify what actions an AI agent could perform with it.

Focus on:
- Graspable vs ungraspable
- Openable containers
- Sittable surfaces
- Interactive displays
- NPCs vs static objects

Return JSON:
{
  ""object_name"": ""string"",
  ""object_type"": ""string"",
  ""affordances"": [""action1"", ""action2""],
  ""confidence"": 0.0-1.0,
  ""notes"": ""additional observations""
}";

        /// <summary>
        /// Get the prompt for a specific request type.
        /// </summary>
        public string GetPrompt(VLMRequestType requestType)
        {
            switch (requestType)
            {
                case VLMRequestType.SceneUnderstanding:
                    return sceneUnderstandingPrompt;
                case VLMRequestType.ActionVerification:
                    return actionVerificationPrompt;
                case VLMRequestType.AffordanceDiscovery:
                    return affordanceDiscoveryPrompt;
                default:
                    return sceneUnderstandingPrompt;
            }
        }

        /// <summary>
        /// Replace variables in a prompt template.
        /// </summary>
        public static string ReplaceVariables(string template, params (string key, string value)[] variables)
        {
            string result = template;
            foreach (var (key, value) in variables)
            {
                result = result.Replace($"{{{key}}}", value);
            }
            return result;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Golem.Vision
{
    /// <summary>
    /// Client for communicating with Vision-Language Model APIs.
    /// Supports multiple providers: OpenAI, Anthropic, Ollama.
    /// </summary>
    public class VLMClient : MonoBehaviour
    {
        [Header("Configuration")]
        public VisionConfig config;

        [Header("Prompt Templates")]
        public VLMPromptTemplates promptTemplates;

        private VisualPerceptionStats stats = new VisualPerceptionStats();
        private Queue<VLMRequest> pendingRequests = new Queue<VLMRequest>();
        private bool isProcessing;

        public VisualPerceptionStats Stats => stats;
        public int PendingRequestCount => pendingRequests.Count;
        public bool IsProcessing => isProcessing;

        public event Action<VLMResponse> OnResponseReceived;
        public event Action<string> OnError;

        /// <summary>
        /// Check if the client can accept new requests.
        /// </summary>
        public bool CanAcceptRequests()
        {
            if (config == null || !config.enabled)
                return false;

            // Ollama doesn't require an API key
            if (config.provider == VLMProvider.Ollama)
                return true;

            // Other providers require an API key
            if (string.IsNullOrEmpty(config.apiKey))
                return false;

            // Check budget
            stats.ResetHourlyIfNeeded(Time.time);
            if (config.pauseOnBudgetExceeded && stats.currentHourCost >= config.maxCostPerHour)
                return false;

            return true;
        }

        /// <summary>
        /// Send a scene understanding request.
        /// </summary>
        public void RequestSceneUnderstanding(string imageBase64, Action<VLMResponse> callback)
        {
            var request = new VLMRequest
            {
                requestType = VLMRequestType.SceneUnderstanding,
                imageBase64 = imageBase64,
                prompt = promptTemplates != null
                    ? promptTemplates.GetPrompt(VLMRequestType.SceneUnderstanding)
                    : GetDefaultPrompt(VLMRequestType.SceneUnderstanding)
            };

            StartCoroutine(SendRequestCoroutine(request, callback));
        }

        /// <summary>
        /// Send an action verification request.
        /// </summary>
        public void RequestActionVerification(
            string beforeImageBase64,
            string afterImageBase64,
            string actionName,
            string targetId,
            string expectedOutcome,
            Action<VLMResponse> callback)
        {
            string prompt = promptTemplates != null
                ? promptTemplates.GetPrompt(VLMRequestType.ActionVerification)
                : GetDefaultPrompt(VLMRequestType.ActionVerification);

            prompt = VLMPromptTemplates.ReplaceVariables(prompt,
                ("action", actionName),
                ("target", targetId),
                ("expected", expectedOutcome));

            var request = new VLMRequest
            {
                requestType = VLMRequestType.ActionVerification,
                imageBase64 = afterImageBase64, // Primary image is "after"
                prompt = prompt
            };
            request.promptVariables["before_image"] = beforeImageBase64;

            StartCoroutine(SendRequestCoroutine(request, callback));
        }

        /// <summary>
        /// Send a request and get a response via coroutine.
        /// </summary>
        private IEnumerator SendRequestCoroutine(VLMRequest request, Action<VLMResponse> callback)
        {
            stats.totalRequests++;
            isProcessing = true;
            float startTime = Time.time;

            VLMResponse response = new VLMResponse
            {
                requestId = request.requestId
            };

            string url = GetEndpointUrl();
            string jsonBody = BuildRequestBody(request);

            if (config.logVLMResponses)
            {
                Debug.Log($"[VLMClient] Sending request to {url}");
            }

            using (var webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                SetRequestHeaders(webRequest);
                webRequest.timeout = Mathf.RoundToInt(config.requestTimeout);

                yield return webRequest.SendWebRequest();

                response.processingTime = Time.time - startTime;

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string responseText = webRequest.downloadHandler.text;

                    if (config.logVLMResponses)
                    {
                        Debug.Log($"[VLMClient] Response: {responseText}");
                    }

                    try
                    {
                        ParseResponse(responseText, request.requestType, response);
                        response.success = true;
                        stats.successfulRequests++;

                        // Estimate cost
                        response.estimatedCost = EstimateCost(response.tokensUsed);
                        stats.currentHourCost += response.estimatedCost;
                        stats.totalCost += response.estimatedCost;
                    }
                    catch (Exception e)
                    {
                        response.success = false;
                        response.errorMessage = $"Parse error: {e.Message}";
                        stats.failedRequests++;
                        OnError?.Invoke(response.errorMessage);
                    }
                }
                else
                {
                    response.success = false;
                    response.errorMessage = $"Request failed: {webRequest.error}";
                    stats.failedRequests++;

                    Debug.LogWarning($"[VLMClient] {response.errorMessage}");
                    OnError?.Invoke(response.errorMessage);
                }
            }

            isProcessing = false;
            response.rawContent = response.success ? "parsed" : response.errorMessage;

            // Update average request time
            stats.averageRequestTime = (stats.averageRequestTime * (stats.totalRequests - 1) + response.processingTime) / stats.totalRequests;

            callback?.Invoke(response);
            OnResponseReceived?.Invoke(response);
        }

        private string GetEndpointUrl()
        {
            string baseUrl = config.GetBaseUrl();

            switch (config.provider)
            {
                case VLMProvider.OpenAI:
                    return $"{baseUrl}/chat/completions";
                case VLMProvider.Anthropic:
                    return $"{baseUrl}/messages";
                case VLMProvider.Ollama:
                    return $"{baseUrl}/chat";
                default:
                    return $"{baseUrl}/chat/completions";
            }
        }

        private void SetRequestHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Content-Type", "application/json");

            switch (config.provider)
            {
                case VLMProvider.OpenAI:
                    request.SetRequestHeader("Authorization", $"Bearer {config.apiKey}");
                    break;
                case VLMProvider.Anthropic:
                    request.SetRequestHeader("x-api-key", config.apiKey);
                    request.SetRequestHeader("anthropic-version", "2023-06-01");
                    break;
                case VLMProvider.Ollama:
                    // Ollama typically doesn't require auth headers
                    break;
            }
        }

        private string BuildRequestBody(VLMRequest request)
        {
            switch (config.provider)
            {
                case VLMProvider.OpenAI:
                    return BuildOpenAIRequest(request);
                case VLMProvider.Anthropic:
                    return BuildAnthropicRequest(request);
                case VLMProvider.Ollama:
                    return BuildOllamaRequest(request);
                default:
                    return BuildOpenAIRequest(request);
            }
        }

        private string BuildOpenAIRequest(VLMRequest request)
        {
            // Build content array with text and image
            var contentParts = new List<string>();

            contentParts.Add($@"{{""type"": ""text"", ""text"": {EscapeJson(request.prompt)}}}");

            if (!string.IsNullOrEmpty(request.imageBase64))
            {
                contentParts.Add($@"{{""type"": ""image_url"", ""image_url"": {{""url"": ""data:image/jpeg;base64,{request.imageBase64}""}}}}");
            }

            // For action verification, include before image if present
            if (request.promptVariables.TryGetValue("before_image", out string beforeImage))
            {
                contentParts.Add($@"{{""type"": ""image_url"", ""image_url"": {{""url"": ""data:image/jpeg;base64,{beforeImage}""}}}}");
            }

            string content = string.Join(",", contentParts);

            return $@"{{
                ""model"": ""{config.modelName}"",
                ""messages"": [
                    {{
                        ""role"": ""user"",
                        ""content"": [{content}]
                    }}
                ],
                ""max_tokens"": 1024
            }}";
        }

        private string BuildAnthropicRequest(VLMRequest request)
        {
            var contentParts = new List<string>();

            if (!string.IsNullOrEmpty(request.imageBase64))
            {
                contentParts.Add($@"{{""type"": ""image"", ""source"": {{""type"": ""base64"", ""media_type"": ""image/jpeg"", ""data"": ""{request.imageBase64}""}}}}");
            }

            if (request.promptVariables.TryGetValue("before_image", out string beforeImage))
            {
                contentParts.Add($@"{{""type"": ""image"", ""source"": {{""type"": ""base64"", ""media_type"": ""image/jpeg"", ""data"": ""{beforeImage}""}}}}");
            }

            contentParts.Add($@"{{""type"": ""text"", ""text"": {EscapeJson(request.prompt)}}}");

            string content = string.Join(",", contentParts);

            return $@"{{
                ""model"": ""{config.modelName}"",
                ""max_tokens"": 1024,
                ""messages"": [
                    {{
                        ""role"": ""user"",
                        ""content"": [{content}]
                    }}
                ]
            }}";
        }

        private string BuildOllamaRequest(VLMRequest request)
        {
            var images = new List<string>();

            if (!string.IsNullOrEmpty(request.imageBase64))
            {
                images.Add($@"""{request.imageBase64}""");
            }

            if (request.promptVariables.TryGetValue("before_image", out string beforeImage))
            {
                images.Add($@"""{beforeImage}""");
            }

            string imagesArray = images.Count > 0 ? $@", ""images"": [{string.Join(",", images)}]" : "";

            return $@"{{
                ""model"": ""{config.modelName}"",
                ""messages"": [
                    {{
                        ""role"": ""user"",
                        ""content"": {EscapeJson(request.prompt)}{imagesArray}
                    }}
                ],
                ""stream"": false
            }}";
        }

        private void ParseResponse(string responseText, VLMRequestType requestType, VLMResponse response)
        {
            string content = ExtractContentFromResponse(responseText);
            response.rawContent = content;

            // Extract JSON from the content (VLM might include markdown code blocks)
            string jsonContent = ExtractJsonFromContent(content);

            switch (requestType)
            {
                case VLMRequestType.SceneUnderstanding:
                    response.sceneResult = ParseSceneUnderstanding(jsonContent);
                    break;
                case VLMRequestType.ActionVerification:
                    response.verificationResult = ParseActionVerification(jsonContent);
                    break;
                case VLMRequestType.AffordanceDiscovery:
                    response.sceneResult = ParseSceneUnderstanding(jsonContent);
                    break;
            }

            // Extract token usage if available
            response.tokensUsed = ExtractTokenUsage(responseText);
        }

        private string ExtractContentFromResponse(string responseText)
        {
            switch (config.provider)
            {
                case VLMProvider.OpenAI:
                    return ExtractOpenAIContent(responseText);
                case VLMProvider.Anthropic:
                    return ExtractAnthropicContent(responseText);
                case VLMProvider.Ollama:
                    return ExtractOllamaContent(responseText);
                default:
                    return responseText;
            }
        }

        private string ExtractOpenAIContent(string json)
        {
            // Simple extraction: find "content": "..." in the response
            int contentStart = json.IndexOf("\"content\":");
            if (contentStart == -1) return json;

            contentStart = json.IndexOf("\"", contentStart + 10) + 1;
            int contentEnd = FindMatchingQuote(json, contentStart);

            if (contentEnd > contentStart)
            {
                return UnescapeJson(json.Substring(contentStart, contentEnd - contentStart));
            }

            return json;
        }

        private string ExtractAnthropicContent(string json)
        {
            // Anthropic: find "text": "..." in content array
            int textStart = json.IndexOf("\"text\":");
            if (textStart == -1) return json;

            textStart = json.IndexOf("\"", textStart + 7) + 1;
            int textEnd = FindMatchingQuote(json, textStart);

            if (textEnd > textStart)
            {
                return UnescapeJson(json.Substring(textStart, textEnd - textStart));
            }

            return json;
        }

        private string ExtractOllamaContent(string json)
        {
            // Ollama: find "content": "..." in message
            return ExtractOpenAIContent(json); // Same format
        }

        private int FindMatchingQuote(string text, int start)
        {
            for (int i = start; i < text.Length; i++)
            {
                if (text[i] == '"' && (i == 0 || text[i - 1] != '\\'))
                {
                    return i;
                }
            }
            return text.Length;
        }

        private string ExtractJsonFromContent(string content)
        {
            // Remove markdown code blocks if present
            if (content.Contains("```json"))
            {
                int start = content.IndexOf("```json") + 7;
                int end = content.IndexOf("```", start);
                if (end > start)
                {
                    content = content.Substring(start, end - start).Trim();
                }
            }
            else if (content.Contains("```"))
            {
                int start = content.IndexOf("```") + 3;
                int end = content.IndexOf("```", start);
                if (end > start)
                {
                    content = content.Substring(start, end - start).Trim();
                }
            }

            // Find JSON object boundaries
            int jsonStart = content.IndexOf('{');
            int jsonEnd = content.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                return content.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            return content;
        }

        private SceneUnderstandingResult ParseSceneUnderstanding(string json)
        {
            var result = new SceneUnderstandingResult();

            try
            {
                // Parse objects array
                result.objects = ParseObjectsArray(json);

                // Parse scene_description
                result.sceneDescription = ExtractStringValue(json, "scene_description");

                // Parse suggested_actions
                result.suggestedActions = ExtractStringArray(json, "suggested_actions");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[VLMClient] Error parsing scene: {e.Message}");
            }

            return result;
        }

        private ActionVerificationResult ParseActionVerification(string json)
        {
            var result = new ActionVerificationResult();

            try
            {
                result.success = ExtractBoolValue(json, "success");
                result.confidence = ExtractFloatValue(json, "confidence");
                result.observedChange = ExtractStringValue(json, "observed_change");
                result.failureReason = ExtractStringValue(json, "failure_reason");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[VLMClient] Error parsing verification: {e.Message}");
            }

            return result;
        }

        private List<VisualObjectReport> ParseObjectsArray(string json)
        {
            var objects = new List<VisualObjectReport>();

            int objectsStart = json.IndexOf("\"objects\"");
            if (objectsStart == -1) return objects;

            int arrayStart = json.IndexOf('[', objectsStart);
            int arrayEnd = FindMatchingBracket(json, arrayStart);

            if (arrayStart < 0 || arrayEnd < 0) return objects;

            string objectsJson = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

            // Split by object boundaries and parse each
            int depth = 0;
            int objStart = -1;

            for (int i = 0; i < objectsJson.Length; i++)
            {
                char c = objectsJson[i];
                if (c == '{')
                {
                    if (depth == 0) objStart = i;
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0 && objStart >= 0)
                    {
                        string objJson = objectsJson.Substring(objStart, i - objStart + 1);
                        var obj = ParseSingleObject(objJson);
                        if (obj != null) objects.Add(obj);
                        objStart = -1;
                    }
                }
            }

            return objects;
        }

        private VisualObjectReport ParseSingleObject(string json)
        {
            return new VisualObjectReport
            {
                id = Guid.NewGuid().ToString("N").Substring(0, 8),
                name = ExtractStringValue(json, "name"),
                type = ExtractStringValue(json, "type"),
                description = ExtractStringValue(json, "description"),
                state = ExtractStringValue(json, "state"),
                confidence = ExtractFloatValue(json, "confidence"),
                relativePosition = ExtractNestedStringValue(json, "position", "relative"),
                inferredAffordances = ExtractStringArray(json, "affordances").ToArray(),
                observationTime = Time.time
            };
        }

        private string ExtractStringValue(string json, string key)
        {
            string searchKey = $"\"{key}\":";
            int keyIndex = json.IndexOf(searchKey);
            if (keyIndex == -1) return "";

            int valueStart = json.IndexOf("\"", keyIndex + searchKey.Length) + 1;
            int valueEnd = FindMatchingQuote(json, valueStart);

            if (valueEnd > valueStart)
            {
                return UnescapeJson(json.Substring(valueStart, valueEnd - valueStart));
            }

            return "";
        }

        private string ExtractNestedStringValue(string json, string parentKey, string childKey)
        {
            string searchKey = $"\"{parentKey}\":";
            int keyIndex = json.IndexOf(searchKey);
            if (keyIndex == -1) return "";

            int objStart = json.IndexOf("{", keyIndex);
            int objEnd = FindMatchingBracket(json, objStart);

            if (objStart >= 0 && objEnd > objStart)
            {
                string nestedJson = json.Substring(objStart, objEnd - objStart + 1);
                return ExtractStringValue(nestedJson, childKey);
            }

            return "";
        }

        private bool ExtractBoolValue(string json, string key)
        {
            string searchKey = $"\"{key}\":";
            int keyIndex = json.IndexOf(searchKey);
            if (keyIndex == -1) return false;

            int valueStart = keyIndex + searchKey.Length;
            string remaining = json.Substring(valueStart).TrimStart();

            return remaining.StartsWith("true");
        }

        private float ExtractFloatValue(string json, string key)
        {
            string searchKey = $"\"{key}\":";
            int keyIndex = json.IndexOf(searchKey);
            if (keyIndex == -1) return 0f;

            int valueStart = keyIndex + searchKey.Length;
            string remaining = json.Substring(valueStart).TrimStart();

            // Find end of number
            int valueEnd = 0;
            while (valueEnd < remaining.Length && (char.IsDigit(remaining[valueEnd]) || remaining[valueEnd] == '.' || remaining[valueEnd] == '-'))
            {
                valueEnd++;
            }

            if (valueEnd > 0 && float.TryParse(remaining.Substring(0, valueEnd), out float result))
            {
                return result;
            }

            return 0f;
        }

        private List<string> ExtractStringArray(string json, string key)
        {
            var result = new List<string>();

            string searchKey = $"\"{key}\":";
            int keyIndex = json.IndexOf(searchKey);
            if (keyIndex == -1) return result;

            int arrayStart = json.IndexOf('[', keyIndex);
            int arrayEnd = FindMatchingBracket(json, arrayStart);

            if (arrayStart >= 0 && arrayEnd > arrayStart)
            {
                string arrayContent = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

                // Extract quoted strings
                int i = 0;
                while (i < arrayContent.Length)
                {
                    int quoteStart = arrayContent.IndexOf('"', i);
                    if (quoteStart < 0) break;

                    int quoteEnd = FindMatchingQuote(arrayContent, quoteStart + 1);
                    if (quoteEnd > quoteStart + 1)
                    {
                        result.Add(UnescapeJson(arrayContent.Substring(quoteStart + 1, quoteEnd - quoteStart - 1)));
                    }

                    i = quoteEnd + 1;
                }
            }

            return result;
        }

        private int FindMatchingBracket(string text, int start)
        {
            if (start < 0 || start >= text.Length) return -1;

            char openBracket = text[start];
            char closeBracket = openBracket == '{' ? '}' : ']';

            int depth = 1;
            for (int i = start + 1; i < text.Length && depth > 0; i++)
            {
                if (text[i] == openBracket) depth++;
                else if (text[i] == closeBracket) depth--;

                if (depth == 0) return i;
            }

            return -1;
        }

        private int ExtractTokenUsage(string json)
        {
            // Try to find usage.total_tokens or similar
            float tokens = ExtractFloatValue(json, "total_tokens");
            if (tokens > 0) return Mathf.RoundToInt(tokens);

            tokens = ExtractFloatValue(json, "prompt_tokens") + ExtractFloatValue(json, "completion_tokens");
            return Mathf.RoundToInt(tokens);
        }

        private float EstimateCost(int tokens)
        {
            // Rough cost estimation per 1K tokens
            float costPer1K = config.provider switch
            {
                VLMProvider.OpenAI => 0.01f,    // GPT-4V approximate
                VLMProvider.Anthropic => 0.008f, // Claude 3 approximate
                VLMProvider.Ollama => 0f,        // Local, no cost
                _ => 0.01f
            };

            return (tokens / 1000f) * costPer1K;
        }

        private string EscapeJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return "\"\"";

            var sb = new StringBuilder();
            sb.Append('"');

            foreach (char c in text)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(c); break;
                }
            }

            sb.Append('"');
            return sb.ToString();
        }

        private string UnescapeJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            return text
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t");
        }

        private string GetDefaultPrompt(VLMRequestType type)
        {
            switch (type)
            {
                case VLMRequestType.SceneUnderstanding:
                    return "Analyze this game scene. Identify interactive objects with their types, affordances, and positions. Return JSON with objects array, scene_description, and suggested_actions.";
                case VLMRequestType.ActionVerification:
                    return "Compare these images. Did the action succeed? Return JSON with success (bool), confidence (0-1), observed_change, and failure_reason.";
                case VLMRequestType.AffordanceDiscovery:
                    return "What actions can be performed with objects in this scene? Return JSON with objects and their affordances.";
                default:
                    return "Describe what you see in this image.";
            }
        }
    }
}

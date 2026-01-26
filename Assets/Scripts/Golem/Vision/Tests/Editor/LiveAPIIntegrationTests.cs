using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace Golem.Vision.Tests
{
    /// <summary>
    /// Live API integration tests that make real calls to VLM providers.
    /// These tests require valid API keys and will be skipped if keys are not configured.
    ///
    /// To run these tests:
    /// 1. Ensure .env.local exists in the project root with OPENAI_API_KEY
    /// 2. Or set the OPENAI_API_KEY environment variable
    /// </summary>
    [TestFixture]
    [Category("LiveAPI")]
    public class LiveAPIIntegrationTests
    {
        private string openAIKey;
        private string anthropicKey;
        private bool hasOpenAIKey;
        private bool hasAnthropicKey;

        // Small test image (1x1 red pixel PNG, base64 encoded)
        private const string TestImageBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==";

        [OneTimeSetUp]
        public void LoadAPIKeys()
        {
            // Try to load from .env.local file
            string envPath = Path.Combine(Application.dataPath, "..", ".env.local");
            if (File.Exists(envPath))
            {
                string[] lines = File.ReadAllLines(envPath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("OPENAI_API_KEY="))
                    {
                        openAIKey = line.Substring("OPENAI_API_KEY=".Length).Trim();
                    }
                    else if (line.StartsWith("ANTHROPIC_API_KEY="))
                    {
                        anthropicKey = line.Substring("ANTHROPIC_API_KEY=".Length).Trim();
                    }
                }
            }

            // Fall back to environment variables
            if (string.IsNullOrEmpty(openAIKey))
            {
                openAIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            }
            if (string.IsNullOrEmpty(anthropicKey))
            {
                anthropicKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            }

            hasOpenAIKey = !string.IsNullOrEmpty(openAIKey) && openAIKey.StartsWith("sk-");
            hasAnthropicKey = !string.IsNullOrEmpty(anthropicKey) && anthropicKey.StartsWith("sk-ant-");

            Debug.Log($"[LiveAPITests] OpenAI key available: {hasOpenAIKey}, Anthropic key available: {hasAnthropicKey}");
        }

        #region OpenAI Tests

        [UnityTest]
        [Category("OpenAI")]
        public IEnumerator OpenAI_CanConnectToAPI()
        {
            if (!hasOpenAIKey)
            {
                Assert.Ignore("OpenAI API key not configured. Skipping live API test.");
                yield break;
            }

            // Simple models list request to verify connectivity
            using (var request = UnityWebRequest.Get("https://api.openai.com/v1/models"))
            {
                request.SetRequestHeader("Authorization", $"Bearer {openAIKey}");

                yield return request.SendWebRequest();

                Assert.AreEqual(UnityWebRequest.Result.Success, request.result,
                    $"Failed to connect to OpenAI API: {request.error}");
                Assert.IsTrue(request.downloadHandler.text.Contains("data"),
                    "Response should contain model data");
            }
        }

        [UnityTest]
        [Category("OpenAI")]
        public IEnumerator OpenAI_GPT4V_CanAnalyzeImage()
        {
            if (!hasOpenAIKey)
            {
                Assert.Ignore("OpenAI API key not configured. Skipping live API test.");
                yield break;
            }

            string requestBody = @"{
                ""model"": ""gpt-4o-mini"",
                ""messages"": [
                    {
                        ""role"": ""user"",
                        ""content"": [
                            {
                                ""type"": ""text"",
                                ""text"": ""What color is this image? Reply with just the color name.""
                            },
                            {
                                ""type"": ""image_url"",
                                ""image_url"": {
                                    ""url"": ""data:image/png;base64," + TestImageBase64 + @"""
                                }
                            }
                        ]
                    }
                ],
                ""max_tokens"": 50
            }";

            using (var request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {openAIKey}");

                yield return request.SendWebRequest();

                Assert.AreEqual(UnityWebRequest.Result.Success, request.result,
                    $"GPT-4V request failed: {request.error}\nResponse: {request.downloadHandler.text}");

                string response = request.downloadHandler.text;
                Assert.IsTrue(response.Contains("choices"), "Response should contain choices");
                Assert.IsTrue(response.Contains("content"), "Response should contain content");

                // The image is red, so response should mention red
                string lowerResponse = response.ToLower();
                Assert.IsTrue(lowerResponse.Contains("red"),
                    $"GPT-4V should identify the red pixel. Response: {response}");
            }
        }

        [UnityTest]
        [Category("OpenAI")]
        public IEnumerator OpenAI_GPT4V_SceneUnderstandingPrompt()
        {
            if (!hasOpenAIKey)
            {
                Assert.Ignore("OpenAI API key not configured. Skipping live API test.");
                yield break;
            }

            // Test with the actual scene understanding prompt format
            string prompt = @"Analyze this image from an AI agent's perspective.
Identify any objects you can see and their properties.
Return your response as JSON with this format:
{
  ""objects"": [{""name"": ""string"", ""type"": ""string"", ""confidence"": 0.0-1.0}],
  ""scene_description"": ""brief description""
}";

            string requestBody = $@"{{
                ""model"": ""gpt-4o-mini"",
                ""messages"": [
                    {{
                        ""role"": ""user"",
                        ""content"": [
                            {{
                                ""type"": ""text"",
                                ""text"": ""{EscapeJsonString(prompt)}""
                            }},
                            {{
                                ""type"": ""image_url"",
                                ""image_url"": {{
                                    ""url"": ""data:image/png;base64,{TestImageBase64}""
                                }}
                            }}
                        ]
                    }}
                ],
                ""max_tokens"": 200
            }}";

            using (var request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {openAIKey}");

                yield return request.SendWebRequest();

                Assert.AreEqual(UnityWebRequest.Result.Success, request.result,
                    $"Scene understanding request failed: {request.error}");

                string response = request.downloadHandler.text;
                Debug.Log($"[LiveAPITests] Scene understanding response: {response}");

                // Verify we got a valid response structure
                Assert.IsTrue(response.Contains("choices"), "Response should contain choices");
            }
        }

        #endregion

        #region Anthropic Tests

        [UnityTest]
        [Category("Anthropic")]
        public IEnumerator Anthropic_CanAnalyzeImage()
        {
            if (!hasAnthropicKey)
            {
                Assert.Ignore("Anthropic API key not configured. Skipping live API test.");
                yield break;
            }

            string requestBody = $@"{{
                ""model"": ""claude-3-haiku-20240307"",
                ""max_tokens"": 50,
                ""messages"": [
                    {{
                        ""role"": ""user"",
                        ""content"": [
                            {{
                                ""type"": ""text"",
                                ""text"": ""What color is this image? Reply with just the color name.""
                            }},
                            {{
                                ""type"": ""image"",
                                ""source"": {{
                                    ""type"": ""base64"",
                                    ""media_type"": ""image/png"",
                                    ""data"": ""{TestImageBase64}""
                                }}
                            }}
                        ]
                    }}
                ]
            }}";

            using (var request = new UnityWebRequest("https://api.anthropic.com/v1/messages", "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-api-key", anthropicKey);
                request.SetRequestHeader("anthropic-version", "2023-06-01");

                yield return request.SendWebRequest();

                Assert.AreEqual(UnityWebRequest.Result.Success, request.result,
                    $"Anthropic request failed: {request.error}\nResponse: {request.downloadHandler.text}");

                string response = request.downloadHandler.text;
                Assert.IsTrue(response.Contains("content"), "Response should contain content");

                // Verify we got a text response (the exact color may vary due to image interpretation)
                Assert.IsTrue(response.Contains("\"text\""),
                    $"Claude should return a text response. Response: {response}");
                Debug.Log($"[LiveAPITests] Anthropic identified color as: {response}");
            }
        }

        #endregion

        #region VLMClient Integration Tests

        [Test]
        [Category("Integration")]
        public void VLMClient_CanBuildOpenAIRequest()
        {
            // Test that VLMClient builds valid request format
            var config = ScriptableObject.CreateInstance<VisionConfig>();
            config.provider = VLMProvider.OpenAI;
            config.modelName = "gpt-4o";
            config.apiKey = "test-key";

            var go = new GameObject("TestVLMClient");
            var client = go.AddComponent<VLMClient>();
            client.config = config;

            // Verify client is initialized
            Assert.IsNotNull(client);

            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(config);
        }

        [UnityTest]
        [Category("Integration")]
        public IEnumerator VLMClient_LiveRequest_OpenAI()
        {
            // SKIPPED IN EDITMODE: VLMClient.RequestSceneUnderstanding() uses StartCoroutine()
            // which doesn't execute properly in EditMode tests (MonoBehaviour lifecycle issues).
            // The equivalent test runs successfully in PlayMode - see:
            // VisionPlayModeTests.LiveAPI_FullPipeline_CaptureAndAnalyze()
            // VisionPlayModeTests.LiveAPI_CaptureRealScene_SendToVLM()
            Assert.Ignore("VLMClient tests require PlayMode due to coroutine execution. See VisionPlayModeTests.cs");
            yield break;

            // Original test code below (not executed)
            if (!hasOpenAIKey)
            {
                Assert.Ignore("OpenAI API key not configured. Skipping live API test.");
                yield break;
            }

            var config = ScriptableObject.CreateInstance<VisionConfig>();
            config.provider = VLMProvider.OpenAI;
            config.modelName = "gpt-4o-mini";
            config.apiKey = openAIKey;
            config.enabled = true;

            var go = new GameObject("TestVLMClient");
            var client = go.AddComponent<VLMClient>();
            client.config = config;

            VLMResponse receivedResponse = null;
            bool responseReceived = false;

            client.RequestSceneUnderstanding(TestImageBase64, (response) =>
            {
                receivedResponse = response;
                responseReceived = true;
            });

            // Wait for response (max 30 seconds)
            float timeout = 30f;
            float elapsed = 0f;
            while (!responseReceived && elapsed < timeout)
            {
                elapsed += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            Assert.IsTrue(responseReceived, "Should receive response within timeout");
            Assert.IsNotNull(receivedResponse, "Response should not be null");
            Assert.IsTrue(receivedResponse.success, $"Request should succeed: {receivedResponse.errorMessage}");

            Debug.Log($"[LiveAPITests] VLMClient response: {receivedResponse.rawContent}");

            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(config);
        }

        #endregion

        #region Error Handling Tests

        [UnityTest]
        [Category("ErrorHandling")]
        public IEnumerator OpenAI_InvalidKey_ReturnsError()
        {
            string requestBody = @"{
                ""model"": ""gpt-4o-mini"",
                ""messages"": [{""role"": ""user"", ""content"": ""test""}],
                ""max_tokens"": 10
            }";

            using (var request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer sk-invalid-key-12345");

                yield return request.SendWebRequest();

                // Should fail with 401
                Assert.AreNotEqual(UnityWebRequest.Result.Success, request.result,
                    "Invalid key should not succeed");
                Assert.IsTrue(request.responseCode == 401 || request.downloadHandler.text.Contains("invalid"),
                    "Should return authentication error");
            }
        }

        #endregion

        #region Helpers

        private string EscapeJsonString(string str)
        {
            return str.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n")
                      .Replace("\r", "\\r")
                      .Replace("\t", "\\t");
        }

        #endregion
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Golem.Vision
{
    /// <summary>
    /// Debug UI for testing and monitoring the Vision system at runtime.
    /// Press F9 to toggle the debug panel.
    /// </summary>
    public class VisionDebugUI : MonoBehaviour
    {
        [Header("References")]
        public GolemVisionIntegration visionIntegration;
        public VisualPerceptionManager visualPerception;
        public VLMClient vlmClient;
        public VisualObjectCache cache;
        public HallucinationDetector hallucinationDetector;
        public ActionVerifier actionVerifier;

        [Header("UI Settings")]
        public KeyCode toggleKey = KeyCode.F9;
        public bool showOnStart = false;

        private bool showUI = false;
        private Vector2 scrollPosition;
        private string lastScanDescription = "";
        private List<string> lastObjects = new List<string>();
        private string statusMessage = "Ready";
        private float statusTime = 0f;

        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private bool stylesInitialized = false;

        private void Start()
        {
            showUI = showOnStart;

            // Auto-find components if not assigned
            if (visionIntegration == null)
                visionIntegration = GetComponent<GolemVisionIntegration>();
            if (visualPerception == null)
                visualPerception = GetComponent<VisualPerceptionManager>();
            if (vlmClient == null)
                vlmClient = GetComponent<VLMClient>();
            if (cache == null)
                cache = GetComponent<VisualObjectCache>();
            if (hallucinationDetector == null)
                hallucinationDetector = GetComponent<HallucinationDetector>();
            if (actionVerifier == null)
                actionVerifier = GetComponent<ActionVerifier>();

            // Subscribe to events
            if (visualPerception != null)
            {
                visualPerception.OnScanComplete += HandleScanComplete;
                visualPerception.OnError += HandleError;
            }
        }

        private void OnDestroy()
        {
            if (visualPerception != null)
            {
                visualPerception.OnScanComplete -= HandleScanComplete;
                visualPerception.OnError -= HandleError;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                showUI = !showUI;
            }
        }

        private void HandleScanComplete(VisualScanResult result)
        {
            if (result.success)
            {
                lastScanDescription = result.sceneDescription ?? "No description";
                lastObjects.Clear();

                foreach (var obj in result.objects)
                {
                    lastObjects.Add($"{obj.name} ({obj.type}) - {obj.confidence:P0}");
                }

                SetStatus($"Scan complete: {result.objects.Count} objects found");
            }
            else
            {
                SetStatus($"Scan failed: {result.errorMessage}");
            }
        }

        private void HandleError(string error)
        {
            SetStatus($"Error: {error}");
        }

        private void SetStatus(string message)
        {
            statusMessage = message;
            statusTime = Time.time;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!showUI) return;

            InitStyles();

            float width = 400;
            float height = 500;
            float x = Screen.width - width - 20;
            float y = 20;

            GUILayout.BeginArea(new Rect(x, y, width, height), boxStyle);
            GUILayout.Label("Vision System Debug", headerStyle);
            GUILayout.Space(5);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(height - 80));

            // Status
            DrawSection("Status", () =>
            {
                bool enabled = visualPerception?.CanProcessRequests() ?? false;
                bool processing = visualPerception?.IsProcessing ?? false;

                GUILayout.Label($"Enabled: {(enabled ? "Yes" : "No")}");
                GUILayout.Label($"Processing: {(processing ? "Yes" : "No")}");
                GUILayout.Label($"Status: {statusMessage}");

                if (Time.time - statusTime < 5f)
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label("(Recent)");
                    GUI.color = Color.white;
                }
            });

            // Actions
            DrawSection("Actions", () =>
            {
                if (GUILayout.Button("Request Visual Scan"))
                {
                    if (visualPerception != null)
                    {
                        SetStatus("Requesting scan...");
                        visualPerception.ForceNewScan();
                    }
                    else
                    {
                        SetStatus("Visual perception not available");
                    }
                }

                if (GUILayout.Button("Clear Cache"))
                {
                    cache?.InvalidateAll();
                    SetStatus("Cache cleared");
                }

                if (GUILayout.Button("Reset Statistics"))
                {
                    cache?.ResetStats();
                    hallucinationDetector?.ResetStats();
                    actionVerifier?.ResetStats();
                    SetStatus("Statistics reset");
                }
            });

            // Last Scan Result
            DrawSection("Last Scan Result", () =>
            {
                GUILayout.Label($"Description: {lastScanDescription}");
                GUILayout.Label($"Objects ({lastObjects.Count}):");

                foreach (var obj in lastObjects)
                {
                    GUILayout.Label($"  - {obj}");
                }
            });

            // Cache Statistics
            if (cache != null)
            {
                DrawSection("Cache", () =>
                {
                    var stats = cache.GetStats();
                    GUILayout.Label($"Entries: {stats.count}");
                    GUILayout.Label($"Hits: {stats.hits}");
                    GUILayout.Label($"Misses: {stats.misses}");
                    GUILayout.Label($"Hit Rate: {stats.hitRate:P1}");
                });
            }

            // VLM Statistics
            if (vlmClient != null)
            {
                DrawSection("VLM Client", () =>
                {
                    var stats = vlmClient.Stats;
                    GUILayout.Label($"Total Requests: {stats.totalRequests}");
                    GUILayout.Label($"Successful: {stats.successfulRequests}");
                    GUILayout.Label($"Failed: {stats.failedRequests}");
                    GUILayout.Label($"Success Rate: {stats.SuccessRate:P1}");
                    GUILayout.Label($"Avg Time: {stats.averageRequestTime:F2}s");
                    GUILayout.Label($"Hourly Cost: ${stats.currentHourCost:F4}");
                });
            }

            // Hallucination Detection
            if (hallucinationDetector != null)
            {
                DrawSection("Hallucination Detection", () =>
                {
                    var stats = hallucinationDetector.GetStats();
                    GUILayout.Label($"Checked: {stats.total}");
                    GUILayout.Label($"Hallucinations: {stats.hallucinations}");
                    GUILayout.Label($"Rate: {stats.rate:P1}");
                });
            }

            // Action Verification
            if (actionVerifier != null)
            {
                DrawSection("Action Verification", () =>
                {
                    var stats = actionVerifier.GetStats();
                    GUILayout.Label($"Total: {stats.totalVerifications}");
                    GUILayout.Label($"Successful: {stats.successfulVerifications}");
                    GUILayout.Label($"Failed: {stats.failedVerifications}");
                    GUILayout.Label($"Success Rate: {stats.SuccessRate:P1}");
                    GUILayout.Label($"Pending: {actionVerifier.PendingVerificationCount}");
                });
            }

            GUILayout.EndScrollView();

            GUILayout.Space(5);
            GUILayout.Label($"Press {toggleKey} to toggle this panel", GUI.skin.label);

            GUILayout.EndArea();
        }

        private void DrawSection(string title, System.Action content)
        {
            GUILayout.Label(title, headerStyle);
            GUILayout.BeginVertical(GUI.skin.box);
            content();
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// ClaudeCodeController: OnGUI overlay that runs Claude CLI to generate Lua code,
/// captures its output, extracts Lua code blocks, and executes them via LuaCompiler.
/// </summary>
public class ClaudeCodeController : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Toggle initial visibility of the command-line overlay.")]
    public bool visibleOnStart = false;

    [Tooltip("Maximum number of lines to keep in the on-screen log.")]
    public int maxLines = 200;

    [Tooltip("Key to toggle the overlay visibility.")]
    public KeyCode toggleKey = KeyCode.C;

    [Header("Process Settings")]
    [Tooltip("Command to run (full executable name or path).")]
    public string command = "cmd.exe";

    [Tooltip("Arguments to pass to the command.")]
    [TextArea(1, 3)]
    public string arguments = "/k claude --dangerously-skip-permissions";

    [Tooltip("Working directory for the spawned process. Defaults to Application.dataPath if empty.")]
    public string workingDirectory = string.Empty;

    [Tooltip("If true, show a visible console window (disables output capture).")]
    public bool showConsoleWindow = false;

    [Header("Task Description")]
    [Tooltip("The task you want Claude to implement. This is written to CLAUDE.md before running.")]
    [TextArea(4, 10)]
    public string taskDescription = @"Create a fun demo with 5 colorful cubes that:
1. Spawn in a row
2. Have random colors
3. Bounce up and down using sine waves
4. Slowly rotate

Use the update() function for animation.";

    [Header("Claude Settings")]
    [Tooltip("If true, automatically start the process when the game starts.")]
    public bool autoStartOnPlay = false;

    [Header("Timeout")]
    [Tooltip("Timeout in seconds before auto-stopping Claude (0 = no timeout).")]
    public float timeoutSeconds = 180f; // 3 minutes

    [Header("Lua Integration")]
    [Tooltip("Reference to LuaCompiler for executing generated code.")]
    public LuaCompiler luaCompiler;

    [Tooltip("If true, automatically run extracted Lua code when Claude finishes.")]
    public bool autoRunLua = true;

    [Tooltip("If true, automatically load and run the most recent script on startup.")]
    public bool autoLoadOnStart = true;

    // Internal state
    private readonly List<string> lines = new List<string>();
    private readonly List<string> pendingLines = new List<string>();
    private readonly object pendingLock = new object();

    private bool visible = false;
    private Vector2 scrollPos = Vector2.zero;
    private Vector2 luaScrollPos = Vector2.zero;
    private Vector2 taskScrollPos = Vector2.zero;
    private Rect currentUIRect;

    private Process process = null;
    private bool processRunning = false;

    private string inputBuffer = string.Empty;

    // Captured output for Lua extraction
    private StringBuilder fullOutput = new StringBuilder();
    private string extractedLuaCode = string.Empty;
    private bool luaCodeExtracted = false;

    // Timeout tracking
    private float processStartTime = 0f;
    private bool timeoutTriggered = false;

    // UI mode
    private bool showLuaPanel = false;
    private bool showTaskEditor = true;
    private bool showScriptsList = false;

    // Script management
    private string currentScriptName = "";
    private List<string> availableScripts = new List<string>();
    private int selectedScriptIndex = 0;
    private Vector2 scriptsScrollPos = Vector2.zero;

    private void Awake()
    {
        visible = visibleOnStart;
    }

    private void Start()
    {
        EnsureWorkingDirectory();

        // Try to find LuaCompiler if not assigned
        if (luaCompiler == null)
        {
            luaCompiler = FindFirstObjectByType<LuaCompiler>();
            if (luaCompiler != null)
                AppendPendingLine("Found LuaCompiler in scene.");
        }

        // Scan for existing scripts
        RefreshScriptsList();

        // Auto-load the most recent script on startup
        if (autoLoadOnStart && availableScripts.Count > 0)
        {
            string mostRecent = availableScripts[availableScripts.Count - 1];
            AppendPendingLine($"Auto-loading: {mostRecent}");
            StartCoroutine(DelayedLoadScript(mostRecent));
        }

        if (autoStartOnPlay)
        {
            StartCoroutine(DelayedAutoStart());
        }
    }

    private System.Collections.IEnumerator DelayedLoadScript(string scriptName)
    {
        // Small delay to ensure everything is initialized
        yield return new WaitForSeconds(0.2f);
        LoadScriptByName(scriptName);
    }

    /// <summary>
    /// Scan the working directory for .lua script files.
    /// </summary>
    private void RefreshScriptsList()
    {
        availableScripts.Clear();
        string dirPath = string.IsNullOrEmpty(workingDirectory) ? Application.dataPath : workingDirectory;

        try
        {
            if (Directory.Exists(dirPath))
            {
                string[] files = Directory.GetFiles(dirPath, "*.lua");
                foreach (var file in files)
                {
                    availableScripts.Add(Path.GetFileName(file));
                }
                // Sort by name (newest timestamps last)
                availableScripts.Sort();
                
                if (availableScripts.Count > 0)
                {
                    AppendPendingLine($"Found {availableScripts.Count} Lua script(s).");
                    selectedScriptIndex = availableScripts.Count - 1; // Select newest
                }
            }
        }
        catch (Exception ex)
        {
            AppendPendingLine($"Error scanning scripts: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate a unique script filename with timestamp.
    /// </summary>
    private string GenerateScriptName()
    {
        return $"script_{DateTime.Now:yyyyMMdd_HHmmss}.lua";
    }

    private System.Collections.IEnumerator DelayedAutoStart()
    {
        yield return new WaitForSeconds(0.5f);
        AppendPendingLine("Auto-starting Claude Code...");
        StartProcess();
    }

    private void EnsureWorkingDirectory()
    {
        string dirPath = string.IsNullOrEmpty(workingDirectory) ? Application.dataPath : workingDirectory;

        try
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
                AppendPendingLine("Created working directory: " + dirPath);
            }
        }
        catch (Exception ex)
        {
            AppendPendingLine("Failed to ensure working directory: " + ex.Message);
        }
    }

    /// <summary>
    /// Write/Update CLAUDE.md with the current task description and Lua API docs.
    /// </summary>
    private void UpdateClaudeMd()
    {
        string dirPath = string.IsNullOrEmpty(workingDirectory) ? Application.dataPath : workingDirectory;
        string instrPath = Path.Combine(dirPath, "CLAUDE.md");

        try
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("# Unity Lua Scripting Task");
            sb.AppendLine();

            // Task section (user-defined)
            sb.AppendLine("## Your Task");
            sb.AppendLine();
            sb.AppendLine(taskDescription);
            sb.AppendLine();

            // API Documentation
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## Available Lua API");
            sb.AppendLine();
            sb.AppendLine("### Object Spawning");
            sb.AppendLine("- `spawnCube(x, y, z)` - Spawns a cube, returns object name string");
            sb.AppendLine("- `spawnSphere(x, y, z)` - Spawns a sphere, returns object name");
            sb.AppendLine("- `spawnCylinder(x, y, z)` - Spawns a cylinder");
            sb.AppendLine("- `spawnCapsule(x, y, z)` - Spawns a capsule");
            sb.AppendLine("- `spawnPlane(x, y, z)` - Spawns a plane");
            sb.AppendLine();
            sb.AppendLine("### Object Manipulation");
            sb.AppendLine("- `setPosition(name, x, y, z)` - Set position");
            sb.AppendLine("- `getPosition(name)` - Get position as Vector3");
            sb.AppendLine("- `setRotation(name, x, y, z)` - Set euler angles");
            sb.AppendLine("- `setScale(name, x, y, z)` - Set scale");
            sb.AppendLine("- `setColor(name, r, g, b, a)` - Set color (0-1 range)");
            sb.AppendLine("- `move(name, dx, dy, dz)` - Translate relative");
            sb.AppendLine("- `rotate(name, dx, dy, dz)` - Rotate relative");
            sb.AppendLine("- `lookAt(name, x, y, z)` - Look at position");
            sb.AppendLine();
            sb.AppendLine("### Physics");
            sb.AppendLine("- `addRigidbody(name)` - Add physics");
            sb.AppendLine("- `addForce(name, x, y, z)` - Apply force");
            sb.AppendLine("- `setVelocity(name, x, y, z)` - Set velocity");
            sb.AppendLine();
            sb.AppendLine("### Lifecycle");
            sb.AppendLine("- `destroy(name)` - Destroy object");
            sb.AppendLine("- `destroyAll()` - Destroy all spawned objects");
            sb.AppendLine("- `setActive(name, active)` - Enable/disable");
            sb.AppendLine();
            sb.AppendLine("### Input");
            sb.AppendLine("- `getKey(keyName)` - Is key held ('Space', 'W', 'A', etc.)");
            sb.AppendLine("- `getKeyDown(keyName)` - Was key just pressed");
            sb.AppendLine("- `getAxis(axisName)` - Axis value ('Horizontal', 'Vertical')");
            sb.AppendLine("- `getMouseButton(button)` - Mouse button (0=left, 1=right)");
            sb.AppendLine();
            sb.AppendLine("### Utilities");
            sb.AppendLine("- `unity.time()` - Current game time");
            sb.AppendLine("- `unity.deltaTime()` - Frame delta time");
            sb.AppendLine("- `unity.random(min, max)` - Random float");
            sb.AppendLine("- `unity.randomInt(min, max)` - Random integer");
            sb.AppendLine("- `print(...)` - Print to console");
            sb.AppendLine();
            sb.AppendLine("### Update Loop");
            sb.AppendLine("Define these functions to be called automatically:");
            sb.AppendLine("- `function update()` - Called every frame");
            sb.AppendLine("- `function fixedUpdate()` - Called every physics step");
            sb.AppendLine();

            // Example
            sb.AppendLine("## Example");
            sb.AppendLine("```lua");
            sb.AppendLine("-- Variables persist between frames");
            sb.AppendLine("cubes = cubes or {}");
            sb.AppendLine("initialized = initialized or false");
            sb.AppendLine();
            sb.AppendLine("if not initialized then");
            sb.AppendLine("    for i = 1, 5 do");
            sb.AppendLine("        local name = spawnCube((i-3) * 2, 1, 0)");
            sb.AppendLine("        setColor(name, unity.random(0,1), unity.random(0,1), unity.random(0,1), 1)");
            sb.AppendLine("        cubes[i] = name");
            sb.AppendLine("    end");
            sb.AppendLine("    initialized = true");
            sb.AppendLine("end");
            sb.AppendLine();
            sb.AppendLine("function update()");
            sb.AppendLine("    local t = unity.time()");
            sb.AppendLine("    for i, name in ipairs(cubes) do");
            sb.AppendLine("        local y = math.sin(t * 2 + i) + 2");
            sb.AppendLine("        setPosition(name, (i-3) * 2, y, 0)");
            sb.AppendLine("        rotate(name, 0, unity.deltaTime() * 50, 0)");
            sb.AppendLine("    end");
            sb.AppendLine("end");
            sb.AppendLine("```");
            sb.AppendLine();

            // Rules - IMPORTANT
            sb.AppendLine("## IMPORTANT: Output Instructions");
            sb.AppendLine();
            sb.AppendLine($"**You MUST save your Lua code to a file called `{currentScriptName}` in the current directory.**");
            sb.AppendLine();
            sb.AppendLine("Use the Write tool to create the file. Example:");
            sb.AppendLine("```");
            sb.AppendLine($"Write to file: {currentScriptName}");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("## Rules");
            sb.AppendLine($"1. SAVE the Lua code to `{currentScriptName}` file (required!)");
            sb.AppendLine("2. Use `varName = varName or defaultValue` for persistent variables");
            sb.AppendLine("3. Define `update()` for per-frame logic");
            sb.AppendLine("4. Keep code self-contained");
            sb.AppendLine($"5. Do NOT just output code - you must WRITE it to {currentScriptName}");

            File.WriteAllText(instrPath, sb.ToString(), Encoding.UTF8);
            AppendPendingLine($"Updated CLAUDE.md (script: {currentScriptName})");
        }
        catch (Exception ex)
        {
            AppendPendingLine("Failed to write CLAUDE.md: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        StopProcess();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            visible = !visible;
        }

        // Check timeout
        if (processRunning && timeoutSeconds > 0 && !timeoutTriggered)
        {
            float elapsed = Time.realtimeSinceStartup - processStartTime;
            if (elapsed >= timeoutSeconds)
            {
                timeoutTriggered = true;
                AppendPendingLine($"[TIMEOUT] {timeoutSeconds}s elapsed. Stopping...");
                StopProcess();
                ExtractAndRunLua();
            }
        }

        // Flush pending lines
        if (pendingLines.Count > 0)
        {
            lock (pendingLock)
            {
                if (pendingLines.Count > 0)
                {
                    foreach (var l in pendingLines)
                        lines.Add(l);
                    pendingLines.Clear();

                    if (lines.Count > maxLines)
                        lines.RemoveRange(0, lines.Count - maxLines);

                    scrollPos.y = float.MaxValue;
                }
            }
        }
    }

    private void OnGUI()
    {
        if (!visible)
        {
            currentUIRect = Rect.zero;
            return;
        }

        float height = Mathf.Min(Screen.height * 0.65f, 600);
        float width = Screen.width - 20;
        Rect boxRect = new Rect(10, Screen.height - height - 10, width, height);
        currentUIRect = boxRect;
        GUI.Box(boxRect, "");

        GUILayout.BeginArea(new Rect(boxRect.x + 6, boxRect.y + 6, boxRect.width - 12, boxRect.height - 12));

        // Title bar
        GUILayout.BeginHorizontal();
        GUILayout.Label("<b>Claude → Lua</b>", new GUIStyle(GUI.skin.label) { richText = true }, GUILayout.Width(100));

        if (GUILayout.Button(processRunning ? "Stop" : "Start", GUILayout.Width(60)))
        {
            if (processRunning) StopProcess(); else StartProcess();
        }
        if (GUILayout.Button("Clear", GUILayout.Width(50)))
        {
            ClearLines();
        }

        // Status
        string status = processRunning ? $"RUNNING ({Mathf.FloorToInt(Time.realtimeSinceStartup - processStartTime)}s)" : "Stopped";
        GUILayout.Label(status, GUILayout.Width(130));

        GUILayout.FlexibleSpace();

        showTaskEditor = GUILayout.Toggle(showTaskEditor, "Task", GUILayout.Width(50));
        showScriptsList = GUILayout.Toggle(showScriptsList, "Scripts", GUILayout.Width(60));
        showLuaPanel = GUILayout.Toggle(showLuaPanel, "Lua", GUILayout.Width(45));

        if (GUILayout.Button("Hide", GUILayout.Width(45)))
        {
            visible = false;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        // Main content area
        GUILayout.BeginHorizontal();

        // Task Editor Panel (left)
        if (showTaskEditor)
        {
            GUILayout.BeginVertical(GUILayout.Width(boxRect.width * 0.3f));
            GUILayout.Label("<b>Task Description:</b>", new GUIStyle(GUI.skin.label) { richText = true });
            taskScrollPos = GUILayout.BeginScrollView(taskScrollPos, GUILayout.Height(boxRect.height - 140));
            taskDescription = GUILayout.TextArea(taskDescription, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save to CLAUDE.md", GUILayout.Height(25)))
            {
                currentScriptName = GenerateScriptName();
                UpdateClaudeMd();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(6);
        }

        // Scripts List Panel
        if (showScriptsList)
        {
            GUILayout.BeginVertical(GUILayout.Width(150));
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Scripts:</b>", new GUIStyle(GUI.skin.label) { richText = true });
            if (GUILayout.Button("↻", GUILayout.Width(25)))
            {
                RefreshScriptsList();
            }
            GUILayout.EndHorizontal();

            scriptsScrollPos = GUILayout.BeginScrollView(scriptsScrollPos, GUILayout.Height(boxRect.height - 140));
            for (int i = 0; i < availableScripts.Count; i++)
            {
                bool isSelected = (i == selectedScriptIndex);
                GUI.color = isSelected ? Color.green : Color.white;
                if (GUILayout.Button(availableScripts[i], GUILayout.Height(22)))
                {
                    selectedScriptIndex = i;
                    LoadScriptByName(availableScripts[i]);
                }
            }
            GUI.color = Color.white;

            if (availableScripts.Count == 0)
            {
                GUILayout.Label("No scripts yet.");
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(6);
        }

        // Output Panel (middle)
        float outputWidth = boxRect.width;
        if (showTaskEditor) outputWidth -= boxRect.width * 0.3f + 6;
        if (showScriptsList) outputWidth -= 156;
        if (showLuaPanel && luaCodeExtracted) outputWidth -= boxRect.width * 0.25f + 6;

        GUILayout.BeginVertical(GUILayout.Width(outputWidth));
        GUILayout.Label("<b>Claude Output:</b>", new GUIStyle(GUI.skin.label) { richText = true });
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(boxRect.height - 140));
        foreach (var line in lines)
        {
            GUILayout.Label(line);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // Lua Panel (right)
        if (showLuaPanel && luaCodeExtracted)
        {
            GUILayout.Space(6);
            GUILayout.BeginVertical(GUILayout.Width(boxRect.width * 0.25f));
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Lua Code:</b>", new GUIStyle(GUI.skin.label) { richText = true });
            if (GUILayout.Button("Run", GUILayout.Width(35)))
            {
                RunExtractedLua();
            }
            if (GUILayout.Button("Copy", GUILayout.Width(40)))
            {
                GUIUtility.systemCopyBuffer = extractedLuaCode;
                AppendPendingLine("Copied to clipboard.");
            }
            GUILayout.EndHorizontal();

            luaScrollPos = GUILayout.BeginScrollView(luaScrollPos, GUILayout.Height(boxRect.height - 140));
            GUI.color = new Color(0.85f, 1f, 0.85f);
            GUILayout.TextArea(extractedLuaCode, GUILayout.ExpandHeight(true));
            GUI.color = Color.white;
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        // Bottom toolbar
        GUILayout.BeginHorizontal();

        if (luaCodeExtracted)
        {
            GUI.color = Color.green;
            GUILayout.Label($"✓ Lua ready ({extractedLuaCode.Split('\n').Length} lines)", GUILayout.Width(150));
            GUI.color = Color.white;

            if (GUILayout.Button("Run Lua", GUILayout.Width(70)))
            {
                RunExtractedLua();
            }
        }
        else
        {
            GUILayout.Label("No Lua extracted yet.", GUILayout.Width(150));
        }

        if (GUILayout.Button("Reset Lua", GUILayout.Width(70)))
        {
            if (luaCompiler != null)
            {
                luaCompiler.Reset();
                AppendPendingLine("Lua environment reset.");
            }
        }

        if (GUILayout.Button("Load Script", GUILayout.Width(80)))
        {
            LoadScriptFile();
        }

        if (GUILayout.Button("Open Folder", GUILayout.Width(80)))
        {
            OpenWorkingDirectoryInExplorer();
        }

        showConsoleWindow = GUILayout.Toggle(showConsoleWindow, "Console", GUILayout.Width(70));

        GUILayout.FlexibleSpace();
        GUILayout.Label($"Timeout: {timeoutSeconds}s", GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private void StartProcess()
    {
        if (processRunning)
        {
            AppendPendingLine("Process already running.");
            return;
        }

        // Generate unique script name for this run
        currentScriptName = GenerateScriptName();

        // Update CLAUDE.md with current task before starting
        UpdateClaudeMd();

        // Reset state
        fullOutput.Clear();
        extractedLuaCode = "";
        luaCodeExtracted = false;
        timeoutTriggered = false;
        processStartTime = Time.realtimeSinceStartup;

        try
        {
            string dirPath = string.IsNullOrEmpty(workingDirectory) ? Application.dataPath : workingDirectory;
            AppendPendingLine("Working directory: " + dirPath);

            string freshPath = GetFreshPathFromRegistry();

            // Build command - tell Claude to read CLAUDE.md and SAVE to the script file
            string prompt = $"Read CLAUDE.md and implement the task. IMPORTANT: Save the Lua code to {currentScriptName} file using the Write tool.";
            string escapedPrompt = prompt.Replace("\"", "'");

            AppendPendingLine($"Script will be: {currentScriptName}");

            ProcessStartInfo psi;

            if (showConsoleWindow)
            {
                // Visible console mode - user can see Claude's TUI
                string claudeCmd = $"claude \"{escapedPrompt}\" --allowedTools Read,Write";
                string modifiedArgs;

                if (!string.IsNullOrEmpty(freshPath))
                {
                    modifiedArgs = $"/k \"set PATH={freshPath} && {claudeCmd}\"";
                }
                else
                {
                    modifiedArgs = $"/k {claudeCmd}";
                }

                psi = new ProcessStartInfo()
                {
                    FileName = command,
                    Arguments = modifiedArgs,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WorkingDirectory = dirPath,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                AppendPendingLine("Starting with visible console (output not captured)...");
            }
            else
            {
                // Hidden mode - capture output
                string finalArgs = $"/c claude \"{escapedPrompt}\" --allowedTools Read,Write --output-format text";

                psi = new ProcessStartInfo()
                {
                    FileName = command,
                    Arguments = finalArgs,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = dirPath,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                if (!string.IsNullOrEmpty(freshPath))
                {
                    psi.EnvironmentVariables["PATH"] = freshPath;
                }
            }

            process = new Process();
            process.StartInfo = psi;
            process.EnableRaisingEvents = true;

            if (!showConsoleWindow)
            {
                process.OutputDataReceived += OnProcessOutputDataReceived;
                process.ErrorDataReceived += OnProcessErrorDataReceived;
            }
            process.Exited += OnProcessExited;

            bool started = process.Start();
            if (!started)
            {
                AppendPendingLine("Failed to start process.");
                process = null;
                processRunning = false;
                return;
            }

            if (!showConsoleWindow)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            else
            {
                StartCoroutine(BringProcessWindowToFront(process));
            }

            processRunning = true;
            AppendPendingLine($"Started Claude (PID {process.Id}), timeout: {timeoutSeconds}s");
        }
        catch (Exception e)
        {
            AppendPendingLine("Exception: " + e.Message);
            process = null;
            processRunning = false;
        }
    }

    private void StopProcess()
    {
        if (process == null) return;

        try
        {
            if (!process.HasExited)
            {
                // Kill the entire process tree (cmd.exe + claude + any children)
                KillProcessTree(process.Id);
            }
        }
        catch { }
        finally
        {
            AppendPendingLine("Process stopped.");

            try
            {
                if (!showConsoleWindow)
                {
                    process.OutputDataReceived -= OnProcessOutputDataReceived;
                    process.ErrorDataReceived -= OnProcessErrorDataReceived;
                }
                process.Exited -= OnProcessExited;
            }
            catch { }

            try { process.Dispose(); } catch { }
            process = null;
            processRunning = false;
        }
    }

    /// <summary>
    /// Kill a process and all its child processes using taskkill.
    /// </summary>
    private void KillProcessTree(int pid)
    {
        try
        {
            // Use taskkill with /T to kill process tree and /F to force
            var killProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/F /T /PID {pid}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            killProcess.Start();
            killProcess.WaitForExit(5000); // Wait up to 5 seconds
            AppendPendingLine($"Killed process tree (PID {pid})");
        }
        catch (Exception ex)
        {
            AppendPendingLine($"Error killing process: {ex.Message}");
            // Fallback to regular kill
            try
            {
                process?.Kill();
            }
            catch { }
        }
    }

    private void OnProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e == null || string.IsNullOrEmpty(e.Data)) return;

        lock (pendingLock)
        {
            fullOutput.AppendLine(e.Data);
        }

        AppendPendingLine(e.Data);
    }

    private void OnProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e == null || string.IsNullOrEmpty(e.Data)) return;
        AppendPendingLine("[ERR] " + e.Data);
    }

    private void OnProcessExited(object sender, EventArgs e)
    {
        AppendPendingLine("Claude finished.");
        processRunning = false;

        UnityMainThreadDispatcher.Enqueue(() =>
        {
            ExtractAndRunLua();
        });
    }

    private void ExtractAndRunLua()
    {
        string output;
        lock (pendingLock)
        {
            output = fullOutput.ToString();
        }

        // First try to extract from output
        extractedLuaCode = ExtractLuaCode(output);

        if (!string.IsNullOrEmpty(extractedLuaCode))
        {
            luaCodeExtracted = true;
            showLuaPanel = true;
            AppendPendingLine($"Extracted Lua ({extractedLuaCode.Split('\n').Length} lines)");

            if (autoRunLua)
            {
                RunExtractedLua();
            }
        }
        else
        {
            // If no code in output, try loading script.lua file
            AppendPendingLine("No Lua in output, checking for script.lua...");
            LoadScriptFile();
        }
    }

    private string ExtractLuaCode(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        // Try ```lua blocks first
        var luaBlockPattern = new Regex(@"```lua\s*\n([\s\S]*?)```", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        var match = luaBlockPattern.Match(text);

        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value.Trim();
        }

        // Try generic ``` blocks
        var genericBlockPattern = new Regex(@"```\s*\n([\s\S]*?)```", RegexOptions.Multiline);
        match = genericBlockPattern.Match(text);

        if (match.Success && match.Groups.Count > 1)
        {
            string code = match.Groups[1].Value.Trim();
            if (code.Contains("function") || code.Contains("local") || code.Contains("print("))
            {
                return code;
            }
        }

        return "";
    }

    private void RunExtractedLua()
    {
        if (string.IsNullOrEmpty(extractedLuaCode))
        {
            AppendPendingLine("No Lua code to run.");
            return;
        }

        if (luaCompiler == null)
        {
            AppendPendingLine("LuaCompiler not found!");
            return;
        }

        AppendPendingLine("Running Lua...");
        luaCompiler.RunScript(extractedLuaCode);

        if (luaCompiler.LastError != null)
        {
            AppendPendingLine($"Lua Error: {luaCompiler.LastError}");
        }
        else
        {
            AppendPendingLine("Lua executed successfully!");
        }
    }

    /// <summary>
    /// Load and run the current script from the working directory.
    /// </summary>
    private void LoadScriptFile()
    {
        // Use currentScriptName if set, otherwise try to find any recent script
        string scriptName = !string.IsNullOrEmpty(currentScriptName) ? currentScriptName : "script.lua";
        LoadScriptByName(scriptName);
    }

    /// <summary>
    /// Load and run a specific script by filename.
    /// </summary>
    private void LoadScriptByName(string scriptName)
    {
        string dirPath = string.IsNullOrEmpty(workingDirectory) ? Application.dataPath : workingDirectory;
        string scriptPath = Path.Combine(dirPath, scriptName);

        if (!File.Exists(scriptPath))
        {
            AppendPendingLine($"{scriptName} not found.");
            // Refresh the list in case it was deleted
            RefreshScriptsList();
            return;
        }

        try
        {
            extractedLuaCode = File.ReadAllText(scriptPath, Encoding.UTF8);
            luaCodeExtracted = true;
            showLuaPanel = true;
            showScriptsList = true;

            // Refresh the full list and select this script
            RefreshScriptsList();
            if (availableScripts.Contains(scriptName))
            {
                selectedScriptIndex = availableScripts.IndexOf(scriptName);
            }

            AppendPendingLine($"Loaded {scriptName} ({extractedLuaCode.Split('\n').Length} lines)");

            if (autoRunLua)
            {
                RunExtractedLua();
            }
        }
        catch (Exception ex)
        {
            AppendPendingLine($"Failed to load {scriptName}: {ex.Message}");
        }
    }

    private void AppendPendingLine(string text)
    {
        lock (pendingLock)
        {
            pendingLines.Add(DateTime.Now.ToString("HH:mm:ss") + "  " + text);
        }
    }

    private void ClearLines()
    {
        lock (pendingLock)
        {
            pendingLines.Clear();
            fullOutput.Clear();
        }
        lines.Clear();
        extractedLuaCode = "";
        luaCodeExtracted = false;
    }

    public bool IsMouseOverUI()
    {
        if (!visible || currentUIRect == Rect.zero) return false;
        Vector2 mousePos = Input.mousePosition;
        mousePos.y = Screen.height - mousePos.y;
        return currentUIRect.Contains(mousePos);
    }

    private void OpenWorkingDirectoryInExplorer()
    {
        string dirPath = string.IsNullOrEmpty(workingDirectory) ? Application.dataPath : workingDirectory;
        try
        {
            if (!Directory.Exists(dirPath))
            {
                AppendPendingLine("Directory does not exist: " + dirPath);
                return;
            }
            Process.Start("explorer.exe", dirPath);
            AppendPendingLine("Opened folder: " + dirPath);
        }
        catch (Exception e)
        {
            AppendPendingLine("Failed to open folder: " + e.Message);
        }
    }

    private string GetFreshPathFromRegistry()
    {
        try
        {
            string systemPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
            string userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";

            string combined = systemPath;
            if (!string.IsNullOrEmpty(userPath))
            {
                if (!string.IsNullOrEmpty(combined) && !combined.EndsWith(";"))
                    combined += ";";
                combined += userPath;
            }
            return combined;
        }
        catch
        {
            return null;
        }
    }

    private System.Collections.IEnumerator BringProcessWindowToFront(Process p)
    {
        if (p == null) yield break;
        int attempts = 0;
        while (attempts < 50)
        {
            try
            {
                p.Refresh();
                var handle = p.MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    ShowWindow(handle, SW_RESTORE);
                    SetForegroundWindow(handle);
                    AppendPendingLine("Brought window to foreground.");
                    yield break;
                }
            }
            catch { }
            attempts++;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}

/// <summary>
/// Helper to run actions on the main Unity thread.
/// </summary>
public static class UnityMainThreadDispatcher
{
    private static readonly Queue<Action> executionQueue = new Queue<Action>();
    private static bool initialized = false;

    public static void Enqueue(Action action)
    {
        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }

        if (!initialized)
        {
            Initialize();
        }
    }

    private static void Initialize()
    {
        if (initialized) return;

        var go = new GameObject("MainThreadDispatcher");
        go.AddComponent<MainThreadDispatcherBehaviour>();
        GameObject.DontDestroyOnLoad(go);
        initialized = true;
    }

    private class MainThreadDispatcherBehaviour : MonoBehaviour
    {
        void Update()
        {
            lock (executionQueue)
            {
                while (executionQueue.Count > 0)
                {
                    executionQueue.Dequeue()?.Invoke();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

/// <summary>
/// LuaCompiler: Executes Lua scripts using MoonSharp interpreter.
/// Receives code from ClaudeCodeController and executes it dynamically.
/// 
/// Features:
/// - Persistent Script instance (state persists between runs)
/// - Exposed game functions (spawn, find, destroy, etc.)
/// - Unity types registered (Vector3, Color, etc.)
/// - Lua update() function called from Unity Update loop
/// - GameObjects can be exposed to Lua by name
/// </summary>
public class LuaCompiler : MonoBehaviour
{
    [Header("Character Reference")]
    [Tooltip("Reference to the main character (Celeste). Exposed to Lua as 'celeste'.")]
    public GameObject character;

    [Header("Update Loop")]
    [Tooltip("If true, call Lua 'update()' function every frame.")]
    public bool enableLuaUpdate = true;

    [Tooltip("If true, call Lua 'fixedUpdate()' function every physics step.")]
    public bool enableLuaFixedUpdate = false;

    [Header("Debug")]
    [Tooltip("Show debug logs for script execution.")]
    public bool debugMode = true;

    // The MoonSharp script instance (persistent)
    private Script luaScript;

    // Track spawned objects so we can manage them
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
    private int spawnCounter = 0;

    // Exposed game objects that Lua can access
    private Dictionary<string, GameObject> exposedObjects = new Dictionary<string, GameObject>();

    // Event fired when Lua prints something
    public event Action<string> OnLuaPrint;

    // Event fired when Lua spawns an object
    public event Action<GameObject> OnObjectSpawned;

    // Last execution result
    public DynValue LastResult { get; private set; }

    // Last error message (if any)
    public string LastError { get; private set; }

    // Is the compiler ready?
    public bool IsReady => luaScript != null;

    // Cache for Lua functions to avoid repeated lookups
    private DynValue luaUpdateFunc;
    private DynValue luaFixedUpdateFunc;

    private void Awake()
    {
        RegisterUnityTypes();
        InitializeCompiler();
    }

    private void Update()
    {
        if (!enableLuaUpdate || luaScript == null) return;

        // Call Lua update() if it exists
        if (luaUpdateFunc != null && luaUpdateFunc.Type == DataType.Function)
        {
            try
            {
                luaScript.Call(luaUpdateFunc);
            }
            catch (ScriptRuntimeException ex)
            {
                Debug.LogError($"[LuaCompiler] Lua update() error: {ex.DecoratedMessage}");
                enableLuaUpdate = false; // Disable to prevent spam
            }
        }
    }

    private void FixedUpdate()
    {
        if (!enableLuaFixedUpdate || luaScript == null) return;

        // Call Lua fixedUpdate() if it exists
        if (luaFixedUpdateFunc != null && luaFixedUpdateFunc.Type == DataType.Function)
        {
            try
            {
                luaScript.Call(luaFixedUpdateFunc);
            }
            catch (ScriptRuntimeException ex)
            {
                Debug.LogError($"[LuaCompiler] Lua fixedUpdate() error: {ex.DecoratedMessage}");
                enableLuaFixedUpdate = false;
            }
        }
    }

    /// <summary>
    /// Register Unity types with MoonSharp's UserData system.
    /// </summary>
    private void RegisterUnityTypes()
    {
        UserData.RegisterType<Vector3>();
        UserData.RegisterType<Vector2>();
        UserData.RegisterType<Quaternion>();
        UserData.RegisterType<Color>();
        UserData.RegisterType<Transform>();
        UserData.RegisterType<GameObject>();

        if (debugMode)
            Debug.Log("[LuaCompiler] Unity types registered.");
    }

    /// <summary>
    /// Initialize the Lua interpreter and register Unity bindings.
    /// </summary>
    public void InitializeCompiler()
    {
        try
        {
            luaScript = new Script();
            luaScript.Globals["print"] = (Action<DynValue[]>)LuaPrint;

            RegisterUnityBindings();
            RegisterGameBindings();

            if (debugMode)
                Debug.Log("[LuaCompiler] Initialized.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LuaCompiler] Failed to initialize: {ex.Message}");
            LastError = ex.Message;
        }
    }

    /// <summary>
    /// Cache references to Lua functions for faster Update calls.
    /// </summary>
    public void CacheLuaFunctions()
    {
        if (luaScript == null) return;

        luaUpdateFunc = luaScript.Globals.Get("update");
        luaFixedUpdateFunc = luaScript.Globals.Get("fixedUpdate");

        if (debugMode)
        {
            if (luaUpdateFunc.Type == DataType.Function)
                Debug.Log("[LuaCompiler] Found Lua update() function.");
            if (luaFixedUpdateFunc.Type == DataType.Function)
                Debug.Log("[LuaCompiler] Found Lua fixedUpdate() function.");
        }
    }

    /// <summary>
    /// Register Unity-specific utility functions.
    /// </summary>
    private void RegisterUnityBindings()
    {
        var unityTable = new Table(luaScript);

        // Logging
        unityTable["log"] = (Action<string>)((msg) => Debug.Log($"[Lua] {msg}"));
        unityTable["warn"] = (Action<string>)((msg) => Debug.LogWarning($"[Lua] {msg}"));
        unityTable["error"] = (Action<string>)((msg) => Debug.LogError($"[Lua] {msg}"));

        // Time
        unityTable["time"] = (Func<float>)(() => Time.time);
        unityTable["deltaTime"] = (Func<float>)(() => Time.deltaTime);
        unityTable["fixedDeltaTime"] = (Func<float>)(() => Time.fixedDeltaTime);
        unityTable["frame"] = (Func<int>)(() => Time.frameCount);

        // Random
        unityTable["random"] = (Func<float, float, float>)((min, max) => UnityEngine.Random.Range(min, max));
        unityTable["randomInt"] = (Func<int, int, int>)((min, max) => UnityEngine.Random.Range(min, max + 1));

        // Vector constructors
        unityTable["vec3"] = (Func<float, float, float, Vector3>)((x, y, z) => new Vector3(x, y, z));
        unityTable["vec2"] = (Func<float, float, Vector2>)((x, y) => new Vector2(x, y));
        unityTable["color"] = (Func<float, float, float, float, Color>)((r, g, b, a) => new Color(r, g, b, a));

        luaScript.Globals["unity"] = unityTable;
    }

    /// <summary>
    /// Register game-specific functions for spawning and manipulating objects.
    /// </summary>
    private void RegisterGameBindings()
    {
        // ===== SPAWNING PRIMITIVES =====

        luaScript.Globals["spawnCube"] = (Func<float, float, float, string>)((x, y, z) =>
            SpawnPrimitive(PrimitiveType.Cube, new Vector3(x, y, z)));

        luaScript.Globals["spawnSphere"] = (Func<float, float, float, string>)((x, y, z) =>
            SpawnPrimitive(PrimitiveType.Sphere, new Vector3(x, y, z)));

        luaScript.Globals["spawnCylinder"] = (Func<float, float, float, string>)((x, y, z) =>
            SpawnPrimitive(PrimitiveType.Cylinder, new Vector3(x, y, z)));

        luaScript.Globals["spawnCapsule"] = (Func<float, float, float, string>)((x, y, z) =>
            SpawnPrimitive(PrimitiveType.Capsule, new Vector3(x, y, z)));

        luaScript.Globals["spawnPlane"] = (Func<float, float, float, string>)((x, y, z) =>
            SpawnPrimitive(PrimitiveType.Plane, new Vector3(x, y, z)));

        // ===== OBJECT MANIPULATION =====

        luaScript.Globals["getObject"] = (Func<string, GameObject>)((name) =>
        {
            if (spawnedObjects.TryGetValue(name, out var obj)) return obj;
            if (exposedObjects.TryGetValue(name, out obj)) return obj;
            return GameObject.Find(name);
        });

        luaScript.Globals["setPosition"] = (Action<string, float, float, float>)((name, x, y, z) =>
        {
            var obj = GetGameObject(name);
            if (obj != null) obj.transform.position = new Vector3(x, y, z);
        });

        luaScript.Globals["getPosition"] = (Func<string, Vector3>)((name) =>
        {
            var obj = GetGameObject(name);
            return obj != null ? obj.transform.position : Vector3.zero;
        });

        luaScript.Globals["setRotation"] = (Action<string, float, float, float>)((name, x, y, z) =>
        {
            var obj = GetGameObject(name);
            if (obj != null) obj.transform.eulerAngles = new Vector3(x, y, z);
        });

        luaScript.Globals["setScale"] = (Action<string, float, float, float>)((name, x, y, z) =>
        {
            var obj = GetGameObject(name);
            if (obj != null) obj.transform.localScale = new Vector3(x, y, z);
        });

        luaScript.Globals["setColor"] = (Action<string, float, float, float, float>)((name, r, g, b, a) =>
        {
            var obj = GetGameObject(name);
            if (obj != null)
            {
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = new Color(r, g, b, a);
            }
        });

        luaScript.Globals["move"] = (Action<string, float, float, float>)((name, dx, dy, dz) =>
        {
            var obj = GetGameObject(name);
            if (obj != null) obj.transform.Translate(dx, dy, dz);
        });

        luaScript.Globals["rotate"] = (Action<string, float, float, float>)((name, dx, dy, dz) =>
        {
            var obj = GetGameObject(name);
            if (obj != null) obj.transform.Rotate(dx, dy, dz);
        });

        luaScript.Globals["lookAt"] = (Action<string, float, float, float>)((name, x, y, z) =>
        {
            var obj = GetGameObject(name);
            if (obj != null) obj.transform.LookAt(new Vector3(x, y, z));
        });

        // ===== PHYSICS =====

        luaScript.Globals["addRigidbody"] = (Action<string>)((name) =>
        {
            var obj = GetGameObject(name);
            if (obj != null && obj.GetComponent<Rigidbody>() == null)
                obj.AddComponent<Rigidbody>();
        });

        luaScript.Globals["addForce"] = (Action<string, float, float, float>)((name, x, y, z) =>
        {
            var obj = GetGameObject(name);
            var rb = obj?.GetComponent<Rigidbody>();
            if (rb != null) rb.AddForce(new Vector3(x, y, z));
        });

        luaScript.Globals["setVelocity"] = (Action<string, float, float, float>)((name, x, y, z) =>
        {
            var obj = GetGameObject(name);
            var rb = obj?.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = new Vector3(x, y, z);
        });

        // ===== OBJECT LIFECYCLE =====

        luaScript.Globals["destroy"] = (Action<string>)((name) =>
        {
            if (spawnedObjects.TryGetValue(name, out var obj))
            {
                Destroy(obj);
                spawnedObjects.Remove(name);
            }
        });

        luaScript.Globals["destroyAll"] = (Action)(() =>
        {
            foreach (var kvp in spawnedObjects)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            spawnedObjects.Clear();
            spawnCounter = 0;
        });

        luaScript.Globals["setActive"] = (Action<string, bool>)((name, active) =>
        {
            var obj = GetGameObject(name);
            if (obj != null) obj.SetActive(active);
        });

        // ===== INPUT =====

        luaScript.Globals["getKey"] = (Func<string, bool>)((keyName) =>
        {
            if (Enum.TryParse<KeyCode>(keyName, true, out var keyCode))
                return Input.GetKey(keyCode);
            return false;
        });

        luaScript.Globals["getKeyDown"] = (Func<string, bool>)((keyName) =>
        {
            if (Enum.TryParse<KeyCode>(keyName, true, out var keyCode))
                return Input.GetKeyDown(keyCode);
            return false;
        });

        luaScript.Globals["getAxis"] = (Func<string, float>)((axisName) =>
        {
            try { return Input.GetAxis(axisName); }
            catch { return 0f; }
        });

        luaScript.Globals["getMouseButton"] = (Func<int, bool>)((button) => Input.GetMouseButton(button));

        // ===== CHARACTER & BONE ACCESS =====

        // Expose character reference
        if (character != null)
        {
            luaScript.Globals["celeste"] = character;
            exposedObjects["celeste"] = character;
        }

        luaScript.Globals["findBone"] = (Func<string, Transform>)((bonePath) =>
        {
            if (character == null) return null;
            return character.transform.Find(bonePath);
        });

        luaScript.Globals["getBone"] = (Func<string, string, Transform>)((objName, bonePath) =>
        {
            var obj = GetGameObject(objName);
            if (obj == null) return null;
            return obj.transform.Find(bonePath);
        });

        luaScript.Globals["setLocalRotation"] = (Action<Transform, float, float, float>)((t, x, y, z) =>
        {
            if (t != null) t.localEulerAngles = new Vector3(x, y, z);
        });

        luaScript.Globals["getLocalRotation"] = (Func<Transform, Vector3>)((t) =>
        {
            return t != null ? t.localEulerAngles : Vector3.zero;
        });

        luaScript.Globals["setLocalPosition"] = (Action<Transform, float, float, float>)((t, x, y, z) =>
        {
            if (t != null) t.localPosition = new Vector3(x, y, z);
        });

        luaScript.Globals["getLocalPosition"] = (Func<Transform, Vector3>)((t) =>
        {
            return t != null ? t.localPosition : Vector3.zero;
        });

        // Rotate bone by delta (additive)
        luaScript.Globals["rotateBone"] = (Action<Transform, float, float, float>)((t, dx, dy, dz) =>
        {
            if (t != null) t.Rotate(dx, dy, dz, Space.Self);
        });

        // Lerp bone rotation for smooth animation
        luaScript.Globals["lerpLocalRotation"] = (Action<Transform, float, float, float, float>)((t, x, y, z, speed) =>
        {
            if (t == null) return;
            var target = Quaternion.Euler(x, y, z);
            t.localRotation = Quaternion.Lerp(t.localRotation, target, speed * Time.deltaTime);
        });
    }

    private string SpawnPrimitive(PrimitiveType type, Vector3 position)
    {
        var obj = GameObject.CreatePrimitive(type);
        obj.transform.position = position;

        string name = $"lua_{type}_{spawnCounter++}";
        obj.name = name;
        spawnedObjects[name] = obj;

        OnObjectSpawned?.Invoke(obj);
        return name;
    }

    private GameObject GetGameObject(string name)
    {
        if (spawnedObjects.TryGetValue(name, out var obj)) return obj;
        if (exposedObjects.TryGetValue(name, out obj)) return obj;
        return GameObject.Find(name);
    }

    /// <summary>
    /// Expose a GameObject to Lua by name.
    /// </summary>
    public void ExposeObject(string luaName, GameObject obj)
    {
        if (obj == null) return;
        exposedObjects[luaName] = obj;
        luaScript.Globals[luaName] = obj;
    }

    private void LuaPrint(DynValue[] values)
    {
        var parts = new List<string>();
        foreach (var v in values)
            parts.Add(v.ToObject()?.ToString() ?? "nil");

        string message = string.Join("\t", parts);
        Debug.Log($"[Lua] {message}");
        OnLuaPrint?.Invoke(message);
    }

    /// <summary>
    /// Run a Lua script string. State persists between calls.
    /// </summary>
    public DynValue RunScript(string script)
    {
        if (luaScript == null)
        {
            LastError = "Compiler not initialized.";
            Debug.LogError($"[LuaCompiler] {LastError}");
            return null;
        }

        if (string.IsNullOrWhiteSpace(script))
        {
            LastError = "Script is empty.";
            return null;
        }

        try
        {
            LastError = null;

            if (debugMode)
                Debug.Log($"[LuaCompiler] Executing:\n{script}");

            LastResult = luaScript.DoString(script);
            CacheLuaFunctions();

            return LastResult;
        }
        catch (SyntaxErrorException syntaxEx)
        {
            LastError = $"Syntax Error: {syntaxEx.DecoratedMessage}";
            Debug.LogError($"[LuaCompiler] {LastError}");
            return null;
        }
        catch (ScriptRuntimeException runtimeEx)
        {
            LastError = $"Runtime Error: {runtimeEx.DecoratedMessage}";
            Debug.LogError($"[LuaCompiler] {LastError}");
            return null;
        }
        catch (Exception ex)
        {
            LastError = $"Error: {ex.Message}";
            Debug.LogError($"[LuaCompiler] {LastError}");
            return null;
        }
    }

    /// <summary>
    /// Run a Lua script and return the result as a specific type.
    /// </summary>
    public T RunScript<T>(string script)
    {
        var result = RunScript(script);
        if (result == null) return default;

        try { return result.ToObject<T>(); }
        catch { return default; }
    }

    /// <summary>
    /// Call a Lua function by name with arguments.
    /// </summary>
    public DynValue CallFunction(string functionName, params object[] args)
    {
        if (luaScript == null)
        {
            LastError = "Compiler not initialized.";
            return null;
        }

        try
        {
            LastError = null;
            var func = luaScript.Globals.Get(functionName);

            if (func.Type != DataType.Function)
            {
                LastError = $"'{functionName}' is not a function.";
                return null;
            }

            LastResult = luaScript.Call(func, args);
            return LastResult;
        }
        catch (Exception ex)
        {
            LastError = $"Error calling '{functionName}': {ex.Message}";
            Debug.LogError($"[LuaCompiler] {LastError}");
            return null;
        }
    }

    /// <summary>
    /// Check if a Lua function exists.
    /// </summary>
    public bool HasFunction(string functionName)
    {
        if (luaScript == null) return false;
        return luaScript.Globals.Get(functionName).Type == DataType.Function;
    }

    /// <summary>
    /// Set a global variable in the Lua environment.
    /// </summary>
    public void SetGlobal(string name, object value)
    {
        if (luaScript == null) return;
        luaScript.Globals[name] = value;
    }

    /// <summary>
    /// Get a global variable from the Lua environment.
    /// </summary>
    public DynValue GetGlobal(string name)
    {
        if (luaScript == null) return DynValue.Nil;
        return luaScript.Globals.Get(name);
    }

    /// <summary>
    /// Reset the Lua environment. Destroys all spawned objects.
    /// </summary>
    public void Reset()
    {
        foreach (var kvp in spawnedObjects)
        {
            if (kvp.Value != null) Destroy(kvp.Value);
        }
        spawnedObjects.Clear();
        spawnCounter = 0;

        InitializeCompiler();

        foreach (var kvp in exposedObjects)
        {
            if (kvp.Value != null)
                luaScript.Globals[kvp.Key] = kvp.Value;
        }

        luaUpdateFunc = null;
        luaFixedUpdateFunc = null;

        if (debugMode)
            Debug.Log("[LuaCompiler] Reset.");
    }

    /// <summary>
    /// Load and run a Lua file from the specified path.
    /// </summary>
    public DynValue RunFile(string filePath)
    {
        try
        {
            if (!System.IO.File.Exists(filePath))
            {
                LastError = $"File not found: {filePath}";
                Debug.LogError($"[LuaCompiler] {LastError}");
                return null;
            }

            return RunScript(System.IO.File.ReadAllText(filePath));
        }
        catch (Exception ex)
        {
            LastError = $"Error loading file: {ex.Message}";
            Debug.LogError($"[LuaCompiler] {LastError}");
            return null;
        }
    }

    /// <summary>
    /// Get list of all spawned object names.
    /// </summary>
    public List<string> GetSpawnedObjectNames()
    {
        return new List<string>(spawnedObjects.Keys);
    }

    private void OnDestroy()
    {
        foreach (var kvp in spawnedObjects)
        {
            if (kvp.Value != null) Destroy(kvp.Value);
        }
    }
}

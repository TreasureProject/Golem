using UnityEngine;

/// <summary>
/// Stub implementation of LuaCompiler for when MoonSharp is not available.
/// </summary>
public class LuaCompiler : MonoBehaviour
{
    public string LastError { get; private set; } = "";

    public void RunCode(string code)
    {
        Debug.LogWarning("LuaCompiler: MoonSharp not available. Cannot execute Lua code.");
    }

    public bool RunScript(string code)
    {
        Debug.LogWarning("LuaCompiler: MoonSharp not available. Cannot execute Lua code.");
        LastError = "MoonSharp not available";
        return false;
    }

    public void Reset()
    {
        LastError = "";
        Debug.Log("LuaCompiler: Reset (stub).");
    }

    public void StopExecution()
    {
        Debug.Log("LuaCompiler: Stopping (stub).");
    }
}

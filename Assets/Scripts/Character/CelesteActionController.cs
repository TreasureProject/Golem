using System;
using System.Collections.Generic;
using UnityEngine;

// Controller that listens for CFConnector character_action events and drives the PointClickController.
// Handles high-level AI commands and translates them into character movement and interactions.
public class CharacterActionController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign the character root GameObject that has the PointClickController component.")]
    public GameObject characterRoot;

    [Header("Camera")]
    [Tooltip("Optional - will attempt to find a CameraStateMachine component on a Camera object at runtime.")]
    public GameObject cameraRoot;

    [Header("Connector")]
    [Tooltip("Optional: drag CFConnector here to subscribe via UnityEvent/C# events. If left empty, will attempt reflection to find CFConnector at runtime.")]
    public CFConnector connector;

    private object cfConnectorInstance;
    private System.Reflection.EventInfo actionEventInfo;
    private Delegate actionDelegate;

    private void Awake()
    {
        if (characterRoot == null)
        {
            // try to guess main character by tag "Player"
            var p = GameObject.FindWithTag("Player");
            if (p != null) characterRoot = p;
        }

        if (connector == null)
            connector = CFConnector.instance;
    }

    private void Start()
    {
        // If connector provided, subscribe to its UnityEvent / C# event instead of reflection
        if (connector != null)
        {
            try { connector.OnCharacterAction += HandleCelesteActionProxy; } catch { }
            try { connector.OnCharacterActionUnity.AddListener(HandleCelesteActionProxy); } catch { }

            Debug.Log("CharacterActionController: Subscribed to CFConnector via inspector reference.");
            return;
        }

        // Try to find CFConnector by scanning loaded MonoBehaviours
        MonoBehaviour[] all = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var mb in all)
        {
            if (mb == null) continue;
            var t = mb.GetType();
            if (string.Equals(t.Name, "CFConnector", StringComparison.Ordinal))
            {
                cfConnectorInstance = mb;
                actionEventInfo = t.GetEvent("OnCharacterAction");
                break;
            }
        }

        if (cfConnectorInstance != null && actionEventInfo != null)
        {
            // create a handler delegate dynamically that matches the event signature
            var handlerType = actionEventInfo.EventHandlerType; // e.g., Action<CelesteActionData>

            // create a MethodInfo for our proxy method with matching signature via dynamic method
            var invoke = handlerType.GetMethod("Invoke");
            var parameters = invoke.GetParameters();

            // The dynamic method will have an extra first parameter for the target (CharacterActionController)
            // so that we can create a closed delegate bound to `this` successfully.
            var dm = new System.Reflection.Emit.DynamicMethod("CharacterActionProxy", typeof(void), new[] { typeof(CharacterActionController), parameters[0].ParameterType }, typeof(CharacterActionController));
            var il = dm.GetILGenerator();

            // Load the target (first argument) onto the evaluation stack
            il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            // Load the event data argument (second argument)
            il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
            // Call the instance method HandleCelesteActionProxy(object actionData)
            var proxyMethod = typeof(CharacterActionController).GetMethod("HandleCelesteActionProxy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Use Callvirt to call the instance method on the target
            il.Emit(System.Reflection.Emit.OpCodes.Callvirt, proxyMethod);
            il.Emit(System.Reflection.Emit.OpCodes.Ret);

            // Create a delegate bound to 'this' so the first parameter (target) is closed over
            actionDelegate = dm.CreateDelegate(handlerType, this);

            actionEventInfo.AddEventHandler(cfConnectorInstance, actionDelegate);
            Debug.Log("CharacterActionController: Subscribed to CFConnector.OnCharacterAction via reflection.");
        }
        else
        {
            Debug.LogWarning("CharacterActionController: Could not find CFConnector.OnCharacterAction to subscribe to.");
        }
    }

    private void OnDestroy()
    {
        if (connector != null)
        {
            try { connector.OnCharacterAction -= HandleCelesteActionProxy; } catch { }
            try { connector.OnCharacterActionUnity.RemoveListener(HandleCelesteActionProxy); } catch { }
        }

        if (cfConnectorInstance != null && actionEventInfo != null && actionDelegate != null)
        {
            actionEventInfo.RemoveEventHandler(cfConnectorInstance, actionDelegate);
        }
    }

    // This proxy will be called with the concrete CelesteActionData instance (unknown type at compile-time).
    // We accept it as object and use reflection to read its fields.
    private void HandleCelesteActionProxy(object actionData)
    {
        if (actionData == null)
        {
            Debug.LogWarning("CharacterActionController: Received null actionData.");
            return;
        }

        try
        {
            var adType = actionData.GetType();
            var actionField = adType.GetField("action");
            if (actionField == null)
            {
                Debug.LogWarning("CharacterActionController: action field not found on actionData.");
                return;
            }

            var actionObj = actionField.GetValue(actionData);
            if (actionObj == null) return;

            var aType = actionObj.GetType();
            var typeField = aType.GetField("type");
            var paramsField = aType.GetField("parameters");
            string type = typeField?.GetValue(actionObj)?.ToString() ?? string.Empty;
            var parameters = paramsField?.GetValue(actionObj) as System.Collections.IDictionary;

            Debug.Log($"CharacterActionController: Received action '{type}' via CFConnector.");

            HandleActionByName(type, parameters);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"CharacterActionController: Exception handling action: {e}");
        }
    }

    private void HandleActionByName(string type, System.Collections.IDictionary parameters)
    {
        switch (type)
        {
            case "moveToLocation":
                {
                    string location = GetParamString(parameters, "location") ?? "cafe";
                    TriggerMoveToLocation(location);
                }
                break;
            case "sitAtChair":
                {
                    int chair = GetParamInt(parameters, "chairNumber", 1);
                    TriggerSitAtChair(chair);
                }
                break;
            case "standUp":
                TriggerStandUp();
                break;
            case "examineMenu":
                TriggerExamineMenu(GetParamString(parameters, "focus"));
                break;
            case "playArcadeGame":
                TriggerPlayArcade(GetParamString(parameters, "game"));
                break;
            case "changeCameraAngle":
                TriggerChangeCameraAngle(GetParamString(parameters, "angle"), GetParamString(parameters, "transition"));
                break;
            case "idle":
                TriggerIdle(GetParamString(parameters, "idleType"));
                break;
            default:
                Debug.LogWarning($"CharacterActionController: Unknown action type '{type}'");
                break;
        }
    }

    #region Triggers (use SendMessage to avoid compile-time dependencies)
    private void TriggerMoveToLocation(string location)
    {
        Debug.Log($"CharacterActionController: MoveToLocation -> {location}");
        if (characterRoot == null) { Debug.LogWarning("No characterRoot assigned for movement."); return; }

        // try to find a GameObject whose name contains the location
        var target = FindTransformByNameContains(location);
        Vector3 dest;
        if (target != null) dest = target.position;
        else dest = characterRoot.transform.position; // fallback

        // send to PointClickController if present via SendMessage
        characterRoot.SendMessage("MoveToPointPublic", dest, SendMessageOptions.DontRequireReceiver);
    }

    private void TriggerSitAtChair(int chairNumber)
    {
        Debug.Log($"CharacterActionController: SitAtChair -> {chairNumber}");
        if (characterRoot == null) { Debug.LogWarning("No characterRoot assigned for sitAtChair."); return; }

        // find chairs by tag "Caffee Chair"
        GameObject[] chairs = null;
        try { chairs = GameObject.FindGameObjectsWithTag("Caffee Chair"); } catch { }
        if (chairs != null && chairs.Length > 0)
        {
            int idx = Mathf.Clamp(chairNumber - 1, 0, chairs.Length - 1);
            var chosen = chairs[idx];
            var interaction = chosen.transform.Find("InteractionSpot");
            if (interaction != null)
                characterRoot.SendMessage("SitAtInteractionSpot", interaction, SendMessageOptions.DontRequireReceiver);
            else
                characterRoot.SendMessage("SitAtInteractionSpot", chosen.transform, SendMessageOptions.DontRequireReceiver);
            return;
        }

        Debug.LogWarning("CharacterActionController: No chairs found with tag 'Caffee Chair'.");
    }

    private void TriggerStandUp()
    {
        Debug.Log("CharacterActionController: StandUp");
        if (characterRoot == null) return;
        characterRoot.SendMessage("ForceStandUp", SendMessageOptions.DontRequireReceiver);
    }

    private void TriggerExamineMenu(string focus = null)
    {
        Debug.Log($"CharacterActionController: ExamineMenu -> {focus}");
        if (characterRoot == null) return;
        GameObject[] ads = null;
        try { ads = GameObject.FindGameObjectsWithTag("Cafe Ad Display"); } catch { }
        if (ads != null && ads.Length > 0)
        {
            var chosen = ads[0];
            var interaction = chosen.transform.Find("InteractionSpot");
            if (interaction != null)
                characterRoot.SendMessage("ExamineAtInteractionSpot", interaction, SendMessageOptions.DontRequireReceiver);
            else
                characterRoot.SendMessage("ExamineAtInteractionSpot", chosen.transform, SendMessageOptions.DontRequireReceiver);
            return;
        }
        Debug.LogWarning("CharacterActionController: No ad displays found with tag 'Cafe Ad Display'. Falling back to moveToLocation('cafe').");
        // fallback: move to cafe center
        TriggerMoveToLocation("cafe");
    }

    private void TriggerPlayArcade(string game = null)
    {
        Debug.Log($"CharacterActionController: PlayArcadeGame -> {game}");
        if (characterRoot == null) return;
        GameObject[] arcades = null;
        try { arcades = GameObject.FindGameObjectsWithTag("Arcade"); } catch { }
        if (arcades != null && arcades.Length > 0)
        {
            var chosen = arcades[0];
            var interaction = chosen.transform.Find("InteractionSpot");
            if (interaction != null)
                characterRoot.SendMessage("PlayArcadeAtSpot", interaction, SendMessageOptions.DontRequireReceiver);
            else
                characterRoot.SendMessage("PlayArcadeAtSpot", chosen.transform, SendMessageOptions.DontRequireReceiver);
            return;
        }
        Debug.LogWarning("CharacterActionController: No arcades found with tag 'Arcade'.");
    }

    private void TriggerChangeCameraAngle(string angle, string transition)
    {
        Debug.Log($"CharacterActionController: ChangeCameraAngle -> {angle} (transition={transition})");
        if (cameraRoot == null)
            cameraRoot = Camera.main != null ? Camera.main.gameObject : null;

        if (cameraRoot == null) { Debug.LogWarning("No camera root found for changing camera angle."); return; }

        // SendMessage - CameraStateMachine.ChangeState expects a CameraStateSO normally; without that type here we simply attempt to send the angle string
        cameraRoot.SendMessage("ChangeStateByName", angle, SendMessageOptions.DontRequireReceiver);
    }

    private void TriggerIdle(string idleType)
    {
        Debug.Log($"CharacterActionController: Idle -> {idleType}");
        if (string.IsNullOrEmpty(idleType) || idleType == "standing")
            TriggerStandUp();
        else if (idleType == "sitting")
            TriggerSitAtChair(1);
        else if (idleType == "leaning")
        {
            // leaning -> Slot Machine Chair
            GameObject[] chairs = null;
            try { chairs = GameObject.FindGameObjectsWithTag("Slot Machine Chair"); } catch { }
            if (chairs != null && chairs.Length > 0)
            {
                var chosen = chairs[0];
                var interaction = chosen.transform.Find("InteractionSpot");
                if (interaction != null)
                    characterRoot.SendMessage("SitAtInteractionSpot", interaction, SendMessageOptions.DontRequireReceiver);
                else
                    characterRoot.SendMessage("SitAtInteractionSpot", chosen.transform, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    #endregion

    #region Helpers
    private Transform FindTransformByNameContains(string namePart)
    {
        if (string.IsNullOrEmpty(namePart)) return null;
        var all = GameObject.FindObjectsOfType<Transform>();
        namePart = namePart.ToLower();
        foreach (var t in all)
        {
            if (t == null || t.gameObject == null) continue;
            if (t.name != null && t.name.ToLower().Contains(namePart))
                return t;
        }
        return null;
    }

    private string GetParamString(System.Collections.IDictionary parameters, string key)
    {
        if (parameters == null || !parameters.Contains(key)) return null;
        var val = parameters[key];
        if (val == null) return null;
        return val.ToString();
    }

    private int GetParamInt(System.Collections.IDictionary parameters, string key, int defaultValue)
    {
        if (parameters == null || !parameters.Contains(key)) return defaultValue;
        var val = parameters[key];
        if (val == null) return defaultValue;

        if (val is long l) return (int)l;
        if (val is int i) return i;
        if (val is double d) return (int)d;
        if (int.TryParse(val.ToString(), out int parsed)) return parsed;
        return defaultValue;
    }
    #endregion

    // Simple keybinds for testing placed outside update so user can hook into their own input system
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) // move to cafe
            TriggerMoveToLocation("cafe");
        if (Input.GetKeyDown(KeyCode.Alpha2)) // sit at chair 1
            TriggerSitAtChair(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) // stand up
            TriggerStandUp();
        if (Input.GetKeyDown(KeyCode.Alpha4)) // examine menu
            TriggerExamineMenu();
        if (Input.GetKeyDown(KeyCode.Alpha5)) // play arcade
            TriggerPlayArcade();
        if (Input.GetKeyDown(KeyCode.Alpha6)) // change camera angle
            TriggerChangeCameraAngle("cafe_close", "smooth");
        if (Input.GetKeyDown(KeyCode.Alpha7)) // idle (standing)
            TriggerIdle("standing");
        if (Input.GetKeyDown(KeyCode.Space)) // alternate stand up
            TriggerStandUp();
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

// Controller that listens for CFConnector celeste_action events and drives the PointClickController.
// This implementation uses reflection to subscribe to CFConnector at runtime and uses SendMessage
// to control the character GameObject so it doesn't hard-depend on specific types at compile time.
public class CelesteActionController : MonoBehaviour
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

    [Header("Interaction Points")]
    [Tooltip("Chairs the character can sit at. Assign the chair GameObjects (should have InteractionSpot child).")]
    public Transform[] chairs;

    [Tooltip("Arcade machines the character can play. Assign the arcade GameObjects (should have InteractionSpot child).")]
    public Transform[] arcades;

    [Tooltip("Ad displays the character can examine. Assign the display GameObjects (should have InteractionSpot child).")]
    public Transform[] adDisplays;

    [Tooltip("Cafe menus the character can examine. Assign the menu GameObjects (should have InteractionSpot child).")]
    public Transform[] cafeMenus;

    [Tooltip("Waypoints for walking around randomly.")]
    public Transform[] walkPoints;

    [Tooltip("Hallway/starting area location(s).")]
    public Transform[] hallwayPoints;

    [Header("Locations")]
    [Tooltip("Cafe area location points (picks randomly).")]
    public Transform[] cafeLocations;

    [Tooltip("Arcade area location points (picks randomly).")]
    public Transform[] arcadeLocations;

    [Tooltip("Foodcourt area location points (picks randomly).")]
    public Transform[] foodcourtLocations;

    [Header("Look-At Targets")]
    [Tooltip("Object to look at when arriving at cafe locations.")]
    public Transform cafeLookAtTarget;

    [Tooltip("Object to look at when arriving at arcade locations.")]
    public Transform arcadeLookAtTarget;

    [Tooltip("Object to look at when arriving at foodcourt locations.")]
    public Transform foodcourtLookAtTarget;

    [Tooltip("Object to look at when arriving at hallway locations.")]
    public Transform hallwayLookAtTarget;

    [Header("Zone Tracking")]
    [Tooltip("Distance threshold to consider character 'in' a zone (meters).")]
    [SerializeField] private float zoneDetectionRadius = 8f;
    
    [Tooltip("How often to check zone (seconds).")]
    [SerializeField] private float zoneCheckInterval = 1f;

    // Current detected zone - sent to server when it changes
    private string currentZone = "unknown";
    public string CurrentZone => currentZone;
    
    // Current activity - sent to server when it changes
    private string currentActivity = "unknown";
    public string CurrentActivity => currentActivity;
    
    private float lastZoneCheckTime = 0f;

    private object cfConnectorInstance;
    private System.Reflection.EventInfo celesteEventInfo;
    private Delegate celesteDelegate;

    // queued sit interaction when we're waiting for stand up to finish
    private Transform queuedSitInteraction = null;
    // queued move destination when waiting for stand up to finish
    private Vector3 queuedMoveDestination = Vector3.zero;
    // queued arcade interaction when waiting for stand up to finish
    private Transform queuedArcadeInteraction = null;

    private void Awake()
    {
        if (characterRoot == null)
        {
            // try to guess main character by tag "Player"
            var p = GameObject.FindWithTag("Player");
            if (p != null) characterRoot = p;
        }

        if (characterRoot == null)
        {
            // fallback: find an object that has PointClickController
            var pcc = FindObjectOfType<PointClickController>();
            if (pcc != null)
            {
                characterRoot = pcc.gameObject;
                Debug.Log("CelesteActionController: Found characterRoot via PointClickController fallback.");
            }
        }

        if (connector == null)
            connector = CFConnector.instance;
    }

    private void Start()
    {
        // If connector provided, subscribe to its UnityEvent / C# event instead of reflection
        if (connector != null)
        {
            try { connector.OnCelesteAction += HandleCelesteActionProxy; } catch { }
            try { connector.OnCelesteActionUnity.AddListener(HandleCelesteActionProxy); } catch { }

            Debug.Log("CelesteActionController: Subscribed to CFConnector via inspector reference.");
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
                celesteEventInfo = t.GetEvent("OnCelesteAction");
                break;
            }
        }

        if (cfConnectorInstance != null && celesteEventInfo != null)
        {
            // create a handler delegate dynamically that matches the event signature
            var handlerType = celesteEventInfo.EventHandlerType; // e.g., Action<CelesteActionData>

            // create a MethodInfo for our proxy method with matching signature via dynamic method
            var invoke = handlerType.GetMethod("Invoke");
            var parameters = invoke.GetParameters();

            // The dynamic method will have an extra first parameter for the target (CelesteActionController)
            // so that we can create a closed delegate bound to `this` successfully.
            var dm = new System.Reflection.Emit.DynamicMethod("CelesteProxy", typeof(void), new[] { typeof(CelesteActionController), parameters[0].ParameterType }, typeof(CelesteActionController));
            var il = dm.GetILGenerator();

            // Load the target (first argument) onto the evaluation stack
            il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            // Load the event data argument (second argument)
            il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
            // Call the instance method HandleCelesteActionProxy(object actionData)
            var proxyMethod = typeof(CelesteActionController).GetMethod("HandleCelesteActionProxy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Use Callvirt to call the instance method on the target
            il.Emit(System.Reflection.Emit.OpCodes.Callvirt, proxyMethod);
            il.Emit(System.Reflection.Emit.OpCodes.Ret);

            // Create a delegate bound to 'this' so the first parameter (target) is closed over
            celesteDelegate = dm.CreateDelegate(handlerType, this);

            celesteEventInfo.AddEventHandler(cfConnectorInstance, celesteDelegate);
            Debug.Log("CelesteActionController: Subscribed to CFConnector.OnCelesteAction via reflection.");
        }
        else
        {
            Debug.LogWarning("CelesteActionController: Could not find CFConnector.OnCelesteAction to subscribe to.");
        }
    }

    private void OnDestroy()
    {
        if (connector != null)
        {
            try { connector.OnCelesteAction -= HandleCelesteActionProxy; } catch { }
            try { connector.OnCelesteActionUnity.RemoveListener(HandleCelesteActionProxy); } catch { }
        }

        if (cfConnectorInstance != null && celesteEventInfo != null && celesteDelegate != null)
        {
            celesteEventInfo.RemoveEventHandler(cfConnectorInstance, celesteDelegate);
        }

        // cleanup any subscription to PointClickController event
        if (characterRoot != null)
        {
            var pcc = characterRoot.GetComponent<PointClickController>();
            if (pcc != null)
            {
                try { pcc.OnFinishedStanding -= OnPointClickFinishedStanding; } catch { }
            }
        }
    }

    // This proxy will be called with the concrete CelesteActionData instance (unknown type at compile-time).
    // We accept it as object and use reflection to read its fields.
    private void HandleCelesteActionProxy(object actionData)
    {
        if (actionData == null)
        {
            Debug.LogWarning("CelesteActionController: Received null actionData.");
            return;
        }

        try
        {
            var adType = actionData.GetType();
            var actionField = adType.GetField("action");
            if (actionField == null)
            {
                Debug.LogWarning("CelesteActionController: action field not found on actionData.");
                return;
            }

            var actionObj = actionField.GetValue(actionData);
            if (actionObj == null) return;

            var aType = actionObj.GetType();
            var typeField = aType.GetField("type");
            var paramsField = aType.GetField("parameters");
            string type = typeField?.GetValue(actionObj)?.ToString() ?? string.Empty;
            var parameters = paramsField?.GetValue(actionObj) as System.Collections.IDictionary;

            // Log full action details for debugging
            string paramsStr = "none";
            if (parameters != null && parameters.Count > 0)
            {
                var paramList = new List<string>();
                foreach (var key in parameters.Keys)
                {
                    paramList.Add($"{key}={parameters[key]}");
                }
                paramsStr = string.Join(", ", paramList);
            }
            Debug.Log($"CelesteActionController: Received action '{type}' with parameters: {paramsStr}");

            HandleActionByName(type, parameters);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"CelesteActionController: Exception handling action: {e}");
        }
    }

    private void HandleActionByName(string type, System.Collections.IDictionary parameters)
    {
        switch (type)
        {
            case "moveToLocation":
                {
                    string location = GetParamString(parameters, "location");
                    if (!string.IsNullOrEmpty(location))
                        TriggerMoveToLocation(location);
                    else
                        Debug.LogWarning("CelesteActionController: moveToLocation called without location parameter.");
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
            case "goToHallway":
                TriggerGoToHallway(GetParamString(parameters, "area"));
                break;
            case "walkAround":
                TriggerWalkAround(GetParamInt(parameters, "steps", 1));
                break;
            case "idle":
                TriggerIdle(GetParamString(parameters, "idleType"));
                break;
            default:
                Debug.LogWarning($"CelesteActionController: Unknown action type '{type}'");
                break;
        }
    }

    #region Triggers (use SendMessage to avoid compile-time dependencies)
    private void TriggerMoveToLocation(string location)
    {
        Debug.Log($"CelesteActionController: MoveToLocation -> {location} (Current zone: {currentZone})");
        if (characterRoot == null) { Debug.LogWarning("No characterRoot assigned for movement."); return; }

        // Check serialized location fields first and pick randomly from arrays
        Transform target = null;
        string locationLower = location?.ToLower() ?? "";

        if (locationLower.Contains("cafe"))
            target = GetRandomValidTransform(cafeLocations);
        else if (locationLower.Contains("arcade"))
            target = GetRandomValidTransform(arcadeLocations);
        else if (locationLower.Contains("foodcourt") || locationLower.Contains("food"))
            target = GetRandomValidTransform(foodcourtLocations);
        else if (locationLower.Contains("hallway"))
            target = GetRandomValidTransform(hallwayPoints);

        if (target == null)
        {
            Debug.LogWarning($"CelesteActionController: No location found matching '{location}'. Assign locations in inspector.");
            return;
        }

        Vector3 dest = target.position;

        // Determine what to look at after arriving based on location type
        Transform lookAtTarget = null;
        if (locationLower.Contains("cafe"))
        {
            // Use cafeLookAtTarget if assigned, otherwise fallback to closest cafe menu
            if (cafeLookAtTarget != null)
            {
                lookAtTarget = cafeLookAtTarget;
            }
            else
            {
                // Fallback to closest cafe menu
                lookAtTarget = GetClosestTransform(dest, cafeMenus);
            }
        }
        else if (locationLower.Contains("arcade"))
        {
            // Use arcadeLookAtTarget if assigned, otherwise fallback to closest arcade machine
            if (arcadeLookAtTarget != null)
            {
                lookAtTarget = arcadeLookAtTarget;
            }
            else
            {
                // Fallback to closest arcade machine
                lookAtTarget = GetClosestTransform(dest, arcades);
            }
        }
        else if (locationLower.Contains("foodcourt") || locationLower.Contains("food"))
        {
            // Use foodcourtLookAtTarget if assigned
            if (foodcourtLookAtTarget != null)
            {
                lookAtTarget = foodcourtLookAtTarget;
            }
        }
        else if (locationLower.Contains("hallway"))
        {
            // Use hallwayLookAtTarget if assigned
            if (hallwayLookAtTarget != null)
            {
                lookAtTarget = hallwayLookAtTarget;
            }
        }

        // If character is sitting or standing up, queue the move until standing finishes
        var pcc = characterRoot.GetComponent<PointClickController>();
        if (pcc != null && (pcc.IsSitting || pcc.IsStandingUp))
        {
            Debug.Log("Character is sitting/standing up: queuing move until standing completes.");
            queuedMoveDestination = dest;
            // Store look-at target for after movement completes
            if (lookAtTarget != null)
            {
                pcc.SetLookAtTargetAfterArrival(lookAtTarget);
            }
            try { pcc.OnFinishedStanding += OnPointClickFinishedStanding; } catch { }
            // If the character is sitting and not already in the process of standing, request a stand up so the queued move will run
            if (pcc.IsSitting && !pcc.IsStandingUp)
            {
                Debug.Log("Requesting stand up to fulfill queued move.");
                characterRoot.SendMessage("ForceStandUp", SendMessageOptions.DontRequireReceiver);
            }
            return;
        }

        // Set look-at target before moving
        if (lookAtTarget != null && pcc != null)
        {
            pcc.SetLookAtTargetAfterArrival(lookAtTarget);
            Debug.Log($"Set look-at target: {lookAtTarget.name}");
        }

        // send to PointClickController if present via SendMessage
        characterRoot.SendMessage("MoveToPointPublic", dest, SendMessageOptions.DontRequireReceiver);
    }

    private void TriggerSitAtChair(int chairNumber)
    {
        Debug.Log($"CelesteActionController: SitAtChair -> {chairNumber}");
        if (characterRoot == null) { Debug.LogWarning("No characterRoot assigned for sitAtChair."); return; }

        if (chairs == null || chairs.Length == 0)
        {
            Debug.LogWarning("CelesteActionController: No chairs assigned in inspector.");
            return;
        }

        Transform chosen;
        // If chairNumber is -1 or 0, pick a random chair
        if (chairNumber <= 0)
        {
            // Build list of valid chairs
            var validChairs = new List<Transform>();
            foreach (var c in chairs)
                if (c != null) validChairs.Add(c);
            
            if (validChairs.Count == 0)
            {
                Debug.LogWarning("CelesteActionController: No valid chairs found.");
                return;
            }
            
            chosen = validChairs[UnityEngine.Random.Range(0, validChairs.Count)];
            Debug.Log($"CelesteActionController: Picked random chair: {chosen.name}");
        }
        else
        {
            // Use specific chair number (for server commands)
            int idx = Mathf.Clamp(chairNumber - 1, 0, chairs.Length - 1);
            chosen = chairs[idx];
            if (chosen == null)
            {
                Debug.LogWarning($"CelesteActionController: Chair at index {idx} is null.");
                return;
            }
        }

        var interaction = chosen.Find("InteractionSpot");
        Transform interactionTransform = interaction != null ? interaction : chosen;

        // if character is currently sitting, force stand up first and queue this sit
        var pcc = characterRoot.GetComponent<PointClickController>();
        if (pcc != null && pcc.IsSitting)
        {
            Debug.Log("Character is sitting: forcing stand up before sitting at requested chair.");
            queuedSitInteraction = interactionTransform;

            // subscribe to finished standing event if available
            try { pcc.OnFinishedStanding += OnPointClickFinishedStanding; } catch { }

            // ask PCC to stand up
            if (!pcc.IsStandingUp)
                characterRoot.SendMessage("ForceStandUp", SendMessageOptions.DontRequireReceiver);
            return;
        }

        // Cancel any current interactions so the character can respond cleanly
        try { characterRoot.SendMessage("CancelInteractions", SendMessageOptions.DontRequireReceiver); } catch { }

        // Use SitAtInteractionSpot which will handle moving to the spot and sitting when arrived
        characterRoot.SendMessage("SitAtInteractionSpot", interactionTransform, SendMessageOptions.DontRequireReceiver);
    }

    private void OnPointClickFinishedStanding()
    {
        // unsubscribe
        if (characterRoot == null) return;
        var pcc = characterRoot.GetComponent<PointClickController>();
        if (pcc != null)
        {
            try { pcc.OnFinishedStanding -= OnPointClickFinishedStanding; } catch { }
        }

        // If there's a queued move, perform it first
        if (queuedMoveDestination != Vector3.zero)
        {
            Debug.Log("Queued move executing after stand up completed.");
            characterRoot.SendMessage("MoveToPointPublic", queuedMoveDestination, SendMessageOptions.DontRequireReceiver);
            queuedMoveDestination = Vector3.zero;

            // clear any queued sit since move takes precedence
            queuedSitInteraction = null;
            return;
        }

        if (queuedSitInteraction != null)
        {
            Debug.Log("Queued sit interaction executing after stand up completed.");
            // Use SitAtInteractionSpot so PCC handles movement then sitting
            characterRoot.SendMessage("SitAtInteractionSpot", queuedSitInteraction, SendMessageOptions.DontRequireReceiver);
            queuedSitInteraction = null;
            return;
        }

        if (queuedArcadeInteraction != null)
        {
            Debug.Log("Queued arcade interaction executing after stand up completed.");
            characterRoot.SendMessage("MoveToPointPublic", queuedArcadeInteraction.position, SendMessageOptions.DontRequireReceiver);
            characterRoot.SendMessage("PlayArcadeAtSpot", queuedArcadeInteraction, SendMessageOptions.DontRequireReceiver);
            queuedArcadeInteraction = null;
            return;
        }
    }

    private void TriggerStandUp()
    {
        Debug.Log("CelesteActionController: StandUp");
        if (characterRoot == null) return;

        // Ask the PointClickController to cancel any current interactions (looking at menu, playing arcade/claw, etc.)
        try { characterRoot.SendMessage("CancelInteractions", SendMessageOptions.DontRequireReceiver); } catch { }

        // Clear any queued interactions so we don't immediately resume them after standing
        queuedSitInteraction = null;
        queuedMoveDestination = Vector3.zero;
        queuedArcadeInteraction = null;

        // Finally request the character to stand up
        characterRoot.SendMessage("ForceStandUp", SendMessageOptions.DontRequireReceiver);
    }

    private void TriggerExamineMenu(string focus = null)
    {
        Debug.Log($"CelesteActionController: ExamineMenu -> {focus}");
        if (characterRoot == null) return;

        // Only use cafe menus (removed ad displays)
        if (cafeMenus == null || cafeMenus.Length == 0)
        {
            Debug.LogWarning("CelesteActionController: No cafe menus assigned in inspector.");
            return;
        }

        // Build list of valid cafe menus
        var validMenus = new List<Transform>();
        foreach (var m in cafeMenus)
            if (m != null) validMenus.Add(m);

        if (validMenus.Count == 0)
        {
            Debug.LogWarning("CelesteActionController: No valid cafe menus found.");
            return;
        }

        var chosen = validMenus[UnityEngine.Random.Range(0, validMenus.Count)];
        var interaction = chosen.Find("InteractionSpot");
        if (interaction != null)
            characterRoot.SendMessage("ExamineAtInteractionSpot", interaction, SendMessageOptions.DontRequireReceiver);
        else
            characterRoot.SendMessage("ExamineAtInteractionSpot", chosen, SendMessageOptions.DontRequireReceiver);
    }

    private void TriggerPlayArcade(string game = null)
    {
        Debug.Log($"CelesteActionController: PlayArcadeGame -> {game}");
        if (characterRoot == null) return;

        if (arcades == null || arcades.Length == 0)
        {
            Debug.LogWarning("CelesteActionController: No arcades assigned in inspector.");
            return;
        }

        // Build list of valid arcades and pick randomly
        var validArcades = new List<Transform>();
        foreach (var a in arcades)
            if (a != null) validArcades.Add(a);
        
        if (validArcades.Count == 0)
        {
            Debug.LogWarning("CelesteActionController: No valid arcades found.");
            return;
        }
        
        var chosen = validArcades[UnityEngine.Random.Range(0, validArcades.Count)];
        Debug.Log($"CelesteActionController: Picked random arcade: {chosen.name}");

        var interaction = chosen.Find("InteractionSpot");

        // If character is sitting or standing up, queue the arcade interaction until standing completes
        var pcc = characterRoot.GetComponent<PointClickController>();
        if (pcc != null && (pcc.IsSitting || pcc.IsStandingUp))
        {
            Debug.Log("Character is sitting/standing up: queuing arcade interaction until standing completes.");
            queuedArcadeInteraction = interaction != null ? interaction : chosen;
            try { pcc.OnFinishedStanding += OnPointClickFinishedStanding; } catch { }
            if (pcc.IsSitting && !pcc.IsStandingUp)
            {
                Debug.Log("Requesting stand up to fulfill queued arcade interaction.");
                characterRoot.SendMessage("ForceStandUp", SendMessageOptions.DontRequireReceiver);
            }
            return;
        }

        if (interaction != null)
            characterRoot.SendMessage("PlayArcadeAtSpot", interaction, SendMessageOptions.DontRequireReceiver);
        else
            characterRoot.SendMessage("PlayArcadeAtSpot", chosen, SendMessageOptions.DontRequireReceiver);
    }

    private void TriggerChangeCameraAngle(string angle, string transition)
    {
        Debug.Log($"CelesteActionController: ChangeCameraAngle -> {angle} (transition={transition})");
        if (cameraRoot == null)
            cameraRoot = Camera.main != null ? Camera.main.gameObject : null;

        if (cameraRoot == null) { Debug.LogWarning("No camera root found for changing camera angle."); return; }

        // SendMessage - CameraStateMachine.ChangeState expects a CameraStateSO normally; without that type here we simply attempt to send the angle string
        cameraRoot.SendMessage("ChangeStateByName", angle, SendMessageOptions.DontRequireReceiver);
    }

    private void TriggerIdle(string idleType)
    {
        Debug.Log($"CelesteActionController: Idle -> {idleType} (NOTE: This only makes character stand up, does NOT move them)");
        // Just stop the current activity and stand up - no other actions
        // IMPORTANT: This does NOT move the character anywhere - it only cancels interactions and stands up
        TriggerStandUp();
    }
    #endregion

    #region Helpers
    private Transform GetRandomValidTransform(Transform[] transforms)
    {
        if (transforms == null || transforms.Length == 0) return null;

        // Build list of valid (non-null) transforms
        var valid = new List<Transform>();
        foreach (var t in transforms)
        {
            if (t != null) valid.Add(t);
        }

        if (valid.Count == 0) return null;
        return valid[UnityEngine.Random.Range(0, valid.Count)];
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
        // Zone tracking - check periodically which zone the character is in
        if (Time.time - lastZoneCheckTime >= zoneCheckInterval)
        {
            lastZoneCheckTime = Time.time;
            UpdateCurrentZone();
        }

        // Navigation actions (1-5)
        if (Input.GetKeyDown(KeyCode.Alpha1)) // go to coffee (cafe)
            TriggerMoveToLocation("cafe");
        if (Input.GetKeyDown(KeyCode.Alpha2)) // go to hallway
            TriggerGoToHallway();
        if (Input.GetKeyDown(KeyCode.Alpha3)) // go to arcade area
            TriggerMoveToLocation("arcade");
        if (Input.GetKeyDown(KeyCode.Alpha4)) // go to food court
            TriggerMoveToLocation("foodcourt");
        if (Input.GetKeyDown(KeyCode.Alpha5)) // walk around
            TriggerWalkAround();
        
        // Interaction actions (6-9)
        if (Input.GetKeyDown(KeyCode.Alpha6)) // sit at random chair
            TriggerSitAtChair(-1); // -1 means pick random
        if (Input.GetKeyDown(KeyCode.Alpha7)) // stand up
            TriggerStandUp();
        if (Input.GetKeyDown(KeyCode.Alpha8)) // examine random menu
            TriggerExamineMenu();
        if (Input.GetKeyDown(KeyCode.Alpha9)) // play random arcade
            TriggerPlayArcade();
        
        // Camera and utility actions
        if (Input.GetKeyDown(KeyCode.Alpha0)) // change camera angle
            TriggerChangeCameraAngle("cafe_close", "smooth");
        if (Input.GetKeyDown(KeyCode.Space)) // alternate stand up
            TriggerStandUp();
    }

    /// <summary>
    /// Determines which zone the character is currently in based on proximity to location points.
    /// Also tracks activity changes. Sends an update to the server if zone or activity changed.
    /// </summary>
    private void UpdateCurrentZone()
    {
        if (characterRoot == null) return;

        Vector3 charPos = characterRoot.transform.position;
        string detectedZone = "unknown";
        float closestDistance = float.MaxValue;

        // Check proximity to each location type
        float cafeDist = GetClosestDistance(charPos, cafeLocations);
        float arcadeDist = GetClosestDistance(charPos, arcadeLocations);
        float foodcourtDist = GetClosestDistance(charPos, foodcourtLocations);
        float hallwayDist = GetClosestDistance(charPos, hallwayPoints);

        // Also consider arcade machines as being in the arcade zone
        float arcadeMachineDist = GetClosestDistance(charPos, arcades);
        arcadeDist = Mathf.Min(arcadeDist, arcadeMachineDist);

        // Also consider chairs as potentially being in the cafe zone
        float chairDist = GetClosestDistance(charPos, chairs);
        cafeDist = Mathf.Min(cafeDist, chairDist);

        // Find the closest zone within detection radius
        if (cafeDist < closestDistance && cafeDist <= zoneDetectionRadius)
        {
            closestDistance = cafeDist;
            detectedZone = "cafe";
        }
        if (arcadeDist < closestDistance && arcadeDist <= zoneDetectionRadius)
        {
            closestDistance = arcadeDist;
            detectedZone = "arcade";
        }
        if (foodcourtDist < closestDistance && foodcourtDist <= zoneDetectionRadius)
        {
            closestDistance = foodcourtDist;
            detectedZone = "foodcourt";
        }
        if (hallwayDist < closestDistance && hallwayDist <= zoneDetectionRadius)
        {
            closestDistance = hallwayDist;
            detectedZone = "hallway";
        }

        // Check current activity
        string detectedActivity = GetCurrentActivity();

        // If zone or activity changed, notify the server
        bool zoneChanged = detectedZone != currentZone;
        bool activityChanged = detectedActivity != currentActivity;

        if (zoneChanged || activityChanged)
        {
            if (zoneChanged)
                Debug.Log($"CelesteActionController: Zone changed from '{currentZone}' to '{detectedZone}'");
            if (activityChanged)
                Debug.Log($"CelesteActionController: Activity changed from '{currentActivity}' to '{detectedActivity}'");
            
            currentZone = detectedZone;
            currentActivity = detectedActivity;
            SendStateUpdateToServer(currentZone, currentActivity);
        }
    }

    /// <summary>
    /// Gets the closest distance from a position to any transform in the array.
    /// </summary>
    private float GetClosestDistance(Vector3 pos, Transform[] transforms)
    {
        if (transforms == null || transforms.Length == 0) return float.MaxValue;

        float closest = float.MaxValue;
        foreach (var t in transforms)
        {
            if (t == null) continue;
            float dist = Vector3.Distance(pos, t.position);
            if (dist < closest) closest = dist;
        }
        return closest;
    }

    /// <summary>
    /// Gets the closest transform from a position in the array.
    /// </summary>
    private Transform GetClosestTransform(Vector3 pos, Transform[] transforms)
    {
        if (transforms == null || transforms.Length == 0) return null;

        Transform closest = null;
        float closestDist = float.MaxValue;
        foreach (var t in transforms)
        {
            if (t == null) continue;
            float dist = Vector3.Distance(pos, t.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = t;
            }
        }
        return closest;
    }

    /// <summary>
    /// Sends the current zone and activity state to the server so the AI knows where the character actually is.
    /// </summary>
    private void SendStateUpdateToServer(string zone, string activity)
    {
        if (connector == null)
        {
            connector = CFConnector.instance;
        }

        if (connector != null)
        {
            try
            {
                // Send zone and activity update via RPC
                // Server should handle "updateCharacterState" method with { location, activity }
                var stateUpdate = new Dictionary<string, object>
                {
                    { "location", zone },
                    { "activity", activity }
                };
                connector.SendRpcFireAndForget("updateCharacterState", new object[] { stateUpdate });
                Debug.Log($"CelesteActionController: Sent state update to server: location={zone}, activity={activity}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"CelesteActionController: Failed to send state update: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Gets the current activity of the character from PointClickController.
    /// </summary>
    private string GetCurrentActivity()
    {
        if (characterRoot == null) return "unknown";

        var pcc = characterRoot.GetComponent<PointClickController>();
        if (pcc == null) return "unknown";

        // Check PointClickController states
        if (pcc.IsPlayingArcade) return "playing_arcade";
        if (pcc.IsPlayingClaw) return "playing_claw_machine";
        if (pcc.IsSitting) return "sitting";
        if (pcc.IsLeaning) return "leaning";
        if (pcc.IsLooking) return "looking_at_display";
        if (pcc.IsStandingUp) return "standing_up";
        
        // Check if the character is moving
        var agent = characterRoot.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.enabled && agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
        {
            return "walking";
        }

        return "idle";
    }

    private void TriggerGoToHallway(string area = null)
    {
        Debug.Log($"CelesteActionController: GoToHallway -> {area}");
        if (characterRoot == null) { Debug.LogWarning("No characterRoot assigned for GoToHallway."); return; }

        if (hallwayPoints == null || hallwayPoints.Length == 0)
        {
            Debug.LogWarning("CelesteActionController: No hallway points assigned in inspector.");
            return;
        }

        // Pick a random valid hallway point
        Transform target = GetRandomValidTransform(hallwayPoints);

        if (target == null)
        {
            Debug.LogWarning("CelesteActionController: All hallway points are null.");
            return;
        }

        Vector3 dest = target.position;

        // Determine what to look at after arriving
        Transform lookAtTarget = hallwayLookAtTarget;

        // If character is sitting or standing up, queue the move until standing finishes
        var pcc = characterRoot.GetComponent<PointClickController>();
        if (pcc != null && (pcc.IsSitting || pcc.IsStandingUp))
        {
            Debug.Log("GoToHallway: Character is sitting/standing up - queuing move until standing completes.");
            queuedMoveDestination = dest;
            // Store look-at target for after movement completes
            if (lookAtTarget != null)
            {
                pcc.SetLookAtTargetAfterArrival(lookAtTarget);
            }
            try { pcc.OnFinishedStanding += OnPointClickFinishedStanding; } catch { }
            if (pcc.IsSitting && !pcc.IsStandingUp)
            {
                Debug.Log("GoToHallway: Requesting stand up to fulfill queued move.");
                characterRoot.SendMessage("ForceStandUp", SendMessageOptions.DontRequireReceiver);
            }
            return;
        }

        // Set look-at target before moving
        if (lookAtTarget != null && pcc != null)
        {
            pcc.SetLookAtTargetAfterArrival(lookAtTarget);
            Debug.Log($"GoToHallway: Set look-at target: {lookAtTarget.name}");
        }

        characterRoot.SendMessage("MoveToPointPublic", dest, SendMessageOptions.DontRequireReceiver);
    }

    private void TriggerWalkAround(int steps = 1)
    {
        Debug.Log($"CelesteActionController: WalkAround -> steps={steps} (NOTE: This WILL move the character to random walk points)");
        if (characterRoot == null)
        {
            Debug.LogWarning("CelesteActionController: characterRoot is null in TriggerWalkAround. Attempting to find PointClickController in scene.");
            var pccFallback = FindObjectOfType<PointClickController>();
            if (pccFallback != null)
            {
                characterRoot = pccFallback.gameObject;
                Debug.Log("CelesteActionController: Found and assigned characterRoot via PointClickController fallback in TriggerWalkAround.");
            }
            else
            {
                Debug.LogWarning("CelesteActionController: No PointClickController found in scene. Cannot perform WalkAround.");
                return;
            }
        }

        // For debugging, show whether PointClickController is present and its state
        var pcc = characterRoot.GetComponent<PointClickController>();
        if (pcc == null)
        {
            Debug.LogWarning("CelesteActionController: characterRoot does not have a PointClickController component. Ensure the correct GameObject is assigned.");
        }
        else
        {
            Debug.Log($"CelesteActionController: PointClickController found. IsSitting={pcc.IsSitting}, IsStandingUp={pcc.IsStandingUp}");
        }

        if (walkPoints == null || walkPoints.Length == 0)
        {
            Debug.LogWarning("CelesteActionController: No walk points assigned in inspector.");
            return;
        }

        // Build list of valid (non-null) walk points
        var validPoints = new List<Transform>();
        foreach (var wp in walkPoints)
        {
            if (wp != null) validPoints.Add(wp);
        }

        if (validPoints.Count == 0)
        {
            Debug.LogWarning("CelesteActionController: All walk points are null.");
            return;
        }

        // Pick a random waypoint and move there
        var chosen = validPoints[UnityEngine.Random.Range(0, validPoints.Count)];
        Debug.Log($"CelesteActionController: Selected waypoint '{chosen.name}' at position {chosen.position}.");

        Vector3 dest = chosen.position;

        // If character is sitting or standing up, queue the move until standing finishes
        if (pcc != null && (pcc.IsSitting || pcc.IsStandingUp))
        {
            Debug.Log("WalkAround: Character is sitting/standing up - queuing move until standing completes.");
            queuedMoveDestination = dest;
            try { pcc.OnFinishedStanding += OnPointClickFinishedStanding; } catch { }
            if (pcc.IsSitting && !pcc.IsStandingUp)
            {
                Debug.Log("WalkAround: Requesting stand up to fulfill queued move.");
                characterRoot.SendMessage("ForceStandUp", SendMessageOptions.DontRequireReceiver);
            }
            return;
        }

        characterRoot.SendMessage("MoveToPointPublic", dest, SendMessageOptions.DontRequireReceiver);
    }
}

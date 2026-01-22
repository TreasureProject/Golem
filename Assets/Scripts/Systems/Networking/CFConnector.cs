using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;

public class CFConnector : MonoBehaviour
{
    public static CFConnector instance;
    [Header("Connection Config (Agent style)")]
    [SerializeField] private string host = "localhost:5173";
    [SerializeField] private string agentId = "0xdacd02dd0ce8a923ad26d4c49bb94ece09306c3e";
    [SerializeField] private bool useSecure = false;  // ws / wss
    [SerializeField] private string queryToken = ""; // optional query

    private ClientWebSocket ws;
    private Uri uri;

    private Queue<string> messageBuffer = new Queue<string>();

    private bool isConnecting = false;
    private CancellationTokenSource cts;

    private int retryCount = 0;
    private const int maxRetries = 10;
    private TimeSpan minDelay = TimeSpan.FromSeconds(1);
    private TimeSpan maxDelay = TimeSpan.FromSeconds(10);

    // –– Events you can subscribe to:
    public event Action OnOpen;
    public event Action<AgentState> OnAgentState;
    public event Action<string> OnText;
    public event Action<int> OnConnection;
    public event Action<VoiceEmoteData> OnVoiceEmote;
    public event Action<AnimatedEmoteData> OnAnimatedEmote;
    public event Action<FacialExpressionData> OnFacialExpression;
    public event Action<bool> OnReceivedChatMessage;
    public event Action<string> OnCharacterFullPrompt;
    public event Action OnClose;
    public event Action<Exception> OnError;
    public event Action<CelesteActionData> OnCelesteAction;

    // UnityEvents (serializable, inspector-assignable) - invoked alongside C# events for compatibility
    [Header("Inspector Events (UnityEvent)")]
    public UnityEvent OnOpenUnity = new UnityEvent();
    public UnityEvent<AgentState> OnAgentStateUnity = new UnityEvent<AgentState>();
    public UnityEvent<string> OnTextUnity = new UnityEvent<string>();
    public UnityEvent<int> OnConnectionUnity = new UnityEvent<int>();
    public UnityEvent<VoiceEmoteData> OnVoiceEmoteUnity = new UnityEvent<VoiceEmoteData>();
    public UnityEvent<AnimatedEmoteData> OnAnimatedEmoteUnity = new UnityEvent<AnimatedEmoteData>();
    public UnityEvent<FacialExpressionData> OnFacialExpressionUnity = new UnityEvent<FacialExpressionData>();
    public UnityEvent<bool> OnReceivedChatMessageUnity = new UnityEvent<bool>();
    public UnityEvent<string> OnCharacterFullPromptUnity = new UnityEvent<string>();
    public UnityEvent OnCloseUnity = new UnityEvent();
    public UnityEvent<Exception> OnErrorUnity = new UnityEvent<Exception>();
    public UnityEvent<CelesteActionData> OnCelesteActionUnity = new UnityEvent<CelesteActionData>();

    private Dictionary<string, TaskCompletionSource<string>> rpcCallbacks = new Dictionary<string, TaskCompletionSource<string>>();

    // queue for dispatching events back to Unity main thread
    private readonly Queue<Action> mainThreadQueue = new Queue<Action>();
    private readonly object mainThreadLock = new object();

    private string GenerateRpcId() => Guid.NewGuid().ToString();

    private void Awake()
    {
        CFConnector.instance = this;
        BuildUri();
        Connect();
    }

    private void OnValidate()
    {
        BuildUri();
    }

    private void Update()
    {
        // execute queued actions on main thread
        lock (mainThreadLock)
        {
            while (mainThreadQueue.Count > 0)
            {
                try
                {
                    var act = mainThreadQueue.Dequeue();
                    act?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CFConnector] Exception running main-thread action: {e}");
                }
            }
        }
    }

    private void EnqueueMainThread(Action a)
    {
        if (a == null) return;
        lock (mainThreadLock)
        {
            mainThreadQueue.Enqueue(a);
        }
    }

    private void BuildUri()
    {
        string scheme = useSecure ? "wss" : "ws";
        string path = $"/agents/chat/external:celeste";
        string query = string.IsNullOrEmpty(queryToken) ? "" : "?" + queryToken;
        string uriString = $"{scheme}://{host}{path}{query}";

        try
        {
            uri = new Uri(uriString);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CFConnector] Invalid WebSocket URI: {uriString} — {ex}");
        }
    }

    public async void Connect()
    {
        if (uri == null)
        {
            Debug.LogError("[CFConnector] URI is not configured correctly.");
            return;
        }
        if (isConnecting) return;

        isConnecting = true;
        cts = new CancellationTokenSource();
        ws = new ClientWebSocket();

        Debug.Log($"[CFConnector] Attempting to connect to {uri}");

        try
        {
            await ws.ConnectAsync(uri, cts.Token);
            retryCount = 0;
            isConnecting = false;

            Debug.Log("[CFConnector] Connected to server.");
            EnqueueMainThread(() => { OnOpen?.Invoke(); OnOpenUnity?.Invoke(); });
            DrainBufferedMessages();
            ReceiveLoop();
        }
        catch (Exception ex)
        {
            isConnecting = false;
            Debug.LogWarning($"[CFConnector] Connection failed: {ex.Message}");
            EnqueueMainThread(() => { OnError?.Invoke(ex); OnErrorUnity?.Invoke(ex); });
            ScheduleReconnect();
        }
    }

    private void ScheduleReconnect()
    {
        retryCount++;
        if (retryCount > maxRetries)
        {
            Debug.LogWarning("[CFConnector] Max reconnect attempts reached. Giving up.");
            EnqueueMainThread(() => { OnClose?.Invoke(); OnCloseUnity?.Invoke(); });
            return;
        }

        double delaySecs = Math.Min(minDelay.TotalSeconds * Math.Pow(1.3, retryCount), maxDelay.TotalSeconds);
        Debug.Log($"[CFConnector] Scheduling reconnect attempt {retryCount} in {delaySecs:F1} seconds.");
        Task.Delay(TimeSpan.FromSeconds(delaySecs)).ContinueWith(_ => Connect());
    }

    public void Send(string message)
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(message);
            ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
        }
        else
        {
            messageBuffer.Enqueue(message);
        }
    }

    private async void DrainBufferedMessages()
    {
        while (messageBuffer.Count > 0 && ws != null && ws.State == WebSocketState.Open)
        {
            var msg = messageBuffer.Dequeue();
            Send(msg);
            await Task.Yield();
        }
    }

    private async void ReceiveLoop()
    {
        var buffer = new byte[4096];

        try
        {
            while (ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                var ms = new System.IO.MemoryStream();

                do
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    if (result.Count > 0)
                        ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, System.IO.SeekOrigin.Begin);

                string msg;
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    using (var reader = new System.IO.StreamReader(ms, System.Text.Encoding.UTF8))
                        msg = reader.ReadToEnd();
                }
                else
                {
                    msg = $"<binary message, length {ms.Length}>";
                }

                // Handle incoming message:
                HandleMessage(msg);
                HandleRpcResponse(msg);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("[CFConnector] Server closed the connection.");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Remote close", CancellationToken.None);
                    EnqueueMainThread(() => { OnClose?.Invoke(); OnCloseUnity?.Invoke(); });
                    ScheduleReconnect();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[CFConnector] ReceiveLoop error: {ex.Message}");
            EnqueueMainThread(() => { OnError?.Invoke(ex); OnErrorUnity?.Invoke(ex); });
            ScheduleReconnect();
        }
    }

    public async void Close()
    {
        if (ws != null)
        {
            Debug.Log("[CFConnector] Closing connection.");
            cts.Cancel();
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
            Debug.Log("[CFConnector] Connection closed.");
            EnqueueMainThread(() => { OnClose?.Invoke(); OnCloseUnity?.Invoke(); });
        }
    }

    // --- RPC logic ---
    public Task<string> SendRpc(string method, object[] args = null)
    {
        string rpcId = GenerateRpcId();
        var tcs = new TaskCompletionSource<string>();
        rpcCallbacks[rpcId] = tcs;

        var rpcObj = new RpcRequest
        {
            type = "rpc",
            id = rpcId,
            method = method,
            args = args ?? new object[0]
        };
        string msg = JsonConvert.SerializeObject(rpcObj);
        Send(msg);

        return tcs.Task;
    }

    public void SendRpcFireAndForget(string method, object[] args = null)
    {
        var rpcObj = new RpcRequest
        {
            type = "rpc",
            id = null,
            method = method,
            args = args ?? new object[0]
        };
        string msg = JsonConvert.SerializeObject(rpcObj);
        Send(msg);
    }

    private void HandleRpcResponse(string rawMsg)
    {
        try
        {
            var resp = JsonConvert.DeserializeObject<RpcResponse>(rawMsg);
            if (resp != null && resp.type == "rpc_response")
            {
                if (!string.IsNullOrEmpty(resp.id) && rpcCallbacks.TryGetValue(resp.id, out var tcs))
                {
                    if (!string.IsNullOrEmpty(resp.error))
                        tcs.SetException(new Exception(resp.error));
                    else
                        tcs.SetResult(resp.result);
                    rpcCallbacks.Remove(resp.id);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CFConnector] RPC parse error: {e}");
        }
    }

    // --- Handler for general message types ---
    private void HandleMessage(string json)
    {
        Debug.Log($"[CFConnector] Received message: {json}");
        JToken root;
        try
        {
            root = JToken.Parse(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[CFConnector] Failed to parse JSON: {e} -- {json}");
            return;
        }

        var typeToken = root["type"];
        if (typeToken == null || typeToken.Type != JTokenType.String)
        {
            Debug.LogWarning($"[CFConnector] No valid \"type\" field in message: {json}");
            return;
        }

        string msgType = typeToken.Value<string>();
        JToken data = root["data"];

        if (msgType == "cf_agent_state" && root["state"] != null)
        {
            try
            {
                var state = root["state"].ToObject<AgentState>();
                EnqueueMainThread(() => { OnAgentState?.Invoke(state); OnAgentStateUnity?.Invoke(state); });
            }
            catch (Exception e)
            {
                Debug.LogError($"[CFConnector] Error deserializing AgentState: {e} -- {root["state"]}");
            }
            return;
        }

        switch (msgType)
        {
            case "text":
                if (data != null)
                    EnqueueMainThread(() => { OnText?.Invoke(data.ToObject<string>()); OnTextUnity?.Invoke(data.ToObject<string>()); });
                break;

            case "connection":
                if (data != null)
                    EnqueueMainThread(() => { OnConnection?.Invoke(data.ToObject<int>()); OnConnectionUnity?.Invoke(data.ToObject<int>()); });
                break;

            case "emote":
                if (data != null)
                    HandleEmote(data);
                break;

            case "facial_expression":
                if (data != null)
                {
                    var face = data.ToObject<FacialExpressionData>();
                    EnqueueMainThread(() => { OnFacialExpression?.Invoke(face); OnFacialExpressionUnity?.Invoke(face); });
                }
                break;

            case "recievedChatMessage":
                if (data != null)
                    EnqueueMainThread(() => { OnReceivedChatMessage?.Invoke(data.ToObject<bool>()); OnReceivedChatMessageUnity?.Invoke(data.ToObject<bool>()); });
                break;

            case "characterFullPrompt_internal":
                if (data != null)
                    EnqueueMainThread(() => { OnCharacterFullPrompt?.Invoke(data.ToObject<string>()); OnCharacterFullPromptUnity?.Invoke(data.ToObject<string>()); });
                break;

            case "celeste_action":
                if (data != null)
                {
                    try
                    {
                        var celesteAction = data.ToObject<CelesteActionData>();
                        EnqueueMainThread(() => { OnCelesteAction?.Invoke(celesteAction); OnCelesteActionUnity?.Invoke(celesteAction); });
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[CFConnector] Error deserializing CelesteActionData: {e} -- {data}");
                    }
                }
                break;

            default:
                Debug.LogWarning($"[CFConnector] Unknown message type: {msgType}");
                break;
        }
    }

    private void HandleEmote(JToken data)
    {
        string emoteType = data["type"]?.Value<string>();
        if (emoteType == "voice")
        {
            try
            {
                var voice = new VoiceEmoteData();
                voice.type = "voice";

                // Prefer explicit audioBase64 if provided
                if (data["audioBase64"] != null && data["audioBase64"].Type == JTokenType.String)
                {
                    voice.audioBase64 = data["audioBase64"].ToObject<string>();
                }
                // Support Node Buffer: { audioBuffer: { type: 'Buffer', data: [ ... ] } }
                else if (data["audioBuffer"] != null && data["audioBuffer"]["data"] != null)
                {
                    var arr = data["audioBuffer"]["data"] as JArray;
                    if (arr != null)
                    {
                        byte[] bytes = new byte[arr.Count];
                        for (int i = 0; i < arr.Count; i++)
                        {
                            bytes[i] = (byte)arr[i].ToObject<int>();
                        }
                        voice.audioBase64 = Convert.ToBase64String(bytes);
                    }
                    else
                    {
                        Debug.LogWarning("[CFConnector] audioBuffer.data is not an array");
                    }
                }
                else
                {
                    // Fallback to default deserialization
                    var parsed = data.ToObject<VoiceEmoteData>();
                    if (parsed != null)
                        voice = parsed;
                }

                EnqueueMainThread(() => { OnVoiceEmote?.Invoke(voice); OnVoiceEmoteUnity?.Invoke(voice); });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CFConnector] Failed to parse voice emote: {e}\nRaw data: {data}");
            }
        }
        else if (emoteType == "animated")
        {
            var animated = data.ToObject<AnimatedEmoteData>();
            EnqueueMainThread(() => { OnAnimatedEmote?.Invoke(animated); OnAnimatedEmoteUnity?.Invoke(animated); });
        }
        else
        {
            Debug.LogWarning($"[CFConnector] Unknown emote subtype: {emoteType}. Raw data: {data}");
        }
    }

    // --- Data Types ---
    [Serializable]
    public class AgentState
    {
        public string lastReplyId;
        public string lastElaborationId;
        public long lastReactionTime;
        public string lastActionType;
        public long lastActionTime;
        public int lastRagOffset;
        public bool routinesRunning;
        public NestedState state;
        public long lastTipResponseVoice;
        public long lastTipResponseText;
        public bool isLiveStreaming;
        public string inviteGameUrl;
        public Starter lastStarter;
        public long nextTwitterSessionStart;
        public TwitterSession[] twitterSessionSchedule;
        public int twitterSessionDurationMinutes;
        public bool isCommentingOnTwitter;
        public object twitterSessionStartTime;

        [Serializable] public class NestedState { public string status; }
        [Serializable] public class Starter { public string full_text; public string user_role; public string[] tags; }
        [Serializable] public class TwitterSession { public int hour; public int minute; }
    }

    [Serializable]
    public class VoiceEmoteData
    {
        public string type;         // "voice"
        public string audioBase64;  // assume base64 encoded audio
    }

    [Serializable]
    public class AnimatedEmoteData
    {
        public string type;        // "animated"
        public string audioBase64;
        public AnimationData animation;
    }

    [Serializable]
    public class AnimationData
    {
        public string name;
        public float duration;
    }

    [Serializable]
    public class FacialExpressionData
    {
        public string expression;
        public float intensity;
    }

    [Serializable]
    public class CelesteActionData
    {
        public bool success;
        public string message;
        public CelesteAction action;

        [Serializable]
        public class CelesteAction
        {
            public string type;
            public Dictionary<string, object> parameters;
        }
    }

    [Serializable]
    private class RpcRequest
    {
        public string type;
        public string id;
        public string method;
        public object[] args;
    }

    [Serializable]
    private class RpcResponse
    {
        public string type;
        public string id;
        public string result;
        public string error;
    }
}

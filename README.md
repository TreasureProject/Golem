# Golem

> Breathe life into virtual characters using Claude.

A Unity project that lets AI control 3D characters in real-time. Move, interact, emote, express — all through natural language.

## How It Works

```
┌─────────────────────────────────────────┐
│            External AI                   │
│      (Claude, custom server, etc.)       │
└──────────────┬──────────────────────────┘
               │ WebSocket
               │ ws://localhost:5173/agents/chat/external:{agentId}
               ▼
┌─────────────────────────────────────────┐
│         CFConnector.cs                   │
│    WebSocket client, routes messages     │
└──────────────┬──────────────────────────┘
               │ C# Events
        ┌──────┴──────┬──────────────┐
        ▼             ▼              ▼
   EmotePlayer   Character      Facial
   (voice/anim)  ActionController  Expressions
        │             │
        ▼             ▼
   AudioSource   PointClickController
   + SALSA       (NavMesh movement)
```

## Core Scripts

| Script | Location | Purpose |
|--------|----------|---------|
| `CFConnector.cs` | Systems/Networking | WebSocket client that connects to external AI server |
| `CharacterActionController.cs` | Character | Routes action commands to character controller |
| `PointClickController.cs` | Character | NavMesh movement + interaction states |
| `EmotePlayer.cs` | Character | Plays voice audio, triggers lip sync |

## Message Protocol

The AI sends JSON messages over WebSocket. Here are the supported message types:

### Voice (Text-to-Speech)

```json
{
  "type": "emote",
  "data": {
    "type": "voice",
    "audioBase64": "<base64-encoded-audio>"
  }
}
```

Supported formats: WAV, MP3, M4A. Audio is played through Unity's AudioSource with SALSA lip sync.

### Animated Emote

```json
{
  "type": "emote",
  "data": {
    "type": "animated",
    "animation": {
      "name": "wave",
      "duration": 2.0
    }
  }
}
```

### Facial Expression

```json
{
  "type": "facial_expression",
  "data": {
    "expression": "happy",
    "intensity": 0.9
  }
}
```

Expressions: `happy`, `sad`, `surprised`, `angry`, `neutral`, `thinking`

### Character Actions

```json
{
  "type": "character_action",
  "data": {
    "success": true,
    "message": "Moving to cafe",
    "action": {
      "type": "moveToLocation",
      "parameters": {
        "location": "cafe"
      }
    }
  }
}
```

### Connect Your AI

Your AI server needs to:

1. Start a WebSocket server (default: `ws://localhost:5173`)
2. Accept connections at path `/agents/chat/external:{agentId}`
3. Send JSON messages in the format described above

The `CFConnector` will automatically connect and retry with exponential backoff.

## Configuration

### CFConnector Settings

In the Unity Inspector, find the GameObject with `CFConnector` and configure:

| Setting | Default | Description |
|---------|---------|-------------|
| Host | `localhost:5173` | WebSocket server address |
| Agent Id | `character` | Agent identifier (used in WebSocket path) |
| Use Secure | `false` | Use `wss://` instead of `ws://` |
| Query Token | — | Optional query string for auth |

### Character Setup

The character needs:
- `NavMeshAgent` component for movement
- `Animator` with states for sitting, standing, playing arcade, etc.
- `PointClickController` for movement control
- `EmotePlayer` for voice playback
- `CharacterActionController` for action routing

## Keyboard Shortcuts (Debug)

While running in Unity, you can test actions with keyboard:

| Key | Action |
|-----|--------|
| `1` | Move to location |
| `2` | Sit at chair 1 |
| `3` | Stand up |
| `4` | Examine display |
| `5` | Play arcade |
| `6` | Change camera angle |
| `7` | Idle (standing) |
| `Space` | Stand up |

## Project Structure

```
Golem/
├── Assets/
│   ├── Scripts/
│   │   ├── Character/
│   │   │   ├── PointClickController.cs       # Movement + interactions
│   │   │   ├── CharacterActionController.cs  # Action routing
│   │   │   └── EmotePlayer.cs                # Voice playback
│   │   ├── Systems/
│   │   │   ├── Networking/
│   │   │   │   └── CFConnector.cs            # WebSocket client
│   │   │   └── Camera/
│   │   │       └── CameraStateMachine.cs     # Camera control
│   │   └── Utils/
│   │       └── WavUtility.cs                 # Audio decoding
│   ├── Plugins/
│   │   └── Crazy Minnow Studio/
│   │       └── SALSA LipSync/                # Lip sync plugin
│   └── Scenes/
│       └── Main.unity                        # Main scene
└── README.md
```

## License

MIT
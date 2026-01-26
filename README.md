# Golem

> Embodied AI agents that learn through experimentation.

Drop a character into a scene. Claude sees it through vision models, experiments with what's possible, remembers what works, and writes new code when needed. No predefined action lists. No hardcoded behaviors. The character discovers its own capabilities.

Golem is open source because the metaverse should not be owned by one company nor should foundational AI character systems. Instead of vendor lock-in, Golem defines an open standard for AI-to-character communication so that AI can control characters in any game engine. Golem characters learn through exploration, not pre-programming. They see their world, experiment, remember what works, and become co-contributors to the virtual worlds they inhabit.

**Bring your own AI. No vendor lock-in. Contribute to Golem's codebase.**

## Why Golem?

**Traditional AI characters (Convai, Inworld):**
- Developer defines 12 actions the character can do
- AI picks from the menu
- Character is limited to what was anticipated
- Locked into their AI, their pricing, their roadmap

**Golem:**
- Developer provides a character and a scene
- Claude explores through vision and trial-and-error
- Character discovers what's possible
- Claude writes new scripts when needed
- **You choose the AI** — Claude, GPT, local models, whatever comes next

As AI models improve, Golem characters automatically inherit those improvements. We're not building AI—we're building the embodiment layer for whatever AI becomes.

## Core Principles

### 🔓 Open Source
Golem is MIT licensed. No API keys required to get started. No per-conversation fees. Run it locally, modify it freely, deploy it anywhere.

### 🔌 Bring Your Own AI
Not locked into any AI provider. Connect Claude for advanced reasoning, GPT for conversation, a local Llama for privacy, or your own fine-tuned model. Swap backends without changing game code.

### 📡 Standard Protocol
A simple, documented WebSocket protocol for AI-to-character communication. Implement it once in any engine—Unity, Unreal, Godot, web. Any AI that speaks the protocol can control any character that implements it. No proprietary SDKs.

### 🧠 Learning Over Programming
Characters discover their capabilities through experimentation, not configuration. Vision models see the scene. Trial-and-error finds what works. Memory retains what's learned. Code generation creates new abilities.

## How It Works

```
┌─────────────────────────────────────────────────────────┐
│                    Your AI Backend                       │
│         Claude • GPT • Llama • Your Fine-tune           │
└─────────────────────┬───────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────┐
│                 Vision Language Model                    │
│                   Sees the Unity scene                   │
└─────────────────────┬───────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────┐
│              Golem Protocol (WebSocket)                  │
│           Standard JSON messages over WS                 │
└─────────────────────┬───────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────┐
│                    Golem Runtime                         │
│         Unity • Unreal (soon) • Godot (soon)            │
└─────────────────────┬───────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────┐
│                   Feedback Loop                          │
│       Did it work? → Memory → Pattern Recognition        │
└─────────────────────────────────────────────────────────┘
```

1. **Vision** — AI sees the scene through vision language models
2. **Experimentation** — Try actions, observe results
3. **Memory** — Remember what works, what doesn't
4. **Pattern Recognition** — Generalize from experience
5. **Code Generation** — Write new capabilities when needed

The character learns its environment like a child learns to walk—through exploration, not instruction.

## Quick Start

### 1. Clone and Open in Unity

```bash
git clone https://github.com/TreasureProject/Golem.git
```

Open the project in Unity 2022.3+.

### 2. Connect Your AI Backend

Golem connects to any AI server via WebSocket:

```
ws://localhost:5173/agents/chat/external:{agentId}
```

Your server receives scene state and sends commands. Use Claude, GPT, a local model—whatever you want.

### 3. Run

Press Play. The AI sees the scene, experiments, and learns.

## The Golem Protocol

A simple JSON-over-WebSocket protocol. Any AI that produces these messages can control any Golem-compatible character.

### Movement
```json
{
  "type": "character_action",
  "data": {
    "action": {
      "type": "moveToLocation",
      "parameters": { "location": "cafe" }
    }
  }
}
```

### Voice + Lip Sync
```json
{
  "type": "emote",
  "data": {
    "type": "voice",
    "audioBase64": "<base64-encoded-audio>"
  }
}
```

### Animations
```json
{
  "type": "emote",
  "data": {
    "type": "animated",
    "animation": { "name": "wave", "duration": 2.0 }
  }
}
```

### Facial Expressions
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

### Dynamic Scripting
```json
{
  "type": "script",
  "data": {
    "code": "<C# code to execute>",
    "target": "character"
  }
}
```

The AI can write and execute new behaviors at runtime—not limited to predefined actions.

### Scene State (Runtime → AI)
```json
{
  "type": "scene_state",
  "data": {
    "character": { "position": [0, 0, 5], "state": "idle" },
    "objects": [...],
    "screenshot": "<base64-encoded-image>"
  }
}
```

The AI receives visual and structured feedback to close the learning loop.

## Comparison

| | Convai/Inworld | Golem |
|---|---|---|
| **Action space** | Predefined by developer | Discovered by AI |
| **Vision** | None | Vision language models |
| **Learning** | None | Trial-and-error + memory |
| **Code generation** | None | Runtime scripting |
| **AI backend** | Locked to their API | Any (Claude, GPT, local) |
| **Protocol** | Proprietary SDK | Open WebSocket standard |
| **Pricing** | Per-API-call | Open source / free |
| **Improvement** | Their roadmap | Inherits AI advances |

## Architecture

```
Golem/
├── Assets/
│   ├── Scripts/
│   │   ├── Character/
│   │   │   ├── PointClickController.cs       # NavMesh movement
│   │   │   ├── CharacterActionController.cs  # Action routing
│   │   │   └── EmotePlayer.cs                # Voice + lip sync
│   │   ├── Systems/
│   │   │   ├── Networking/
│   │   │   │   └── CFConnector.cs            # WebSocket client
│   │   │   └── Camera/
│   │   │       └── CameraStateMachine.cs     # Camera control
│   │   └── Utils/
│   │       └── WavUtility.cs                 # Audio decoding
│   ├── Plugins/
│   │   └── SALSA LipSync/                    # Lip sync
│   └── Scenes/
│       └── Main.unity
└── README.md
```

## Core Components

| Component | Purpose |
|---|---|
| `CFConnector.cs` | WebSocket client, connects to any AI backend |
| `CharacterActionController.cs` | Routes AI commands to character |
| `PointClickController.cs` | NavMesh movement + interaction states |
| `EmotePlayer.cs` | Voice playback with SALSA lip sync |

## Configuration

In the Unity Inspector, configure `CFConnector`:

| Setting | Default | Description |
|---|---|---|
| Host | `localhost:5173` | AI server address |
| Agent Id | `character` | Agent identifier |
| Use Secure | `false` | Use `wss://` |
| Query Token | — | Auth token |

## Debug Controls

Test actions manually while developing:

| Key | Action |
|---|---|
| `1` | Move to location |
| `2` | Sit at chair |
| `3` | Stand up |
| `4` | Examine display |
| `5` | Play arcade |
| `6` | Change camera |
| `7` | Idle |
| `Space` | Stand up |

## Contributing

We welcome contributions:
- Protocol improvements
- New runtime implementations (Unreal, Godot, web)
- AI backend adapters
- Documentation

## License

MIT — Use it however you want.

## Links

- [Treasure Project](https://treasure.lol)

---

*Golem is built by [Treasure](https://treasure.lol), building the future of interactive IP and AI-driven entertainment experiences.*

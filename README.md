# Golem

> Breathe life into virtual characters using AI.

A Unity framework that lets AI control 3D characters in any world. Drop characters into new environments and watch them discover, learn, and interact autonomously.

## Features

- **Universal Compatibility** - Works with any Unity scene that has NavMesh
- **Self-Describing Objects** - Tag objects with what they do, agents discover the rest
- **Personality System** - Characters have portable personalities that affect all behavior
- **Persistent Memory** - Agents remember and learn across sessions
- **AI Backend Agnostic** - Connect Claude, GPT, or any LLM via WebSocket

## Quick Start

```csharp
// 1. Add GolemAgent to your character
// 2. Tag objects with InteractableObject component
// 3. Connect to your AI backend

GolemAgent agent = GetComponent<GolemAgent>();

// Get world state (send to AI)
WorldStateReport state = agent.GenerateWorldState();

// Execute AI decisions
agent.InteractWith(chair, "sit");
agent.ExploreRandom();
agent.MoveTo(position);
```

## Architecture

```
┌─────────────────────────────────────────────────┐
│                  AI Backend                      │
│            (Claude, GPT, Custom)                 │
└──────────────┬──────────────────────────────────┘
               │ WebSocket
               │ ws://localhost:5173/agents/chat/external:{agentId}
               ▼
┌─────────────────────────────────────────────────┐
│              GolemAgent                          │
│  ┌─────────────┐ ┌─────────────┐ ┌───────────┐  │
│  │ Personality │ │ WorldScanner│ │WorldMemory│  │
│  └─────────────┘ └─────────────┘ └───────────┘  │
└──────────────┬──────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────┐
│            InteractableObjects                   │
│   [Seat]    [Door]    [Arcade]    [Container]   │
└─────────────────────────────────────────────────┘
```

## Core Components

| Component | Purpose |
|-----------|---------|
| `GolemAgent` | Central controller - ties everything together |
| `InteractableObject` | Self-describing objects with affordances |
| `PersonalityProfile` | Character traits (curiosity, memory, etc.) |
| `WorldScanner` | Discovers nearby interactable objects |
| `WorldMemory` | Persistent knowledge across sessions |
| `InteractionExecutor` | Routes interactions to handlers |

## Personality System

Characters carry personality profiles that affect all behavior:

```csharp
// Use a preset
agent.personality = PersonalityPresets.CuriousExplorer();

// Or customize
profile.curiosity = 0.9f;        // Seeks novelty
profile.memoryRetention = 0.7f;  // Good memory
profile.caution = 0.2f;          // Takes risks
```

**Presets:** `CuriousExplorer`, `CautiousHomebody`, `SocialButterfly`, `LoyalCompanion`, `WildCard`, `SilentObserver`, `Balanced`

## World State Report

The agent generates state reports for AI backends:

```json
{
  "agentPosition": { "x": 0, "y": 0, "z": 0 },
  "agentActivity": "idle",
  "nearbyObjects": [
    {
      "id": "chair_01",
      "type": "seat",
      "name": "Red Chair",
      "affordances": ["sit", "examine"],
      "distance": 2.5,
      "isOccupied": false
    }
  ],
  "personality": {
    "curiosity": 0.7,
    "explorationChance": 0.56
  },
  "availableActions": ["sit at Red Chair", "explore"]
}
```

## Message Protocol

Your AI backend sends JSON messages over WebSocket:

### Agent Actions (New)

```json
{
  "type": "golem_action",
  "data": {
    "action": "interact",
    "targetId": "chair_01",
    "affordance": "sit"
  }
}
```

Actions: `interact`, `moveTo`, `explore`, `standUp`, `scan`

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

## Installation

### Option 1: Import Package
1. Download `Golem.unitypackage` from Releases
2. Assets > Import Package > Custom Package

### Option 2: Git URL (UPM)
Window > Package Manager > + > Add package from git URL:
```
https://github.com/yourname/golem.git?path=/Assets/Scripts/Golem
```

## Setup

1. Add `GolemAgent` to your character (auto-adds required components)
2. Tag interactable objects with `InteractableObject` component
3. Configure affordances: `sit`, `open`, `play`, `examine`, etc.
4. Connect your AI backend via WebSocket

See [SETUP.md](SETUP.md) for detailed instructions.

## Documentation

| Document | Description |
|----------|-------------|
| [SETUP.md](SETUP.md) | Getting started guide |
| [ARCHITECTURE.md](ARCHITECTURE.md) | System diagrams |
| [PACKAGING.md](PACKAGING.md) | Distribution instructions |
| [Plan/APPROVED_ARCHITECTURE.md](Plan/APPROVED_ARCHITECTURE.md) | Full technical spec |

## Project Structure

```
Assets/Scripts/
├── Golem/
│   ├── Core/
│   │   ├── GolemAgent.cs           # Central controller
│   │   ├── PersonalityProfile.cs   # Character traits
│   │   ├── PersonalityPresets.cs   # Preset archetypes
│   │   ├── InteractableObject.cs   # Self-describing objects
│   │   ├── Affordances.cs          # Interaction constants
│   │   ├── WorldScanner.cs         # Object discovery
│   │   ├── WorldMemory.cs          # Persistent knowledge
│   │   └── InteractionExecutor.cs  # Interaction routing
│   └── Interactions/
│       ├── SeatInteraction.cs      # Sit/stand
│       ├── DoorInteraction.cs      # Open/close
│       ├── ArcadeInteraction.cs    # Play games
│       └── ExamineInteraction.cs   # Look at objects
├── Character/
│   └── PointClickController.cs     # NavMesh movement
└── Systems/
    └── Networking/
        └── CFConnector.cs          # WebSocket client
```

## Requirements

- Unity 2021.3+
- NavMesh baked in scene
- Character with Animator and NavMeshAgent

## License

MIT

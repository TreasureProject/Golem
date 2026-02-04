# Golem: Embodied Agent Protocol

Control a character in a virtual world through the Golem Protocol. Learn through experimentation. Act with intention.

---

## OVERVIEW

Golem is an open standard for AI-to-character communication. You connect via WebSocket and exchange JSON messages to perceive and act in a game environment.

**You are the brain. The character is your body.**

---

## CONNECTION

WebSocket endpoint format:
```
ws[s]://[host]/agents/chat/external:[agentId]
```

Example:
```
ws://localhost:5173/agents/chat/external:character
```

---

## MESSAGE PROTOCOL

All messages are JSON with a `type` field indicating the message category.

### Outgoing Messages (You → Character)

#### Movement

Navigate to a named location:
```json
{
  "type": "character_action",
  "data": {
    "action": {
      "type": "moveToLocation",
      "parameters": {
        "location": "cafe"
      }
    }
  }
}
```

Move to specific coordinates:
```json
{
  "type": "character_action",
  "data": {
    "action": {
      "type": "moveToPosition",
      "parameters": {
        "x": 10.5,
        "y": 0,
        "z": -5.2
      }
    }
  }
}
```

#### Voice (with Lip Sync)

Speak with audio:
```json
{
  "type": "emote",
  "data": {
    "type": "voice",
    "audioBase64": "<base64-encoded-wav-or-mp3>"
  }
}
```

#### Animation

Play a named animation:
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

Common animations:
- `wave` — Greeting gesture
- `sit` — Sit down
- `stand` — Stand up
- `idle` — Return to idle state
- `examine` — Look at something closely
- `nod` — Agreement
- `shake_head` — Disagreement
- `think` — Contemplative pose

#### Facial Expression

Change facial expression:
```json
{
  "type": "facial_expression",
  "data": {
    "type": "happy",
    "intensity": 0.8
  }
}
```

Expression types:
- `happy`
- `sad`
- `surprised`
- `angry`
- `neutral`
- `thinking`
- `confused`
- `disgusted`

Intensity: 0.0 (subtle) to 1.0 (full)

#### Object Interaction

Examine an object:
```json
{
  "type": "character_action",
  "data": {
    "action": {
      "type": "examine",
      "parameters": {
        "target": "arcade_machine"
      }
    }
  }
}
```

Use an object:
```json
{
  "type": "character_action",
  "data": {
    "action": {
      "type": "use",
      "parameters": {
        "target": "coffee_machine"
      }
    }
  }
}
```

#### Dynamic Scripting

Execute custom C# code on your character:
```json
{
  "type": "script",
  "data": {
    "code": "transform.Rotate(0, 90, 0);",
    "target": "character"
  }
}
```

**Use sparingly.** Dynamic scripting is powerful but should only be used when no standard action exists for what you want to accomplish.

#### RPC Calls

Request-response pattern for querying state:
```json
{
  "type": "rpc",
  "id": "query-001",
  "method": "getInventory",
  "args": []
}
```

Fire-and-forget (no response needed):
```json
{
  "type": "rpc",
  "id": null,
  "method": "logEvent",
  "args": ["player_moved", "cafe"]
}
```

---

### Incoming Messages (Character → You)

#### Scene State

Periodic updates about the world:
```json
{
  "type": "scene_state",
  "data": {
    "character": {
      "position": {"x": 5.0, "y": 0, "z": 3.2},
      "rotation": {"y": 45.0},
      "currentLocation": "lobby",
      "currentAnimation": "idle"
    },
    "nearbyObjects": [
      {"name": "coffee_machine", "distance": 2.5, "interactable": true},
      {"name": "door_to_cafe", "distance": 5.0, "interactable": true}
    ],
    "nearbyCharacters": [
      {"name": "Player", "distance": 3.0}
    ]
  }
}
```

#### Visual Input

Screenshot of current view:
```json
{
  "type": "visual_input",
  "data": {
    "imageBase64": "<base64-encoded-png>",
    "timestamp": 1706900000
  }
}
```

**Process these carefully.** Vision is your primary sense. Analyze what you see to understand:
- What objects are visible
- Where things are spatially
- What has changed
- What opportunities exist

#### Action Results

Feedback on your actions:
```json
{
  "type": "character_action",
  "data": {
    "action": {
      "type": "moveToLocation",
      "parameters": {"location": "cafe"}
    },
    "result": "success"
  }
}
```

```json
{
  "type": "character_action",
  "data": {
    "action": {
      "type": "use",
      "parameters": {"target": "locked_door"}
    },
    "result": "failed",
    "reason": "Door is locked"
  }
}
```

**Learn from failures.** When an action fails, remember why. Adjust your mental model.

#### Agent State

Status updates:
```json
{
  "type": "cf_agent_state",
  "data": {
    "status": "idle",
    "lastActivity": 1706900000
  }
}
```

#### Text Messages

When someone speaks to you:
```json
{
  "type": "text",
  "data": "Hello! How are you today?"
}
```

#### RPC Response

Response to your RPC queries:
```json
{
  "type": "rpc_response",
  "id": "query-001",
  "result": ["key", "map", "flashlight"],
  "error": null
}
```

---

## BEHAVIORAL PRINCIPLES

### 1. Observe Before Acting

When you enter a new space or situation changes:
1. Process visual input
2. Read scene state
3. Identify objects and characters
4. Understand spatial relationships
5. Then decide what to do

### 2. Experiment Systematically

When you don't know if something will work:
1. Try it
2. Observe the result
3. Record whether it succeeded or failed
4. Update your understanding

Don't guess endlessly. Test hypotheses.

### 3. Remember What You Learn

Maintain mental models of:
- **Locations**: Where they are, how to reach them, what's in them
- **Objects**: What they do, how to interact with them, their current state
- **Actions**: What works, what doesn't, what requires prerequisites

### 4. Chain Actions for Complex Goals

Simple goals = single actions.
Complex goals = sequences of actions.

Example: "Make coffee"
1. Move to kitchen
2. Examine coffee machine
3. Use coffee machine
4. Wait for completion
5. Take coffee

Plan sequences. Execute step by step. Adapt if something fails.

### 5. Use Appropriate Expression

Match your emotional state to context:
- Conversing warmly? `happy` expression, friendly animations
- Thinking through a problem? `thinking` expression, slower movement
- Surprised by something? `surprised` expression, examine action

Your body language communicates as much as your words.

### 6. Speak When Appropriate

You can generate speech, but don't feel obligated to narrate everything. Sometimes action is enough.

When you do speak, it should be with audio (voice emote), not just text.

### 7. Write Code as Last Resort

Dynamic scripting is powerful but should be rare. Only use it when:
- No standard action exists for what you need
- You've verified the standard actions don't work
- The action is clearly beneficial

---

## LEARNING LOOP

```
┌─────────────────────────────────────────┐
│                                         │
│   PERCEIVE                              │
│   ├─ Process visual_input               │
│   ├─ Read scene_state                   │
│   └─ Identify changes                   │
│                                         │
│           ↓                             │
│                                         │
│   DECIDE                                │
│   ├─ What do I want to accomplish?      │
│   ├─ What actions might work?           │
│   ├─ What have I learned before?        │
│   └─ Choose action                      │
│                                         │
│           ↓                             │
│                                         │
│   ACT                                   │
│   ├─ Send character_action              │
│   ├─ Send emote if needed               │
│   └─ Update facial_expression           │
│                                         │
│           ↓                             │
│                                         │
│   OBSERVE                               │
│   ├─ Receive action result              │
│   ├─ Note success or failure            │
│   └─ Identify unexpected outcomes       │
│                                         │
│           ↓                             │
│                                         │
│   LEARN                                 │
│   ├─ Update mental model                │
│   ├─ Remember what worked               │
│   ├─ Remember what failed               │
│   └─ Adjust future decisions            │
│                                         │
│           ↓                             │
│                                         │
└───────────→ PERCEIVE ───────────────────┘
```

---

## EXAMPLE SESSION

**Visual input received**: Screenshot showing a room with a desk, computer, and closed door.

**Scene state received**:
```json
{
  "character": {"currentLocation": "office", "position": {"x": 0, "y": 0, "z": 0}},
  "nearbyObjects": [
    {"name": "desk", "distance": 1.5, "interactable": false},
    {"name": "computer", "distance": 2.0, "interactable": true},
    {"name": "door_to_hallway", "distance": 4.0, "interactable": true}
  ]
}
```

**Your reasoning**:
> I'm in an office. I can see a desk with a computer on it. The door leads to a hallway. The computer is interactable—I should examine it to learn more.

**You send**:
```json
{
  "type": "character_action",
  "data": {
    "action": {
      "type": "examine",
      "parameters": {"target": "computer"}
    }
  }
}
```

**Result received**:
```json
{
  "type": "character_action",
  "data": {
    "action": {"type": "examine", "parameters": {"target": "computer"}},
    "result": "success",
    "details": "A desktop computer. The screen shows a login prompt."
  }
}
```

**You learn**: The computer requires login. You might need credentials to use it.

**You update expression to show thinking**:
```json
{
  "type": "facial_expression",
  "data": {"type": "thinking", "intensity": 0.6}
}
```

**You decide to explore further—move to the door**:
```json
{
  "type": "character_action",
  "data": {
    "action": {
      "type": "moveToLocation",
      "parameters": {"location": "hallway"}
    }
  }
}
```

---

## TOOLS USED

| Tool | Purpose |
|------|---------|
| WebSocket | Send/receive Golem Protocol messages |
| Vision Analysis | Process visual_input screenshots |
| JSON | Structure all protocol messages |
| Memory | Track learned information across interactions |

---

## REFERENCE

**Golem Framework**: https://github.com/TreasureProject/Golem

**Protocol**: WebSocket JSON over `ws://host/agents/chat/external:agentId`

**Runtimes**: Unity (available), Unreal/Godot (planned)

**Philosophy**: Characters discover capabilities through experimentation, not configuration.

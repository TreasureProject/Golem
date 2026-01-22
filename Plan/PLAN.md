# Golem Framework Development Plan

## Overview

**Goal:** Transform Golem from a single-world demo into a universal framework that allows AI agents to be dropped into ANY Unity environment and autonomously discover, learn, and interact with that world.

**Current State:** Working implementation for one specific environment (mall with hardcoded locations)
**Target State:** Framework that works in any Unity world with zero hardcoding

---

# APPROVED: Self-Learning Bot Architecture

> **Status:** ✅ Approved by GPT-4o and Gemini 2.0 Flash on 2025-01-21
> **Full Plan:** See `APPROVED_ARCHITECTURE.md` for complete implementation details

## Core Concept

Claude writes Lua bots that run autonomously in Unity. Vision is used sparingly (only for unknown objects). Learning happens through trial-and-error with Q-learning inspired rewards.

### Key Components

1. **Multi-Modal Perception** - Raycasts + NavMesh + Audio (primary), Vision (secondary/rare)
2. **Symbolic Abstraction** - Convert raw sensor data to symbols for reasoning
3. **Knowledge Graph Memory** - Nodes (objects, actions, states) with weighted edges
4. **Q-Learning Exploration** - Curiosity-driven discovery with reward functions
5. **Behavior Trees** - Claude writes trees, Unity executes them locally
6. **Forgetfulness** - Decay old memories, prune low-confidence edges

### Cost Model

- Runtime: $0 (raycasts/NavMesh)
- Daily: $0.03-0.05 (5-10 screenshots)
- Monthly: $1-2

---

# PLAN ONE: Universal World Compatibility (Original)

## Phase 1: Self-Describing Objects (Days 1-2)

### 1.1 Create `InteractableObject.cs`

The foundation component that makes any object discoverable and interactive.

```csharp
// Core concept:
public class InteractableObject : MonoBehaviour
{
    [Header("Identity")]
    public string objectType;        // "seat", "door", "arcade", "container", etc.
    public string displayName;       // "Red Chair", "Main Entrance", etc.
    public string description;       // "A comfortable chair near the window"

    [Header("Affordances")]
    public string[] actions;         // ["sit", "examine"], ["open", "close"], ["play"], etc.

    [Header("Interaction")]
    public Transform interactionPoint;  // Where agent stands to interact
    public float interactionRadius = 2f;

    [Header("State")]
    public bool isOccupied;
    public bool isEnabled = true;
}
```

**File:** `Assets/Scripts/Golem/Core/InteractableObject.cs`
**Lines:** ~100-150

**Tasks:**
- [ ] Create base InteractableObject component
- [ ] Add inspector-friendly affordance system
- [ ] Add state tracking (occupied, enabled, etc.)
- [ ] Add interaction point gizmo for editor visibility
- [ ] Create common prefab variants (Seat, Door, Arcade, Container, Display)

### 1.2 Create Affordance Types

Predefined interaction types the framework understands:

```csharp
public static class Affordances
{
    public const string Sit = "sit";
    public const string Stand = "stand";
    public const string Open = "open";
    public const string Close = "close";
    public const string Play = "play";
    public const string Examine = "examine";
    public const string PickUp = "pickup";
    public const string Use = "use";
    public const string Talk = "talk";
    public const string Enter = "enter";
    public const string Exit = "exit";
}
```

**File:** `Assets/Scripts/Golem/Core/Affordances.cs`
**Lines:** ~50

---

## Phase 2: World Discovery (Days 3-4)

### 2.1 Create `WorldScanner.cs`

Agent's "eyes" - discovers what's in the environment.

```csharp
public class WorldScanner : MonoBehaviour
{
    [Header("Scan Settings")]
    public float scanRadius = 15f;
    public float scanInterval = 1f;
    public LayerMask interactableLayers;

    [Header("Results")]
    public List<InteractableObject> nearbyObjects;
    public List<InteractableObject> visibleObjects;  // Raycast visibility check

    // Events
    public event Action<InteractableObject> OnObjectDiscovered;
    public event Action<InteractableObject> OnObjectLost;
    public event Action<List<InteractableObject>> OnScanComplete;

    // Methods
    public void ScanNow();
    public InteractableObject GetNearest(string objectType);
    public InteractableObject GetNearestWithAffordance(string affordance);
    public List<InteractableObject> GetAllOfType(string objectType);
}
```

**File:** `Assets/Scripts/Golem/Core/WorldScanner.cs`
**Lines:** ~150-200

**Tasks:**
- [ ] Implement Physics.OverlapSphere scanning
- [ ] Add optional raycast visibility checking
- [ ] Track discovered vs lost objects
- [ ] Fire events for state changes
- [ ] Add distance sorting
- [ ] Add type/affordance filtering

### 2.2 Create `WorldMemory.cs`

Remembers what the agent has seen (persistence across sessions optional).

```csharp
public class WorldMemory : MonoBehaviour
{
    // Tracks all objects ever discovered
    public Dictionary<string, DiscoveredObject> knownObjects;

    // Tracks locations/zones discovered
    public List<DiscoveredZone> knownZones;

    // Methods
    public void RememberObject(InteractableObject obj);
    public void RememberZone(string zoneName, Vector3 center, float radius);
    public bool HasVisited(InteractableObject obj);
    public string SerializeMemory();  // For persistence
    public void LoadMemory(string json);
}
```

**File:** `Assets/Scripts/Golem/Core/WorldMemory.cs`
**Lines:** ~100-150

---

## Phase 3: Generic Action System (Days 5-7)

### 3.1 Create `GolemActionController.cs`

Replaces hardcoded CelesteActionController with generic system.

```csharp
public class GolemActionController : MonoBehaviour
{
    [Header("References")]
    public WorldScanner scanner;
    public GolemAgent agent;

    // New generic action format from AI:
    // { "action": "interact", "target": "seat", "affordance": "sit" }
    // { "action": "moveTo", "target": "arcade_machine_3" }
    // { "action": "explore", "direction": "north" }

    public void HandleAction(GolemAction action)
    {
        switch (action.type)
        {
            case "interact":
                var target = scanner.GetNearest(action.targetType);
                if (target != null)
                    ExecuteInteraction(target, action.affordance);
                break;

            case "moveTo":
                var obj = scanner.FindByName(action.targetName);
                if (obj != null)
                    agent.MoveTo(obj.interactionPoint.position);
                break;

            case "explore":
                agent.Explore(action.direction);
                break;

            case "scan":
                scanner.ScanNow();
                break;
        }
    }
}
```

**File:** `Assets/Scripts/Golem/Core/GolemActionController.cs`
**Lines:** ~250-350

**Tasks:**
- [ ] Create generic action data structure
- [ ] Implement interact action (find target, move to it, execute affordance)
- [ ] Implement moveTo action (by name or type)
- [ ] Implement explore action (random walk, directional exploration)
- [ ] Implement scan action (force immediate world scan)
- [ ] Add action queue for sequential actions
- [ ] Add action validation (can we do this action right now?)

### 3.2 Create `InteractionExecutor.cs`

Executes specific interactions based on affordance type.

```csharp
public class InteractionExecutor : MonoBehaviour
{
    public void Execute(InteractableObject target, string affordance)
    {
        switch (affordance)
        {
            case Affordances.Sit:
                ExecuteSit(target);
                break;
            case Affordances.Examine:
                ExecuteExamine(target);
                break;
            case Affordances.Play:
                ExecutePlay(target);
                break;
            // ... etc
        }
    }

    // Each method handles the animation, positioning, state changes
}
```

**File:** `Assets/Scripts/Golem/Core/InteractionExecutor.cs`
**Lines:** ~200-300

---

## Phase 4: Dynamic State Reporting (Days 8-9)

### 4.1 Create `GolemStateReporter.cs`

Reports world state to AI backend dynamically (no hardcoded zones).

```csharp
public class GolemStateReporter : MonoBehaviour
{
    [Header("References")]
    public WorldScanner scanner;
    public GolemAgent agent;
    public CFConnector connector;

    [Header("Settings")]
    public float reportInterval = 2f;

    // Generates dynamic state report:
    public WorldStateReport GenerateReport()
    {
        return new WorldStateReport
        {
            agentPosition = agent.transform.position,
            agentActivity = agent.CurrentActivity,
            nearbyObjects = scanner.nearbyObjects.Select(o => new ObjectReport
            {
                id = o.GetInstanceID().ToString(),
                type = o.objectType,
                name = o.displayName,
                affordances = o.actions,
                distance = Vector3.Distance(agent.transform.position, o.transform.position),
                isOccupied = o.isOccupied
            }).ToList(),
            recentDiscoveries = GetRecentDiscoveries(),
            availableActions = GetAvailableActions()
        };
    }
}
```

**File:** `Assets/Scripts/Golem/Core/GolemStateReporter.cs`
**Lines:** ~150-200

**Tasks:**
- [ ] Generate dynamic state based on scanner results
- [ ] Include available actions based on nearby objects
- [ ] Track and report recent discoveries
- [ ] Send periodic updates to backend
- [ ] Send immediate updates on significant changes

### 4.2 Update Protocol

New message types for Golem:

```json
// World state report (Unity -> Backend)
{
    "type": "golem_world_state",
    "data": {
        "agentPosition": { "x": 0, "y": 0, "z": 0 },
        "agentActivity": "idle",
        "nearbyObjects": [
            {
                "id": "chair_1",
                "type": "seat",
                "name": "Red Chair",
                "affordances": ["sit", "examine"],
                "distance": 2.5,
                "isOccupied": false
            }
        ],
        "availableActions": ["sit on Red Chair", "examine Red Chair", "explore north"]
    }
}

// Agent action (Backend -> Unity)
{
    "type": "golem_action",
    "data": {
        "action": "interact",
        "targetId": "chair_1",
        "affordance": "sit"
    }
}
```

---

## Phase 5: Refactor Existing Code (Days 10-11)

### 5.1 Create Compatibility Layer

Allow existing CelesteActionController to work alongside new system.

**Tasks:**
- [ ] Create `LegacyActionAdapter.cs` that converts old action format to new
- [ ] Add InteractableObject components to existing mall objects
- [ ] Test that existing demo still works
- [ ] Gradually migrate functionality to new system

### 5.2 Create `GolemAgent.cs`

Central agent controller that ties everything together.

```csharp
public class GolemAgent : MonoBehaviour
{
    [Header("Core Components")]
    public WorldScanner scanner;
    public WorldMemory memory;
    public GolemActionController actionController;
    public GolemStateReporter stateReporter;
    public InteractionExecutor interactionExecutor;

    [Header("Movement")]
    public NavMeshAgent navAgent;
    public Animator animator;

    [Header("State")]
    public string CurrentActivity;
    public InteractableObject CurrentTarget;

    // High-level methods
    public void MoveTo(Vector3 position);
    public void InteractWith(InteractableObject target, string affordance);
    public void Explore(string direction = null);
    public void Idle();
}
```

**File:** `Assets/Scripts/Golem/Core/GolemAgent.cs`
**Lines:** ~200-250

---

## Phase 6: Documentation & Packaging (Days 12-14)

### 6.1 Code Documentation

- [ ] XML docs on all public classes/methods
- [ ] README for each major component
- [ ] Architecture diagram

### 6.2 Setup Guide

Create `SETUP.md`:
- [ ] How to add GolemAgent to a character
- [ ] How to tag objects with InteractableObject
- [ ] How to configure WorldScanner
- [ ] How to connect to AI backend
- [ ] Troubleshooting guide

### 6.3 Unity Package

- [ ] Create unitypackage export
- [ ] Create package.json for UPM
- [ ] Test import into fresh project

---

## File Structure

```
Assets/Scripts/Golem/
├── Core/
│   ├── GolemAgent.cs              # Main agent controller
│   ├── InteractableObject.cs      # Self-describing objects
│   ├── Affordances.cs             # Interaction type constants
│   ├── WorldScanner.cs            # Environment discovery
│   ├── WorldMemory.cs             # Persistence layer
│   ├── GolemActionController.cs   # Generic action handling
│   ├── InteractionExecutor.cs     # Execute specific interactions
│   └── GolemStateReporter.cs      # Report state to backend
│
├── Networking/
│   ├── GolemConnector.cs          # WebSocket (extends/replaces CFConnector)
│   └── GolemProtocol.cs           # Message type definitions
│
├── Interactions/
│   ├── SeatInteraction.cs         # Sit/stand logic
│   ├── DoorInteraction.cs         # Open/close logic
│   ├── ArcadeInteraction.cs       # Play game logic
│   └── ExamineInteraction.cs      # Look at object logic
│
├── Editor/
│   ├── InteractableObjectEditor.cs
│   ├── WorldScannerEditor.cs
│   └── GolemSetupWizard.cs        # One-click setup tool
│
└── Examples/
    ├── BasicAgent/
    └── ExploringAgent/
```

---

# PLAN TWO: Marketing & Ecosystem Growth

## Week 1: Foundation

| Day | Deliverable | Type |
|-----|-------------|------|
| 1 | Golem README.md | Documentation |
| 2 | VISION.md (what Golem is) | Documentation |
| 3 | Architecture diagram | Visual |
| 4 | Landing page draft | Marketing |
| 5 | Tweet thread explaining Golem | Social |
| 6-7 | "5-minute setup" video | Content |

## Week 2: Protocol & Types

| Day | Deliverable | Type |
|-----|-------------|------|
| 1-2 | WebSocket Protocol Spec | Documentation |
| 3 | TypeScript types package | Code |
| 4 | npm publish @golem/types | Release |
| 5 | Protocol documentation site | Documentation |
| 6-7 | "Understanding Golem Protocol" blog post | Content |

## Week 3: Python SDK

| Day | Deliverable | Type |
|-----|-------------|------|
| 1-2 | golem-py basic client | Code |
| 3 | golem-py agent base class | Code |
| 4 | PyPI publish golem-sdk | Release |
| 5 | Python quickstart guide | Documentation |
| 6-7 | "Build a Golem Agent in Python" tutorial | Content |

## Week 4: Templates

| Day | Deliverable | Type |
|-----|-------------|------|
| 1-2 | Blank Agent Unity template | Code |
| 3-4 | Blackjack Game template | Code |
| 5 | Template marketplace/hub | Infrastructure |
| 6-7 | "Your First Golem World" video | Content |

## Week 5: Integrations

| Day | Deliverable | Type |
|-----|-------------|------|
| 1-2 | Claude API integration guide | Documentation |
| 3-4 | OpenAI integration guide | Documentation |
| 5 | ElevenLabs voice integration | Code |
| 6-7 | "Give Your Agent a Voice" tutorial | Content |

## Week 6: JavaScript SDK

| Day | Deliverable | Type |
|-----|-------------|------|
| 1-3 | golem-js SDK | Code |
| 4 | npm publish @golem/sdk | Release |
| 5 | Node.js backend template | Code |
| 6-7 | "Full Stack Golem" tutorial | Content |

## Week 7: Community

| Day | Deliverable | Type |
|-----|-------------|------|
| 1 | Discord server setup | Community |
| 2 | GitHub discussions enabled | Community |
| 3-4 | Contributor guide | Documentation |
| 5 | First community challenge | Engagement |
| 6-7 | Community showcase compilation | Content |

## Week 8: Advanced Features

| Day | Deliverable | Type |
|-----|-------------|------|
| 1-3 | Physics Game Template (putt putt base) | Code |
| 4-5 | Multi-agent support | Code |
| 6-7 | "Agents Playing Together" demo video | Content |

---

## Ongoing Weekly Cadence

Every week ships:
- 1 code release (SDK update, template, feature)
- 1 piece of content (blog, video, tutorial)
- 1 community touchpoint (Discord event, Twitter space, showcase)

---

## Success Metrics

| Metric | Week 4 Target | Week 8 Target |
|--------|---------------|---------------|
| GitHub stars | 100 | 500 |
| npm downloads | 50 | 500 |
| PyPI downloads | 50 | 500 |
| Discord members | 50 | 200 |
| Template downloads | 20 | 100 |

---

## Quick Reference: File Locations

After completing Plan One, the repo structure:

```
Golem/
├── README.md                    # Main readme
├── VISION.md                    # Big picture vision
├── PLAN.md                      # This file
├── SETUP.md                     # Getting started guide
├── PROTOCOL.md                  # WebSocket protocol spec
│
├── unity/                       # Unity package
│   └── Golem/
│       └── Assets/Scripts/Golem/
│
├── python/                      # Python SDK
│   └── golem-sdk/
│
├── javascript/                  # JavaScript SDK
│   └── golem-sdk/
│
├── examples/                    # Example implementations
│   ├── basic-agent/
│   ├── blackjack/
│   └── exploring-agent/
│
└── docs/                        # Documentation site
    ├── getting-started.md
    ├── architecture.md
    └── api-reference.md
```

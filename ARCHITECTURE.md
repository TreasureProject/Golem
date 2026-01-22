# Golem Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              AI BACKEND                                      │
│                     (Claude, GPT, Custom Agent)                              │
│                                                                              │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐          │
│  │ Decision Making │    │  Lua Bot Writer │    │ Behavior Trees  │          │
│  └────────┬────────┘    └────────┬────────┘    └────────┬────────┘          │
│           │                      │                      │                    │
└───────────┼──────────────────────┼──────────────────────┼────────────────────┘
            │                      │                      │
            │    WebSocket / HTTP  │                      │
            ▼                      ▼                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              UNITY RUNTIME                                   │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                         GolemAgent                                   │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐   │    │
│  │  │ Personality  │  │    State     │  │      Public API          │   │    │
│  │  │   Profile    │  │   Tracking   │  │  MoveTo(), InteractWith()│   │    │
│  │  └──────────────┘  └──────────────┘  │  ExploreRandom(), etc.   │   │    │
│  │                                       └──────────────────────────┘   │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│           │                      │                      │                    │
│           ▼                      ▼                      ▼                    │
│  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐          │
│  │  WorldScanner   │    │  WorldMemory    │    │  Interaction    │          │
│  │                 │    │                 │    │   Executor      │          │
│  │ - Discovers     │    │ - Remembers     │    │                 │          │
│  │   objects       │    │   objects       │    │ - Routes to     │          │
│  │ - Tracks range  │    │ - Learns        │    │   handlers      │          │
│  │ - Events        │    │   affordances   │    │ - Monitors      │          │
│  │                 │    │ - Persistence   │    │   completion    │          │
│  └────────┬────────┘    └────────┬────────┘    └────────┬────────┘          │
│           │                      │                      │                    │
│           ▼                      ▼                      ▼                    │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                     InteractableObject(s)                            │    │
│  │                                                                      │    │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐     │    │
│  │  │   Seat     │  │   Door     │  │  Arcade    │  │ Container  │     │    │
│  │  │            │  │            │  │            │  │            │     │    │
│  │  │ [sit]      │  │ [open]     │  │ [play]     │  │ [open]     │     │    │
│  │  │ [examine]  │  │ [close]    │  │ [examine]  │  │ [close]    │     │    │
│  │  └────────────┘  └────────────┘  └────────────┘  └────────────┘     │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                      Interaction Handlers                            │    │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐     │    │
│  │  │   Seat     │  │   Door     │  │  Arcade    │  │  Examine   │     │    │
│  │  │ Interaction│  │ Interaction│  │ Interaction│  │ Interaction│     │    │
│  │  └────────────┘  └────────────┘  └────────────┘  └────────────┘     │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                    PointClickController                              │    │
│  │          (NavMesh Movement, Animation, Legacy Support)               │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Data Flow

```
┌────────────────────────────────────────────────────────────────────────────┐
│                           PERCEPTION FLOW                                   │
│                                                                             │
│   Unity World                                                               │
│       │                                                                     │
│       ▼                                                                     │
│   Physics.OverlapSphere (WorldScanner)                                      │
│       │                                                                     │
│       ▼                                                                     │
│   InteractableObject[] discovered                                           │
│       │                                                                     │
│       ├────────────────────────┐                                            │
│       ▼                        ▼                                            │
│   WorldMemory             WorldStateReport                                  │
│   (persist)               (to AI Backend)                                   │
│                                                                             │
└────────────────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────────────────┐
│                            ACTION FLOW                                      │
│                                                                             │
│   AI Backend Decision                                                       │
│       │                                                                     │
│       ▼                                                                     │
│   GolemAgent.InteractWith(target, affordance)                              │
│       │                                                                     │
│       ▼                                                                     │
│   InteractionExecutor.Execute()                                             │
│       │                                                                     │
│       ├─────────────────┬─────────────────┐                                 │
│       ▼                 ▼                 ▼                                 │
│   Custom Handler    Built-in         PointClickController                   │
│   (if registered)   Affordances      (movement, animation)                  │
│       │                 │                 │                                 │
│       └─────────────────┴─────────────────┘                                 │
│                         │                                                   │
│                         ▼                                                   │
│               Interaction Complete                                          │
│                         │                                                   │
│                         ▼                                                   │
│               WorldMemory.RecordAffordanceAttempt(success/fail)            │
│                                                                             │
└────────────────────────────────────────────────────────────────────────────┘
```

## Personality System

```
┌────────────────────────────────────────────────────────────────────────────┐
│                        PERSONALITY PROFILE                                  │
│                     (ScriptableObject, Portable)                            │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │  Core Traits (0.0 - 1.0)                                            │  │
│   │                                                                     │  │
│   │  curiosity ────────────────► Exploration radius, novelty seeking    │  │
│   │  memoryRetention ──────────► Memory decay rate, prune threshold     │  │
│   │  sociability ──────────────► NPC interaction priority               │  │
│   │  caution ──────────────────► Approach distance, risk assessment     │  │
│   │  routinePreference ────────► Repeat known behaviors                 │  │
│   │  adaptability ─────────────► Learning speed                         │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │  Derived Values (Computed)                                          │  │
│   │                                                                     │  │
│   │  ExplorationChance ────────► curiosity * (1 - caution * 0.5)       │  │
│   │  MemoryHalfLifeDays ───────► 1 + 29 * memoryRetention (1-30 days)  │  │
│   │  MaxMemoryObjects ─────────► 100 + 9900 * memoryRetention          │  │
│   │  MemoryPruneThreshold ─────► 0.3 - 0.25 * memoryRetention          │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
└────────────────────────────────────────────────────────────────────────────┘

                              PRESETS
    ┌──────────────┬──────────────┬──────────────┬──────────────┐
    │   Curious    │   Cautious   │   Social     │    Wild      │
    │   Explorer   │   Homebody   │   Butterfly  │    Card      │
    ├──────────────┼──────────────┼──────────────┼──────────────┤
    │ curiosity:   │ curiosity:   │ curiosity:   │ curiosity:   │
    │     0.9      │     0.2      │     0.6      │     0.85     │
    │ caution:     │ caution:     │ caution:     │ caution:     │
    │     0.2      │     0.9      │     0.4      │     0.1      │
    │ memory:      │ memory:      │ memory:      │ memory:      │
    │     0.7      │     0.8      │     0.6      │     0.4      │
    └──────────────┴──────────────┴──────────────┴──────────────┘
```

## Memory System

```
┌────────────────────────────────────────────────────────────────────────────┐
│                           WORLD MEMORY                                      │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │  Object Memory                                                      │  │
│   │                                                                     │  │
│   │  objectId ─────────► Unique identifier                              │  │
│   │  objectType ───────► "seat", "door", "arcade"                       │  │
│   │  affordances ──────► ["sit", "examine"]                             │  │
│   │  lastPosition ─────► Vector3                                        │  │
│   │  timesEncountered ─► 5                                              │  │
│   │  confidence ───────► 0.85 (decays over time)                        │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │  Affordance Memory (Learning)                                       │  │
│   │                                                                     │  │
│   │  objectType:affordance ─► "seat:sit"                                │  │
│   │  totalAttempts ────────► 10                                         │  │
│   │  successfulAttempts ───► 9                                          │  │
│   │  successRate ──────────► 0.9                                        │  │
│   │  confidence ───────────► 0.95                                       │  │
│   │                                                                     │  │
│   │  Learning Rule:                                                     │  │
│   │    Success: confidence += 0.1 (max 1.0)                             │  │
│   │    Failure: confidence -= 0.2 (min 0.0)                             │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │  Zone Memory                                                        │  │
│   │                                                                     │  │
│   │  zoneName ────────────► "cafe", "arcade_area"                       │  │
│   │  center ──────────────► Vector3                                     │  │
│   │  visitCount ──────────► 12                                          │  │
│   │  objectsDiscovered ───► ["Chair_01", "Table_02"]                    │  │
│   │  explorationScore ────► Computed (higher = less explored)           │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐  │
│   │  Persistence                                                        │  │
│   │                                                                     │  │
│   │  Save Path: {persistentDataPath}/Golem/{agentId}_memory.json        │  │
│   │  Auto-save: Every 60 seconds                                        │  │
│   │  Auto-load: On Awake                                                │  │
│   │  Decay: Applied on load based on real time elapsed                  │  │
│   └─────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
└────────────────────────────────────────────────────────────────────────────┘
```

## File Structure

```
Assets/Scripts/Golem/
├── Core/
│   ├── GolemAgent.cs              # Central controller, entry point
│   ├── PersonalityProfile.cs      # ScriptableObject for traits
│   ├── PersonalityPresets.cs      # Factory for common archetypes
│   ├── InteractableObject.cs      # Self-describing objects
│   ├── Affordances.cs             # Interaction type constants
│   ├── WorldScanner.cs            # Object discovery
│   ├── WorldMemory.cs             # Persistent knowledge
│   ├── InteractionExecutor.cs     # Routes interactions
│   ├── GolemActionController.cs   # WebSocket command handler
│   └── GolemStateReporter.cs      # Reports to AI backend
│
├── Interactions/
│   ├── SeatInteraction.cs         # Sit/stand handler
│   ├── DoorInteraction.cs         # Open/close handler
│   ├── ArcadeInteraction.cs       # Play handler
│   └── ExamineInteraction.cs      # Look/examine handler
│
└── Networking/
    └── (CFConnector.cs)           # WebSocket client (existing)
```

## Integration Points

### For AI Backend Developers

```
WorldStateReport (JSON from Unity)
├── agentPosition: Vector3
├── agentRotation: Vector3
├── agentActivity: string ("idle", "walking", "sitting", etc.)
├── isInteracting: bool
├── nearbyObjects: ObjectReport[]
│   ├── id: string
│   ├── type: string
│   ├── name: string
│   ├── affordances: string[]
│   ├── distance: float
│   └── isOccupied: bool
├── availableActions: string[]
├── personality: PersonalityReport
│   ├── curiosity: float
│   ├── memoryRetention: float
│   ├── sociability: float
│   ├── caution: float
│   ├── routinePreference: float
│   ├── adaptability: float
│   └── explorationChance: float
└── memoryStats: MemoryStats
    ├── knownObjectCount: int
    ├── learnedAffordanceCount: int
    └── visitedZoneCount: int
```

### Commands (Backend to Unity)

```
{ "action": "interact", "targetId": "chair_01", "affordance": "sit" }
{ "action": "moveTo", "position": { "x": 10, "y": 0, "z": 5 } }
{ "action": "explore" }
{ "action": "standUp" }
{ "action": "scan" }
```

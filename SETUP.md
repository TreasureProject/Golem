# Golem Setup Guide

This guide will walk you through setting up Golem in your Unity project.

## Prerequisites

- Unity 2021.3 or later
- NavMesh baked in your scene
- Characters with Animator and NavMeshAgent components

## Quick Start (5 Minutes)

### Step 1: Add GolemAgent to Your Character

1. Select your character GameObject in the Hierarchy
2. Click **Add Component** > search for **GolemAgent**
3. The following components will be auto-added if missing:
   - `WorldScanner` - Discovers nearby objects
   - `WorldMemory` - Remembers what the agent learns
   - `NavMeshAgent` - Required for pathfinding

### Step 2: Tag Your Interactable Objects

For each object you want the agent to interact with:

1. Select the object in the Hierarchy
2. Click **Add Component** > search for **InteractableObject**
3. Configure in the Inspector:

```
Object Type:    seat              (or door, arcade, container, etc.)
Display Name:   Red Chair         (human-readable name)
Description:    A comfy chair     (optional description)
Affordances:    [sit, examine]    (what can be done with this object)
```

**Common Affordance Combinations:**

| Object Type | Typical Affordances |
|-------------|---------------------|
| seat | sit, examine |
| door | open, close, examine |
| arcade | play, examine |
| container | open, close, examine |
| display | examine |
| npc | talk, examine |

### Step 3: Set the Interaction Point (Optional)

If you want the agent to stand at a specific position when interacting:

1. Create an empty child GameObject named "InteractionPoint"
2. Position it where the agent should stand
3. Drag it into the **Interaction Transform** field

If not set, the agent will use the object's position.

### Step 4: Configure Personality (Optional)

1. Select your character with GolemAgent
2. In the Inspector, find the **Personality** section
3. Either:
   - Assign a `PersonalityProfile` ScriptableObject, OR
   - Set **Default Preset** to one of: `Balanced`, `CuriousExplorer`, `CautiousHomebody`, `SocialButterfly`, `LoyalCompanion`, `WildCard`, `SilentObserver`

**Personality Traits:**

| Trait | Low (0) | High (1) |
|-------|---------|----------|
| curiosity | Prefers familiar areas | Seeks novelty |
| memoryRetention | Forgets quickly | Remembers everything |
| sociability | Avoids interaction | Seeks social contact |
| caution | Reckless | Very careful |
| routinePreference | Spontaneous | Creature of habit |
| adaptability | Slow to learn | Adapts quickly |

### Step 5: Connect to AI Backend

The agent reports its state and receives commands via events:

```csharp
// Get reference to agent
GolemAgent agent = GetComponent<GolemAgent>();

// Get current world state (send to AI backend)
WorldStateReport state = agent.GenerateWorldState();
string json = JsonUtility.ToJson(state);
// Send json to your WebSocket/HTTP backend

// Execute commands from AI backend
agent.InteractWith(targetObject, "sit");
agent.MoveTo(position);
agent.ExploreRandom();
agent.StandUp();
```

## Complete Setup Example

Here's a minimal scene setup:

```
Scene
├── Character
│   ├── GolemAgent
│   ├── WorldScanner
│   ├── WorldMemory
│   ├── NavMeshAgent
│   └── Animator
│
├── Furniture
│   ├── Chair_01
│   │   ├── InteractableObject (type: seat, affordances: [sit, examine])
│   │   └── InteractionPoint (child transform)
│   └── Chair_02
│       └── InteractableObject (type: seat, affordances: [sit, examine])
│
├── Games
│   └── ArcadeMachine
│       └── InteractableObject (type: arcade, affordances: [play, examine])
│
└── Environment
    └── NavMesh Surface
```

## Component Reference

### GolemAgent

The central controller. Attach to your character.

**Key Properties:**
- `personality` - PersonalityProfile ScriptableObject
- `scanner` - WorldScanner reference (auto-found)
- `memory` - WorldMemory reference (auto-found)
- `currentActivity` - Current state (idle, walking, sitting, etc.)

**Key Methods:**
- `MoveTo(Vector3 position)` - Navigate to position
- `InteractWith(InteractableObject target, string affordance)` - Interact with object
- `InteractWithNearest(string affordance)` - Find and interact with nearest
- `ExploreRandom()` - Wander in a random direction
- `StandUp()` - Stop current interaction
- `GenerateWorldState()` - Get full state report

### WorldScanner

Discovers InteractableObjects in range. Auto-attached by GolemAgent.

**Key Properties:**
- `scanRadius` - How far to scan (default: 15m)
- `scanInterval` - How often to scan (default: 1s)
- `nearbyObjects` - List of discovered objects

**Events:**
- `OnObjectDiscovered` - Fired when new object found
- `OnObjectLost` - Fired when object goes out of range

### WorldMemory

Persistent memory across sessions. Auto-attached by GolemAgent.

**Key Properties:**
- `agentId` - Unique ID for save file
- `autoSaveInterval` - Save frequency (default: 60s)
- `autoLoadOnStart` - Load on Awake (default: true)

**Key Methods:**
- `RememberObject(InteractableObject)` - Store object in memory
- `RecordAffordanceAttempt(objectType, affordance, success)` - Learn from interaction
- `GetAffordanceConfidence(objectType, affordance)` - Get learned confidence (0-1)
- `SaveMemory()` / `LoadMemory()` - Manual persistence
- `ApplyMemoryDecay()` - Apply personality-based forgetting

### InteractableObject

Makes any object discoverable and interactive.

**Key Properties:**
- `objectType` - Category (seat, door, arcade, etc.)
- `displayName` - Human-readable name
- `description` - Optional description
- `affordances` - Array of possible interactions
- `interactionTransform` - Where agent stands (optional)
- `isOccupied` - Whether object is in use

**Key Methods:**
- `CanInteract()` - Check if available
- `CanInteract(affordance)` - Check if specific action available
- `SetOccupied(bool)` - Mark as in use

## Troubleshooting

### Agent doesn't move
- Check that NavMesh is baked in the scene
- Check that NavMeshAgent component is present and enabled
- Verify target position is on NavMesh

### Agent doesn't find objects
- Check that InteractableObject components are attached
- Verify objects are within `scanRadius` (default 15m)
- Check that objects have colliders (required for Physics.OverlapSphere)

### Interactions don't work
- Verify affordances are spelled correctly (case-sensitive)
- Check that `interactionTransform` is reachable
- Ensure object is not marked as `isOccupied`

### Memory not persisting
- Check `Application.persistentDataPath` is writable
- Verify `agentId` is unique per character
- Check for errors in console during save/load

## Creating Custom Personalities

Create a new PersonalityProfile:

1. Right-click in Project window
2. Select **Create > Golem > Personality Profile**
3. Name it and adjust traits in Inspector
4. Drag onto GolemAgent's `personality` field

Or create via code:

```csharp
var profile = ScriptableObject.CreateInstance<PersonalityProfile>();
profile.curiosity = 0.8f;
profile.memoryRetention = 0.9f;
profile.sociability = 0.3f;
profile.caution = 0.6f;
profile.routinePreference = 0.2f;
profile.adaptability = 0.7f;
```

## Creating Custom Interactions

Implement `IInteractionHandler`:

```csharp
public class DanceInteraction : MonoBehaviour, IInteractionHandler
{
    public InteractionExecutor executor;

    void Start()
    {
        executor.RegisterHandler("dance", this);
    }

    public bool CanHandle(string affordance) => affordance == "dance";

    public void Execute(InteractableObject target, string affordance, Action<bool, string> onComplete)
    {
        // Play dance animation
        StartCoroutine(DoDance(onComplete));
    }

    public void Cancel() { /* Stop dancing */ }

    IEnumerator DoDance(Action<bool, string> onComplete)
    {
        yield return new WaitForSeconds(5f);
        onComplete(true, null);
    }
}
```

## Next Steps

- Review `APPROVED_ARCHITECTURE.md` for full system design
- Check `Plan/PLAN.md` for roadmap
- Connect to your AI backend (Claude, GPT, etc.)

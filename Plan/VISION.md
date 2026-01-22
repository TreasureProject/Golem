# Golem: The Vision

## What is Golem?

**Golem is a framework for AI agents that can live in any 3D world.**

Drop an agent into a new environment. It discovers what's there. It learns what's possible. It acts autonomously. No hardcoding. No manual configuration. Just intelligent presence.

---

## The Problem with Current Approaches

### How AI Agents Work Today

Most 3D AI agents are **tightly coupled to their environments**:

```
Traditional Approach:
├── Agent knows: "There is a chair at position (5, 0, 3)"
├── Agent knows: "I can sit by calling SitAtChair()"
├── Agent knows: "The cafe is north, the arcade is south"
└── New environment = rewrite everything
```

Every object must be manually registered. Every action must be explicitly coded. Every location must be hardcoded. Move the agent to a new world? Start over.

This isn't intelligence. It's scripting.

### The Golem Approach

```
Golem Approach:
├── Agent sees: "There's something 3 meters away"
├── Agent discovers: "It's a seat. I can sit on it."
├── Agent learns: "Sitting here gives me a good view of the entrance"
└── New environment = same agent, new discoveries
```

**Golem agents discover their world.** Objects describe themselves. The agent scans, learns, remembers, and adapts.

---

## Core Principles

### 1. Self-Describing Worlds

Objects announce what they are and what can be done with them:

```
"I am a door. You can open me. You can close me."
"I am a seat. You can sit on me. I am currently occupied."
"I am a terminal. You can use me. I require authentication."
```

The agent doesn't need prior knowledge. It asks the world: "What's here? What can I do?"

### 2. Discovery Over Configuration

Instead of:
```csharp
// Old way: manually assign everything
public Transform[] chairs;
public Transform[] doors;
public Transform cafeLocation;
```

Golem does:
```csharp
// Golem way: discover at runtime
var nearbySeats = scanner.GetAllOfType("seat");
var nearestDoor = scanner.GetNearestWithAffordance("open");
```

**Zero manual configuration. Drop in and go.**

### 3. Universal Actions

Instead of:
```
"sitAtChair" → specific to chairs
"playArcade" → specific to arcades
"openDoor" → specific to doors
```

Golem uses:
```
"interact" + target + affordance → works with anything
```

The agent says "interact with the nearest seat using the sit affordance." Works whether it's a chair, a bench, a couch, or a throne.

### 4. Persistent Memory

Agents remember what they've discovered:

```
"I've been to this area before."
"Last time, that door was locked."
"The seat by the window has a good view."
```

Memory persists across sessions. Agents build familiarity with their environment over time.

---

## Who is Golem For?

### For Developers

**Python developers** can build agent backends:
```python
from golem import Agent, WorldState

class ExplorerAgent(Agent):
    def on_world_state(self, state: WorldState):
        # See what's nearby
        seats = state.objects_of_type("seat")
        if seats and not self.is_sitting:
            self.interact(seats[0], "sit")
```

**JavaScript developers** can build web-based agents:
```javascript
import { GolemClient } from '@golem/sdk';

const agent = new GolemClient({ url: 'ws://localhost:8080' });
agent.on('worldState', (state) => {
    const nearest = state.nearestObject();
    if (nearest?.affordances.includes('examine')) {
        agent.interact(nearest, 'examine');
    }
});
```

**Unity developers** can create new worlds:
```
1. Import Golem package
2. Add InteractableObject components to objects
3. Drop in a GolemAgent prefab
4. Connect to any AI backend
```

### For Creators (Non-Developers)

**Bring your companion to any world:**

1. Download a Golem-compatible world from the marketplace
2. Import your agent (character model + personality)
3. Watch them explore and interact

No coding required. Your agent discovers the world on their own.

### For Game Developers

**NPCs that actually live in your world:**

- NPCs discover your environment automatically
- They find seats, use objects, explore areas
- They remember players and places
- They react to changes in the world

No more scripted patrol paths. No more "NPC walks to point A, waits, walks to point B."

---

## The Technical Stack

```
┌─────────────────────────────────────────────────────────┐
│                    AI Backend                            │
│              (Claude, GPT, Local LLM)                   │
│                                                          │
│   Receives world state → Decides actions → Sends commands│
└─────────────────────────────────────────────────────────┘
                           │
                           │ WebSocket (Golem Protocol)
                           │
┌─────────────────────────────────────────────────────────┐
│                    Golem Runtime                         │
│                      (Unity)                            │
│                                                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │ WorldScanner │  │GolemAgent   │  │StateReporter    │  │
│  │ - Discovers  │  │- Moves      │  │- Sends state    │  │
│  │ - Tracks     │  │- Interacts  │  │- Receives cmds  │  │
│  └─────────────┘  └─────────────┘  └─────────────────┘  │
│                                                          │
│  ┌─────────────────────────────────────────────────────┐│
│  │              InteractableObjects                     ││
│  │   [Seat]  [Door]  [Terminal]  [Arcade]  [Display]   ││
│  │   Each object describes itself and its affordances   ││
│  └─────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────┘
```

---

## What Makes Golem Different

| Aspect | Traditional Agents | Golem Agents |
|--------|-------------------|--------------|
| World Knowledge | Hardcoded | Discovered |
| New Environments | Requires rewrite | Works immediately |
| Object Interactions | Specific functions per object | Universal affordance system |
| Configuration | Manual inspector assignment | Automatic scanning |
| Memory | None or session-only | Persistent across sessions |
| Extensibility | Add code for each new object | Add component, agent discovers |

---

## The Golem Ecosystem

### SDKs

- **Python SDK** - Build AI backends with any LLM
- **JavaScript SDK** - Build web-based agents and dashboards
- **Unity Package** - Create Golem-compatible worlds

### Templates

- **Blank World** - Empty scene with Golem infrastructure
- **Card Game** - Blackjack, poker, etc. with agent dealer
- **Social Space** - Cafe, lounge with seating and objects
- **Exploration World** - Maze/dungeon with discoverable areas

### Marketplace (Future)

- Download Golem-compatible worlds
- Share and sell agent personalities
- Community-created templates and extensions

---

## The Long-Term Vision

### Phase 1: Discovery (Now)
Agents can discover and interact with objects in any Golem-compatible world.

### Phase 2: Learning
Agents learn preferences. "I like sitting by the window." "This area is usually crowded at night."

### Phase 3: Social
Multiple agents in the same world. They discover each other. They interact. They form relationships.

### Phase 4: Creation
Agents can modify their environment. Place objects. Decorate spaces. Build.

### Phase 5: Persistence
Agents live continuously. They have schedules. They have routines. They have lives.

---

## Why "Golem"?

In folklore, a golem is a being brought to life to serve and protect. It's animated clay given purpose.

Golem the framework does the same for AI agents:
- **Brings them to life** in 3D worlds
- **Gives them autonomy** to discover and act
- **Serves creators** who want intelligent companions

The golem doesn't need instructions for every step. It understands its purpose and figures out the rest.

---

## Get Started

```bash
# Clone the repo
git clone https://github.com/[your-org]/golem

# Python backend
pip install golem-sdk

# JavaScript client
npm install @golem/sdk

# Unity: Import the package and drop in a GolemAgent
```

**Read the [PLAN.md](./PLAN.md) for implementation details.**

---

## Join the Movement

Golem is open source. We're building the future of AI agents in 3D worlds.

- **Contribute** - Help build the core framework
- **Create** - Build Golem-compatible worlds
- **Connect** - Join the community, share your agents

**AI agents shouldn't be scripted. They should be alive.**

*That's Golem.*

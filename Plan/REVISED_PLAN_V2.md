# REVISED: Golem Self-Learning Bot Architecture v2

## Key Changes from v1
- Added multi-modal perception (not just vision)
- Added symbolic abstraction layer
- Replaced flat JSON with knowledge graph
- Added curiosity-driven exploration
- Added forgetfulness mechanism
- Hybrid approach: learned behaviors + fallback rules

## Architecture

### Perception Layer (Multi-Modal)

1. **Primary: Agent-Centric Sensors** (cheap, fast, local)
   - Raycasts in 8 directions (what objects are nearby)
   - Collision detection (what am I touching)
   - NavMesh queries (where can I walk)
   - Audio detection (what sounds are playing)

2. **Secondary: Vision** (expensive, rare, cloud)
   - Screenshots only when sensors detect unknown object
   - Used for initial scene understanding
   - Used when behavior fails unexpectedly

### Symbolic Abstraction Layer

Convert raw sensor data to symbols:
```
Raw: Raycast hit at 2.3m, layer "Furniture", name "Chair_01"
Symbolic: {type: "seat", distance: "near", direction: "left"}
```
AI reasons about symbols, not pixels.

### Knowledge Graph Memory

```
[Chair_01] --affordance--> [sit]
[Chair_01] --location--> [Cafe Zone]
[sit] --requires--> [standing]
[sit] --results_in--> [seated]
[Arcade_01] --affordance--> [play]
[play] --reward--> [entertainment: 0.8]
```

- Nodes: objects, actions, states, zones
- Edges: relationships (affordance, requires, causes, located_in)
- Queryable: "What can I sit on nearby?" → traverse graph

### Learning System

1. **Curiosity-Driven Exploration**
   - Intrinsic reward for discovering new objects
   - Prioritize unexplored areas
   - Avoid repeating failed interactions

2. **Hierarchical Actions**
   - High-level: "go_to_cafe", "find_seat", "entertain_self"
   - Low-level: pathfinding, animation, interaction
   - Claude writes high-level behavior trees
   - Unity handles low-level execution

3. **Forgetfulness**
   - Decay old memories based on recency/frequency
   - Prune rarely-used knowledge
   - Re-learn if environment changes

### Hybrid Behavior System

```
IF knowledge_graph.has_answer(situation) THEN
   use learned behavior
ELSE IF fallback_rules.has_answer(situation) THEN
   use predefined rule
ELSE
   trigger curiosity exploration
   request vision analysis if stuck
ENDIF
```

### Bot Generation

Claude writes behavior trees (not raw Lua):

```lua
behavior_tree = {
  sequence = {
    {action = "find_nearest", type = "seat"},
    {action = "move_to", target = "$result"},
    {action = "interact", affordance = "sit"}
  }
}
```

Unity interprets the tree, handles edge cases.

### Cost Model (Revised)

| Phase | Method | Cost |
|-------|--------|------|
| Runtime perception | Raycasts/NavMesh | $0 |
| Unknown object | 1 screenshot | $0.005 |
| Daily operation | ~5-10 screenshots | $0.03-0.05 |
| Monthly | ~200 screenshots | $1-2 |

### Minimal Developer Integration

1. Import Golem package
2. Add GolemAgent to character
3. (Optional) Tag important objects with simple names
4. Done - AI discovers the rest

## Success Criteria (Revised)

1. Minimal Unity developer configuration (just add component)
2. Learn new environment to reasonable competency in <1 hour
3. Runtime cost <$0.10/day
4. Memories persist and gradually improve
5. Works with most Unity scenes (not all)

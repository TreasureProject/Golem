# Golem Self-Learning Bot Architecture v3.1

## Key Changes from v3
- Added modular personality system (traits affect all behaviors)
- Forgetfulness and exploration are now personality-driven, not hardcoded
- Characters are portable across worlds with consistent personality
- Added personality presets for common archetypes

## Key Changes from v2
- Added specific learning implementation details (Q-learning inspired)
- Added knowledge graph ontology with confidence weights
- Added behavior tree parameter passing protocol
- Added error handling and recovery mechanisms
- Added specific thresholds for forgetfulness
- Added cost overrun fail-safes

---

## Architecture

### 0. Modular Personality System (NEW)

> **Core Principle:** Personality is data. Behavior reads data.

Characters carry a `PersonalityProfile` that affects ALL behavior systems. This profile is portable across worlds.

**PersonalityProfile (ScriptableObject):**
```csharp
[CreateAssetMenu(fileName = "Personality", menuName = "Golem/Personality")]
public class PersonalityProfile : ScriptableObject
{
    [Header("Core Traits (0-1)")]
    public float curiosity = 0.5f;        // 0=homebody, 1=explorer
    public float memoryRetention = 0.5f;  // 0=forgetful, 1=perfect memory
    public float sociability = 0.5f;      // 0=loner, 1=social butterfly
    public float caution = 0.5f;          // 0=reckless, 1=very cautious
    public float routinePreference = 0.5f; // 0=spontaneous, 1=creature of habit
    public float adaptability = 0.5f;     // 0=rigid, 1=flexible
}
```

**Trait Effects:**
| Trait | Affects |
|-------|---------|
| curiosity | Exploration frequency, novelty-seeking |
| memoryRetention | Memory decay rate, knowledge graph pruning |
| sociability | Interaction priority with NPCs/players |
| caution | Risk assessment, approach to unknown objects |
| routinePreference | Tendency to repeat known behaviors |
| adaptability | Speed of learning in new environments |

**Preset Archetypes:**
```
CuriousExplorer:   curiosity=0.9, memoryRetention=0.7, caution=0.2
CautiousHomebody:  curiosity=0.2, memoryRetention=0.8, caution=0.9, routinePreference=0.8
SocialButterfly:   sociability=0.95, curiosity=0.6, adaptability=0.8
LoyalCompanion:    sociability=0.8, routinePreference=0.7, adaptability=0.6
WildCard:          curiosity=0.8, caution=0.1, routinePreference=0.1
```

**Portability:**
- Profile serializes to JSON
- Stored in cloud or local storage
- Loaded when character enters any world
- Same character behaves consistently across different Unity scenes

**Trait Evolution (Optional):**
```csharp
void OnExperience(string trait, float delta) {
    // Successful explorations: curiosity += 0.01
    // Dangerous encounters: caution += 0.02
    // Social successes: sociability += 0.01
}
```

---

### 1. Perception Layer (Multi-Modal)

**Primary: Agent-Centric Sensors** (local, free)
- Raycasts: 16 directions, 15m range, returns (objectName, layer, distance, direction)
- Collision: OnTriggerEnter/Stay/Exit events
- NavMesh: IsReachable(position), GetNearestWalkable(position)
- Audio: Distance to nearest AudioSource, clip name if playing

**Secondary: Vision** (cloud, $0.005/call)
Triggered ONLY when:
- Sensor detects object with unknown name (not in knowledge graph)
- Action fails 3+ times on same object
- Explicit user request

**Cost Fail-Safe:**
- Daily screenshot limit: 50 (configurable)
- If limit reached: fallback to sensor-only mode
- Alert sent to backend: "Vision budget exhausted"

---

### 2. Symbolic Abstraction Layer

**Conversion Rules:**
```
Raw Input                          → Symbol
-----------------------------------------
distance < 2m                      → "near"
distance 2-10m                     → "medium"
distance > 10m                     → "far"
layer == "Furniture"               → category: "furniture"
name.contains("chair", "seat")     → type: "seat"
name.contains("door", "gate")      → type: "door"
name.contains("arcade", "game")    → type: "arcade"
```

**Output Format:**
```json
{
  "id": "Chair_01_39482",
  "type": "seat",
  "category": "furniture",
  "distance": "near",
  "direction": "left",
  "reachable": true
}
```

---

### 3. Knowledge Graph (Detailed Ontology)

**Node Types:**
| Type | Examples | Properties |
|------|----------|------------|
| Object | Chair_01, Door_02 | position, type, category |
| Action | sit, open, play | duration, animation |
| State | seated, standing, walking | exclusive_group |
| Zone | cafe, arcade, hallway | bounds, objects[] |
| Affordance | sittable, openable | success_rate |

**Edge Types:**
| Edge | Meaning | Weight Range |
|------|---------|--------------|
| has_affordance | Object can do Action | 0.0 - 1.0 |
| located_in | Object is in Zone | 1.0 (fixed) |
| requires_state | Action needs State | 1.0 (fixed) |
| results_in | Action causes State | 0.0 - 1.0 |
| adjacent_to | Zone connects Zone | 1.0 (fixed) |

**Confidence Weights:**
- Initial discovery: weight = 0.5
- Successful interaction: weight += 0.1 (max 1.0)
- Failed interaction: weight -= 0.2 (min 0.0)
- If weight < 0.1: edge is pruned

**Example Graph:**
```
[Chair_01] --has_affordance(0.95)--> [sit]
[Chair_01] --located_in(1.0)--> [Cafe]
[sit] --requires_state(1.0)--> [standing]
[sit] --results_in(0.9)--> [seated]
[Shelf_01] --has_affordance(0.05)--> [sit]  ← Will be pruned
```

---

### 4. Learning System (Q-Learning Inspired)

**State Representation:**
```
S = {
  current_zone: "cafe",
  current_state: "standing",
  nearby_objects: ["Chair_01", "Table_02"],
  time_idle: 45,
  goal: "find_entertainment"
}
```

**Action Selection:**
```
Q(s,a) = base_value + exploration_bonus + knowledge_confidence

base_value = reward history for this action
exploration_bonus = 1.0 / sqrt(times_tried + 1)  # UCB-style
knowledge_confidence = graph.get_edge_weight(object, action)

chosen_action = argmax(Q(s,a)) with epsilon=0.1 random exploration
```

**Reward Function:**
| Outcome | Reward |
|---------|--------|
| Action succeeded | +1.0 |
| Action failed | -0.5 |
| Discovered new object | +0.5 |
| Discovered new affordance | +1.0 |
| Idle for >60s | -0.1/second |
| Reached goal state | +2.0 |

**Learning a New Affordance (Example):**
```
1. Agent sees unknown object "Jukebox_01"
2. Sensors: {type: unknown, category: furniture, distance: near}
3. Vision triggered → Claude: "This is a jukebox, try: play, examine"
4. Agent tries "play" → Success
5. Graph updated: [Jukebox_01] --has_affordance(0.6)--> [play]
6. Agent tries "play" again → Success
7. Graph updated: [Jukebox_01] --has_affordance(0.7)--> [play]
8. After 5 successes: weight = 1.0, learned!
```

---

### 5. Exploration System (Personality-Driven)

**Exploration is modulated by personality traits:**
```
explore_chance = personality.curiosity * (1 - personality.caution * 0.5)
explore_radius = base_radius * (1 + personality.curiosity)
routine_weight = personality.routinePreference
```

**Unexplored Area Detection:**
```
unexplored_score(zone) =
  (1 - visit_count / max_visits) *
  (unknown_objects_count / total_objects) *
  (time_since_last_visit / decay_factor) *
  personality.curiosity  // Curious characters weight this higher
```

**Exploration Priority (adjusted by personality):**
1. Zones with unexplored_score > (0.7 - personality.curiosity * 0.3)
2. Objects with unknown affordances (if curiosity > 0.5)
3. Objects with low confidence edges (0.3-0.7)
4. Random walk (epsilon = 0.05 + personality.curiosity * 0.15)
5. Return to known areas (if routinePreference > 0.7)

**Novelty Metric:**
```
novelty(object) =
  1.0 * personality.curiosity if object not in graph
  0.5 * personality.curiosity if object has untried affordances
  0.1 if object has low-confidence edges
  0.0 if fully explored
```

**Caution Modifier:**
```
// High caution = approach slowly, observe first
if personality.caution > 0.7:
  approach_distance *= 1.5
  observation_time *= 2
```

---

### 6. Behavior Tree Protocol

**Tree Format (JSON):**
```json
{
  "type": "sequence",
  "children": [
    {
      "type": "action",
      "name": "find_nearest",
      "params": {"object_type": "seat"},
      "output": "$target"
    },
    {
      "type": "action",
      "name": "move_to",
      "params": {"target": "$target"},
      "output": "$move_result"
    },
    {
      "type": "condition",
      "check": "$move_result == success"
    },
    {
      "type": "action",
      "name": "interact",
      "params": {"target": "$target", "affordance": "sit"}
    }
  ]
}
```

**Parameter Passing:**
- Variables prefixed with "$" are stored in blackboard
- Unity BehaviorTreeRunner maintains blackboard dict
- Claude writes trees, Unity executes with variable substitution

**Error Handling:**
```json
{
  "type": "selector",
  "children": [
    {
      "type": "sequence",
      "children": [
        {"type": "action", "name": "move_to", "params": {"target": "$target"}},
        {"type": "action", "name": "interact", "params": {"affordance": "sit"}}
      ]
    },
    {
      "type": "fallback",
      "name": "on_failure",
      "action": "find_alternative",
      "params": {"original_target": "$target", "affordance": "sit"}
    }
  ]
}
```

**Failure Recovery:**
| Failure Type | Recovery Action |
|--------------|-----------------|
| Path blocked | Try alternative route, then find_alternative |
| Object occupied | Wait 5s, then find_alternative |
| Action failed | Decrease confidence, try different affordance |
| Unknown object | Trigger vision analysis |

---

### 7. Forgetfulness Mechanism (Personality-Driven)

**Memory retention is modulated by personality:**
```csharp
// memoryRetention: 0 = very forgetful, 1 = perfect memory
float halfLife = Mathf.Lerp(1f, 30f, personality.memoryRetention);  // 1-30 days
float pruneThreshold = Mathf.Lerp(0.3f, 0.05f, personality.memoryRetention);
int maxObjects = Mathf.RoundToInt(Mathf.Lerp(100, 10000, personality.memoryRetention));
```

**Decay Parameters (personality-adjusted):**
- Recency half-life: `1 + 29 * memoryRetention` days (1-30 days)
- Minimum access count: `1 + 4 * memoryRetention` in 30 days
- Confidence threshold: `0.3 - 0.25 * memoryRetention` (0.05-0.3)

**Decay Formula:**
```
effective_half_life = 1 + 29 * personality.memoryRetention
decayed_weight = weight * (0.5 ^ (days_since_use / effective_half_life))
```

**Pruning Rules:**
- Every 24 hours: scan all edges
- Prune threshold: `0.3 - 0.25 * memoryRetention`
- If decayed_weight < threshold: mark for pruning
- Pruned edges are logged for potential re-learning

**Memory Compaction:**
- Keep top `100 + 9900 * memoryRetention` objects
- Perfect memory (1.0) = keep ~10,000 objects
- Forgetful (0.0) = keep ~100 objects
- Archive older objects to cold storage
- Re-load archived objects on re-encounter

**Examples by Personality:**
| memoryRetention | Half-life | Max Objects | Prune Threshold |
|-----------------|-----------|-------------|-----------------|
| 0.0 (forgetful) | 1 day | 100 | 0.30 |
| 0.5 (average) | 15 days | 5,000 | 0.175 |
| 1.0 (perfect) | 30 days | 10,000 | 0.05 |

---

### 8. Cost Model (Final)

| Operation | Cost | Daily Limit |
|-----------|------|-------------|
| Sensor perception | $0 | Unlimited |
| Vision (screenshot) | $0.005 | 50/day |
| Behavior tree generation | $0.001 | 100/day |
| Knowledge graph query | $0 | Unlimited |

**Budget Enforcement:**
```
if daily_vision_calls >= 50:
  mode = "sensors_only"
  alert("Vision budget exhausted")

if daily_bt_generations >= 100:
  mode = "cached_behaviors_only"
  alert("Generation budget exhausted")
```

**Expected Costs:**
- Learning phase (day 1): ~$0.25 (50 screenshots)
- Steady state: ~$0.03/day (5-10 screenshots)
- Monthly: ~$1-2

---

## Success Criteria (Final)

1. **Integration:** Developer adds 1 component, optionally tags objects
2. **Learning Speed:** Reasonable competency in new environment within 1 hour
3. **Cost:** < $0.10/day steady state, < $1/day learning phase
4. **Memory:** Persists across sessions, improves over time
5. **Compatibility:** Works with most Unity scenes using NavMesh
6. **Robustness:** Graceful degradation when budget exhausted

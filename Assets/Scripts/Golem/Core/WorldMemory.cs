using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Golem
{
    /// <summary>
    /// Persistent memory system for Golem agents.
    /// Remembers discovered objects, learned affordances, and visited zones across sessions.
    /// Memory decay and capacity are driven by the agent's personality.
    /// </summary>
    public class WorldMemory : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Agent's personality affects memory retention and decay.")]
        public PersonalityProfile personality;

        [Header("Settings")]
        [Tooltip("Unique identifier for this agent's memory file.")]
        public string agentId = "default_agent";

        [Tooltip("Auto-save memory at this interval (seconds). 0 = manual only.")]
        public float autoSaveInterval = 60f;

        [Tooltip("Auto-load memory on Awake.")]
        public bool autoLoadOnStart = true;

        [Header("Debug")]
        [SerializeField] private int _knownObjectCount;
        [SerializeField] private int _knownAffordanceCount;
        [SerializeField] private int _knownZoneCount;

        // Public accessors for memory stats
        public int knownObjectCount => _knownObjectCount;
        public int knownAffordanceCount => _knownAffordanceCount;
        public int knownZoneCount => _knownZoneCount;

        // In-memory data structures
        private Dictionary<string, ObjectMemory> knownObjects = new Dictionary<string, ObjectMemory>();
        private Dictionary<string, AffordanceMemory> knownAffordances = new Dictionary<string, AffordanceMemory>();
        private Dictionary<string, ZoneMemory> knownZones = new Dictionary<string, ZoneMemory>();

        // Events
        public event Action<ObjectMemory> OnObjectRemembered;
        public event Action<AffordanceMemory> OnAffordanceLearned;
        public event Action<ZoneMemory> OnZoneDiscovered;
        public event Action OnMemoryDecayed;
        public event Action OnMemorySaved;
        public event Action OnMemoryLoaded;

        private float lastSaveTime;
        private float lastDecayTime;

        private void Awake()
        {
            if (autoLoadOnStart)
            {
                LoadMemory();
            }
        }

        private void Update()
        {
            // Auto-save
            if (autoSaveInterval > 0 && Time.time - lastSaveTime > autoSaveInterval)
            {
                SaveMemory();
                lastSaveTime = Time.time;
            }

            // Daily decay check (every 24 hours of real time, scaled for testing)
            // In practice, decay happens based on real-world time between sessions
        }

        private void OnApplicationQuit()
        {
            SaveMemory();
        }

        #region Object Memory

        /// <summary>
        /// Remember a discovered object.
        /// </summary>
        public void RememberObject(InteractableObject obj)
        {
            if (obj == null) return;

            string id = obj.UniqueId;

            if (knownObjects.TryGetValue(id, out ObjectMemory existing))
            {
                // Update existing memory
                existing.lastSeenTime = DateTime.UtcNow;
                existing.timesEncountered++;
                existing.lastPosition = obj.transform.position;
            }
            else
            {
                // Create new memory
                var memory = new ObjectMemory
                {
                    objectId = id,
                    objectType = obj.objectType,
                    displayName = obj.displayName,
                    description = obj.description,
                    affordances = obj.affordances.ToList(),
                    firstSeenTime = DateTime.UtcNow,
                    lastSeenTime = DateTime.UtcNow,
                    lastPosition = obj.transform.position,
                    timesEncountered = 1,
                    confidence = 0.5f
                };

                knownObjects[id] = memory;
                OnObjectRemembered?.Invoke(memory);
            }

            UpdateDebugCounts();
        }

        /// <summary>
        /// Check if we remember an object.
        /// </summary>
        public bool RemembersObject(string objectId)
        {
            return knownObjects.ContainsKey(objectId);
        }

        /// <summary>
        /// Get memory of a specific object.
        /// </summary>
        public ObjectMemory GetObjectMemory(string objectId)
        {
            knownObjects.TryGetValue(objectId, out ObjectMemory memory);
            return memory;
        }

        /// <summary>
        /// Get all remembered objects of a specific type.
        /// </summary>
        public List<ObjectMemory> GetObjectsOfType(string objectType)
        {
            return knownObjects.Values
                .Where(o => o.objectType == objectType)
                .OrderByDescending(o => o.confidence)
                .ToList();
        }

        #endregion

        #region Affordance Learning

        /// <summary>
        /// Record an interaction attempt with an affordance.
        /// </summary>
        public void RecordAffordanceAttempt(string objectType, string affordance, bool succeeded)
        {
            string key = $"{objectType}:{affordance}";

            if (!knownAffordances.TryGetValue(key, out AffordanceMemory memory))
            {
                memory = new AffordanceMemory
                {
                    objectType = objectType,
                    affordance = affordance,
                    firstAttemptTime = DateTime.UtcNow,
                    confidence = 0.5f
                };
                knownAffordances[key] = memory;
            }

            memory.totalAttempts++;
            memory.lastAttemptTime = DateTime.UtcNow;

            if (succeeded)
            {
                memory.successfulAttempts++;
                memory.confidence = Mathf.Min(1f, memory.confidence + 0.1f);
            }
            else
            {
                memory.confidence = Mathf.Max(0f, memory.confidence - 0.2f);
            }

            memory.successRate = (float)memory.successfulAttempts / memory.totalAttempts;

            OnAffordanceLearned?.Invoke(memory);
            UpdateDebugCounts();
        }

        /// <summary>
        /// Get confidence that an object type supports an affordance.
        /// </summary>
        public float GetAffordanceConfidence(string objectType, string affordance)
        {
            string key = $"{objectType}:{affordance}";
            if (knownAffordances.TryGetValue(key, out AffordanceMemory memory))
            {
                return memory.confidence;
            }
            return 0.5f; // Unknown = neutral confidence
        }

        /// <summary>
        /// Check if we've learned that an affordance works (confidence > 0.7).
        /// </summary>
        public bool HasLearnedAffordance(string objectType, string affordance)
        {
            return GetAffordanceConfidence(objectType, affordance) > 0.7f;
        }

        /// <summary>
        /// Check if we've learned that an affordance doesn't work (confidence < 0.3).
        /// </summary>
        public bool HasLearnedAffordanceFails(string objectType, string affordance)
        {
            return GetAffordanceConfidence(objectType, affordance) < 0.3f;
        }

        #endregion

        #region Zone Memory

        /// <summary>
        /// Remember a discovered zone/area.
        /// </summary>
        public void RememberZone(string zoneName, Vector3 center, float radius)
        {
            if (knownZones.TryGetValue(zoneName, out ZoneMemory existing))
            {
                existing.lastVisitTime = DateTime.UtcNow;
                existing.visitCount++;
            }
            else
            {
                var memory = new ZoneMemory
                {
                    zoneName = zoneName,
                    center = center,
                    radius = radius,
                    firstVisitTime = DateTime.UtcNow,
                    lastVisitTime = DateTime.UtcNow,
                    visitCount = 1,
                    objectsDiscovered = new List<string>()
                };

                knownZones[zoneName] = memory;
                OnZoneDiscovered?.Invoke(memory);
            }

            UpdateDebugCounts();
        }

        /// <summary>
        /// Record that an object was found in a zone.
        /// </summary>
        public void RecordObjectInZone(string zoneName, string objectId)
        {
            if (knownZones.TryGetValue(zoneName, out ZoneMemory zone))
            {
                if (!zone.objectsDiscovered.Contains(objectId))
                {
                    zone.objectsDiscovered.Add(objectId);
                }
            }
        }

        /// <summary>
        /// Get exploration score for a zone (higher = less explored, more interesting).
        /// </summary>
        public float GetZoneExplorationScore(string zoneName)
        {
            if (!knownZones.TryGetValue(zoneName, out ZoneMemory zone))
            {
                return 1f; // Unknown zone = very interesting
            }

            float timeFactor = (float)(DateTime.UtcNow - zone.lastVisitTime).TotalHours / 24f;
            float visitFactor = 1f / (zone.visitCount + 1);

            return Mathf.Clamp01(timeFactor * 0.5f + visitFactor * 0.5f);
        }

        #endregion

        #region Memory Decay

        /// <summary>
        /// Apply memory decay based on personality and time elapsed.
        /// Call this periodically (e.g., on session start).
        /// </summary>
        public void ApplyMemoryDecay()
        {
            if (personality == null)
            {
                Debug.LogWarning("WorldMemory: No personality assigned, using default decay");
                ApplyDecayWithHalfLife(15f, 0.175f);
            }
            else
            {
                ApplyDecayWithHalfLife(personality.MemoryHalfLifeDays, personality.MemoryPruneThreshold);
            }

            EnforceMemoryCapacity();
            OnMemoryDecayed?.Invoke();
            UpdateDebugCounts();
        }

        private void ApplyDecayWithHalfLife(float halfLifeDays, float pruneThreshold)
        {
            var objectsToRemove = new List<string>();

            foreach (var kvp in knownObjects)
            {
                float daysSinceLastSeen = (float)(DateTime.UtcNow - kvp.Value.lastSeenTime).TotalDays;
                float decayFactor = Mathf.Pow(0.5f, daysSinceLastSeen / halfLifeDays);
                kvp.Value.confidence *= decayFactor;

                if (kvp.Value.confidence < pruneThreshold)
                {
                    objectsToRemove.Add(kvp.Key);
                }
            }

            foreach (var id in objectsToRemove)
            {
                knownObjects.Remove(id);
            }

            // Decay affordance confidence
            var affordancesToRemove = new List<string>();

            foreach (var kvp in knownAffordances)
            {
                float daysSinceLastAttempt = (float)(DateTime.UtcNow - kvp.Value.lastAttemptTime).TotalDays;
                float decayFactor = Mathf.Pow(0.5f, daysSinceLastAttempt / halfLifeDays);
                kvp.Value.confidence = 0.5f + (kvp.Value.confidence - 0.5f) * decayFactor; // Decay towards neutral

                if (Mathf.Abs(kvp.Value.confidence - 0.5f) < 0.05f && kvp.Value.totalAttempts < 3)
                {
                    affordancesToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in affordancesToRemove)
            {
                knownAffordances.Remove(key);
            }

            if (objectsToRemove.Count > 0 || affordancesToRemove.Count > 0)
            {
                Debug.Log($"WorldMemory: Pruned {objectsToRemove.Count} objects and {affordancesToRemove.Count} affordances");
            }
        }

        private void EnforceMemoryCapacity()
        {
            int maxObjects = personality != null ? personality.MaxMemoryObjects : 5000;

            if (knownObjects.Count > maxObjects)
            {
                // Remove lowest confidence objects first
                var sorted = knownObjects
                    .OrderBy(kvp => kvp.Value.confidence)
                    .ThenBy(kvp => kvp.Value.lastSeenTime)
                    .Take(knownObjects.Count - maxObjects)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var id in sorted)
                {
                    knownObjects.Remove(id);
                }

                Debug.Log($"WorldMemory: Enforced capacity, removed {sorted.Count} objects");
            }
        }

        #endregion

        #region Persistence

        /// <summary>
        /// Save memory to disk.
        /// </summary>
        public void SaveMemory()
        {
            var data = new MemoryData
            {
                agentId = agentId,
                savedAt = DateTime.UtcNow.ToString("o"),
                objects = knownObjects.Values.ToList(),
                affordances = knownAffordances.Values.ToList(),
                zones = knownZones.Values.ToList()
            };

            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, json);
                Debug.Log($"WorldMemory: Saved to {path}");
                OnMemorySaved?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"WorldMemory: Failed to save - {e.Message}");
            }
        }

        /// <summary>
        /// Load memory from disk.
        /// </summary>
        public void LoadMemory()
        {
            string path = GetSavePath();

            if (!File.Exists(path))
            {
                Debug.Log($"WorldMemory: No save file found at {path}, starting fresh");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<MemoryData>(json);

                knownObjects.Clear();
                knownAffordances.Clear();
                knownZones.Clear();

                foreach (var obj in data.objects)
                {
                    knownObjects[obj.objectId] = obj;
                }

                foreach (var aff in data.affordances)
                {
                    string key = $"{aff.objectType}:{aff.affordance}";
                    knownAffordances[key] = aff;
                }

                foreach (var zone in data.zones)
                {
                    knownZones[zone.zoneName] = zone;
                }

                Debug.Log($"WorldMemory: Loaded {knownObjects.Count} objects, {knownAffordances.Count} affordances, {knownZones.Count} zones");

                // Apply decay for time elapsed since last save
                ApplyMemoryDecay();

                OnMemoryLoaded?.Invoke();
                UpdateDebugCounts();
            }
            catch (Exception e)
            {
                Debug.LogError($"WorldMemory: Failed to load - {e.Message}");
            }
        }

        /// <summary>
        /// Clear all memory (use with caution).
        /// </summary>
        public void ClearMemory()
        {
            knownObjects.Clear();
            knownAffordances.Clear();
            knownZones.Clear();
            UpdateDebugCounts();
            Debug.Log("WorldMemory: Cleared all memory");
        }

        /// <summary>
        /// Export memory to JSON string (for cloud sync, etc.).
        /// </summary>
        public string ExportToJson()
        {
            var data = new MemoryData
            {
                agentId = agentId,
                savedAt = DateTime.UtcNow.ToString("o"),
                objects = knownObjects.Values.ToList(),
                affordances = knownAffordances.Values.ToList(),
                zones = knownZones.Values.ToList()
            };

            return JsonUtility.ToJson(data, true);
        }

        /// <summary>
        /// Import memory from JSON string.
        /// </summary>
        public void ImportFromJson(string json)
        {
            var data = JsonUtility.FromJson<MemoryData>(json);

            foreach (var obj in data.objects)
            {
                knownObjects[obj.objectId] = obj;
            }

            foreach (var aff in data.affordances)
            {
                string key = $"{aff.objectType}:{aff.affordance}";
                knownAffordances[key] = aff;
            }

            foreach (var zone in data.zones)
            {
                knownZones[zone.zoneName] = zone;
            }

            UpdateDebugCounts();
        }

        private string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, "Golem", $"{agentId}_memory.json");
        }

        private void UpdateDebugCounts()
        {
            _knownObjectCount = knownObjects.Count;
            _knownAffordanceCount = knownAffordances.Count;
            _knownZoneCount = knownZones.Count;
        }

        #endregion
    }

    #region Data Structures

    [Serializable]
    public class ObjectMemory
    {
        public string objectId;
        public string objectType;
        public string displayName;
        public string description;
        public List<string> affordances;
        public Vector3 lastPosition;
        public string firstSeenTimeStr;
        public string lastSeenTimeStr;
        public int timesEncountered;
        public float confidence;

        [NonSerialized] private DateTime? _firstSeen;
        [NonSerialized] private DateTime? _lastSeen;

        public DateTime firstSeenTime
        {
            get
            {
                if (!_firstSeen.HasValue)
                    _firstSeen = DateTime.TryParse(firstSeenTimeStr, out var dt) ? dt : DateTime.UtcNow;
                return _firstSeen.Value;
            }
            set
            {
                _firstSeen = value;
                firstSeenTimeStr = value.ToString("o");
            }
        }

        public DateTime lastSeenTime
        {
            get
            {
                if (!_lastSeen.HasValue)
                    _lastSeen = DateTime.TryParse(lastSeenTimeStr, out var dt) ? dt : DateTime.UtcNow;
                return _lastSeen.Value;
            }
            set
            {
                _lastSeen = value;
                lastSeenTimeStr = value.ToString("o");
            }
        }
    }

    [Serializable]
    public class AffordanceMemory
    {
        public string objectType;
        public string affordance;
        public int totalAttempts;
        public int successfulAttempts;
        public float successRate;
        public float confidence;
        public string firstAttemptTimeStr;
        public string lastAttemptTimeStr;

        [NonSerialized] private DateTime? _firstAttempt;
        [NonSerialized] private DateTime? _lastAttempt;

        public DateTime firstAttemptTime
        {
            get
            {
                if (!_firstAttempt.HasValue)
                    _firstAttempt = DateTime.TryParse(firstAttemptTimeStr, out var dt) ? dt : DateTime.UtcNow;
                return _firstAttempt.Value;
            }
            set
            {
                _firstAttempt = value;
                firstAttemptTimeStr = value.ToString("o");
            }
        }

        public DateTime lastAttemptTime
        {
            get
            {
                if (!_lastAttempt.HasValue)
                    _lastAttempt = DateTime.TryParse(lastAttemptTimeStr, out var dt) ? dt : DateTime.UtcNow;
                return _lastAttempt.Value;
            }
            set
            {
                _lastAttempt = value;
                lastAttemptTimeStr = value.ToString("o");
            }
        }
    }

    [Serializable]
    public class ZoneMemory
    {
        public string zoneName;
        public Vector3 center;
        public float radius;
        public string firstVisitTimeStr;
        public string lastVisitTimeStr;
        public int visitCount;
        public List<string> objectsDiscovered;

        [NonSerialized] private DateTime? _firstVisit;
        [NonSerialized] private DateTime? _lastVisit;

        public DateTime firstVisitTime
        {
            get
            {
                if (!_firstVisit.HasValue)
                    _firstVisit = DateTime.TryParse(firstVisitTimeStr, out var dt) ? dt : DateTime.UtcNow;
                return _firstVisit.Value;
            }
            set
            {
                _firstVisit = value;
                firstVisitTimeStr = value.ToString("o");
            }
        }

        public DateTime lastVisitTime
        {
            get
            {
                if (!_lastVisit.HasValue)
                    _lastVisit = DateTime.TryParse(lastVisitTimeStr, out var dt) ? dt : DateTime.UtcNow;
                return _lastVisit.Value;
            }
            set
            {
                _lastVisit = value;
                lastVisitTimeStr = value.ToString("o");
            }
        }
    }

    [Serializable]
    public class MemoryData
    {
        public string agentId;
        public string savedAt;
        public List<ObjectMemory> objects;
        public List<AffordanceMemory> affordances;
        public List<ZoneMemory> zones;
    }

    #endregion
}

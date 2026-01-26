using System.Collections.Generic;
using UnityEngine;

namespace Golem.Vision
{
    /// <summary>
    /// Cache for visual scan results.
    /// Uses position and rotation based invalidation with LRU eviction.
    /// </summary>
    public class VisualObjectCache : MonoBehaviour
    {
        [Header("Configuration")]
        public VisionConfig config;

        private Dictionary<string, CacheEntry> positionCache = new Dictionary<string, CacheEntry>();
        private Dictionary<string, CacheEntry> imageHashCache = new Dictionary<string, CacheEntry>();
        private LinkedList<string> lruOrder = new LinkedList<string>();
        private CacheStats stats = new CacheStats();

        private class CacheEntry
        {
            public VisualScanResult result;
            public Vector3 position;
            public Vector3 forward;
            public string imageHash;
            public float timestamp;
            public LinkedListNode<string> lruNode;
        }

        /// <summary>
        /// Store a scan result in the cache.
        /// </summary>
        public void Store(Vector3 position, Vector3 forward, VisualScanResult result, string imageHash = null)
        {
            if (config == null || !config.enableCache)
                return;

            string key = GetPositionKey(position, forward);

            // Evict if at capacity
            while (positionCache.Count >= config.maxCacheEntries && lruOrder.Count > 0)
            {
                EvictOldest();
            }

            var entry = new CacheEntry
            {
                result = result,
                position = position,
                forward = forward,
                imageHash = imageHash,
                timestamp = Time.time
            };

            // Add to LRU list
            entry.lruNode = lruOrder.AddLast(key);

            positionCache[key] = entry;
            stats.count = positionCache.Count;

            if (!string.IsNullOrEmpty(imageHash))
            {
                imageHashCache[imageHash] = entry;
            }
        }

        /// <summary>
        /// Try to retrieve a cached result by position and forward direction.
        /// </summary>
        public bool TryGetCached(Vector3 position, Vector3 forward, out VisualScanResult result)
        {
            result = null;

            if (config == null || !config.enableCache)
            {
                stats.misses++;
                return false;
            }

            // Find entries within distance and angle thresholds
            foreach (var kvp in positionCache)
            {
                var entry = kvp.Value;
                float distance = Vector3.Distance(position, entry.position);
                float angle = Vector3.Angle(forward, entry.forward);

                if (distance <= config.cacheInvalidationDistance &&
                    angle <= config.cacheInvalidationAngle)
                {
                    // Check TTL
                    if (Time.time - entry.timestamp <= config.cacheTTL)
                    {
                        result = entry.result;
                        stats.hits++;

                        // Move to end of LRU list
                        lruOrder.Remove(entry.lruNode);
                        entry.lruNode = lruOrder.AddLast(kvp.Key);

                        return true;
                    }
                }
            }

            stats.misses++;
            return false;
        }

        /// <summary>
        /// Try to retrieve a cached result by image hash.
        /// </summary>
        public bool TryGetByImageHash(string imageHash, out VisualScanResult result)
        {
            result = null;

            if (string.IsNullOrEmpty(imageHash) || !imageHashCache.TryGetValue(imageHash, out var entry))
            {
                return false;
            }

            // Check TTL
            if (Time.time - entry.timestamp <= config.cacheTTL)
            {
                result = entry.result;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Invalidate cache entries near a position.
        /// </summary>
        public void InvalidateNear(Vector3 position, float radius)
        {
            var keysToRemove = new List<string>();

            foreach (var kvp in positionCache)
            {
                if (Vector3.Distance(position, kvp.Value.position) <= radius)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                RemoveEntry(key);
            }
        }

        /// <summary>
        /// Clear all cached entries.
        /// </summary>
        public void InvalidateAll()
        {
            positionCache.Clear();
            imageHashCache.Clear();
            lruOrder.Clear();
            stats.count = 0;
        }

        /// <summary>
        /// Get cache statistics.
        /// </summary>
        public CacheStats GetStats()
        {
            stats.count = positionCache.Count;
            return stats;
        }

        /// <summary>
        /// Reset statistics.
        /// </summary>
        public void ResetStats()
        {
            stats.hits = 0;
            stats.misses = 0;
        }

        private string GetPositionKey(Vector3 position, Vector3 forward)
        {
            // Quantize position to reduce key variations
            int px = Mathf.RoundToInt(position.x);
            int py = Mathf.RoundToInt(position.y);
            int pz = Mathf.RoundToInt(position.z);

            // Quantize forward direction to 45-degree increments
            float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            int qa = Mathf.RoundToInt(angle / 45f) * 45;

            return $"{px}_{py}_{pz}_{qa}";
        }

        private void EvictOldest()
        {
            if (lruOrder.Count == 0)
                return;

            string oldestKey = lruOrder.First.Value;
            RemoveEntry(oldestKey);
        }

        private void RemoveEntry(string key)
        {
            if (positionCache.TryGetValue(key, out var entry))
            {
                if (!string.IsNullOrEmpty(entry.imageHash))
                {
                    imageHashCache.Remove(entry.imageHash);
                }

                lruOrder.Remove(entry.lruNode);
                positionCache.Remove(key);
            }
        }
    }
}

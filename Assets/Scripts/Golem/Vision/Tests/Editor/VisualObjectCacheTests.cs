using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Golem.Vision.Tests
{
    /// <summary>
    /// Unit tests for VisualObjectCache.
    /// Tests caching, invalidation, and retrieval logic.
    /// </summary>
    [TestFixture]
    public class VisualObjectCacheTests
    {
        private GameObject testObject;
        private VisualObjectCache cache;
        private VisionConfig config;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("TestCache");
            cache = testObject.AddComponent<VisualObjectCache>();
            config = TestUtilities.CreateTestConfig();
            cache.config = config;
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        #region Basic Cache Operations

        [Test]
        public void Store_AddsEntryToCache()
        {
            // Arrange
            var position = new Vector3(5, 0, 5);
            var forward = Vector3.forward;
            var result = TestUtilities.CreateMockScanResult(2, true, position, forward);

            // Act
            cache.Store(position, forward, result);

            // Assert
            var stats = cache.GetStats();
            Assert.AreEqual(1, stats.count, "Cache should have 1 entry");
        }

        [Test]
        public void TryGetCached_ReturnsTrueForValidEntry()
        {
            // Arrange
            var position = new Vector3(5, 0, 5);
            var forward = Vector3.forward;
            var storedResult = TestUtilities.CreateMockScanResult(2, true, position, forward);
            cache.Store(position, forward, storedResult);

            // Act
            bool found = cache.TryGetCached(position, forward, out VisualScanResult retrieved);

            // Assert
            Assert.IsTrue(found, "Should find cached entry");
            Assert.IsNotNull(retrieved, "Retrieved result should not be null");
            Assert.AreEqual(storedResult.scanId, retrieved.scanId, "Should retrieve same result");
        }

        [Test]
        public void TryGetCached_ReturnsFalseForEmptyCache()
        {
            // Arrange
            var position = new Vector3(5, 0, 5);
            var forward = Vector3.forward;

            // Act
            bool found = cache.TryGetCached(position, forward, out VisualScanResult result);

            // Assert
            Assert.IsFalse(found, "Should not find entry in empty cache");
            Assert.IsNull(result, "Result should be null");
        }

        [Test]
        public void TryGetCached_UpdatesHitStatistics()
        {
            // Arrange
            var position = Vector3.zero;
            var forward = Vector3.forward;
            var storedResult = TestUtilities.CreateMockScanResult();
            cache.Store(position, forward, storedResult);

            // Act
            cache.TryGetCached(position, forward, out _);
            cache.TryGetCached(position, forward, out _);

            // Assert
            var stats = cache.GetStats();
            Assert.AreEqual(2, stats.hits, "Should have 2 cache hits");
        }

        [Test]
        public void TryGetCached_UpdatesMissStatistics()
        {
            // Arrange
            var position = Vector3.zero;
            var forward = Vector3.forward;

            // Act
            cache.TryGetCached(position, forward, out _);
            cache.TryGetCached(new Vector3(100, 0, 100), forward, out _);

            // Assert
            var stats = cache.GetStats();
            Assert.AreEqual(2, stats.misses, "Should have 2 cache misses");
        }

        #endregion

        #region Position-Based Invalidation

        [Test]
        public void TryGetCached_ReturnsFalseWhenPositionChangedBeyondThreshold()
        {
            // Arrange
            var originalPosition = Vector3.zero;
            var forward = Vector3.forward;
            var result = TestUtilities.CreateMockScanResult(2, true, originalPosition, forward);
            cache.Store(originalPosition, forward, result);

            // Move beyond threshold
            var newPosition = new Vector3(config.cacheInvalidationDistance + 1, 0, 0);

            // Act
            bool found = cache.TryGetCached(newPosition, forward, out _);

            // Assert
            Assert.IsFalse(found, "Should not find entry when moved beyond threshold");
        }

        [Test]
        public void TryGetCached_ReturnsTrueWhenPositionChangedWithinThreshold()
        {
            // Arrange
            var originalPosition = Vector3.zero;
            var forward = Vector3.forward;
            var result = TestUtilities.CreateMockScanResult(2, true, originalPosition, forward);
            cache.Store(originalPosition, forward, result);

            // Move within threshold
            var newPosition = new Vector3(config.cacheInvalidationDistance * 0.5f, 0, 0);

            // Act
            bool found = cache.TryGetCached(newPosition, forward, out _);

            // Assert
            Assert.IsTrue(found, "Should find entry when moved within threshold");
        }

        [Test]
        public void TryGetCached_ReturnsFalseWhenRotationChangedBeyondThreshold()
        {
            // Arrange
            var position = Vector3.zero;
            var originalForward = Vector3.forward;
            var result = TestUtilities.CreateMockScanResult(2, true, position, originalForward);
            cache.Store(position, originalForward, result);

            // Rotate beyond threshold
            var newForward = Quaternion.Euler(0, config.cacheInvalidationAngle + 10, 0) * Vector3.forward;

            // Act
            bool found = cache.TryGetCached(position, newForward, out _);

            // Assert
            Assert.IsFalse(found, "Should not find entry when rotated beyond threshold");
        }

        #endregion

        #region Invalidation Methods

        [Test]
        public void InvalidateNear_RemovesEntriesWithinRadius()
        {
            // Arrange
            cache.Store(new Vector3(0, 0, 0), Vector3.forward, TestUtilities.CreateMockScanResult());
            cache.Store(new Vector3(1, 0, 0), Vector3.forward, TestUtilities.CreateMockScanResult());
            cache.Store(new Vector3(10, 0, 0), Vector3.forward, TestUtilities.CreateMockScanResult());

            // Act
            cache.InvalidateNear(Vector3.zero, 5f);

            // Assert
            var stats = cache.GetStats();
            Assert.AreEqual(1, stats.count, "Should have 1 entry remaining (the one at 10,0,0)");
        }

        [Test]
        public void InvalidateAll_ClearsEntireCache()
        {
            // Arrange
            cache.Store(Vector3.zero, Vector3.forward, TestUtilities.CreateMockScanResult());
            cache.Store(new Vector3(5, 0, 5), Vector3.forward, TestUtilities.CreateMockScanResult());
            cache.Store(new Vector3(10, 0, 10), Vector3.forward, TestUtilities.CreateMockScanResult());

            // Act
            cache.InvalidateAll();

            // Assert
            var stats = cache.GetStats();
            Assert.AreEqual(0, stats.count, "Cache should be empty");
        }

        #endregion

        #region LRU Eviction

        [Test]
        public void Store_EvictsOldestWhenMaxEntriesReached()
        {
            // Arrange
            config.maxCacheEntries = 3;

            // Store entries
            for (int i = 0; i < 5; i++)
            {
                var pos = new Vector3(i * 10, 0, 0); // Space them out so they get different cache keys
                cache.Store(pos, Vector3.forward, TestUtilities.CreateMockScanResult());
            }

            // Assert
            var stats = cache.GetStats();
            Assert.LessOrEqual(stats.count, config.maxCacheEntries,
                "Cache should not exceed max entries");
        }

        #endregion

        #region Image Hash Lookup

        [Test]
        public void TryGetByImageHash_FindsMatchingEntry()
        {
            // Arrange
            var position = Vector3.zero;
            var forward = Vector3.forward;
            var result = TestUtilities.CreateMockScanResult();
            string imageHash = "ABC123";
            cache.Store(position, forward, result, imageHash);

            // Act
            bool found = cache.TryGetByImageHash(imageHash, out VisualScanResult retrieved);

            // Assert
            Assert.IsTrue(found, "Should find entry by image hash");
            Assert.IsNotNull(retrieved);
        }

        [Test]
        public void TryGetByImageHash_ReturnsFalseForUnknownHash()
        {
            // Arrange
            cache.Store(Vector3.zero, Vector3.forward, TestUtilities.CreateMockScanResult(), "HASH1");

            // Act
            bool found = cache.TryGetByImageHash("UNKNOWN_HASH", out VisualScanResult result);

            // Assert
            Assert.IsFalse(found, "Should not find entry with unknown hash");
        }

        #endregion

        #region Statistics

        [Test]
        public void GetStats_ReturnsCorrectHitRate()
        {
            // Arrange
            var position = Vector3.zero;
            var forward = Vector3.forward;
            cache.Store(position, forward, TestUtilities.CreateMockScanResult());

            // 2 hits
            cache.TryGetCached(position, forward, out _);
            cache.TryGetCached(position, forward, out _);

            // 1 miss
            cache.TryGetCached(new Vector3(100, 100, 100), forward, out _);

            // Act
            var stats = cache.GetStats();

            // Assert
            Assert.AreEqual(2, stats.hits);
            Assert.AreEqual(1, stats.misses);
            float expectedHitRate = 2f / 3f;
            Assert.AreEqual(expectedHitRate, stats.hitRate, 0.01f, "Hit rate should be ~66%");
        }

        [Test]
        public void ResetStats_ClearsStatistics()
        {
            // Arrange
            var position = Vector3.zero;
            var forward = Vector3.forward;
            cache.Store(position, forward, TestUtilities.CreateMockScanResult());
            cache.TryGetCached(position, forward, out _);
            cache.TryGetCached(new Vector3(100, 0, 0), forward, out _);

            // Act
            cache.ResetStats();
            var stats = cache.GetStats();

            // Assert
            Assert.AreEqual(0, stats.hits);
            Assert.AreEqual(0, stats.misses);
        }

        #endregion

        #region Disabled Cache

        [Test]
        public void Store_DoesNotAddWhenCacheDisabled()
        {
            // Arrange
            config.enableCache = false;

            // Act
            cache.Store(Vector3.zero, Vector3.forward, TestUtilities.CreateMockScanResult());

            // Assert
            var stats = cache.GetStats();
            Assert.AreEqual(0, stats.count, "Should not store when cache disabled");
        }

        #endregion
    }
}

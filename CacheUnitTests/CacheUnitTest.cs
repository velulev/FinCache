using CacheLib;
using FluentAssertions;

namespace CacheUnitTests
{
    [TestClass]
    public class CacheUnitTest
    {
        [TestMethod]
        public void CheckCacheSetupParameters()
        {
            FinCache cache = new FinCache(2, 1);

            cache.Count.Should().Be(0);
            cache.Capacity.Should().Be(1);
            cache.ConcurrencyLevel.Should().Be(2);
        }

        [TestMethod]
        public void CheckNonExistingKey()
        {
            FinCache cache = new FinCache(2, 1);

            string key = "key";

            bool existsKey = cache.ContainsKey(key);
            bool existsValue = cache.TryGetValue(key, out object value);
            bool existsValueForDelete1 = cache.TryRemove(key, out object valueForDelete);
            bool existsValueForDelete2 = cache.TryRemove(key);
            bool existsTimestamp = cache.TryGetLastAccessedTimestamp(key, out DateTime accessTimestamp);

            existsKey.Should().BeFalse();
            existsValue.Should().BeFalse();
            existsValueForDelete1.Should().BeFalse();
            existsValueForDelete2.Should().BeFalse();
            existsTimestamp.Should().BeFalse();
            value.Should().BeNull();
            valueForDelete.Should().BeNull();
        }

        [TestMethod]
        public void AddAnEntityToCache()
        {
            DateTime timestampBeforeAddition = DateTime.Now;

            FinCache cache = new FinCache(2, 1);
            string key = "key";
            string value = "value";
            
            bool added = cache.AddOrUpdate(key, value);
            // Remember, this is going to increase the timestamp again
            bool exists = cache.TryGetValue(key, out object cacheValue);
            bool existsTimestamp = cache.TryGetLastAccessedTimestamp(key, out DateTime accessTimestamp);

            added.Should().BeTrue();
            exists.Should().BeTrue();
            existsTimestamp.Should().BeTrue();
            cache.Count.Should().Be(1);
            value.Should().Be(cacheValue.ToString());
            accessTimestamp.Should().BeAfter(timestampBeforeAddition);
        }

        [TestMethod]
        public void ClearCache()
        {
            FinCache cache = new FinCache(2, 10);

            bool added = cache.AddOrUpdate("key", 0);
            bool addedLater = cache.AddOrUpdate("key", null!);
            bool added1 = cache.AddOrUpdate("key1", 1);
            bool added2 = cache.AddOrUpdate("key2", 2.0);
            bool added3 = cache.AddOrUpdate("key3", DateTime.Now);

            added.Should().BeTrue();
            addedLater.Should().BeFalse();
            added1.Should().BeTrue();
            added2.Should().BeTrue();
            added3.Should().BeTrue();
            cache.Count.Should().Be(4);

            cache.Clear();
            cache.Count.Should().Be(0);
        }

        [TestMethod]
        public void RemoveFromCache()
        {
            FinCache cache = new FinCache(2, 10);

            bool added = cache.AddOrUpdate("key", 0);
            bool addedLater = cache.AddOrUpdate("key", null!);

            bool added1 = cache.AddOrUpdate("key1", 1);

            added.Should().BeTrue();
            addedLater.Should().BeFalse();
            added1.Should().BeTrue();
            cache.Count.Should().Be(2);

            bool deleted = cache.TryRemove("key");
            bool deleted1 = cache.TryRemove("key1", out object value );

            deleted.Should().BeTrue();
            deleted1.Should().BeTrue();
            ((int)value).Should().Be(1);
            cache.Count.Should().Be(0);
        }

        [TestMethod]
        public void UpdateAnEntityInCache()
        {
            FinCache cache = new FinCache(2, 1);
            string key = "key";
            string value = "value";

            bool added = cache.AddOrUpdate(key, value);
            bool existsTimestampAfterAdd = cache.TryGetLastAccessedTimestamp(key, out DateTime accessTimestampAfterAdd);

            value = "new value";

            // should be false, as update returns false
            bool addedLater = cache.AddOrUpdate(key, value);
            bool existsTimestampAfterUpdate = cache.TryGetLastAccessedTimestamp(key, out DateTime accessTimestampAfterUpdate);

            // Remember timestamp gets updated here too
            bool exists = cache.TryGetValue(key, out object cacheValue);
            bool existsTimestamp = cache.TryGetLastAccessedTimestamp(key, out DateTime accessTimestamp);

            added.Should().BeTrue();
            addedLater.Should().BeFalse();
            exists.Should().BeTrue();
            existsTimestampAfterAdd.Should().BeTrue();
            existsTimestampAfterUpdate.Should().BeTrue();
            existsTimestamp.Should().BeTrue();
            cache.Count.Should().Be(1);
            value.Should().Be(cacheValue.ToString());
            accessTimestampAfterUpdate.Should().BeAfter(accessTimestampAfterAdd);
        }

        [TestMethod]
        public void GetValueShouldUpdateTimestampButNotValue()
        {
            DateTime timestampBeforeAdd = DateTime.Now;
            FinCache cache = new FinCache(2, 1);
            string key = "key";
            string value = "value";

            bool added = cache.AddOrUpdate(key, value);

            bool existsAddedTimestamp = cache.TryGetLastAccessedTimestamp(key, out DateTime timestampAdded);

            bool exists = cache.TryGetValue(key, out object cacheValue);
            bool existsTimestampAfterRead = cache.TryGetLastAccessedTimestamp(key, out DateTime timestampAfterRead);

            bool exists1 = cache.TryGetValue(key, out object cacheValue1);

            added.Should().BeTrue();
            exists.Should().BeTrue();
            existsAddedTimestamp.Should().BeTrue();
            existsTimestampAfterRead.Should().BeTrue();
            cache.Count.Should().Be(1);
            value.Should().Be(cacheValue.ToString());
            existsTimestampAfterRead.Should().BeTrue();

            timestampAdded.Should().BeAfter(timestampBeforeAdd);
            timestampAfterRead.Should().BeAfter(timestampAdded);
            exists1.Should().BeTrue();
            value.Should().Be(cacheValue1.ToString());
        }

        [TestMethod]
        public void GetLastAccessedTimestampShouldNotUpdateTimestampOrValue()
        {
            DateTime timestampBeforeAdd = DateTime.Now;
            FinCache cache = new FinCache(2, 1);
            string key = "key";
            string value = "value";

            bool added = cache.AddOrUpdate(key, value);

            bool existsAddedTimestamp = cache.TryGetLastAccessedTimestamp(key, out DateTime timestampAdded);
            bool existsAddedTimestamp1 = cache.TryGetLastAccessedTimestamp(key, out DateTime timestampAdded1);
            bool existsAddedTimestamp2 = cache.TryGetLastAccessedTimestamp(key, out DateTime timestampAdded2);

            bool exists = cache.TryGetValue(key, out object cacheValue);
            bool existsTimestampAfterRead = cache.TryGetLastAccessedTimestamp(key, out DateTime timestampAfterRead);
            bool existsTimestampAfterRead1 = cache.TryGetLastAccessedTimestamp(key, out DateTime timestampAfterRead1);
            bool existsTimestampAfterRead2 = cache.TryGetLastAccessedTimestamp(key, out DateTime timestampAfterRead2);

            bool exists1 = cache.TryGetValue(key, out object cacheValue1);

            added.Should().BeTrue();
            exists.Should().BeTrue();

            existsAddedTimestamp.Should().BeTrue();
            existsAddedTimestamp1.Should().BeTrue();
            existsAddedTimestamp2.Should().BeTrue();

            existsTimestampAfterRead.Should().BeTrue();
            cache.Count.Should().Be(1);
            value.Should().Be(cacheValue.ToString());

            existsTimestampAfterRead.Should().BeTrue();
            existsTimestampAfterRead1.Should().BeTrue();
            existsTimestampAfterRead2.Should().BeTrue();

            timestampAdded.Should().BeAfter(timestampBeforeAdd);
            timestampAdded.Should().Be(timestampAdded1);
            timestampAdded.Should().Be(timestampAdded2);

            timestampAfterRead.Should().BeAfter(timestampAdded);
            timestampAfterRead.Should().Be(timestampAfterRead1);
            timestampAfterRead.Should().Be(timestampAfterRead2);

            exists1.Should().BeTrue();
            value.Should().Be(cacheValue1.ToString());

        }

        [TestMethod]
        public void EvictEarliestAccessedItem_SimpleAddOnly()
        {
            EvictionParameters evicted = new EvictionParameters();

            FinCache cache = new FinCache(2, 3);
            cache.ItemEvicted += evicted.SetEvictionParameters;

            bool added1 = cache.AddOrUpdate("key1", 0);
            bool existsTimestampAfterAdd1 = cache.TryGetLastAccessedTimestamp("key1", out DateTime accessTimestampAfterAdd1);

            bool added2 = cache.AddOrUpdate("key2", 1.1);
            bool existsTimestampAfterAdd2 = cache.TryGetLastAccessedTimestamp("key2", out DateTime accessTimestampAfterAdd2);

            bool added3 = cache.AddOrUpdate("key3", 'X');
            bool existsTimestampAfterAdd3 = cache.TryGetLastAccessedTimestamp("key3", out DateTime accessTimestampAfterAdd3);

            added1.Should().BeTrue();
            added2.Should().BeTrue();
            added3.Should().BeTrue();

            existsTimestampAfterAdd1.Should().BeTrue();
            existsTimestampAfterAdd2.Should().BeTrue();
            existsTimestampAfterAdd3.Should().BeTrue();

            accessTimestampAfterAdd1.Should().BeBefore(accessTimestampAfterAdd2);
            accessTimestampAfterAdd2.Should().BeBefore(accessTimestampAfterAdd3);

            cache.Count.Should().Be(3);

            bool added4 = cache.AddOrUpdate("overflow", "VIP");
            bool existsTimestampAfterAdd4 = cache.TryGetLastAccessedTimestamp("overflow", out DateTime accessTimestampAfterAdd4);
            
            added4.Should().BeTrue();
            accessTimestampAfterAdd3.Should().BeBefore(accessTimestampAfterAdd4);
            evicted.Key.Should().Be("key1");
            ((int)evicted.Value).Should().Be(0);
            evicted.LastAccessed.Should().Be(accessTimestampAfterAdd1);
            cache.Count.Should().Be(3);
        }

        [TestMethod]
        public void EvictEarliestAccessedItem_ComplexAfterReadAndUpdate()
        {
            EvictionParameters evicted = new EvictionParameters();

            FinCache cache = new FinCache(2, 3);
            cache.ItemEvicted += evicted.SetEvictionParameters;

            bool added1 = cache.AddOrUpdate("key1", 0);
            bool existsTimestampAfterAdd1 = cache.TryGetLastAccessedTimestamp("key1", out DateTime accessTimestampAfterAdd1);

            bool added2 = cache.AddOrUpdate("key2", 1.1);
            bool existsTimestampAfterAdd2 = cache.TryGetLastAccessedTimestamp("key2", out DateTime accessTimestampAfterAdd2);

            bool added3 = cache.AddOrUpdate("key3", 'X');
            bool existsTimestampAfterAdd3 = cache.TryGetLastAccessedTimestamp("key3", out DateTime accessTimestampAfterAdd3);

            added1.Should().BeTrue();
            added2.Should().BeTrue();
            added3.Should().BeTrue();

            existsTimestampAfterAdd1.Should().BeTrue();
            existsTimestampAfterAdd2.Should().BeTrue();
            existsTimestampAfterAdd3.Should().BeTrue();

            accessTimestampAfterAdd1.Should().BeBefore(accessTimestampAfterAdd2);
            accessTimestampAfterAdd2.Should().BeBefore(accessTimestampAfterAdd3);

            cache.Count.Should().Be(3);

            // Now update 1 st object
            bool addedAfter1 = cache.AddOrUpdate("key1", 1);
            bool existsTimestampAfterUpdate1 = cache.TryGetLastAccessedTimestamp("key1", out DateTime accessTimestampAfterUpdate1);

            // And read second object
            bool existsDuringRead = cache.TryGetValue("key2", out object value);
            bool existsTimestampAfterRead2 = cache.TryGetLastAccessedTimestamp("key2", out DateTime accessTimestampAfterRead2);

            existsTimestampAfterUpdate1.Should().BeTrue();
            existsTimestampAfterRead2.Should().BeTrue();
            accessTimestampAfterAdd3.Should().BeBefore(accessTimestampAfterUpdate1);
            accessTimestampAfterAdd3.Should().BeBefore(accessTimestampAfterRead2);
            cache.Count.Should().Be(3);

            bool added4 = cache.AddOrUpdate("overflow", "VIP");
            bool existsTimestampAfterAdd4 = cache.TryGetLastAccessedTimestamp("overflow", out DateTime accessTimestampAfterAdd4);

            added4.Should().BeTrue();
            accessTimestampAfterRead2.Should().BeBefore(accessTimestampAfterAdd4);
            evicted.Key.Should().Be("key3");
            ((char)evicted.Value).Should().Be('X');
            evicted.LastAccessed.Should().Be(accessTimestampAfterAdd3);
            cache.Count.Should().Be(3);
        }

        class EvictionParameters
        {
            private string key = null!;
            private object value = null!;
            private DateTime lastAccessed = DateTime.MinValue;

            public EvictionParameters()
            {
            }

            public void SetEvictionParameters(string key, object value, DateTime lastAccessed)
            {
                this.key = key;
                this.value = value;
                this.lastAccessed = lastAccessed;
            }

            public string Key => key;
            public object Value => value;
            public DateTime LastAccessed => lastAccessed;
        }

    }
}
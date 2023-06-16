namespace CacheLib.Interfaces
{
    public interface IFinCache
    {
        public int Capacity { get; }

        public int ConcurrencyLevel { get; }

        public int Count { get; }

        void Clear();

        bool ContainsKey(string key);

        bool AddOrUpdate(string key, object value);

        bool TryGetValue(string key, out object value);

        bool TryGetLastAccessedTimestamp(string key, out DateTime timestamp);

        bool TryRemove(string key, out object value);

        bool TryRemove(string key);

        event Action<string, object, DateTime> ItemEvicted;
    }
}

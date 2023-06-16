using System.Collections.Concurrent;

namespace CacheLib
{
    public class FinCache : Interfaces.IFinCache
    {
        protected readonly int m_capacity;
        protected readonly int m_concurrencyLevel;

        protected readonly ConcurrentDictionary<string, (DateTime,object)> m_cache;
        protected readonly System.Collections.SortedList m_timestamps;

        //protected readonly SortedList<DateTime, HashSet<string>> m_timestamps;

        public FinCache(int concurrencyLevel, int capacity)
        {
            m_capacity = capacity;
            m_concurrencyLevel = concurrencyLevel;

            m_cache = new ConcurrentDictionary<string, (DateTime, object)>(concurrencyLevel, capacity);
            m_timestamps = new System.Collections.SortedList(capacity);

            //m_timestamps = new SortedList<DateTime, HashSet<string>>(capacity);
        }

        public int Count => m_cache.Count;

        public int Capacity => m_capacity;

        public int ConcurrencyLevel => m_concurrencyLevel;

        protected void EvictIfNecessaryPreAdd()
        {
            while (m_cache.Count > m_capacity - 1)
            {
                RemoveEarliestTimestampKey();
            }
        }

        protected void EvictIfNecessaryPostAdd()
        {
            while (m_cache.Count > m_capacity)
            {
                RemoveEarliestTimestampKey();
            }
        }

        protected bool RemoveEarliestTimestampKey()
        {
            DateTime minTimestamp = (DateTime)m_timestamps.GetKey(0);
            HashSet<string> keys = (HashSet<string>)m_timestamps[minTimestamp]!;
            string key = keys.First<string>();
            return EvictItem(key);
        }

        protected bool RemoveTimestampKey(DateTime timestamp, string key)
        {
            lock (m_timestamps.SyncRoot)
            {
                HashSet<string> keys = (HashSet<string>)m_timestamps[timestamp]!;

                if (keys.Count == 1)
                {
                    m_timestamps.Remove(timestamp);
                }
                else
                {
                    keys.Remove(key);
                }
            }
            return true;
        }

        protected bool AddTimestampKey(DateTime timestamp, string key)
        {
            lock (m_timestamps.SyncRoot)
            {
                if(m_timestamps.Contains(timestamp))
                {
                    HashSet<string> keys = (HashSet<string>)m_timestamps[timestamp]!;
                    keys.Add(key);
                }
                else
                {
                    m_timestamps.Add(timestamp, new HashSet<string> { key });
                }
            }
            return true;
        }

        public void Clear()
        {
            m_cache.Clear();
            m_timestamps.Clear();
        }

        public bool ContainsKey(string key)
        {
            return m_cache.ContainsKey(key);
        }

        protected bool TryAdd(string key, object value)
        {
            EvictIfNecessaryPreAdd();

            DateTime now = DateTime.Now;
            bool result = m_cache.TryAdd(key, (now, value));

            if(result && AddTimestampKey(now, key))
            {
                //EvictIfNecessaryPostAdd();
                return true;
            }
            return result;
        }

        protected bool TryUpdate(string key, object newValue)
        {
            while (m_cache.TryGetValue(key, out (DateTime, object) oldValue))
            {
                (DateTime, object) newVal = (DateTime.Now, newValue);
                if (m_cache.TryUpdate(key, newVal, oldValue) && RemoveTimestampKey(oldValue.Item1, key) && AddTimestampKey(newVal.Item1, key))
                {
                    return true;
                }
            }

            return false;
        }

        public bool AddOrUpdate(string key, object value)
        {
            if (!TryUpdate(key, value))
            {
                TryAdd(key, value);
                return true;
            }
            return false;
        }

        public bool TryGetValue(string key, out object value)
        {
            while (m_cache.TryGetValue(key, out (DateTime, object) oldValue))
            {
                (DateTime, object) newVal = (DateTime.Now, oldValue.Item2);
                if (m_cache.TryUpdate(key, newVal, oldValue) && RemoveTimestampKey(oldValue.Item1, key) && AddTimestampKey(newVal.Item1, key))
                {
                    value = newVal.Item2;
                    return true;
                }
            }

            value = null!;
            return false;
        }

        public bool TryGetLastAccessedTimestamp(string key, out DateTime timestamp)
        {
            if (m_cache.TryGetValue(key, out (DateTime, object) oldValue))
            {
                timestamp = oldValue.Item1;
                return true;
            }

            timestamp = DateTime.MinValue;
            return false;
        }

        public bool TryRemove(string key, out object value)
        {
            while (m_cache.TryGetValue(key, out (DateTime, object) oldValue))
            {
                if (m_cache.TryRemove(key, out oldValue) && RemoveTimestampKey(oldValue.Item1, key))
                {
                    value = oldValue.Item2;
                    return true;
                }
            }

            value = null!;
            return false;
        }

        public bool TryRemove(string key)
        {
            while (m_cache.TryGetValue(key, out (DateTime, object) oldValue))
            {
                if (m_cache.TryRemove(key, out oldValue) && RemoveTimestampKey(oldValue.Item1, key))
                {
                    return true;
                }
            }

            return false;
        }

        private bool EvictItem(string key)
        {
            while (m_cache.TryGetValue(key, out (DateTime, object) oldValue))
            {
                if (m_cache.TryRemove(key, out oldValue) && RemoveTimestampKey(oldValue.Item1, key))
                {
                    if(ItemEvicted != null)
                    {
                        ItemEvicted(key, oldValue.Item2, oldValue.Item1);
                    }
                    return true;
                }
            }

            return false;
        }

        public event Action<string, object, DateTime>? ItemEvicted;
    }
}
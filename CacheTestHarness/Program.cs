using CacheLib;

namespace CacheLibTestHarness
{
    internal class Program
    {
        static void PrintEvictedItem(string key, object value, DateTime lastAccessed)
        {
            Console.WriteLine("Item Key : {0} accessed last {1} with value {2} has been evicted from cache.",key,lastAccessed,value);
        }

        static void LogEvictedItem(string key, object value, DateTime lastAccessed)
        {
            Console.WriteLine("Item Key : {0} accessed last {1} with value {2} has been evicted from cache", key, lastAccessed, value);
        }

        static void Main(string[] args)
        {
            FinCache cache = new FinCache(2, 1);
            
            // Check multicast
            cache.ItemEvicted += PrintEvictedItem;
            cache.ItemEvicted += LogEvictedItem;

            Console.WriteLine("Current cache entity count {0}", cache.Count);

            if (cache.TryGetValue("abcd", out object o))
            {
                Console.WriteLine("Key present!");
            }
            else
            {
                Console.WriteLine("Key not present!");
            }

            if (cache.AddOrUpdate("abcd", 1234))
            {
                Console.WriteLine("Key added!");
            }
            else
            {
                Console.WriteLine("Key not added!");
            }

            Console.WriteLine("Current cache entity count {0}", cache.Count);

            if (cache.TryGetValue("abcd", out object o1))
            {
                Console.WriteLine("Key present, with value {0}!", o1);
            }
            else
            {
                Console.WriteLine("Key not present!");
            }

            if (!cache.AddOrUpdate("abcd", 5678))
            {
                Console.WriteLine("Key updated!");
            }
            else
            {
                Console.WriteLine("Key not updated!");
            }

            Console.WriteLine("Current cache entity count {0}", cache.Count);

            if (cache.TryGetValue("abcd", out object o2))
            {
                Console.WriteLine("Key present, with value {0}!", o2);
            }
            else
            {
                Console.WriteLine("Key not present!");
            }

            if (cache.TryRemove("abcd"))
            {
                Console.WriteLine("Key removed!");
            }
            else
            {
                Console.WriteLine("Key not removed!");
            }

            if (cache.TryGetValue("abcd", out object o3))
            {
                Console.WriteLine("Key present!");
            }
            else
            {
                Console.WriteLine("Key not present!");
            }

            Console.WriteLine("Current cache entity count {0}", cache.Count);

            if (cache.AddOrUpdate("xyz", 1234))
            {
                Console.WriteLine("Key added!");
            }
            else
            {
                Console.WriteLine("Key not added!");
            }

            Console.WriteLine("Current cache entity count {0}", cache.Count);

            if (cache.AddOrUpdate("abcd", 1234))
            {
                Console.WriteLine("Key added!");
            }
            else
            {
                Console.WriteLine("Key not added!");
            }

            Console.WriteLine("Current cache entity count {0}", cache.Count);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;

public class AppCache
{
    
    private readonly TimeSpan ttl;
    private readonly uint maxEntries;

    
    private readonly Dictionary<string, CacheItem> cache;

    
    private readonly ReaderWriterLockSlim cacheLock;

    
    public AppCache(TimeSpan ttl, uint maxEntries)
    {
        this.ttl = ttl; 
        this.maxEntries = maxEntries; 
        cache = new Dictionary<string, CacheItem>(); 
        cacheLock = new ReaderWriterLockSlim();
    }

    
    private class CacheItem
    {
        
        public object Data { get; } 
        public DateTime TimeAdded { get; } 

        
        public CacheItem(object data)
        {
            Data = data; 
            TimeAdded = DateTime.UtcNow; 
        }
    }

    
    public void Save(string key, object data)
    {
        cacheLock.EnterWriteLock(); 
        try
        {
            
            if (cache.ContainsKey(key))
                throw new ArgumentException("An element with the same key already exists.");

            
            if (cache.Count >= maxEntries)
                RemoveOldestItem();

            
            cache[key] = new CacheItem(data);
        }
        finally
        {
            cacheLock.ExitWriteLock(); 
        }
    }

    
    private void RemoveOldestItem()
    {
       
        var oldest = default(KeyValuePair<string, CacheItem>);

        
        foreach (var entry in cache)
        {
            
            if (oldest.Equals(default(KeyValuePair<string, CacheItem>)) || entry.Value.TimeAdded < oldest.Value.TimeAdded)
            {
                oldest = entry;
            }
        }

        
        if (!oldest.Equals(default(KeyValuePair<string, CacheItem>)))
        {
            cache.Remove(oldest.Key);
        }
    }

    
    public object Get(string key)
    {
        
        cacheLock.EnterUpgradeableReadLock();
        try
        {
            
            if (!cache.ContainsKey(key))
                throw new KeyNotFoundException("The specified key was not found.");

            var item = cache[key]; 

            
            if ((DateTime.UtcNow - item.TimeAdded) > ttl)
            {
                cacheLock.EnterWriteLock(); 
                try
                {
                    cache.Remove(key); 
                }
                finally
                {
                    cacheLock.ExitWriteLock(); 
                }
                throw new KeyNotFoundException("The specified key was not found."); 
            }

            return item.Data;
        }
        finally
        {
            cacheLock.ExitUpgradeableReadLock(); 
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        
        var cache = new AppCache(TimeSpan.FromSeconds(5), 3);

        try
        {
            
            cache.Save("key1", "Hello, World!");
            Console.WriteLine("Saved key1.");

            
            Console.WriteLine("Retrieved key1: " + cache.Get("key1"));

            
            Thread.Sleep(6000);

            
            Console.WriteLine("Trying to retrieve key1 again: " + cache.Get("key1"));
        }
        catch (Exception ex)
        {
           
            Console.WriteLine($"Error: {ex.Message}");
        }

        try
        {
            
            cache.Save("key2", "C# is great!");
            cache.Save("key3", 42);
            cache.Save("key4", new { Name = "John", Age = 30 });
            Console.WriteLine("Saved key2, key3, and key4.");

            
            Console.WriteLine("Retrieved key2: " + cache.Get("key2"));
            Console.WriteLine("Retrieved key3: " + cache.Get("key3"));
            Console.WriteLine("Retrieved key4: " + cache.Get("key4"));

            
            Console.WriteLine("Trying to save key3 again.");
            cache.Save("key3", "This should fail.");
        }
        catch (Exception ex)
        {
            
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

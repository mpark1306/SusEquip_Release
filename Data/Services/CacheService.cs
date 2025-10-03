using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SusEquip.Data.Interfaces.Services;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SusEquip.Data.Services
{
    /// <summary>
    /// Memory-based cache service implementation
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(15);
        
        // Statistics tracking
        private int _hitCount = 0;
        private int _missCount = 0;
        private readonly object _statsLock = new object();

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<T?> GetAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            var found = _cache.TryGetValue(key, out var value);
            
            lock (_statsLock)
            {
                if (found)
                {
                    _hitCount++;
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                }
                else
                {
                    _missCount++;
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                }
            }

            return Task.FromResult(found && value is T t ? t : default(T));
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // Handle edge case of zero or negative expiration
            if (expiration <= TimeSpan.Zero)
            {
                _logger.LogWarning("Cache expiration time is zero or negative for key {Key}. Using minimum expiration of 1 second.", key);
                expiration = TimeSpan.FromSeconds(1);
            }

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                Priority = CacheItemPriority.Normal,
                Size = CalculateSize(value)
            };

            // Add sliding expiration for frequently accessed items
            if (expiration > TimeSpan.FromMinutes(5))
            {
                options.SlidingExpiration = TimeSpan.FromMinutes(2);
            }

            _cache.Set(key, value, options);
            _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", key, expiration);

            return Task.CompletedTask;
        }

        public Task SetAsync<T>(string key, T value)
        {
            return SetAsync(key, value, _defaultExpiration);
        }

        public Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

            _cache.Remove(key);
            _logger.LogDebug("Cache removed for key: {Key}", key);

            return Task.CompletedTask;
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

            // Convert simple wildcards to regex
            var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

            // Get all cache keys (this is a limitation of IMemoryCache - no built-in key enumeration)
            // For a production system, consider using a distributed cache like Redis
            var field = typeof(MemoryCache).GetField("_coherentState", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field?.GetValue(_cache) is not object coherentState) 
                return Task.CompletedTask;

            var entriesCollection = coherentState.GetType()
                .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (entriesCollection?.GetValue(coherentState) is not System.Collections.IDictionary entries) 
                return Task.CompletedTask;

            var keysToRemove = new List<string>();
            foreach (System.Collections.DictionaryEntry entry in entries)
            {
                if (entry.Key?.ToString() is string key && regex.IsMatch(key))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _logger.LogDebug("Cache removed for pattern match key: {Key}", key);
            }

            _logger.LogDebug("Cache pattern removal completed for pattern: {Pattern}, removed {Count} keys", 
                pattern, keysToRemove.Count);

            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            // Note: IMemoryCache doesn't have a built-in Clear method
            // In a production environment, consider using IDistributedCache with Redis
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Dispose();
                _logger.LogInformation("Memory cache cleared");
            }

            lock (_statsLock)
            {
                _hitCount = 0;
                _missCount = 0;
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult(false);

            var exists = _cache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }

        public Task<CacheStatistics> GetStatisticsAsync()
        {
            lock (_statsLock)
            {
                var stats = new CacheStatistics
                {
                    HitCount = _hitCount,
                    MissCount = _missCount,
                    // Note: Memory usage calculation is approximated
                    // In a real implementation, you'd want more precise tracking
                    MemoryUsageBytes = GC.GetTotalMemory(false),
                    TotalKeys = GetApproximateKeyCount()
                };

                return Task.FromResult(stats);
            }
        }

        private int CalculateSize<T>(T value)
        {
            // Approximate size calculation
            // In a real implementation, you might use more sophisticated sizing
            try
            {
                if (value is string str)
                    return str.Length * 2; // UTF-16 encoding

                if (value is byte[] bytes)
                    return bytes.Length;

                // For objects, serialize to JSON and estimate size
                var json = JsonSerializer.Serialize(value);
                return json.Length * 2;
            }
            catch
            {
                // Fallback to default size
                return 1;
            }
        }

        private int GetApproximateKeyCount()
        {
            try
            {
                var field = typeof(MemoryCache).GetField("_coherentState", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field?.GetValue(_cache) is not object coherentState) 
                    return 0;

                var entriesCollection = coherentState.GetType()
                    .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (entriesCollection?.GetValue(coherentState) is System.Collections.IDictionary entries)
                    return entries.Count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get cache key count");
            }

            return 0;
        }
    }
}
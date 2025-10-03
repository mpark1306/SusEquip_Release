using SusEquip.Data.Models;
using SusEquip.Data.Interfaces.Services;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace SusEquip.Data.Services
{
    public class DashboardCacheService : IDashboardCacheService
    {
        private readonly EquipmentService _equipmentService;
        private readonly IMemoryCache _memoryCache;
        private readonly ICookieService _cookieService;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5); // Cache for 5 minutes
        private readonly string _cacheKey = "dashboard_stats";
        private readonly string _cookieKey = "dashboard_cache_time";

        public DashboardCacheService(EquipmentService equipmentService, IMemoryCache memoryCache, ICookieService cookieService)
        {
            _equipmentService = equipmentService;
            _memoryCache = memoryCache;
            _cookieService = cookieService;
        }

        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            // Try to get from memory cache first (fastest)
            if (_memoryCache.TryGetValue(_cacheKey, out DashboardStats? cachedStats) && cachedStats != null)
            {
                return cachedStats;
            }

            // Try to check browser cache, but handle prerendering gracefully
            try
            {
                var cookieTime = await _cookieService.GetCookieAsync(_cookieKey);
                if (!string.IsNullOrEmpty(cookieTime) && DateTime.TryParse(cookieTime, out var lastCacheTime))
                {
                    if (DateTime.Now - lastCacheTime < _cacheExpiration)
                    {
                        // Data is still fresh, try to get from cookie
                        var cachedData = await _cookieService.GetCookieAsync(_cacheKey);
                        if (!string.IsNullOrEmpty(cachedData))
                        {
                            try
                            {
                                var stats = JsonSerializer.Deserialize<DashboardStats>(cachedData);
                                if (stats != null)
                                {
                                    // Store back in memory cache for faster subsequent access
                                    _memoryCache.Set(_cacheKey, stats, _cacheExpiration);
                                    return stats;
                                }
                            }
                            catch (JsonException)
                            {
                                // Invalid cached data, continue to refresh
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // JavaScript interop not available (probably during prerendering)
                // Continue to fetch fresh data from database
            }

            // No valid cache found, fetch fresh data from database
            return await RefreshDashboardStatsAsync();
        }

        public async Task<DashboardStats> RefreshDashboardStatsAsync()
        {
            // Get all data in one optimized call instead of 4 separate calls
            var allStats = await GetOptimizedStatsAsync();

            // Cache in memory
            _memoryCache.Set(_cacheKey, allStats, _cacheExpiration);

            // Cache in browser cookie as backup (but handle prerendering gracefully)
            try
            {
                var jsonData = JsonSerializer.Serialize(allStats);
                await _cookieService.SetCookieAsync(_cacheKey, jsonData, (int)_cacheExpiration.TotalMinutes);
                await _cookieService.SetCookieAsync(_cookieKey, DateTime.Now.ToString(), (int)_cacheExpiration.TotalMinutes);
            }
            catch (InvalidOperationException)
            {
                // JavaScript interop not available (probably during prerendering)
                // This is fine, we'll just use memory cache
            }

            return allStats;
        }

        private async Task<DashboardStats> GetOptimizedStatsAsync()
        {
            // Use the new optimized single-query method instead of 4 separate calls
            return await Task.Run(() =>
            {
                try
                {
                    var (activeCount, newCount, usedCount, quarantinedCount) = _equipmentService.GetDashboardStatistics();
                    
                    return new DashboardStats
                    {
                        ActiveCount = activeCount,
                        NewCount = newCount,
                        UsedCount = usedCount,
                        QuarantinedCount = quarantinedCount,
                        LastUpdated = DateTime.Now
                    };
                }
                catch (Exception)
                {
                    // Log the error for debugging
                    
                    // Return default stats instead of throwing
                    return new DashboardStats
                    {
                        ActiveCount = 0,
                        NewCount = 0,
                        UsedCount = 0,
                        QuarantinedCount = 0,
                        LastUpdated = DateTime.Now
                    };
                }
            });
        }

        public void ClearCache()
        {
            _memoryCache.Remove(_cacheKey);
        }
    }
}

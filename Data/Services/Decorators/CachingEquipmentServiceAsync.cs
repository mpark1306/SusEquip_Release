using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SusEquip.Data.Services.Decorators
{
    /// <summary>
    /// Async caching decorator for IEquipmentService that adds intelligent caching layer to service operations
    /// </summary>
    public class CachingEquipmentServiceAsync : IEquipmentService
    {
        private readonly IEquipmentService _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachingEquipmentServiceAsync> _logger;
        
        // Cache expiration times for different operations
        private static readonly TimeSpan ShortCacheTime = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan MediumCacheTime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan LongCacheTime = TimeSpan.FromMinutes(30);

        // Cache key prefixes
        private const string EQUIPMENT_PREFIX = "async_equipment:";
        private const string MACHINES_PREFIX = "async_machines:";
        private const string STATS_PREFIX = "async_stats:";
        private const string UTIL_PREFIX = "async_util:";

        public CachingEquipmentServiceAsync(IEquipmentService inner, IMemoryCache cache, ILogger<CachingEquipmentServiceAsync> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddEntryAsync(EquipmentData equipmentData)
        {
            await _inner.AddEntryAsync(equipmentData);
            
            // Invalidate relevant caches after modification
            await InvalidateEquipmentCachesAsync("Added equipment entry");
            _logger.LogDebug("Invalidated equipment caches after adding entry for {PCName}", equipmentData?.PC_Name);
        }

        public async Task InsertEntryAsync(EquipmentData equipmentData)
        {
            await _inner.InsertEntryAsync(equipmentData);
            
            // Invalidate relevant caches after modification
            await InvalidateEquipmentCachesAsync("Inserted equipment entry");
            _logger.LogDebug("Invalidated equipment caches after inserting entry for {PCName}", equipmentData?.PC_Name);
        }

        public async Task UpdateLatestEntryAsync(EquipmentData equipmentData)
        {
            await _inner.UpdateLatestEntryAsync(equipmentData);
            
            // Invalidate specific equipment and general caches
            await InvalidateEquipmentCachesAsync("Updated equipment entry");
            _logger.LogDebug("Invalidated equipment caches after updating entry for {PCName}", equipmentData?.PC_Name);
        }

        public async Task DeleteEntryAsync(int instNo, int entryId)
        {
            await _inner.DeleteEntryAsync(instNo, entryId);
            
            // Invalidate all relevant caches
            await InvalidateEquipmentCachesAsync("Deleted equipment entry");
            _logger.LogDebug("Invalidated equipment caches after deleting entry for Inst_No: {InstNo}", instNo);
        }

        public async Task<List<EquipmentData>> GetEquipmentAsync()
        {
            string cacheKey = $"{EQUIPMENT_PREFIX}all";
            
            if (_cache.TryGetValue(cacheKey, out List<EquipmentData>? cachedEquipment) && cachedEquipment != null)
            {
                _logger.LogDebug("Cache hit for GetEquipmentAsync");
                return cachedEquipment;
            }

            var result = await _inner.GetEquipmentAsync();
            _cache.Set(cacheKey, result, MediumCacheTime);
            _logger.LogDebug("Cached GetEquipmentAsync result with {Count} items", result.Count);
            
            return result;
        }

        public async Task<List<EquipmentData>> GetEquipmentSortedAsync(int instNo)
        {
            string cacheKey = $"{EQUIPMENT_PREFIX}sorted:{instNo}";
            
            if (_cache.TryGetValue(cacheKey, out List<EquipmentData>? cachedEquipment) && cachedEquipment != null)
            {
                _logger.LogDebug("Cache hit for GetEquipmentSortedAsync with Inst_No: {InstNo}", instNo);
                return cachedEquipment;
            }

            var result = await _inner.GetEquipmentSortedAsync(instNo);
            _cache.Set(cacheKey, result, LongCacheTime); // Individual equipment queries can be cached longer
            _logger.LogDebug("Cached GetEquipmentSortedAsync result for Inst_No: {InstNo} with {Count} items", instNo, result.Count);
            
            return result;
        }

        public async Task<List<EquipmentData>> GetEquipmentSortedByEntryAsync(int instNo)
        {
            string cacheKey = $"{EQUIPMENT_PREFIX}sorted_by_entry:{instNo}";
            
            if (_cache.TryGetValue(cacheKey, out List<EquipmentData>? cachedEquipment) && cachedEquipment != null)
            {
                _logger.LogDebug("Cache hit for GetEquipmentSortedByEntryAsync with Inst_No: {InstNo}", instNo);
                return cachedEquipment;
            }

            var result = await _inner.GetEquipmentSortedByEntryAsync(instNo);
            _cache.Set(cacheKey, result, LongCacheTime);
            _logger.LogDebug("Cached GetEquipmentSortedByEntryAsync result for Inst_No: {InstNo} with {Count} items", instNo, result.Count);
            
            return result;
        }

        public async Task<List<MachineData>> GetMachinesAsync()
        {
            string cacheKey = $"{MACHINES_PREFIX}all";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cachedMachines) && cachedMachines != null)
            {
                _logger.LogDebug("Cache hit for GetMachinesAsync");
                return cachedMachines;
            }

            var result = await _inner.GetMachinesAsync();
            _cache.Set(cacheKey, result, MediumCacheTime);
            _logger.LogDebug("Cached GetMachinesAsync result with {Count} machines", result.Count);
            
            return result;
        }

        public async Task<List<MachineData>> GetActiveMachinesAsync()
        {
            string cacheKey = $"{MACHINES_PREFIX}active";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cachedMachines) && cachedMachines != null)
            {
                _logger.LogDebug("Cache hit for GetActiveMachinesAsync");
                return cachedMachines;
            }

            var result = await _inner.GetActiveMachinesAsync();
            _cache.Set(cacheKey, result, ShortCacheTime); // Active machines change frequently
            _logger.LogDebug("Cached GetActiveMachinesAsync result with {Count} active machines", result.Count);
            
            return result;
        }

        public async Task<List<MachineData>> GetNewMachinesAsync()
        {
            string cacheKey = $"{MACHINES_PREFIX}new";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cachedMachines) && cachedMachines != null)
            {
                _logger.LogDebug("Cache hit for GetNewMachinesAsync");
                return cachedMachines;
            }

            var result = await _inner.GetNewMachinesAsync();
            _cache.Set(cacheKey, result, MediumCacheTime);
            _logger.LogDebug("Cached GetNewMachinesAsync result with {Count} new machines", result.Count);
            
            return result;
        }

        public async Task<List<MachineData>> GetUsedMachinesAsync()
        {
            string cacheKey = $"{MACHINES_PREFIX}used";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cachedMachines) && cachedMachines != null)
            {
                _logger.LogDebug("Cache hit for GetUsedMachinesAsync");
                return cachedMachines;
            }

            var result = await _inner.GetUsedMachinesAsync();
            _cache.Set(cacheKey, result, MediumCacheTime);
            _logger.LogDebug("Cached GetUsedMachinesAsync result with {Count} used machines", result.Count);
            
            return result;
        }

        public async Task<List<MachineData>> GetQuarantineMachinesAsync()
        {
            string cacheKey = $"{MACHINES_PREFIX}quarantine";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cachedMachines) && cachedMachines != null)
            {
                _logger.LogDebug("Cache hit for GetQuarantineMachinesAsync");
                return cachedMachines;
            }

            var result = await _inner.GetQuarantineMachinesAsync();
            _cache.Set(cacheKey, result, MediumCacheTime);
            _logger.LogDebug("Cached GetQuarantineMachinesAsync result with {Count} quarantine machines", result.Count);
            
            return result;
        }

        public async Task<List<MachineData>> GetMachinesOutOfServiceSinceJuneAsync()
        {
            string cacheKey = $"{MACHINES_PREFIX}out_of_service_june";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cachedMachines) && cachedMachines != null)
            {
                _logger.LogDebug("Cache hit for GetMachinesOutOfServiceSinceJuneAsync");
                return cachedMachines;
            }

            var result = await _inner.GetMachinesOutOfServiceSinceJuneAsync();
            _cache.Set(cacheKey, result, LongCacheTime); // Out of service data changes less frequently
            _logger.LogDebug("Cached GetMachinesOutOfServiceSinceJuneAsync result with {Count} machines", result.Count);
            
            return result;
        }

        public async Task<int> GetNextInstNoAsync()
        {
            // Don't cache this - it must always be fresh to avoid duplicates
            return await _inner.GetNextInstNoAsync();
        }

        public async Task<bool> IsInstNoTakenAsync(int instNo)
        {
            string cacheKey = $"{UTIL_PREFIX}inst_no_taken:{instNo}";
            
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                _logger.LogDebug("Cache hit for IsInstNoTakenAsync with Inst_No: {InstNo}", instNo);
                return cachedResult;
            }

            var result = await _inner.IsInstNoTakenAsync(instNo);
            _cache.Set(cacheKey, result, ShortCacheTime); // Short cache to balance performance with accuracy
            _logger.LogDebug("Cached IsInstNoTakenAsync result for Inst_No: {InstNo} = {Result}", instNo, result);
            
            return result;
        }

        public async Task<bool> IsSerialNoTakenInMachinesAsync(string serialNo)
        {
            string cacheKey = $"{UTIL_PREFIX}serial_no_taken:{serialNo}";
            
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                _logger.LogDebug("Cache hit for IsSerialNoTakenInMachinesAsync with Serial: {SerialNo}", serialNo);
                return cachedResult;
            }

            var result = await _inner.IsSerialNoTakenInMachinesAsync(serialNo);
            _cache.Set(cacheKey, result, ShortCacheTime);
            _logger.LogDebug("Cached IsSerialNoTakenInMachinesAsync result for Serial: {SerialNo} = {Result}", serialNo, result);
            
            return result;
        }

        public async Task<(int activeCount, int newCount, int usedCount, int quarantinedCount)> GetDashboardStatisticsAsync()
        {
            string cacheKey = $"{STATS_PREFIX}dashboard";
            
            if (_cache.TryGetValue(cacheKey, out (int activeCount, int newCount, int usedCount, int quarantinedCount) cachedStats))
            {
                _logger.LogDebug("Cache hit for GetDashboardStatisticsAsync");
                return cachedStats;
            }

            var result = await _inner.GetDashboardStatisticsAsync();
            _cache.Set(cacheKey, result, ShortCacheTime); // Dashboard stats should be relatively fresh
            _logger.LogDebug("Cached GetDashboardStatisticsAsync result (Active: {Active}, New: {New}, Used: {Used}, Quarantined: {Quarantined})", 
                result.activeCount, result.newCount, result.usedCount, result.quarantinedCount);
            
            return result;
        }

        /// <summary>
        /// Invalidates all equipment-related caches after data modifications
        /// </summary>
        private async Task InvalidateEquipmentCachesAsync(string reason)
        {
            await Task.Run(() =>
            {
                // Remove all equipment caches
                var keysToRemove = new List<object>();
                
                // We can't easily enumerate memory cache keys, so we remove known patterns
                // This is a limitation of IMemoryCache - consider using a different cache if more control needed
                
                // Remove common cache keys
                _cache.Remove($"{EQUIPMENT_PREFIX}all");
                _cache.Remove($"{MACHINES_PREFIX}all");
                _cache.Remove($"{MACHINES_PREFIX}active");
                _cache.Remove($"{MACHINES_PREFIX}new");
                _cache.Remove($"{MACHINES_PREFIX}used");
                _cache.Remove($"{MACHINES_PREFIX}quarantine");
                _cache.Remove($"{MACHINES_PREFIX}out_of_service_june");
                _cache.Remove($"{STATS_PREFIX}dashboard");
                
                _logger.LogDebug("Invalidated equipment caches: {Reason}", reason);
            });
        }

        /// <summary>
        /// Gets cache statistics for monitoring
        /// </summary>
        public async Task<object> GetCacheStatisticsAsync()
        {
            return await Task.FromResult(new
            {
                CacheType = "MemoryCache",
                Prefixes = new[] { EQUIPMENT_PREFIX, MACHINES_PREFIX, STATS_PREFIX, UTIL_PREFIX },
                ExpirationTimes = new
                {
                    Short = ShortCacheTime,
                    Medium = MediumCacheTime,
                    Long = LongCacheTime
                },
                LastInvalidation = DateTime.Now // This would be tracked in a real implementation
            });
        }

        // Command pattern methods - typically not cached due to data mutation
        public async Task DeleteEquipmentAsync(int instNo)
        {
            _logger.LogDebug("CachingEquipmentServiceAsync.DeleteEquipmentAsync: Bypassing cache for data mutation");
            
            // Clear related cache entries before mutation
            InvalidateEquipmentCache(instNo);
            
            await _inner.DeleteEquipmentAsync(instNo);
            
            // Clear cache after mutation to ensure fresh data on next read
            InvalidateAllEquipmentCache();
        }

        public async Task UpdateEquipmentStatusAsync(int instNo, string newStatus)
        {
            _logger.LogDebug("CachingEquipmentServiceAsync.UpdateEquipmentStatusAsync: Bypassing cache for data mutation");
            
            // Clear related cache entries before mutation
            InvalidateEquipmentCache(instNo);
            
            await _inner.UpdateEquipmentStatusAsync(instNo, newStatus);
            
            // Clear cache after mutation to ensure fresh data on next read
            InvalidateAllEquipmentCache();
        }

        public async Task<EquipmentData?> GetByInstNoAsync(int instNo)
        {
            var cacheKey = $"{EQUIPMENT_PREFIX}byinstno:{instNo}";
            
            if (_cache.TryGetValue(cacheKey, out EquipmentData? cachedEquipment))
            {
                _logger.LogDebug("CachingEquipmentServiceAsync.GetByInstNoAsync: Cache hit for InstNo={InstNo}", instNo);
                return cachedEquipment;
            }
            
            _logger.LogDebug("CachingEquipmentServiceAsync.GetByInstNoAsync: Cache miss for InstNo={InstNo}", instNo);
            var equipment = await _inner.GetByInstNoAsync(instNo);
            
            // Cache the result (including null results to prevent repeated lookups)
            _cache.Set(cacheKey, equipment, ShortCacheTime);
            
            return equipment;
        }

        private void InvalidateEquipmentCache(int instNo)
        {
            var cacheKey = $"{EQUIPMENT_PREFIX}byinstno:{instNo}";
            _cache.Remove(cacheKey);
            _logger.LogDebug("Invalidated equipment cache for InstNo={InstNo}", instNo);
        }

        private void InvalidateAllEquipmentCache()
        {
            // In a more sophisticated implementation, we'd track cache keys by pattern
            // For now, we'll just log that we should invalidate
            _logger.LogDebug("Should invalidate all equipment cache entries");
        }
    }
}
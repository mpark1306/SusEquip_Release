using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SusEquip.Data.Services.Decorators
{
    /// <summary>
    /// Synchronous caching decorator for IEquipmentServiceSync that adds intelligent caching layer to service operations
    /// </summary>
    public class CachingEquipmentService : IEquipmentServiceSync
    {
        private readonly IEquipmentServiceSync _inner;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachingEquipmentService> _logger;
        
        // Cache expiration times for different operations
        private static readonly TimeSpan ShortCacheTime = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MediumCacheTime = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan LongCacheTime = TimeSpan.FromHours(1);

        // Cache key prefixes
        private const string EQUIPMENT_PREFIX = "equipment:";
        private const string MACHINES_PREFIX = "machines:";
        private const string STATS_PREFIX = "stats:";
        private const string UTIL_PREFIX = "util:";

        public CachingEquipmentService(IEquipmentServiceSync inner, IMemoryCache cache, ILogger<CachingEquipmentService> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddEntry(EquipmentData equipmentData)
        {
            _inner.AddEntry(equipmentData);
            
            // Invalidate relevant caches after modification
            InvalidateEquipmentCaches("Added equipment entry");
            _logger.LogDebug("Invalidated equipment caches after adding entry for {PCName}", equipmentData?.PC_Name);
        }

        public void InsertEntry(EquipmentData equipmentData)
        {
            _inner.InsertEntry(equipmentData);
            
            // Invalidate relevant caches after modification
            InvalidateEquipmentCaches("Inserted equipment entry");
            _logger.LogDebug("Invalidated equipment caches after inserting entry for {PCName}", equipmentData?.PC_Name);
        }

        public void UpdateLatestEntry(EquipmentData equipmentData)
        {
            _inner.UpdateLatestEntry(equipmentData);
            
            // Invalidate relevant caches after modification
            InvalidateEquipmentCaches("Updated equipment entry");
            _logger.LogDebug("Invalidated equipment caches after updating entry for {PCName}", equipmentData?.PC_Name);
        }

        public void DeleteEntry(int inst_no, int entry_id)
        {
            _inner.DeleteEntry(inst_no, entry_id);
            
            // Invalidate relevant caches after modification
            InvalidateEquipmentCaches("Deleted equipment entry");
            _logger.LogDebug("Invalidated equipment caches after deleting entry for Inst_No: {InstNo}", inst_no);
        }

        public List<EquipmentData> GetEquipment()
        {
            const string cacheKey = EQUIPMENT_PREFIX + "all";
            
            if (_cache.TryGetValue(cacheKey, out List<EquipmentData>? cached))
            {
                _logger.LogDebug("Cache hit for all equipment data");
                return cached ?? new List<EquipmentData>();
            }

            _logger.LogDebug("Cache miss for all equipment data, fetching from service");
            var result = _inner.GetEquipment();
            _cache.Set(cacheKey, result, MediumCacheTime);
            return result ?? new List<EquipmentData>();
        }

        public List<EquipmentData> GetEquipmentSorted(int inst_no)
        {
            string cacheKey = $"{EQUIPMENT_PREFIX}sorted:{inst_no}";
            
            if (_cache.TryGetValue(cacheKey, out List<EquipmentData>? cached))
            {
                _logger.LogDebug("Cache hit for sorted equipment data for Inst_No: {InstNo}", inst_no);
                return cached ?? new List<EquipmentData>();
            }

            _logger.LogDebug("Cache miss for sorted equipment data for Inst_No: {InstNo}, fetching from service", inst_no);
            var result = _inner.GetEquipmentSorted(inst_no);
            _cache.Set(cacheKey, result, MediumCacheTime);
            return result ?? new List<EquipmentData>();
        }

        public List<EquipmentData> GetEquipSortedByEntry(int inst_no)
        {
            string cacheKey = $"{EQUIPMENT_PREFIX}sortedByEntry:{inst_no}";
            
            if (_cache.TryGetValue(cacheKey, out List<EquipmentData>? cached))
            {
                _logger.LogDebug("Cache hit for equipment data sorted by entry for Inst_No: {InstNo}", inst_no);
                return cached ?? new List<EquipmentData>();
            }

            _logger.LogDebug("Cache miss for equipment data sorted by entry for Inst_No: {InstNo}, fetching from service", inst_no);
            var result = _inner.GetEquipSortedByEntry(inst_no);
            _cache.Set(cacheKey, result, MediumCacheTime);
            return result ?? new List<EquipmentData>();
        }

        public List<MachineData> GetMachines()
        {
            const string cacheKey = MACHINES_PREFIX + "all";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cached))
            {
                _logger.LogDebug("Cache hit for all machines data");
                return cached ?? new List<MachineData>();
            }

            _logger.LogDebug("Cache miss for all machines data, fetching from service");
            var result = _inner.GetMachines();
            _cache.Set(cacheKey, result, LongCacheTime); // Machines change less frequently
            return result ?? new List<MachineData>();
        }

        public List<MachineData> GetActiveMachines()
        {
            const string cacheKey = MACHINES_PREFIX + "active";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cached))
            {
                _logger.LogDebug("Cache hit for active machines data");
                return cached ?? new List<MachineData>();
            }

            _logger.LogDebug("Cache miss for active machines data, fetching from service");
            var result = _inner.GetActiveMachines();
            _cache.Set(cacheKey, result, MediumCacheTime);
            return result ?? new List<MachineData>();
        }

        public List<MachineData> GetNewMachines()
        {
            const string cacheKey = MACHINES_PREFIX + "new";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cached))
            {
                _logger.LogDebug("Cache hit for new machines data");
                return cached ?? new List<MachineData>();
            }

            _logger.LogDebug("Cache miss for new machines data, fetching from service");
            var result = _inner.GetNewMachines();
            _cache.Set(cacheKey, result, MediumCacheTime);
            return result ?? new List<MachineData>();
        }

        public List<MachineData> GetUsedMachines()
        {
            const string cacheKey = MACHINES_PREFIX + "used";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cached))
            {
                _logger.LogDebug("Cache hit for used machines data");
                return cached ?? new List<MachineData>();
            }

            _logger.LogDebug("Cache miss for used machines data, fetching from service");
            var result = _inner.GetUsedMachines();
            _cache.Set(cacheKey, result, MediumCacheTime);
            return result ?? new List<MachineData>();
        }

        public List<MachineData> GetQuarantineMachines()
        {
            const string cacheKey = MACHINES_PREFIX + "quarantine";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cached))
            {
                _logger.LogDebug("Cache hit for quarantine machines data");
                return cached ?? new List<MachineData>();
            }

            _logger.LogDebug("Cache miss for quarantine machines data, fetching from service");
            var result = _inner.GetQuarantineMachines();
            _cache.Set(cacheKey, result, MediumCacheTime);
            return result ?? new List<MachineData>();
        }

        public List<MachineData> GetMachinesOutOfServiceSinceJune()
        {
            const string cacheKey = MACHINES_PREFIX + "outOfServiceSinceJune";
            
            if (_cache.TryGetValue(cacheKey, out List<MachineData>? cached))
            {
                _logger.LogDebug("Cache hit for machines out of service since June data");
                return cached ?? new List<MachineData>();
            }

            _logger.LogDebug("Cache miss for machines out of service since June data, fetching from service");
            var result = _inner.GetMachinesOutOfServiceSinceJune();
            _cache.Set(cacheKey, result, LongCacheTime); // This data changes infrequently
            return result ?? new List<MachineData>();
        }

        public int GetNextInstNo()
        {
            // Don't cache GetNextInstNo as it should always return a fresh value
            _logger.LogDebug("Getting next InstNo (not cached)");
            return _inner.GetNextInstNo();
        }

        public bool IsInstNoTaken(int instNo)
        {
            string cacheKey = $"{UTIL_PREFIX}instNoTaken:{instNo}";
            
            if (_cache.TryGetValue(cacheKey, out bool? cached))
            {
                _logger.LogDebug("Cache hit for InstNo taken check: {InstNo}", instNo);
                return cached ?? false;
            }

            _logger.LogDebug("Cache miss for InstNo taken check: {InstNo}, fetching from service", instNo);
            var result = _inner.IsInstNoTaken(instNo);
            _cache.Set(cacheKey, result, ShortCacheTime); // Short cache as this can change frequently
            return result;
        }

        public bool IsSerialNoTakenInMachines(string serialNo)
        {
            if (string.IsNullOrEmpty(serialNo))
            {
                return false;
            }

            string cacheKey = $"{UTIL_PREFIX}serialNoTaken:{serialNo}";
            
            if (_cache.TryGetValue(cacheKey, out bool? cached))
            {
                _logger.LogDebug("Cache hit for SerialNo taken check: {SerialNo}", serialNo);
                return cached ?? false;
            }

            _logger.LogDebug("Cache miss for SerialNo taken check: {SerialNo}, fetching from service", serialNo);
            var result = _inner.IsSerialNoTakenInMachines(serialNo);
            _cache.Set(cacheKey, result, ShortCacheTime); // Short cache as this can change frequently
            return result;
        }

        public (int activeCount, int newCount, int usedCount, int quarantinedCount) GetDashboardStatistics()
        {
            const string cacheKey = STATS_PREFIX + "dashboard";
            
            if (_cache.TryGetValue(cacheKey, out (int, int, int, int)? cached))
            {
                _logger.LogDebug("Cache hit for dashboard statistics");
                return cached ?? (0, 0, 0, 0);
            }

            _logger.LogDebug("Cache miss for dashboard statistics, fetching from service");
            var result = _inner.GetDashboardStatistics();
            _cache.Set(cacheKey, result, ShortCacheTime); // Short cache for frequently updated stats
            return result;
        }

        /// <summary>
        /// Invalidates all equipment-related caches
        /// </summary>
        private void InvalidateEquipmentCaches(string reason)
        {
            try
            {
                // Since IMemoryCache doesn't provide a way to enumerate keys,
                // we'll use a simpler approach and just remove commonly used cache keys
                var keysToInvalidate = new[]
                {
                    // Equipment keys
                    EQUIPMENT_PREFIX + "all",
                    
                    // Machine keys  
                    MACHINES_PREFIX + "all",
                    MACHINES_PREFIX + "active",
                    MACHINES_PREFIX + "new",
                    MACHINES_PREFIX + "used",
                    MACHINES_PREFIX + "quarantine",
                    MACHINES_PREFIX + "outOfServiceSinceJune",
                    
                    // Statistics keys
                    STATS_PREFIX + "dashboard"
                };

                foreach (var key in keysToInvalidate)
                {
                    _cache.Remove(key);
                }

                // Note: We can't easily invalidate inst_no specific keys without maintaining our own key tracking
                _logger.LogDebug("Cache invalidation completed for common keys, reason: {Reason}", reason);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate caches for reason: {Reason}", reason);
                // Don't throw - cache invalidation failure shouldn't break the operation
            }
        }
    }
}
using Microsoft.Extensions.Logging;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SusEquip.Data.Services.Decorators
{
    /// <summary>
    /// Performance monitoring decorator for IEquipmentService that adds performance tracking and alerting
    /// </summary>
    public class PerformanceMonitoringEquipmentService : IEquipmentService
    {
        private readonly IEquipmentService _inner;
        private readonly ILogger<PerformanceMonitoringEquipmentService> _logger;
        private static readonly ActivitySource ActivitySource = new("SusEquip.PerformanceMonitoring");

        // Performance thresholds (in milliseconds)
        private const long FastThreshold = 100;
        private const long SlowThreshold = 1000;
        private const long CriticalThreshold = 5000;

        public PerformanceMonitoringEquipmentService(IEquipmentService inner, ILogger<PerformanceMonitoringEquipmentService> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddEntryAsync(EquipmentData equipmentData)
        {
            using var activity = ActivitySource.StartActivity("AddEntryAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                activity?.SetTag("equipment.pc_name", equipmentData?.PC_Name);
                
                await _inner.AddEntryAsync(equipmentData);
                
                stopwatch.Stop();
                LogPerformance("AddEntryAsync", stopwatch.ElapsedMilliseconds, equipmentData?.PC_Name);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in AddEntryAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task InsertEntryAsync(EquipmentData equipmentData)
        {
            using var activity = ActivitySource.StartActivity("InsertEntryAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                activity?.SetTag("equipment.pc_name", equipmentData?.PC_Name);
                
                await _inner.InsertEntryAsync(equipmentData);
                
                stopwatch.Stop();
                LogPerformance("InsertEntryAsync", stopwatch.ElapsedMilliseconds, equipmentData?.PC_Name);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in InsertEntryAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task UpdateLatestEntryAsync(EquipmentData equipmentData)
        {
            using var activity = ActivitySource.StartActivity("UpdateLatestEntryAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                activity?.SetTag("equipment.pc_name", equipmentData?.PC_Name);
                
                await _inner.UpdateLatestEntryAsync(equipmentData);
                
                stopwatch.Stop();
                LogPerformance("UpdateLatestEntryAsync", stopwatch.ElapsedMilliseconds, equipmentData?.PC_Name);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in UpdateLatestEntryAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task DeleteEntryAsync(int instNo, int entryId)
        {
            using var activity = ActivitySource.StartActivity("DeleteEntryAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                activity?.SetTag("equipment.inst_no", instNo);
                activity?.SetTag("equipment.entry_id", entryId);
                
                await _inner.DeleteEntryAsync(instNo, entryId);
                
                stopwatch.Stop();
                LogPerformance("DeleteEntryAsync", stopwatch.ElapsedMilliseconds, instNo);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in DeleteEntryAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<EquipmentData>> GetEquipmentAsync()
        {
            using var activity = ActivitySource.StartActivity("GetEquipmentAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await _inner.GetEquipmentAsync();
                
                stopwatch.Stop();
                LogPerformance("GetEquipmentAsync", stopwatch.ElapsedMilliseconds, result.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetEquipmentAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<EquipmentData>> GetEquipmentSortedAsync(int instNo)
        {
            using var activity = ActivitySource.StartActivity("GetEquipmentSortedAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                activity?.SetTag("equipment.inst_no", instNo);
                
                var result = await _inner.GetEquipmentSortedAsync(instNo);
                
                stopwatch.Stop();
                LogPerformance("GetEquipmentSortedAsync", stopwatch.ElapsedMilliseconds, $"InstNo:{instNo},Count:{result.Count}");
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetEquipmentSortedAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<EquipmentData>> GetEquipmentSortedByEntryAsync(int instNo)
        {
            using var activity = ActivitySource.StartActivity("GetEquipmentSortedByEntryAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                activity?.SetTag("equipment.inst_no", instNo);
                
                var result = await _inner.GetEquipmentSortedByEntryAsync(instNo);
                
                stopwatch.Stop();
                LogPerformance("GetEquipmentSortedByEntryAsync", stopwatch.ElapsedMilliseconds, $"InstNo:{instNo},Count:{result.Count}");
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetEquipmentSortedByEntryAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetMachinesAsync()
        {
            using var activity = ActivitySource.StartActivity("GetMachinesAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await _inner.GetMachinesAsync();
                
                stopwatch.Stop();
                LogPerformance("GetMachinesAsync", stopwatch.ElapsedMilliseconds, result.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetMachinesAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetActiveMachinesAsync()
        {
            using var activity = ActivitySource.StartActivity("GetActiveMachinesAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await _inner.GetActiveMachinesAsync();
                
                stopwatch.Stop();
                LogPerformance("GetActiveMachinesAsync", stopwatch.ElapsedMilliseconds, result.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetActiveMachinesAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetNewMachinesAsync()
        {
            using var activity = ActivitySource.StartActivity("GetNewMachinesAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await _inner.GetNewMachinesAsync();
                
                stopwatch.Stop();
                LogPerformance("GetNewMachinesAsync", stopwatch.ElapsedMilliseconds, result.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetNewMachinesAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetUsedMachinesAsync()
        {
            using var activity = ActivitySource.StartActivity("GetUsedMachinesAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await _inner.GetUsedMachinesAsync();
                
                stopwatch.Stop();
                LogPerformance("GetUsedMachinesAsync", stopwatch.ElapsedMilliseconds, result.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetUsedMachinesAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetQuarantineMachinesAsync()
        {
            using var activity = ActivitySource.StartActivity("GetQuarantineMachinesAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await _inner.GetQuarantineMachinesAsync();
                
                stopwatch.Stop();
                LogPerformance("GetQuarantineMachinesAsync", stopwatch.ElapsedMilliseconds, result.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetQuarantineMachinesAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetMachinesOutOfServiceSinceJuneAsync()
        {
            using var activity = ActivitySource.StartActivity("GetMachinesOutOfServiceSinceJuneAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await _inner.GetMachinesOutOfServiceSinceJuneAsync();
                
                stopwatch.Stop();
                LogPerformance("GetMachinesOutOfServiceSinceJuneAsync", stopwatch.ElapsedMilliseconds, result.Count);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetMachinesOutOfServiceSinceJuneAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<int> GetNextInstNoAsync()
        {
            using var activity = ActivitySource.StartActivity("GetNextInstNoAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await _inner.GetNextInstNoAsync();
                
                stopwatch.Stop();
                LogPerformance("GetNextInstNoAsync", stopwatch.ElapsedMilliseconds, result);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetNextInstNoAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> IsInstNoTakenAsync(int instNo)
        {
            using var activity = ActivitySource.StartActivity("IsInstNoTakenAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                activity?.SetTag("equipment.inst_no", instNo);
                
                var result = await _inner.IsInstNoTakenAsync(instNo);
                
                stopwatch.Stop();
                LogPerformance("IsInstNoTakenAsync", stopwatch.ElapsedMilliseconds, $"InstNo:{instNo},Result:{result}");
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in IsInstNoTakenAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> IsSerialNoTakenInMachinesAsync(string serialNo)
        {
            using var activity = ActivitySource.StartActivity("IsSerialNoTakenInMachinesAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                activity?.SetTag("equipment.serial_no", serialNo);
                
                var result = await _inner.IsSerialNoTakenInMachinesAsync(serialNo);
                
                stopwatch.Stop();
                LogPerformance("IsSerialNoTakenInMachinesAsync", stopwatch.ElapsedMilliseconds, $"SerialNo:{serialNo},Result:{result}");
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in IsSerialNoTakenInMachinesAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<(int activeCount, int newCount, int usedCount, int quarantinedCount)> GetDashboardStatisticsAsync()
        {
            using var activity = ActivitySource.StartActivity("GetDashboardStatisticsAsync");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await _inner.GetDashboardStatisticsAsync();
                
                stopwatch.Stop();
                LogPerformance("GetDashboardStatisticsAsync", stopwatch.ElapsedMilliseconds, 
                    $"Active:{result.activeCount},New:{result.newCount},Used:{result.usedCount},Quarantined:{result.quarantinedCount}");
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetDashboardStatisticsAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Logs performance metrics and categorizes based on execution time thresholds
        /// </summary>
        private void LogPerformance(string methodName, long elapsedMs, object? context = null)
        {
            var contextInfo = context?.ToString() ?? "N/A";
            
            if (elapsedMs <= FastThreshold)
            {
                _logger.LogTrace("⚡ Fast execution: {MethodName} completed in {ElapsedMs}ms. Context: {Context}", 
                    methodName, elapsedMs, contextInfo);
            }
            else if (elapsedMs <= SlowThreshold)
            {
                _logger.LogDebug("🟡 Normal execution: {MethodName} completed in {ElapsedMs}ms. Context: {Context}", 
                    methodName, elapsedMs, contextInfo);
            }
            else if (elapsedMs <= CriticalThreshold)
            {
                _logger.LogWarning("🟠 Slow execution detected: {MethodName} took {ElapsedMs}ms. Context: {Context}. Consider optimization.", 
                    methodName, elapsedMs, contextInfo);
            }
            else
            {
                _logger.LogError("🔴 Critical performance issue: {MethodName} took {ElapsedMs}ms. Context: {Context}. Immediate attention required!", 
                    methodName, elapsedMs, contextInfo);
            }
            
            // Add to activity for distributed tracing
            Activity.Current?.SetTag("performance.elapsed_ms", elapsedMs);
            Activity.Current?.SetTag("performance.category", GetPerformanceCategory(elapsedMs));
        }

        /// <summary>
        /// Gets performance category based on elapsed time
        /// </summary>
        private string GetPerformanceCategory(long elapsedMs)
        {
            return elapsedMs switch
            {
                <= FastThreshold => "fast",
                <= SlowThreshold => "normal",
                <= CriticalThreshold => "slow",
                _ => "critical"
            };
        }

        // Command pattern methods
        public async Task DeleteEquipmentAsync(int instNo)
        {
            using var activity = ActivitySource.StartActivity("DeleteEquipmentAsync");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _inner.DeleteEquipmentAsync(instNo);
                stopwatch.Stop();
                LogPerformance("DeleteEquipmentAsync", stopwatch.ElapsedMilliseconds, $"InstNo={instNo}");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in DeleteEquipmentAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task UpdateEquipmentStatusAsync(int instNo, string newStatus)
        {
            using var activity = ActivitySource.StartActivity("UpdateEquipmentStatusAsync");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _inner.UpdateEquipmentStatusAsync(instNo, newStatus);
                stopwatch.Stop();
                LogPerformance("UpdateEquipmentStatusAsync", stopwatch.ElapsedMilliseconds, $"InstNo={instNo}, Status={newStatus}");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in UpdateEquipmentStatusAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<EquipmentData?> GetByInstNoAsync(int instNo)
        {
            using var activity = ActivitySource.StartActivity("GetByInstNoAsync");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _inner.GetByInstNoAsync(instNo);
                stopwatch.Stop();
                LogPerformance("GetByInstNoAsync", stopwatch.ElapsedMilliseconds, $"InstNo={instNo}");
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Performance monitoring error in GetByInstNoAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Gets current performance thresholds for monitoring
        /// </summary>
        public static (long Fast, long Slow, long Critical) GetThresholds()
        {
            return (FastThreshold, SlowThreshold, CriticalThreshold);
        }
    }
}
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
    /// Async logging decorator for IEquipmentService that adds comprehensive logging and performance monitoring
    /// </summary>
    public class LoggingEquipmentServiceAsync : IEquipmentService
    {
        private readonly IEquipmentService _inner;
        private readonly ILogger<LoggingEquipmentServiceAsync> _logger;

        public LoggingEquipmentServiceAsync(IEquipmentService inner, ILogger<LoggingEquipmentServiceAsync> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AddEntryAsync(EquipmentData equipmentData)
        {
            var stopwatch = Stopwatch.StartNew();
            var pcName = equipmentData?.PC_Name ?? "null";
            var instNo = equipmentData?.Inst_No ?? -1;

            _logger.LogInformation("Starting AddEntryAsync for equipment {PCName} (Inst_No: {InstNo})", pcName, instNo);

            try
            {
                if (equipmentData == null)
                {
                    _logger.LogWarning("AddEntryAsync called with null equipment data");
                    throw new ArgumentNullException(nameof(equipmentData));
                }
                
                await _inner.AddEntryAsync(equipmentData);
                
                _logger.LogInformation("Successfully completed AddEntryAsync for equipment {PCName} (Inst_No: {InstNo}) in {ElapsedMs}ms", 
                    pcName, instNo, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed AddEntryAsync for equipment {PCName} (Inst_No: {InstNo}) after {ElapsedMs}ms", 
                    pcName, instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task InsertEntryAsync(EquipmentData equipmentData)
        {
            var stopwatch = Stopwatch.StartNew();
            var pcName = equipmentData?.PC_Name ?? "null";

            _logger.LogInformation("Starting InsertEntryAsync for equipment {PCName}", pcName);

            try
            {
                await _inner.InsertEntryAsync(equipmentData);
                _logger.LogInformation("Successfully completed InsertEntryAsync for equipment {PCName} in {ElapsedMs}ms", 
                    pcName, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed InsertEntryAsync for equipment {PCName} after {ElapsedMs}ms", 
                    pcName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task UpdateLatestEntryAsync(EquipmentData equipmentData)
        {
            var stopwatch = Stopwatch.StartNew();
            var pcName = equipmentData?.PC_Name ?? "null";
            var instNo = equipmentData?.Inst_No ?? -1;

            _logger.LogInformation("Starting UpdateLatestEntryAsync for equipment {PCName} (Inst_No: {InstNo})", pcName, instNo);

            try
            {
                await _inner.UpdateLatestEntryAsync(equipmentData);
                _logger.LogInformation("Successfully completed UpdateLatestEntryAsync for equipment {PCName} (Inst_No: {InstNo}) in {ElapsedMs}ms", 
                    pcName, instNo, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed UpdateLatestEntryAsync for equipment {PCName} (Inst_No: {InstNo}) after {ElapsedMs}ms", 
                    pcName, instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task DeleteEntryAsync(int instNo, int entryId)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting DeleteEntryAsync for Inst_No: {InstNo}, Entry_Id: {EntryId}", instNo, entryId);

            try
            {
                await _inner.DeleteEntryAsync(instNo, entryId);
                _logger.LogInformation("Successfully completed DeleteEntryAsync for Inst_No: {InstNo}, Entry_Id: {EntryId} in {ElapsedMs}ms", 
                    instNo, entryId, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed DeleteEntryAsync for Inst_No: {InstNo}, Entry_Id: {EntryId} after {ElapsedMs}ms", 
                    instNo, entryId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<EquipmentData>> GetEquipmentAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetEquipmentAsync");

            try
            {
                var result = await _inner.GetEquipmentAsync();
                _logger.LogDebug("Successfully completed GetEquipmentAsync returning {Count} items in {ElapsedMs}ms", 
                    result.Count, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed GetEquipmentAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<EquipmentData>> GetEquipmentSortedAsync(int instNo)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetEquipmentSortedAsync for Inst_No: {InstNo}", instNo);

            try
            {
                var result = await _inner.GetEquipmentSortedAsync(instNo);
                _logger.LogDebug("Successfully completed GetEquipmentSortedAsync for Inst_No: {InstNo} returning {Count} items in {ElapsedMs}ms", 
                    instNo, result.Count, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed GetEquipmentSortedAsync for Inst_No: {InstNo} after {ElapsedMs}ms", 
                    instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<EquipmentData>> GetEquipmentSortedByEntryAsync(int instNo)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetEquipmentSortedByEntryAsync for Inst_No: {InstNo}", instNo);

            try
            {
                var result = await _inner.GetEquipmentSortedByEntryAsync(instNo);
                _logger.LogDebug("Successfully completed GetEquipmentSortedByEntryAsync for Inst_No: {InstNo} returning {Count} items in {ElapsedMs}ms", 
                    instNo, result.Count, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed GetEquipmentSortedByEntryAsync for Inst_No: {InstNo} after {ElapsedMs}ms", 
                    instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetMachinesAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetMachinesAsync");

            try
            {
                var result = await _inner.GetMachinesAsync();
                _logger.LogDebug("Successfully completed GetMachinesAsync returning {Count} machines in {ElapsedMs}ms", 
                    result.Count, stopwatch.ElapsedMilliseconds);
                
                if (stopwatch.ElapsedMilliseconds > 1000) // Log slow operations
                {
                    _logger.LogWarning("GetMachinesAsync took {ElapsedMs}ms - consider optimization", stopwatch.ElapsedMilliseconds);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed GetMachinesAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetActiveMachinesAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetActiveMachinesAsync");

            try
            {
                var result = await _inner.GetActiveMachinesAsync();
                _logger.LogDebug("Successfully completed GetActiveMachinesAsync returning {Count} active machines in {ElapsedMs}ms", 
                    result.Count, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed GetActiveMachinesAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetNewMachinesAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetNewMachinesAsync");

            try
            {
                var result = await _inner.GetNewMachinesAsync();
                _logger.LogDebug("Successfully completed GetNewMachinesAsync returning {Count} new machines in {ElapsedMs}ms", 
                    result.Count, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed GetNewMachinesAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetUsedMachinesAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetUsedMachinesAsync");

            try
            {
                var result = await _inner.GetUsedMachinesAsync();
                _logger.LogDebug("Successfully completed GetUsedMachinesAsync returning {Count} used machines in {ElapsedMs}ms", 
                    result.Count, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed GetUsedMachinesAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetQuarantineMachinesAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetQuarantineMachinesAsync");

            try
            {
                var result = await _inner.GetQuarantineMachinesAsync();
                _logger.LogDebug("Successfully completed GetQuarantineMachinesAsync returning {Count} quarantine machines in {ElapsedMs}ms", 
                    result.Count, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed GetQuarantineMachinesAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<List<MachineData>> GetMachinesOutOfServiceSinceJuneAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetMachinesOutOfServiceSinceJuneAsync");

            try
            {
                var result = await _inner.GetMachinesOutOfServiceSinceJuneAsync();
                _logger.LogDebug("Successfully completed GetMachinesOutOfServiceSinceJuneAsync returning {Count} machines in {ElapsedMs}ms", 
                    result.Count, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed GetMachinesOutOfServiceSinceJuneAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<int> GetNextInstNoAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetNextInstNoAsync");

            try
            {
                var result = await _inner.GetNextInstNoAsync();
                _logger.LogDebug("Successfully completed GetNextInstNoAsync returning {InstNo} in {ElapsedMs}ms", 
                    result, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed GetNextInstNoAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> IsInstNoTakenAsync(int instNo)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting IsInstNoTakenAsync for Inst_No: {InstNo}", instNo);

            try
            {
                var result = await _inner.IsInstNoTakenAsync(instNo);
                _logger.LogDebug("Successfully completed IsInstNoTakenAsync for Inst_No: {InstNo} returning {Result} in {ElapsedMs}ms", 
                    instNo, result, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed IsInstNoTakenAsync for Inst_No: {InstNo} after {ElapsedMs}ms", 
                    instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<bool> IsSerialNoTakenInMachinesAsync(string serialNo)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting IsSerialNoTakenInMachinesAsync for Serial: {SerialNo}", serialNo);

            try
            {
                var result = await _inner.IsSerialNoTakenInMachinesAsync(serialNo);
                _logger.LogDebug("Successfully completed IsSerialNoTakenInMachinesAsync for Serial: {SerialNo} returning {Result} in {ElapsedMs}ms", 
                    serialNo, result, stopwatch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed IsSerialNoTakenInMachinesAsync for Serial: {SerialNo} after {ElapsedMs}ms", 
                    serialNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<(int activeCount, int newCount, int usedCount, int quarantinedCount)> GetDashboardStatisticsAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetDashboardStatisticsAsync");

            try
            {
                var result = await _inner.GetDashboardStatisticsAsync();
                _logger.LogDebug("Successfully completed GetDashboardStatisticsAsync returning stats (Active: {Active}, New: {New}, Used: {Used}, Quarantined: {Quarantined}) in {ElapsedMs}ms", 
                    result.activeCount, result.newCount, result.usedCount, result.quarantinedCount, stopwatch.ElapsedMilliseconds);
                
                if (stopwatch.ElapsedMilliseconds > 2000) // Dashboard stats should be fast
                {
                    _logger.LogWarning("GetDashboardStatisticsAsync took {ElapsedMs}ms - consider caching", stopwatch.ElapsedMilliseconds);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed GetDashboardStatisticsAsync after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        // Command pattern methods
        public async Task DeleteEquipmentAsync(int instNo)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Starting DeleteEquipmentAsync for InstNo={InstNo}", instNo);
                await _inner.DeleteEquipmentAsync(instNo);
                stopwatch.Stop();
                _logger.LogInformation("Completed DeleteEquipmentAsync for InstNo={InstNo} in {ElapsedMs}ms", instNo, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed DeleteEquipmentAsync for InstNo={InstNo} after {ElapsedMs}ms", instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task UpdateEquipmentStatusAsync(int instNo, string newStatus)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Starting UpdateEquipmentStatusAsync for InstNo={InstNo}, Status={Status}", instNo, newStatus);
                await _inner.UpdateEquipmentStatusAsync(instNo, newStatus);
                stopwatch.Stop();
                _logger.LogInformation("Completed UpdateEquipmentStatusAsync for InstNo={InstNo} in {ElapsedMs}ms", instNo, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed UpdateEquipmentStatusAsync for InstNo={InstNo} after {ElapsedMs}ms", instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public async Task<EquipmentData?> GetByInstNoAsync(int instNo)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Starting GetByInstNoAsync for InstNo={InstNo}", instNo);
                var result = await _inner.GetByInstNoAsync(instNo);
                stopwatch.Stop();
                _logger.LogDebug("Completed GetByInstNoAsync for InstNo={InstNo} in {ElapsedMs}ms, Found={Found}", 
                    instNo, stopwatch.ElapsedMilliseconds, result != null);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed GetByInstNoAsync for InstNo={InstNo} after {ElapsedMs}ms", instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
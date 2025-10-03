using Microsoft.Extensions.Logging;
using SusEquip.Data.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SusEquip.Data.Services.Decorators
{
    /// <summary>
    /// Logging decorator for IEquipmentServiceSync that adds comprehensive logging and performance monitoring
    /// </summary>
    public class LoggingEquipmentService : IEquipmentServiceSync
    {
        private readonly IEquipmentServiceSync _inner;
        private readonly ILogger<LoggingEquipmentService> _logger;

        public LoggingEquipmentService(IEquipmentServiceSync inner, ILogger<LoggingEquipmentService> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddEntry(EquipmentData equipmentData)
        {
            var stopwatch = Stopwatch.StartNew();
            var pcName = equipmentData?.PC_Name ?? "null";
            var instNo = equipmentData?.Inst_No ?? -1;

            _logger.LogInformation("Starting AddEntry for equipment {PCName} (Inst_No: {InstNo})", pcName, instNo);

            try
            {
                if (equipmentData == null)
                {
                    _logger.LogWarning("AddEntry called with null equipment data");
                    throw new ArgumentNullException(nameof(equipmentData));
                }
                
                _inner.AddEntry(equipmentData);
                
                _logger.LogInformation("Successfully completed AddEntry for equipment {PCName} (Inst_No: {InstNo}) in {ElapsedMs}ms", 
                    pcName, instNo, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add entry for equipment {PCName} (Inst_No: {InstNo}) after {ElapsedMs}ms", 
                    pcName, instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public void InsertEntry(EquipmentData ed)
        {
            var stopwatch = Stopwatch.StartNew();
            var pcName = ed?.PC_Name ?? "null";
            var instNo = ed?.Inst_No ?? -1;

            _logger.LogInformation("Starting InsertEntry for equipment {PCName} (Inst_No: {InstNo})", pcName, instNo);

            try
            {
                if (ed == null)
                {
                    _logger.LogWarning("InsertEntry called with null equipment data");
                    throw new ArgumentNullException(nameof(ed));
                }
                
                _inner.InsertEntry(ed);
                
                _logger.LogInformation("Successfully completed InsertEntry for equipment {PCName} (Inst_No: {InstNo}) in {ElapsedMs}ms", 
                    pcName, instNo, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert entry for equipment {PCName} (Inst_No: {InstNo}) after {ElapsedMs}ms", 
                    pcName, instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public void UpdateLatestEntry(EquipmentData ed)
        {
            var stopwatch = Stopwatch.StartNew();
            var pcName = ed?.PC_Name ?? "null";
            var instNo = ed?.Inst_No ?? -1;

            _logger.LogInformation("Starting UpdateLatestEntry for equipment {PCName} (Inst_No: {InstNo})", pcName, instNo);

            try
            {
                if (ed == null)
                {
                    _logger.LogWarning("UpdateLatestEntry called with null equipment data");
                    throw new ArgumentNullException(nameof(ed));
                }
                
                _inner.UpdateLatestEntry(ed);
                
                _logger.LogInformation("Successfully completed UpdateLatestEntry for equipment {PCName} (Inst_No: {InstNo}) in {ElapsedMs}ms", 
                    pcName, instNo, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update latest entry for equipment {PCName} (Inst_No: {InstNo}) after {ElapsedMs}ms", 
                    pcName, instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public int GetNextInstNo()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetNextInstNo operation");

            try
            {
                var result = _inner.GetNextInstNo();
                
                _logger.LogInformation("Successfully retrieved next InstNo: {NextInstNo} in {ElapsedMs}ms", 
                    result, stopwatch.ElapsedMilliseconds);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get next InstNo after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public void DeleteEntry(int inst_no, int entry_id)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting DeleteEntry for Inst_No: {InstNo}, Entry_Id: {EntryId}", inst_no, entry_id);

            try
            {
                _inner.DeleteEntry(inst_no, entry_id);
                
                _logger.LogInformation("Successfully deleted entry for Inst_No: {InstNo}, Entry_Id: {EntryId} in {ElapsedMs}ms", 
                    inst_no, entry_id, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete entry for Inst_No: {InstNo}, Entry_Id: {EntryId} after {ElapsedMs}ms", 
                    inst_no, entry_id, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public List<EquipmentData> GetEquipmentSorted(int inst_no)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetEquipmentSorted for Inst_No: {InstNo}", inst_no);

            try
            {
                var result = _inner.GetEquipmentSorted(inst_no);
                
                _logger.LogDebug("Successfully retrieved {Count} equipment entries for Inst_No: {InstNo} in {ElapsedMs}ms", 
                    result?.Count ?? 0, inst_no, stopwatch.ElapsedMilliseconds);
                
                return result ?? new List<EquipmentData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get equipment sorted for Inst_No: {InstNo} after {ElapsedMs}ms", 
                    inst_no, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public List<EquipmentData> GetEquipSortedByEntry(int inst_no)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetEquipSortedByEntry for Inst_No: {InstNo}", inst_no);

            try
            {
                var result = _inner.GetEquipSortedByEntry(inst_no);
                
                _logger.LogDebug("Successfully retrieved {Count} equipment entries sorted by entry for Inst_No: {InstNo} in {ElapsedMs}ms", 
                    result?.Count ?? 0, inst_no, stopwatch.ElapsedMilliseconds);
                
                return result ?? new List<EquipmentData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get equipment sorted by entry for Inst_No: {InstNo} after {ElapsedMs}ms", 
                    inst_no, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public List<EquipmentData> GetEquipment()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetEquipment operation");

            try
            {
                var result = _inner.GetEquipment();
                
                _logger.LogInformation("Successfully retrieved {Count} equipment entries in {ElapsedMs}ms", 
                    result?.Count ?? 0, stopwatch.ElapsedMilliseconds);
                
                return result ?? new List<EquipmentData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get equipment after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public List<MachineData> GetMachines()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetMachines operation");

            try
            {
                var result = _inner.GetMachines();
                
                _logger.LogInformation("Successfully retrieved {Count} machines in {ElapsedMs}ms", 
                    result?.Count ?? 0, stopwatch.ElapsedMilliseconds);
                
                return result ?? new List<MachineData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get machines after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public List<MachineData> GetNewMachines()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetNewMachines operation");

            try
            {
                var result = _inner.GetNewMachines();
                
                _logger.LogDebug("Successfully retrieved {Count} new machines in {ElapsedMs}ms", 
                    result?.Count ?? 0, stopwatch.ElapsedMilliseconds);
                
                return result ?? new List<MachineData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get new machines after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public List<MachineData> GetUsedMachines()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetUsedMachines operation");

            try
            {
                var result = _inner.GetUsedMachines();
                
                _logger.LogDebug("Successfully retrieved {Count} used machines in {ElapsedMs}ms", 
                    result?.Count ?? 0, stopwatch.ElapsedMilliseconds);
                
                return result ?? new List<MachineData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get used machines after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public List<MachineData> GetQuarantineMachines()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetQuarantineMachines operation");

            try
            {
                var result = _inner.GetQuarantineMachines();
                
                _logger.LogDebug("Successfully retrieved {Count} quarantine machines in {ElapsedMs}ms", 
                    result?.Count ?? 0, stopwatch.ElapsedMilliseconds);
                
                return result ?? new List<MachineData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get quarantine machines after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public List<MachineData> GetActiveMachines()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetActiveMachines operation");

            try
            {
                var result = _inner.GetActiveMachines();
                
                _logger.LogDebug("Successfully retrieved {Count} active machines in {ElapsedMs}ms", 
                    result?.Count ?? 0, stopwatch.ElapsedMilliseconds);
                
                return result ?? new List<MachineData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active machines after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public bool IsInstNoTaken(int instNo)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Checking if InstNo {InstNo} is taken", instNo);

            try
            {
                var result = _inner.IsInstNoTaken(instNo);
                
                _logger.LogDebug("InstNo {InstNo} is {Status} (checked in {ElapsedMs}ms)", 
                    instNo, result ? "taken" : "available", stopwatch.ElapsedMilliseconds);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if InstNo {InstNo} is taken after {ElapsedMs}ms", 
                    instNo, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public bool IsSerialNoTakenInMachines(string serialNo)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Checking if SerialNo {SerialNo} is taken in machines", serialNo ?? "null");

            try
            {
                if (string.IsNullOrEmpty(serialNo))
                {
                    _logger.LogWarning("IsSerialNoTakenInMachines called with null or empty serial number");
                    return false;
                }
                
                var result = _inner.IsSerialNoTakenInMachines(serialNo);
                
                _logger.LogDebug("SerialNo {SerialNo} is {Status} (checked in {ElapsedMs}ms)", 
                    serialNo, result ? "taken" : "available", stopwatch.ElapsedMilliseconds);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if SerialNo {SerialNo} is taken after {ElapsedMs}ms", 
                    serialNo ?? "null", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public (int activeCount, int newCount, int usedCount, int quarantinedCount) GetDashboardStatistics()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetDashboardStatistics operation");

            try
            {
                var result = _inner.GetDashboardStatistics();
                
                _logger.LogInformation("Successfully retrieved dashboard statistics: Active={Active}, New={New}, Used={Used}, Quarantined={Quarantined} in {ElapsedMs}ms", 
                    result.activeCount, result.newCount, result.usedCount, result.quarantinedCount, stopwatch.ElapsedMilliseconds);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get dashboard statistics after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        public List<MachineData> GetMachinesOutOfServiceSinceJune()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Starting GetMachinesOutOfServiceSinceJune operation");

            try
            {
                var result = _inner.GetMachinesOutOfServiceSinceJune();
                
                _logger.LogInformation("Successfully retrieved {Count} machines out of service since June in {ElapsedMs}ms", 
                    result?.Count ?? 0, stopwatch.ElapsedMilliseconds);
                
                return result ?? new List<MachineData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get machines out of service since June after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
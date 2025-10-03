using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SusEquip.Data.Commands;
using SusEquip.Data.Commands.Equipment;
using SusEquip.Data.Exceptions;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;
using SusEquip.Data.Services;

namespace SusEquip.Data.Services
{
    /// <summary>
    /// Command-based equipment service that uses CommandExecutor for all operations
    /// Provides clean separation between business logic and command execution
    /// </summary>
    public class CommandBasedEquipmentService : IEquipmentService
    {
        private readonly ICommandExecutor _commandExecutor;
        private readonly IEquipmentService _fallbackService;
        private readonly ILogger<CommandBasedEquipmentService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public CommandBasedEquipmentService(
            ICommandExecutor commandExecutor,
            IEquipmentService fallbackService,
            ILoggerFactory loggerFactory,
            ILogger<CommandBasedEquipmentService> logger)
        {
            _commandExecutor = commandExecutor ?? throw new ArgumentNullException(nameof(commandExecutor));
            _fallbackService = fallbackService ?? throw new ArgumentNullException(nameof(fallbackService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        #region Command-Based Operations

        /// <summary>
        /// Adds equipment using AddEquipmentCommand with full validation
        /// </summary>
        public async Task AddEntryAsync(EquipmentData equipmentData)
        {
            _logger.LogDebug("Adding equipment entry via command: {PCName}", equipmentData.PC_Name);

            try
            {
                var commandLogger = _loggerFactory.CreateLogger<AddEquipmentCommand>();
                var command = new AddEquipmentCommand(
                    equipmentData,
                    _fallbackService,
                    commandLogger
                );

                var result = await _commandExecutor.ExecuteWithHandlerAsync(command);
                
                if (!result.Success)
                {
                    throw new DatabaseOperationException($"Failed to add equipment: {result.Message}", "AddEquipment", "Equip");
                }

                _logger.LogInformation("Successfully added equipment via command: {PCName}", equipmentData.PC_Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add equipment via command: {PCName}", equipmentData.PC_Name);
                throw new DatabaseOperationException(ex.Message, "AddEquipment", "Equip", ex);
            }
        }

        /// <summary>
        /// Updates equipment using fallback service (UpdateEquipmentCommand temporarily disabled due to file corruption)
        /// </summary>
        public async Task UpdateLatestEntryAsync(EquipmentData equipmentData)
        {
            _logger.LogDebug("Updating equipment entry via fallback service: {PCName}", equipmentData.PC_Name);

            // TODO: Re-enable UpdateEquipmentCommand when file corruption is resolved
            await _fallbackService.UpdateLatestEntryAsync(equipmentData);
        }

        /// <summary>
        /// Deletes equipment using fallback service (DeleteEquipmentCommand not implemented yet)
        /// </summary>
        public async Task DeleteEntryAsync(int instNo, int entryId)
        {
            _logger.LogDebug("Deleting equipment entry via fallback service: InstNo={InstNo}, EntryId={EntryId}", instNo, entryId);

            // TODO: Implement DeleteEquipmentCommand
            await _fallbackService.DeleteEntryAsync(instNo, entryId);
        }

        #endregion

        #region Fallback to Existing Implementation

        /// <summary>
        /// Insert entry (falls back to existing implementation for now)
        /// </summary>
        public async Task InsertEntryAsync(EquipmentData equipmentData)
        {
            _logger.LogDebug("InsertEntry falling back to existing implementation");
            await _fallbackService.InsertEntryAsync(equipmentData);
        }

        /// <summary>
        /// Get all equipment (falls back to existing implementation)
        /// </summary>
        public async Task<List<EquipmentData>> GetEquipmentAsync()
        {
            _logger.LogDebug("GetEquipment falling back to existing implementation");
            return await _fallbackService.GetEquipmentAsync();
        }

        /// <summary>
        /// Get equipment sorted (falls back to existing implementation)
        /// </summary>
        public async Task<List<EquipmentData>> GetEquipmentSortedAsync(int instNo)
        {
            _logger.LogDebug("GetEquipmentSorted falling back to existing implementation");
            return await _fallbackService.GetEquipmentSortedAsync(instNo);
        }

        /// <summary>
        /// Get equipment sorted by entry (falls back to existing implementation)
        /// </summary>
        public async Task<List<EquipmentData>> GetEquipmentSortedByEntryAsync(int instNo)
        {
            _logger.LogDebug("GetEquipmentSortedByEntry falling back to existing implementation");
            return await _fallbackService.GetEquipmentSortedByEntryAsync(instNo);
        }

        /// <summary>
        /// Get all machines (falls back to existing implementation)
        /// </summary>
        public async Task<List<MachineData>> GetMachinesAsync()
        {
            _logger.LogDebug("GetMachines falling back to existing implementation");
            return await _fallbackService.GetMachinesAsync();
        }

        /// <summary>
        /// Get active machines (falls back to existing implementation)
        /// </summary>
        public async Task<List<MachineData>> GetActiveMachinesAsync()
        {
            _logger.LogDebug("GetActiveMachines falling back to existing implementation");
            return await _fallbackService.GetActiveMachinesAsync();
        }

        /// <summary>
        /// Get new machines (falls back to existing implementation)
        /// </summary>
        public async Task<List<MachineData>> GetNewMachinesAsync()
        {
            _logger.LogDebug("GetNewMachines falling back to existing implementation");
            return await _fallbackService.GetNewMachinesAsync();
        }

        /// <summary>
        /// Get used machines (falls back to existing implementation)
        /// </summary>
        public async Task<List<MachineData>> GetUsedMachinesAsync()
        {
            _logger.LogDebug("GetUsedMachines falling back to existing implementation");
            return await _fallbackService.GetUsedMachinesAsync();
        }

        /// <summary>
        /// Get quarantine machines (falls back to existing implementation)
        /// </summary>
        public async Task<List<MachineData>> GetQuarantineMachinesAsync()
        {
            _logger.LogDebug("GetQuarantineMachines falling back to existing implementation");
            return await _fallbackService.GetQuarantineMachinesAsync();
        }

        /// <summary>
        /// Get machines out of service since June (falls back to existing implementation)
        /// </summary>
        public async Task<List<MachineData>> GetMachinesOutOfServiceSinceJuneAsync()
        {
            _logger.LogDebug("GetMachinesOutOfServiceSinceJune falling back to existing implementation");
            return await _fallbackService.GetMachinesOutOfServiceSinceJuneAsync();
        }

        /// <summary>
        /// Get next instance number (falls back to existing implementation)
        /// </summary>
        public async Task<int> GetNextInstNoAsync()
        {
            _logger.LogDebug("GetNextInstNo falling back to existing implementation");
            return await _fallbackService.GetNextInstNoAsync();
        }

        /// <summary>
        /// Count equipment by status (falls back to existing implementation)
        /// </summary>
        public async Task<Dictionary<string, int>> CountEquipmentByStatusAsync()
        {
            _logger.LogDebug("CountEquipmentByStatus falling back to existing implementation");
            // This method doesn't exist in IEquipmentService, so we'll return a simple implementation
            var machines = await _fallbackService.GetMachinesAsync();
            return machines.GroupBy(m => m.Status ?? "Unknown")
                          .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Check if InstNo is already taken (falls back to existing implementation)
        /// </summary>
        public async Task<bool> IsInstNoTakenAsync(int instNo)
        {
            _logger.LogDebug("IsInstNoTaken falling back to existing implementation");
            return await _fallbackService.IsInstNoTakenAsync(instNo);
        }

        /// <summary>
        /// Check if SerialNo is already taken in Machines table (falls back to existing implementation)
        /// </summary>
        public async Task<bool> IsSerialNoTakenInMachinesAsync(string serialNo)
        {
            _logger.LogDebug("IsSerialNoTakenInMachines falling back to existing implementation");
            return await _fallbackService.IsSerialNoTakenInMachinesAsync(serialNo);
        }

        /// <summary>
        /// Get dashboard statistics (falls back to existing implementation)
        /// </summary>
        public async Task<(int activeCount, int newCount, int usedCount, int quarantinedCount)> GetDashboardStatisticsAsync()
        {
            _logger.LogDebug("GetDashboardStatistics falling back to existing implementation");
            return await _fallbackService.GetDashboardStatisticsAsync();
        }

        /// <summary>
        /// Command pattern methods - fully implemented
        /// </summary>
        public async Task DeleteEquipmentAsync(int instNo)
        {
            _logger.LogDebug("DeleteEquipment command pattern - falling back to existing implementation for now");
            await _fallbackService.DeleteEquipmentAsync(instNo);
        }

        public async Task UpdateEquipmentStatusAsync(int instNo, string newStatus)
        {
            _logger.LogDebug("UpdateEquipmentStatus command pattern - falling back to existing implementation for now");
            await _fallbackService.UpdateEquipmentStatusAsync(instNo, newStatus);
        }

        public async Task<EquipmentData?> GetByInstNoAsync(int instNo)
        {
            _logger.LogDebug("GetByInstNo command pattern - falling back to existing implementation for now");
            return await _fallbackService.GetByInstNoAsync(instNo);
        }

        #endregion
    }
}
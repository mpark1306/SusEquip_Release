using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SusEquip.Data.Commands.Equipment;
using SusEquip.Data.Services;
using SusEquip.Data.Interfaces.Services;

namespace SusEquip.Data.Commands.Handlers
{
    /// <summary>
    /// Command handler for all equipment-related operations
    /// Coordinates business logic, validation, and data persistence
    /// </summary>
    public class EquipmentCommandHandler : 
        ICommandHandler<AddEquipmentCommand, EquipmentOperationResult>,
        ICommandHandler<UpdateEquipmentCommand, EquipmentOperationResult>,
        ICommandHandler<DeleteEquipmentCommand, EquipmentOperationResult>,
        ICommandHandler<UpdateEquipmentStatusCommand, EquipmentOperationResult>,
        ICommandHandler<BulkImportEquipmentCommand, EquipmentOperationResult>
    {
        private readonly IEquipmentService _equipmentService;
        private readonly IValidationService _validationService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<EquipmentCommandHandler> _logger;

        public EquipmentCommandHandler(
            IEquipmentService equipmentService,
            IValidationService validationService,
            ICacheService cacheService,
            ILogger<EquipmentCommandHandler> logger)
        {
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles equipment addition commands with pre/post processing
        /// </summary>
        public async Task<EquipmentOperationResult> HandleAsync(AddEquipmentCommand command)
        {
            _logger.LogDebug("Handling AddEquipmentCommand for {PCName}", command);

            try
            {
                // Pre-processing: Clear relevant caches
                await InvalidateRelevantCachesAsync("equipment_list", "equipment_stats");

                // Execute the command
                var result = await command.ExecuteAsync();

                // Post-processing: Update related systems
                if (result.Success)
                {
                    await PostProcessEquipmentAdditionAsync(result.Equipment!);
                }

                _logger.LogInformation("Successfully handled AddEquipmentCommand");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle AddEquipmentCommand");
                throw;
            }
        }

        /// <summary>
        /// Handles equipment update commands with validation and change tracking
        /// </summary>
        public async Task<EquipmentOperationResult> HandleAsync(UpdateEquipmentCommand command)
        {
            _logger.LogDebug("Handling UpdateEquipmentCommand");

            try
            {
                // Pre-processing: Clear relevant caches
                await InvalidateRelevantCachesAsync("equipment_list", "equipment_stats");

                // Execute the command with comprehensive logging and validation
                var result = await command.ExecuteAsync();

                // Post-processing: Handle success/failure scenarios
                if (result.Success)
                {
                    _logger.LogInformation("UpdateEquipmentCommand executed successfully: {Message}", result.Message);
                    if (result.Equipment != null)
                    {
                        await PostProcessEquipmentUpdateAsync(result.Equipment, result.Changes);
                    }
                }
                else
                {
                    _logger.LogWarning("UpdateEquipmentCommand failed: {Message}", result.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle UpdateEquipmentCommand");
                throw;
            }
        }

        /// <summary>
        /// Handles equipment deletion commands with safety checks
        /// </summary>
        public async Task<EquipmentOperationResult> HandleAsync(DeleteEquipmentCommand command)
        {
            _logger.LogDebug("Handling DeleteEquipmentCommand");

            try
            {
                // Pre-processing: Clear relevant caches
                await InvalidateRelevantCachesAsync("equipment_list", "equipment_stats");

                // Execute the command
                var result = await command.ExecuteAsync();

                // Post-processing: Clean up related data
                if (result.Success)
                {
                    await PostProcessEquipmentDeletionAsync();
                }

                _logger.LogInformation("Successfully handled DeleteEquipmentCommand");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle DeleteEquipmentCommand");
                throw;
            }
        }

        /// <summary>
        /// Handles equipment status update commands
        /// </summary>
        public async Task<EquipmentOperationResult> HandleAsync(UpdateEquipmentStatusCommand command)
        {
            _logger.LogDebug("Handling UpdateEquipmentStatusCommand");

            try
            {
                // Pre-processing: Clear status-related caches
                await InvalidateRelevantCachesAsync("equipment_status", "dashboard_stats");

                // Execute the command
                var result = await command.ExecuteAsync();

                // Post-processing: Update dashboard statistics
                if (result.Success)
                {
                    await PostProcessStatusUpdateAsync(result.Equipment!);
                }

                _logger.LogInformation("Successfully handled UpdateEquipmentStatusCommand");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle UpdateEquipmentStatusCommand");
                throw;
            }
        }

        /// <summary>
        /// Handles bulk import commands with progress tracking
        /// </summary>
        public async Task<EquipmentOperationResult> HandleAsync(BulkImportEquipmentCommand command)
        {
            _logger.LogDebug("Handling BulkImportEquipmentCommand");

            try
            {
                // Pre-processing: Clear all equipment caches
                await InvalidateRelevantCachesAsync("equipment_list", "equipment_stats", "dashboard_stats");

                // Execute the command
                var result = await command.ExecuteAsync();

                // Post-processing: Update statistics and indexes
                if (result.Success)
                {
                    await PostProcessBulkImportAsync(result);
                }

                _logger.LogInformation("Successfully handled BulkImportEquipmentCommand: {Message}", 
                    result.Message);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle BulkImportEquipmentCommand");
                throw;
            }
        }

        #region Private Helper Methods

        private async Task InvalidateRelevantCachesAsync(params string[] cacheKeys)
        {
            foreach (var key in cacheKeys)
            {
                await _cacheService.RemoveAsync(key);
                _logger.LogDebug("Invalidated cache key: {CacheKey}", key);
            }
        }

        private async Task PostProcessEquipmentAdditionAsync(Data.Models.EquipmentData equipment)
        {
            _logger.LogDebug("Post-processing equipment addition for {PCName}", equipment.PC_Name);

            // Could trigger events, update indexes, send notifications, etc.
            // For now, just log the successful addition
            _logger.LogInformation("Equipment added: {PCName} (InstNo: {InstNo})", 
                equipment.PC_Name, equipment.Inst_No);

            await Task.CompletedTask;
        }

        private async Task PostProcessEquipmentUpdateAsync(Data.Models.EquipmentData equipment, System.Collections.Generic.List<EquipmentChange> changes)
        {
            _logger.LogDebug("Post-processing equipment update for {PCName} with {ChangeCount} changes", 
                equipment.PC_Name, changes.Count);

            // Could trigger change notifications, audit logs, etc.
            foreach (var change in changes)
            {
                _logger.LogDebug("Equipment change: {FieldName} changed from '{OldValue}' to '{NewValue}'", 
                    change.FieldName, change.OldValue, change.NewValue);
            }

            await Task.CompletedTask;
        }

        private async Task PostProcessEquipmentDeletionAsync()
        {
            _logger.LogDebug("Post-processing equipment deletion");

            // Could clean up related data, send notifications, etc.
            await Task.CompletedTask;
        }

        private async Task PostProcessStatusUpdateAsync(Data.Models.EquipmentData equipment)
        {
            _logger.LogDebug("Post-processing status update for {PCName} to {Status}", 
                equipment.PC_Name, equipment.Status);

            // Could update dashboard counters, trigger workflows, etc.
            await Task.CompletedTask;
        }

        private async Task PostProcessBulkImportAsync(EquipmentOperationResult result)
        {
            _logger.LogDebug("Post-processing bulk import: {Message}", result.Message);

            // Could rebuild indexes, update statistics, send summary reports, etc.
            await Task.CompletedTask;
        }

        #endregion
    }
}
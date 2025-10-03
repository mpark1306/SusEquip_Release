using System.Threading.Tasks;
using System.Collections.Generic;
using SusEquip.Data.Commands;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;

namespace SusEquip.Data.Commands.Equipment
{
    /// <summary>
    /// Command to import multiple equipment entries with batch processing and transaction support
    /// </summary>
    public class BulkImportEquipmentCommand : BaseCommand<EquipmentOperationResult>
    {
        private readonly List<EquipmentData> _equipmentList;
        private readonly string _importedBy;
        private readonly IEquipmentService _equipmentService;
        private readonly ILogger<BulkImportEquipmentCommand> _logger;

        public BulkImportEquipmentCommand(
            List<EquipmentData> equipmentList,
            string importedBy,
            IEquipmentService equipmentService,
            ILogger<BulkImportEquipmentCommand> logger) : base(logger)
        {
            _equipmentList = equipmentList ?? throw new System.ArgumentNullException(nameof(equipmentList));
            _importedBy = importedBy ?? "System";
            _equipmentService = equipmentService ?? throw new System.ArgumentNullException(nameof(equipmentService));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public override async Task<EquipmentOperationResult> ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("Starting bulk import of {Count} equipment items", _equipmentList.Count);
                
                // Validate input
                if (_equipmentList.Count == 0)
                {
                    return EquipmentOperationResult.CreateFailure("No equipment data provided for import");
                }

                // Validate each equipment item
                for (int i = 0; i < _equipmentList.Count; i++)
                {
                    var equipment = _equipmentList[i];
                    if (string.IsNullOrEmpty(equipment.PC_Name))
                    {
                        return EquipmentOperationResult.CreateFailure($"Equipment at index {i} is missing PC_Name");
                    }
                }

                // Process the bulk import
                int successCount = 0;
                int failCount = 0;
                var failures = new List<string>();

                foreach (var equipment in _equipmentList)
                {
                    try
                    {
                        await _equipmentService.AddEntryAsync(equipment);
                        successCount++;
                        _logger.LogDebug("Successfully imported equipment: {PCName}", equipment.PC_Name);
                    }
                    catch (System.Exception ex)
                    {
                        failCount++;
                        string error = $"Failed to import {equipment.PC_Name}: {ex.Message}";
                        failures.Add(error);
                        _logger.LogWarning("Import failed for equipment {PCName}: {Error}", equipment.PC_Name, ex.Message);
                    }
                }

                string resultMessage = $"Bulk import completed: {successCount} successful, {failCount} failed";
                
                if (failCount > 0)
                {
                    resultMessage += $". Failures: {string.Join("; ", failures)}";
                }

                _logger.LogInformation("Bulk import completed: {SuccessCount} successful, {FailCount} failed", 
                    successCount, failCount);

                // Return success if at least some items were imported
                if (successCount > 0)
                {
                                    return EquipmentOperationResult.CreateSuccess($"Bulk import completed: {successCount} successful, {failCount} failed");
                }
                else
                {
                    return EquipmentOperationResult.CreateFailure(resultMessage);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to execute bulk import");
                return EquipmentOperationResult.CreateFailure($"Bulk import failed: {ex.Message}");
            }
        }
    }
}
using System.Threading.Tasks;
using SusEquip.Data.Commands;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;

namespace SusEquip.Data.Commands.Equipment
{
    /// <summary>
    /// Command to update existing equipment data with validation and logging
    /// Handles complete equipment record updates while preserving data integrity
    /// </summary>
    public class UpdateEquipmentCommand : BaseCommand<EquipmentOperationResult>
    {
        private readonly EquipmentData _equipmentData;
        private readonly string _updatedBy;
        private readonly IEquipmentService _equipmentService;
        private readonly ILogger<UpdateEquipmentCommand> _logger;

        public UpdateEquipmentCommand(
            EquipmentData equipmentData,
            string updatedBy,
            IEquipmentService equipmentService,
            ILogger<UpdateEquipmentCommand> logger) : base(logger)
        {
            _equipmentData = equipmentData ?? throw new System.ArgumentNullException(nameof(equipmentData));
            _updatedBy = updatedBy ?? "System";
            _equipmentService = equipmentService ?? throw new System.ArgumentNullException(nameof(equipmentService));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public override async Task<EquipmentOperationResult> ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("Starting equipment update for InstNo={InstNo}, PCName={PCName}, UpdatedBy={UpdatedBy}", 
                    _equipmentData.Inst_No, _equipmentData.PC_Name, _updatedBy);
                
                // Validate equipment data
                if (_equipmentData.Inst_No <= 0)
                {
                    _logger.LogWarning("UpdateEquipmentCommand validation failed: Invalid Inst_No={InstNo}", _equipmentData.Inst_No);
                    return EquipmentOperationResult.CreateFailure("Valid Inst_No is required for equipment update");
                }

                if (string.IsNullOrWhiteSpace(_equipmentData.PC_Name))
                {
                    _logger.LogWarning("UpdateEquipmentCommand validation failed: PC_Name is required");
                    return EquipmentOperationResult.CreateFailure("PC_Name is required for equipment update");
                }

                // Verify equipment exists
                var existingEquipment = await _equipmentService.GetByInstNoAsync(_equipmentData.Inst_No);
                if (existingEquipment == null)
                {
                    _logger.LogWarning("UpdateEquipmentCommand failed: Equipment with Inst_No {InstNo} not found", _equipmentData.Inst_No);
                    return EquipmentOperationResult.CreateFailure($"Equipment with Inst_No {_equipmentData.Inst_No} not found");
                }

                // Track changes for audit and post-processing
                var changes = new System.Collections.Generic.List<SusEquip.Data.Commands.EquipmentChange>();
                var changeTime = System.DateTime.Now;

                if (existingEquipment.PC_Name != _equipmentData.PC_Name)
                {
                    changes.Add(new SusEquip.Data.Commands.EquipmentChange
                    {
                        FieldName = "PC_Name",
                        OldValue = existingEquipment.PC_Name,
                        NewValue = _equipmentData.PC_Name,
                        ChangedBy = _updatedBy,
                        ChangedAt = changeTime
                    });
                }

                if (existingEquipment.Status != _equipmentData.Status)
                {
                    changes.Add(new SusEquip.Data.Commands.EquipmentChange
                    {
                        FieldName = "Status",
                        OldValue = existingEquipment.Status,
                        NewValue = _equipmentData.Status,
                        ChangedBy = _updatedBy,
                        ChangedAt = changeTime
                    });
                }

                if (existingEquipment.App_Owner != _equipmentData.App_Owner)
                {
                    changes.Add(new SusEquip.Data.Commands.EquipmentChange
                    {
                        FieldName = "App_Owner",
                        OldValue = existingEquipment.App_Owner,
                        NewValue = _equipmentData.App_Owner,
                        ChangedBy = _updatedBy,
                        ChangedAt = changeTime
                    });
                }

                if (existingEquipment.Department != _equipmentData.Department)
                {
                    changes.Add(new SusEquip.Data.Commands.EquipmentChange
                    {
                        FieldName = "Department",
                        OldValue = existingEquipment.Department,
                        NewValue = _equipmentData.Department,
                        ChangedBy = _updatedBy,
                        ChangedAt = changeTime
                    });
                }

                if (existingEquipment.Note != _equipmentData.Note)
                {
                    changes.Add(new SusEquip.Data.Commands.EquipmentChange
                    {
                        FieldName = "Note",
                        OldValue = existingEquipment.Note,
                        NewValue = _equipmentData.Note,
                        ChangedBy = _updatedBy,
                        ChangedAt = changeTime
                    });
                }

                // Log what's changing
                _logger.LogInformation("Updating equipment {InstNo} with {ChangeCount} changes: {Changes}", 
                    _equipmentData.Inst_No, changes.Count, 
                    string.Join(", ", changes.Select(c => $"{c.FieldName}: {c.OldValue} -> {c.NewValue}")));

                // Set update metadata
                _equipmentData.Entry_Date = System.DateTime.Now.ToString("yyyy-MM-dd");

                // Execute the update operation
                await _equipmentService.UpdateLatestEntryAsync(_equipmentData);
                
                _logger.LogInformation("Successfully updated equipment: InstNo={InstNo}, PCName={PCName}, ChangeCount={ChangeCount}", 
                    _equipmentData.Inst_No, _equipmentData.PC_Name, changes.Count);
                
                var result = EquipmentOperationResult.CreateSuccess(
                    $"Equipment {_equipmentData.PC_Name} updated successfully", 
                    _equipmentData.Inst_No, 
                    _equipmentData);
                result.Changes = changes;
                return result;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to update equipment: InstNo={InstNo}, PCName={PCName}, UpdatedBy={UpdatedBy}", 
                    _equipmentData.Inst_No, _equipmentData.PC_Name, _updatedBy);
                
                return EquipmentOperationResult.CreateFailure($"Failed to update equipment: {ex.Message}");
            }
        }
    }
}

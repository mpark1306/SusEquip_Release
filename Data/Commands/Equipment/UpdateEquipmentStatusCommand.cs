using System.Threading.Tasks;
using SusEquip.Data.Commands;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;

namespace SusEquip.Data.Commands.Equipment
{
    /// <summary>
    /// Command to update equipment status with validation and logging
    /// </summary>
    public class UpdateEquipmentStatusCommand : BaseCommand<EquipmentOperationResult>
    {
        private readonly int _instNo;
        private readonly string _newStatus;
        private readonly string _updatedBy;
        private readonly IEquipmentService _equipmentService;
        private readonly ILogger<UpdateEquipmentStatusCommand> _logger;

        public UpdateEquipmentStatusCommand(
            int instNo,
            string newStatus,
            string updatedBy,
            IEquipmentService equipmentService,
            ILogger<UpdateEquipmentStatusCommand> logger) : base(logger)
        {
            _instNo = instNo;
            _newStatus = newStatus ?? throw new System.ArgumentNullException(nameof(newStatus));
            _updatedBy = updatedBy ?? "System";
            _equipmentService = equipmentService ?? throw new System.ArgumentNullException(nameof(equipmentService));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public override async Task<EquipmentOperationResult> ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("Updating status for equipment with InstNo: {InstNo} to {Status}", 
                    _instNo, _newStatus);
                
                // Validate parameters
                if (_instNo <= 0)
                {
                    return EquipmentOperationResult.CreateFailure("Valid Inst_No is required for status update");
                }

                if (string.IsNullOrEmpty(_newStatus))
                {
                    return EquipmentOperationResult.CreateFailure("Status is required for update");
                }

                // Check if equipment exists and get current status
                var equipment = await _equipmentService.GetByInstNoAsync(_instNo);
                if (equipment == null)
                {
                    return EquipmentOperationResult.CreateFailure($"Equipment with Inst_No {_instNo} not found");
                }

                string oldStatus = equipment.Status;
                
                // Update the status
                await _equipmentService.UpdateEquipmentStatusAsync(_instNo, _newStatus);
                
                _logger.LogInformation("Successfully updated status for equipment: {PCName} from {OldStatus} to {NewStatus}", 
                    equipment.PC_Name, oldStatus, _newStatus);
                    
                return EquipmentOperationResult.CreateSuccess($"Equipment status updated from {oldStatus} to {_newStatus}");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to update status for equipment with InstNo: {InstNo}", _instNo);
                return EquipmentOperationResult.CreateFailure($"Failed to update equipment status: {ex.Message}");
            }
        }
    }
}
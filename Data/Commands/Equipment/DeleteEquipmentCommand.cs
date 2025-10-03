using System.Threading.Tasks;
using SusEquip.Data.Commands;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;

namespace SusEquip.Data.Commands.Equipment
{
    /// <summary>
    /// Command to delete equipment with safety checks and logging
    /// </summary>
    public class DeleteEquipmentCommand : BaseCommand<EquipmentOperationResult>
    {
        private readonly int _instNo;
        private readonly string _deletedBy;
        private readonly IEquipmentService _equipmentService;
        private readonly ILogger<DeleteEquipmentCommand> _logger;

        public DeleteEquipmentCommand(
            int instNo,
            string deletedBy,
            IEquipmentService equipmentService,
            ILogger<DeleteEquipmentCommand> logger) : base(logger)
        {
            _instNo = instNo;
            _deletedBy = deletedBy ?? "System";
            _equipmentService = equipmentService ?? throw new System.ArgumentNullException(nameof(equipmentService));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public override async Task<EquipmentOperationResult> ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("Deleting equipment with InstNo: {InstNo}", _instNo);
                
                // Validate parameters
                if (_instNo <= 0)
                {
                    return EquipmentOperationResult.CreateFailure("Valid Inst_No is required for deletion");
                }

                // Check if equipment exists
                var equipment = await _equipmentService.GetByInstNoAsync(_instNo);
                if (equipment == null)
                {
                    return EquipmentOperationResult.CreateFailure($"Equipment with Inst_No {_instNo} not found");
                }

                // Execute the delete operation
                await _equipmentService.DeleteEquipmentAsync(_instNo);
                
                _logger.LogInformation("Successfully deleted equipment: {PCName} (InstNo: {InstNo})", 
                    equipment.PC_Name, _instNo);
                return EquipmentOperationResult.CreateSuccess($"Equipment {equipment.PC_Name} deleted successfully");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to delete equipment with InstNo: {InstNo}", _instNo);
                return EquipmentOperationResult.CreateFailure($"Failed to delete equipment: {ex.Message}");
            }
        }
    }
}
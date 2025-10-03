using System.Threading.Tasks;
using SusEquip.Data.Commands;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;

namespace SusEquip.Data.Commands.Equipment
{
    /// <summary>
    /// Command to add new equipment entry with validation and logging
    /// </summary>
    public class AddEquipmentCommand : BaseCommand<EquipmentOperationResult>
    {
        private readonly EquipmentData _equipmentData;
        private readonly IEquipmentService _equipmentService;
        private readonly ILogger<AddEquipmentCommand> _logger;

        public AddEquipmentCommand(
            EquipmentData equipmentData,
            IEquipmentService equipmentService,
            ILogger<AddEquipmentCommand> logger) : base(logger)
        {
            _equipmentData = equipmentData ?? throw new System.ArgumentNullException(nameof(equipmentData));
            _equipmentService = equipmentService ?? throw new System.ArgumentNullException(nameof(equipmentService));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public override async Task<EquipmentOperationResult> ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("Adding equipment: {PCName}", _equipmentData.PC_Name);
                
                // Validate equipment data
                if (string.IsNullOrEmpty(_equipmentData.PC_Name))
                {
                    return EquipmentOperationResult.CreateFailure("PC Name is required");
                }

                // Execute the add operation
                await _equipmentService.AddEntryAsync(_equipmentData);
                
                _logger.LogInformation("Successfully added equipment: {PCName}", _equipmentData.PC_Name);
                return EquipmentOperationResult.CreateSuccess("Equipment added successfully");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to add equipment: {PCName}", _equipmentData.PC_Name);
                return EquipmentOperationResult.CreateFailure($"Failed to add equipment: {ex.Message}");
            }
        }
    }
}
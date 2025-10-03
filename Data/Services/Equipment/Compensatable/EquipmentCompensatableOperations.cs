using Microsoft.Extensions.Logging;
using SusEquip.Data.Models;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Services.ErrorHandling;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SusEquip.Data.Services.Equipment.Compensatable
{
    /// <summary>
    /// Compensatable operation for adding equipment
    /// </summary>
    public class AddEquipmentCompensatableOperation : CompensatableOperationBase<EquipmentData>
    {
        private readonly IEquipmentService _equipmentService;
        private readonly EquipmentData _equipmentData;
        private readonly ILogger<AddEquipmentCompensatableOperation> _logger;
        private int? _addedInstNo;

        public AddEquipmentCompensatableOperation(
            IEquipmentService equipmentService,
            EquipmentData equipmentData,
            ILogger<AddEquipmentCompensatableOperation> logger)
            : base($"AddEquipment_{equipmentData.Inst_No}")
        {
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
            _equipmentData = equipmentData ?? throw new ArgumentNullException(nameof(equipmentData));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<EquipmentData> ExecuteTypedOperationAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding equipment with Inst_No: {InstNo}", _equipmentData.Inst_No);

            await _equipmentService.AddEntryAsync(_equipmentData);
            _addedInstNo = _equipmentData.Inst_No;

            _logger.LogInformation("Successfully added equipment with Inst_No: {InstNo}", _addedInstNo);
            return _equipmentData;
        }

        protected override async Task CompensateOperationAsync(CancellationToken cancellationToken)
        {
            if (_addedInstNo.HasValue)
            {
                _logger.LogWarning("Compensating: Deleting equipment with Inst_No: {InstNo}", _addedInstNo);
                
                try
                {
                    // Get the equipment entries to find the entry ID to delete
                    var equipment = await _equipmentService.GetEquipmentSortedAsync(_addedInstNo.Value);
                    if (equipment?.Any() == true)
                    {
                        var latestEntry = equipment.First();
                        await _equipmentService.DeleteEntryAsync(_addedInstNo.Value, latestEntry.EntryId);
                        _logger.LogInformation("Successfully compensated by deleting equipment Inst_No: {InstNo}", _addedInstNo);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to compensate equipment deletion for Inst_No: {InstNo}", _addedInstNo);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("No Inst_No to compensate - equipment was not successfully added");
            }
        }
    }

    /// <summary>
    /// Compensatable operation for updating equipment entry
    /// </summary>
    public class UpdateEquipmentCompensatableOperation : CompensatableOperationBase<EquipmentData>
    {
        private readonly IEquipmentService _equipmentService;
        private readonly EquipmentData _updatedEquipmentData;
        private readonly ILogger<UpdateEquipmentCompensatableOperation> _logger;
        private EquipmentData? _previousEquipmentData;

        public UpdateEquipmentCompensatableOperation(
            IEquipmentService equipmentService,
            EquipmentData updatedEquipmentData,
            ILogger<UpdateEquipmentCompensatableOperation> logger)
            : base($"UpdateEquipment_{updatedEquipmentData.Inst_No}")
        {
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
            _updatedEquipmentData = updatedEquipmentData ?? throw new ArgumentNullException(nameof(updatedEquipmentData));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<EquipmentData> ExecuteTypedOperationAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating equipment with Inst_No: {InstNo}", _updatedEquipmentData.Inst_No);

            // Get current equipment data for compensation
            var currentEquipment = await _equipmentService.GetEquipmentSortedAsync(_updatedEquipmentData.Inst_No);
            if (currentEquipment?.Any() == true)
            {
                _previousEquipmentData = currentEquipment.First();
            }

            await _equipmentService.UpdateLatestEntryAsync(_updatedEquipmentData);
            
            _logger.LogInformation("Successfully updated equipment with Inst_No: {InstNo}", _updatedEquipmentData.Inst_No);
            return _updatedEquipmentData;
        }

        protected override async Task CompensateOperationAsync(CancellationToken cancellationToken)
        {
            if (_previousEquipmentData != null)
            {
                _logger.LogWarning("Compensating: Reverting equipment Inst_No: {InstNo} to previous state", 
                    _previousEquipmentData.Inst_No);
                
                try
                {
                    await _equipmentService.UpdateLatestEntryAsync(_previousEquipmentData);
                    _logger.LogInformation("Successfully compensated by reverting equipment Inst_No: {InstNo}", 
                        _previousEquipmentData.Inst_No);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to compensate equipment update for Inst_No: {InstNo}", 
                        _previousEquipmentData.Inst_No);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("No previous equipment data to compensate for Inst_No: {InstNo}", 
                    _updatedEquipmentData.Inst_No);
            }
        }
    }

    /// <summary>
    /// Compensatable operation for equipment workflow using next available Inst_No
    /// </summary>
    public class EquipmentDeploymentCompensatableOperation : CompensatableOperationBase<int>
    {
        private readonly IEquipmentService _equipmentService;
        private readonly EquipmentData _equipmentData;
        private readonly ILogger<EquipmentDeploymentCompensatableOperation> _logger;
        private int? _assignedInstNo;

        public EquipmentDeploymentCompensatableOperation(
            IEquipmentService equipmentService,
            EquipmentData equipmentData,
            ILogger<EquipmentDeploymentCompensatableOperation> logger)
            : base($"DeployEquipment_{equipmentData.PC_Name}")
        {
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
            _equipmentData = equipmentData ?? throw new ArgumentNullException(nameof(equipmentData));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<int> ExecuteTypedOperationAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deploying equipment '{PCName}' with serial '{SerialNo}'", 
                _equipmentData.PC_Name, _equipmentData.Serial_No);

            // Get next available Inst_No
            var nextInstNo = await _equipmentService.GetNextInstNoAsync();
            _assignedInstNo = nextInstNo;
            _equipmentData.Inst_No = nextInstNo;

            // Add the equipment
            await _equipmentService.AddEntryAsync(_equipmentData);
            
            _logger.LogInformation("Successfully deployed equipment with assigned Inst_No: {InstNo}", nextInstNo);
            return nextInstNo;
        }

        protected override async Task CompensateOperationAsync(CancellationToken cancellationToken)
        {
            if (_assignedInstNo.HasValue)
            {
                _logger.LogWarning("Compensating: Removing deployed equipment with Inst_No: {InstNo}", _assignedInstNo);
                
                try
                {
                    // Get the equipment entries to find the entry to delete
                    var equipment = await _equipmentService.GetEquipmentSortedAsync(_assignedInstNo.Value);
                    if (equipment?.Any() == true)
                    {
                        var latestEntry = equipment.First();
                        await _equipmentService.DeleteEntryAsync(_assignedInstNo.Value, latestEntry.EntryId);
                        _logger.LogInformation("Successfully compensated deployment for Inst_No: {InstNo}", _assignedInstNo);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to compensate deployment for Inst_No: {InstNo}", _assignedInstNo);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("No Inst_No to compensate - equipment was not successfully deployed");
            }
        }
    }

    /// <summary>
    /// Factory for creating equipment-specific compensatable operations
    /// </summary>
    public static class EquipmentCompensatableOperationFactory
    {
        /// <summary>
        /// Create a compensatable operation for adding equipment
        /// </summary>
        public static AddEquipmentCompensatableOperation CreateAddOperation(
            IEquipmentService equipmentService,
            EquipmentData equipmentData,
            ILogger<AddEquipmentCompensatableOperation> logger)
        {
            return new AddEquipmentCompensatableOperation(equipmentService, equipmentData, logger);
        }

        /// <summary>
        /// Create a compensatable operation for updating equipment
        /// </summary>
        public static UpdateEquipmentCompensatableOperation CreateUpdateOperation(
            IEquipmentService equipmentService,
            EquipmentData updatedEquipmentData,
            ILogger<UpdateEquipmentCompensatableOperation> logger)
        {
            return new UpdateEquipmentCompensatableOperation(equipmentService, updatedEquipmentData, logger);
        }

        /// <summary>
        /// Create a compensatable operation for equipment deployment
        /// </summary>
        public static EquipmentDeploymentCompensatableOperation CreateDeploymentOperation(
            IEquipmentService equipmentService,
            EquipmentData equipmentData,
            ILogger<EquipmentDeploymentCompensatableOperation> logger)
        {
            return new EquipmentDeploymentCompensatableOperation(equipmentService, equipmentData, logger);
        }
    }
}
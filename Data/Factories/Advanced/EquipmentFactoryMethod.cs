using Microsoft.Extensions.Logging;
using SusEquip.Data.Models;
using SusEquip.Data.Interfaces;
using SusEquip.Data.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SusEquip.Data.Factories.Advanced
{
    /// <summary>
    /// Request object for equipment creation
    /// </summary>
    public class EquipmentCreateRequest
    {
        public string PCName { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string MachineType { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string AppOwner { get; set; } = string.Empty;
        public string MAC1 { get; set; } = string.Empty;
        public string MAC2 { get; set; } = string.Empty;
        public string UUID { get; set; } = string.Empty;
        public string ProductNo { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public string ServiceStart { get; set; } = string.Empty;
        public string ServiceEnd { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalProperties { get; set; } = new();

        // Type-specific properties
        public int? InstanceNumber { get; set; }
        public string? OldInstanceNumber { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
    }

    /// <summary>
    /// Abstract factory method creator for equipment
    /// </summary>
    public abstract class EquipmentCreator
    {
        protected readonly ILogger Logger;
        protected readonly IValidationService ValidationService;

        protected EquipmentCreator(ILogger logger, IValidationService validationService)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ValidationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        /// <summary>
        /// Abstract factory method - must be implemented by concrete creators
        /// </summary>
        /// <param name="request">Equipment creation request</param>
        /// <returns>Created equipment instance</returns>
        public abstract BaseEquipmentData CreateEquipment(EquipmentCreateRequest request);

        /// <summary>
        /// Template method that uses the factory method and adds common behavior
        /// </summary>
        /// <param name="request">Equipment creation request</param>
        /// <returns>Created and validated equipment instance</returns>
        public async Task<BaseEquipmentData> CreateAndValidateEquipmentAsync(EquipmentCreateRequest request)
        {
            Logger.LogInformation("Starting equipment creation and validation for PC: {PCName}", request.PCName);

            try
            {
                // Step 1: Validate request
                ValidateRequest(request);

                // Step 2: Create equipment using factory method
                var equipment = CreateEquipment(request);

                // Step 3: Set common properties
                SetCommonProperties(equipment, request);

                // Step 4: Validate created equipment
                await ValidateEquipment(equipment);

                // Step 5: Post-creation processing
                await PostCreationProcessing(equipment, request);

                Logger.LogInformation("Successfully created and validated equipment: {PCName} ({SerialNo})", 
                    equipment.PC_Name, equipment.Serial_No);

                return equipment;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to create equipment for PC: {PCName}", request.PCName);
                throw;
            }
        }

        /// <summary>
        /// Validates the creation request
        /// </summary>
        protected virtual void ValidateRequest(EquipmentCreateRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.PCName))
                errors.Add("PC Name is required");
            
            if (string.IsNullOrWhiteSpace(request.SerialNo))
                errors.Add("Serial Number is required");
            
            if (string.IsNullOrWhiteSpace(request.Creator))
                errors.Add("Creator is required");

            if (errors.Count > 0)
            {
                var errorMessage = $"Invalid equipment creation request: {string.Join(", ", errors)}";
                Logger.LogWarning(errorMessage);
                throw new ArgumentException(errorMessage, nameof(request));
            }
        }

        /// <summary>
        /// Sets properties common to all equipment types
        /// </summary>
        protected virtual void SetCommonProperties(BaseEquipmentData equipment, EquipmentCreateRequest request)
        {
            equipment.PC_Name = request.PCName;
            equipment.Serial_No = request.SerialNo;
            equipment.Creator_Initials = request.Creator;
            equipment.Department = request.Department;
            equipment.MachineType = request.MachineType;
            equipment.Status = request.Status;
            equipment.App_Owner = request.AppOwner;
            equipment.Mac_Address1 = request.MAC1;
            equipment.Mac_Address2 = request.MAC2;
            equipment.UUID = request.UUID;
            equipment.Product_No = request.ProductNo;
            equipment.Model_Name_and_No = request.ModelName;
            equipment.Service_Start = request.ServiceStart;
            equipment.Service_Ends = request.ServiceEnd;
            equipment.Entry_Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Add creation note
            var creationNote = $"[{DateTime.Now:yyyy-MM-dd HH:mm}] Created via {GetType().Name}";
            equipment.Note = string.IsNullOrWhiteSpace(request.Note) 
                ? creationNote 
                : $"{request.Note}\n{creationNote}";

            Logger.LogDebug("Set common properties for equipment: {PCName}", request.PCName);
        }

        /// <summary>
        /// Validates the created equipment using validation service
        /// </summary>
        protected virtual async Task ValidateEquipment(BaseEquipmentData equipment)
        {
            // Convert to EquipmentData if needed for the current validation service interface
            if (equipment is EquipmentData equipmentData)
            {
                var validationIssues = await ValidationService.ValidateEquipmentAsync(equipmentData);
                var highSeverityIssues = validationIssues.Where(i => i.Severity == "High").ToList();

                if (highSeverityIssues.Any())
                {
                    var errorMessage = $"High severity validation issues: {string.Join(", ", highSeverityIssues.Select(i => i.Reason))}";
                    Logger.LogWarning("Equipment validation failed: {ErrorMessage}", errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                if (validationIssues.Any())
                {
                    Logger.LogInformation("Equipment created with validation warnings: {WarningCount} issues", validationIssues.Count());
                }
            }
            else
            {
                // For non-EquipmentData types, perform basic validation
                Logger.LogInformation("Basic validation performed for {EquipmentType}", equipment.GetType().Name);
                if (string.IsNullOrWhiteSpace(equipment.PC_Name))
                    throw new InvalidOperationException("PC Name is required");
                if (string.IsNullOrWhiteSpace(equipment.Serial_No))
                    throw new InvalidOperationException("Serial Number is required");
            }
        }

        /// <summary>
        /// Post-creation processing hook for subclasses
        /// </summary>
        protected virtual async Task PostCreationProcessing(BaseEquipmentData equipment, EquipmentCreateRequest request)
        {
            Logger.LogDebug("Completed post-creation processing for equipment: {PCName}", equipment.PC_Name);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Concrete creator for standard equipment
    /// </summary>
    public class StandardEquipmentCreator : EquipmentCreator
    {
        private readonly IEquipmentRepository _equipmentRepository;

        public StandardEquipmentCreator(
            ILogger<StandardEquipmentCreator> logger, 
            IValidationService validationService,
            IEquipmentRepository equipmentRepository) 
            : base(logger, validationService)
        {
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
        }

        public override BaseEquipmentData CreateEquipment(EquipmentCreateRequest request)
        {
            Logger.LogDebug("Creating standard equipment for PC: {PCName}", request.PCName);

            var equipment = new EquipmentData();

            // Set instance number if provided, otherwise it will be set later
            if (request.InstanceNumber.HasValue)
            {
                equipment.Inst_No = request.InstanceNumber.Value;
            }

            return equipment;
        }

        protected override async Task PostCreationProcessing(BaseEquipmentData equipment, EquipmentCreateRequest request)
        {
            if (equipment is EquipmentData equipmentData)
            {
                // Auto-assign instance number if not provided
                if (equipmentData.Inst_No <= 0)
                {
                    equipmentData.Inst_No = await _equipmentRepository.GetNextInstNoAsync();
                    Logger.LogDebug("Auto-assigned instance number {InstNo} to equipment {PCName}", 
                        equipmentData.Inst_No, equipment.PC_Name);
                }

                // Check for conflicts
                var isInstNoTaken = await _equipmentRepository.IsInstNoTakenAsync(equipmentData.Inst_No);
                if (isInstNoTaken)
                {
                    throw new InvalidOperationException($"Instance number {equipmentData.Inst_No} is already taken");
                }

                var existingBySerial = await _equipmentRepository.GetBySerialNumberAsync(equipment.Serial_No);
                if (existingBySerial != null)
                {
                    throw new InvalidOperationException($"Serial number {equipment.Serial_No} is already in use by equipment ID {existingBySerial.EntryId}");
                }
            }

            await base.PostCreationProcessing(equipment, request);
        }
    }

    /// <summary>
    /// Concrete creator for old equipment
    /// </summary>
    public class OldEquipmentCreator : EquipmentCreator
    {
        private readonly IOldEquipmentRepository _oldEquipmentRepository;

        public OldEquipmentCreator(
            ILogger<OldEquipmentCreator> logger, 
            IValidationService validationService,
            IOldEquipmentRepository oldEquipmentRepository) 
            : base(logger, validationService)
        {
            _oldEquipmentRepository = oldEquipmentRepository ?? throw new ArgumentNullException(nameof(oldEquipmentRepository));
        }

        public override BaseEquipmentData CreateEquipment(EquipmentCreateRequest request)
        {
            Logger.LogDebug("Creating old equipment for PC: {PCName}", request.PCName);

            var equipment = new OLDEquipmentData();

            // Set instance number if provided
            if (!string.IsNullOrWhiteSpace(request.OldInstanceNumber))
            {
                equipment.Inst_No = request.OldInstanceNumber;
            }
            else if (request.InstanceNumber.HasValue)
            {
                equipment.Inst_No = $"O-{request.InstanceNumber.Value}";
            }

            return equipment;
        }

        protected override async Task PostCreationProcessing(BaseEquipmentData equipment, EquipmentCreateRequest request)
        {
            if (equipment is OLDEquipmentData oldEquipmentData)
            {
                // Auto-assign instance number if not provided
                if (string.IsNullOrWhiteSpace(oldEquipmentData.Inst_No))
                {
                    // For old equipment, we might use a different numbering scheme
                    var nextNumber = await GetNextOldEquipmentNumber();
                    oldEquipmentData.Inst_No = $"O-{nextNumber}";
                    Logger.LogDebug("Auto-assigned old equipment instance number {InstNo} to equipment {PCName}", 
                        oldEquipmentData.Inst_No, equipment.PC_Name);
                }

                // Check if this is really an old machine
                var isOldMachine = await _oldEquipmentRepository.IsOLDMachineAsync(equipment.PC_Name, equipment.Department);
                if (!isOldMachine)
                {
                    Logger.LogWarning("PC {PCName} in department {Department} may not be a true old machine", 
                        equipment.PC_Name, equipment.Department);
                }
            }

            await base.PostCreationProcessing(equipment, request);
        }

        private async Task<int> GetNextOldEquipmentNumber()
        {
            // This is a simplified implementation
            // In reality, you'd query the database for the highest old equipment number
            var allOldEquipment = await _oldEquipmentRepository.GetAllAsync();
            var maxNumber = allOldEquipment
                .Select(e => e.Inst_No)
                .Where(instNo => instNo.StartsWith("O-"))
                .Select(instNo => int.TryParse(instNo.Substring(2), out int num) ? num : 0)
                .DefaultIfEmpty(0)
                .Max();
            
            return maxNumber + 1;
        }
    }

    /// <summary>
    /// Concrete creator for machine equipment (extends standard equipment with maintenance info)
    /// </summary>
    public class MachineEquipmentCreator : EquipmentCreator
    {
        private readonly IEquipmentRepository _equipmentRepository;

        public MachineEquipmentCreator(
            ILogger<MachineEquipmentCreator> logger, 
            IValidationService validationService,
            IEquipmentRepository equipmentRepository) 
            : base(logger, validationService)
        {
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
        }

        public override BaseEquipmentData CreateEquipment(EquipmentCreateRequest request)
        {
            Logger.LogDebug("Creating machine equipment for PC: {PCName}", request.PCName);

            var equipment = new MachineData();

            // Set instance number if provided
            if (request.InstanceNumber.HasValue)
            {
                equipment.Inst_No = request.InstanceNumber.Value;
            }

            // Note: MachineData currently inherits from EquipmentData without additional properties
            // If maintenance tracking is needed, these properties would need to be added to MachineData

            return equipment;
        }

        protected override async Task PostCreationProcessing(BaseEquipmentData equipment, EquipmentCreateRequest request)
        {
            if (equipment is MachineData machineData)
            {
                // Auto-assign instance number if not provided
                if (machineData.Inst_No <= 0)
                {
                    machineData.Inst_No = await _equipmentRepository.GetNextInstNoAsync();
                    Logger.LogDebug("Auto-assigned instance number {InstNo} to machine {PCName}", 
                        machineData.Inst_No, equipment.PC_Name);
                }

                // Check for conflicts
                var isInstNoTaken = await _equipmentRepository.IsInstNoTakenAsync(machineData.Inst_No);
                if (isInstNoTaken)
                {
                    throw new InvalidOperationException($"Instance number {machineData.Inst_No} is already taken");
                }

                // Add machine-specific note
                var machineNote = $"[{DateTime.Now:yyyy-MM-dd}] Machine equipment created";
                if (request.LastMaintenanceDate.HasValue)
                {
                    machineNote += $" - Maintenance date: {request.LastMaintenanceDate.Value:yyyy-MM-dd}";
                }
                
                equipment.Note = string.IsNullOrWhiteSpace(equipment.Note) 
                    ? machineNote 
                    : $"{equipment.Note}\n{machineNote}";

                Logger.LogInformation("Created machine equipment {PCName} with instance number {InstNo}", 
                    equipment.PC_Name, machineData.Inst_No);
            }

            await base.PostCreationProcessing(equipment, request);
        }

        protected override void ValidateRequest(EquipmentCreateRequest request)
        {
            base.ValidateRequest(request);

            // Additional validation for machines
            if (string.IsNullOrWhiteSpace(request.MachineType))
            {
                Logger.LogWarning("Machine type not specified for machine equipment: {PCName}", request.PCName);
            }
        }
    }

    /// <summary>
    /// Factory for creating appropriate equipment creators
    /// </summary>
    public class EquipmentCreatorFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EquipmentCreatorFactory> _logger;

        public EquipmentCreatorFactory(IServiceProvider serviceProvider, ILogger<EquipmentCreatorFactory> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public EquipmentCreator GetCreator(string equipmentType)
        {
            return equipmentType.ToUpper() switch
            {
                "STANDARD" => _serviceProvider.GetRequiredService<StandardEquipmentCreator>(),
                "OLD" => _serviceProvider.GetRequiredService<OldEquipmentCreator>(),
                "MACHINE" => _serviceProvider.GetRequiredService<MachineEquipmentCreator>(),
                _ => throw new ArgumentException($"Unknown equipment type: {equipmentType}", nameof(equipmentType))
            };
        }

        public async Task<BaseEquipmentData> CreateEquipmentAsync(string equipmentType, EquipmentCreateRequest request)
        {
            _logger.LogInformation("Creating equipment of type {EquipmentType} for PC {PCName}", equipmentType, request.PCName);
            
            var creator = GetCreator(equipmentType);
            return await creator.CreateAndValidateEquipmentAsync(request);
        }
    }
}
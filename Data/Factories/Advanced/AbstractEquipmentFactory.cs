using Microsoft.Extensions.Logging;
using SusEquip.Data.Models;
using SusEquip.Data.Interfaces;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Services;
using SusEquip.Data.Services.Validation;
using SusEquip.Data.Repositories;
using SusEquip.Data.Commands;
using SusEquip.Data.Commands.Equipment;
using SusEquip.Data.Exceptions;
using System;

namespace SusEquip.Data.Factories.Advanced
{
    /// <summary>
    /// Abstract factory interface for creating families of equipment-related objects
    /// </summary>
    public interface IEquipmentAbstractFactory
    {
        /// <summary>
        /// Creates an equipment data instance
        /// </summary>
        BaseEquipmentData CreateEquipment();

        /// <summary>
        /// Creates a validator for this equipment type
        /// </summary>
        IValidationStrategy CreateValidator();

        /// <summary>
        /// Creates a repository for this equipment type
        /// </summary>
        IRepository<BaseEquipmentData> CreateRepository();

        /// <summary>
        /// Creates a command factory for this equipment type
        /// </summary>
        ICommandFactory CreateCommandFactory();

        /// <summary>
        /// Gets the equipment type identifier
        /// </summary>
        string EquipmentType { get; }
    }

    /// <summary>
    /// Command factory interface for creating equipment-related commands
    /// </summary>
    public interface ICommandFactory
    {
        ICommand<EquipmentOperationResult> CreateAddCommand(BaseEquipmentData equipment);
        ICommand<EquipmentOperationResult> CreateUpdateCommand(BaseEquipmentData equipment);
        ICommand<EquipmentOperationResult> CreateDeleteCommand(int entryId);
        string SupportedEquipmentType { get; }
    }

    /// <summary>
    /// Abstract factory for standard equipment (EquipmentData)
    /// </summary>
    public class StandardEquipmentFactory : IEquipmentAbstractFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StandardEquipmentFactory> _logger;

        public StandardEquipmentFactory(IServiceProvider serviceProvider, ILogger<StandardEquipmentFactory> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string EquipmentType => "STANDARD";

        public BaseEquipmentData CreateEquipment()
        {
            _logger.LogDebug("Creating standard equipment instance");
            return new EquipmentData();
        }

        public IValidationStrategy CreateValidator()
        {
            // For standard equipment, we might use a combination of validators
            // This is a placeholder - in real implementation, you might have StandardEquipmentValidator
            _logger.LogDebug("Creating validator for standard equipment");
            return _serviceProvider.GetService(typeof(IValidationService)) as IValidationStrategy 
                ?? throw new InvalidOperationException("No validation strategy available for standard equipment");
        }

        public IRepository<BaseEquipmentData> CreateRepository()
        {
            _logger.LogDebug("Creating repository for standard equipment");
            var equipmentRepo = _serviceProvider.GetService(typeof(IEquipmentRepository)) as IEquipmentRepository;
            if (equipmentRepo == null)
                throw new InvalidOperationException("Equipment repository not available");
            
            // Return a wrapper that adapts IEquipmentRepository to IRepository<BaseEquipmentData>
            return new EquipmentRepositoryAdapter(equipmentRepo);
        }

        public ICommandFactory CreateCommandFactory()
        {
            _logger.LogDebug("Creating command factory for standard equipment");
            return new StandardEquipmentCommandFactory(_serviceProvider, _logger);
        }
    }

    /// <summary>
    /// Abstract factory for old equipment (OLDEquipmentData)
    /// </summary>
    public class OldEquipmentFactory : IEquipmentAbstractFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OldEquipmentFactory> _logger;

        public OldEquipmentFactory(IServiceProvider serviceProvider, ILogger<OldEquipmentFactory> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string EquipmentType => "OLD";

        public BaseEquipmentData CreateEquipment()
        {
            _logger.LogDebug("Creating old equipment instance");
            return new OLDEquipmentData();
        }

        public IValidationStrategy CreateValidator()
        {
            _logger.LogDebug("Creating validator for old equipment");
            return _serviceProvider.GetService(typeof(IValidationService)) as IValidationStrategy 
                ?? throw new InvalidOperationException("No validation strategy available for old equipment");
        }

        public IRepository<BaseEquipmentData> CreateRepository()
        {
            _logger.LogDebug("Creating repository for old equipment");
            var oldEquipmentRepo = _serviceProvider.GetService(typeof(IOldEquipmentRepository)) as IOldEquipmentRepository;
            if (oldEquipmentRepo == null)
                throw new InvalidOperationException("Old equipment repository not available");
                
            return new OldEquipmentRepositoryAdapter(oldEquipmentRepo);
        }

        public ICommandFactory CreateCommandFactory()
        {
            _logger.LogDebug("Creating command factory for old equipment");
            return new OldEquipmentCommandFactory(_serviceProvider, _logger);
        }
    }

    /// <summary>
    /// Abstract factory for machine equipment (MachineData)
    /// </summary>
    public class MachineEquipmentFactory : IEquipmentAbstractFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MachineEquipmentFactory> _logger;

        public MachineEquipmentFactory(IServiceProvider serviceProvider, ILogger<MachineEquipmentFactory> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string EquipmentType => "MACHINE";

        public BaseEquipmentData CreateEquipment()
        {
            _logger.LogDebug("Creating machine equipment instance");
            return new MachineData();
        }

        public IValidationStrategy CreateValidator()
        {
            _logger.LogDebug("Creating validator for machine equipment");
            return _serviceProvider.GetService(typeof(IValidationService)) as IValidationStrategy 
                ?? throw new InvalidOperationException("No validation strategy available for machine equipment");
        }

        public IRepository<BaseEquipmentData> CreateRepository()
        {
            _logger.LogDebug("Creating repository for machine equipment");
            var equipmentRepo = _serviceProvider.GetService(typeof(IEquipmentRepository)) as IEquipmentRepository;
            if (equipmentRepo == null)
                throw new InvalidOperationException("Equipment repository not available");
                
            return new EquipmentRepositoryAdapter(equipmentRepo);
        }

        public ICommandFactory CreateCommandFactory()
        {
            _logger.LogDebug("Creating command factory for machine equipment");
            return new StandardEquipmentCommandFactory(_serviceProvider, _logger); // Machines use standard commands
        }
    }

    /// <summary>
    /// Command factory for standard equipment operations
    /// </summary>
    public class StandardEquipmentCommandFactory : ICommandFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public StandardEquipmentCommandFactory(IServiceProvider serviceProvider, ILogger logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string SupportedEquipmentType => "STANDARD";

        public ICommand<EquipmentOperationResult> CreateAddCommand(BaseEquipmentData equipment)
        {
            if (equipment is not EquipmentData equipmentData)
                throw new ArgumentException("Equipment must be of type EquipmentData for standard commands", nameof(equipment));

            var equipmentService = _serviceProvider.GetRequiredService<IEquipmentService>();
            var commandLogger = _serviceProvider.GetRequiredService<ILogger<AddEquipmentCommand>>();

            return new AddEquipmentCommand(equipmentData, equipmentService, commandLogger);
        }

        public ICommand<EquipmentOperationResult> CreateUpdateCommand(BaseEquipmentData equipment)
        {
            if (equipment is not EquipmentData equipmentData)
                throw new ArgumentException("Equipment must be of type EquipmentData for standard commands", nameof(equipment));

            var equipmentService = _serviceProvider.GetRequiredService<IEquipmentService>();
            var commandLogger = _serviceProvider.GetRequiredService<ILogger<UpdateEquipmentStatusCommand>>();

            // For simplicity, update status to "Updated" - in real scenario, you'd have more parameters
            return new UpdateEquipmentStatusCommand(equipmentData.Inst_No, "Updated", "System", equipmentService, commandLogger);
        }

        public ICommand<EquipmentOperationResult> CreateDeleteCommand(int entryId)
        {
            var equipmentService = _serviceProvider.GetRequiredService<IEquipmentService>();
            var commandLogger = _serviceProvider.GetRequiredService<ILogger<DeleteEquipmentCommand>>();

            return new DeleteEquipmentCommand(entryId, "System", equipmentService, commandLogger);
        }
    }

    /// <summary>
    /// Command factory for old equipment operations
    /// </summary>
    public class OldEquipmentCommandFactory : ICommandFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public OldEquipmentCommandFactory(IServiceProvider serviceProvider, ILogger logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string SupportedEquipmentType => "OLD";

        public ICommand<EquipmentOperationResult> CreateAddCommand(BaseEquipmentData equipment)
        {
            if (equipment is not OLDEquipmentData oldEquipmentData)
                throw new ArgumentException("Equipment must be of type OLDEquipmentData for old equipment commands", nameof(equipment));

            // For old equipment, we might need to convert to standard format first
            var factory = _serviceProvider.GetRequiredService<IEquipmentFactory>();
            var convertedEquipment = factory.CreateFromExisting(oldEquipmentData, "STANDARD") as EquipmentData;
            
            var equipmentService = _serviceProvider.GetRequiredService<IEquipmentService>();
            var commandLogger = _serviceProvider.GetRequiredService<ILogger<AddEquipmentCommand>>();

            return new AddEquipmentCommand(convertedEquipment!, equipmentService, commandLogger);
        }

        public ICommand<EquipmentOperationResult> CreateUpdateCommand(BaseEquipmentData equipment)
        {
            if (equipment is not OLDEquipmentData oldEquipmentData)
                throw new ArgumentException("Equipment must be of type OLDEquipmentData for old equipment commands", nameof(equipment));

            var equipmentService = _serviceProvider.GetRequiredService<IEquipmentService>();
            var commandLogger = _serviceProvider.GetRequiredService<ILogger<UpdateEquipmentStatusCommand>>();

            // Convert string Inst_No to int for the command parameter - use 0 as fallback
            int instNo = int.TryParse(oldEquipmentData.Inst_No, out int parsedInstNo) ? parsedInstNo : 0;
            
            return new UpdateEquipmentStatusCommand(instNo, "Updated", "System", equipmentService, commandLogger);
        }

        public ICommand<EquipmentOperationResult> CreateDeleteCommand(int entryId)
        {
            var equipmentService = _serviceProvider.GetRequiredService<IEquipmentService>();
            var commandLogger = _serviceProvider.GetRequiredService<ILogger<DeleteEquipmentCommand>>();

            return new DeleteEquipmentCommand(entryId, "System", equipmentService, commandLogger);
        }
    }
}
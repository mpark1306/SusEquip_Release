using SusEquip.Data.Models;
using SusEquip.Data.Factories.Abstract;
using SusEquip.Data.Factories.Methods;
using SusEquip.Data.Builders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace SusEquip.Data.Factories
{
    /// <summary>
    /// Advanced factory manager that orchestrates all factory patterns
    /// Provides unified interface for equipment creation using different patterns
    /// </summary>
    public interface IAdvancedEquipmentFactory
    {
        // Basic factory methods
        BaseEquipmentData CreateEquipment(string type);
        T CreateEquipment<T>() where T : BaseEquipmentData, new();
        
        // Abstract factory methods
        BaseEquipmentData CreateEquipmentFamily(string familyType, string role = "PRIMARY");
        IEquipmentFamilyFactory GetFamilyFactory(string familyType);
        
        // Builder pattern methods
        IEquipmentBuilder CreateBuilder();
        BaseEquipmentData CreateWithBuilder(Action<IEquipmentBuilder> builderAction);
        
        // Extensible factory methods
        BaseEquipmentData CreateWithParameters(string typeName, Dictionary<string, object> parameters);
        void RegisterCustomType(string typeName, Func<BaseEquipmentData> factory);
        
        // Utility methods
        IEnumerable<string> GetSupportedTypes();
        IEnumerable<string> GetSupportedFamilies();
        bool SupportsType(string typeName);
        bool SupportsFamilyType(string familyType);
    }

    /// <summary>
    /// Implementation of advanced factory manager
    /// Combines all factory patterns into a cohesive system
    /// </summary>
    public class AdvancedEquipmentFactory : IAdvancedEquipmentFactory
    {
        private readonly ILogger<AdvancedEquipmentFactory> _logger;
        private readonly IEquipmentFactory _basicFactory;
        private readonly IExtensibleEquipmentFactory _extensibleFactory;
        private readonly IEquipmentTypeRegistry _typeRegistry;
        private readonly Dictionary<string, Func<ILogger, IEquipmentFamilyFactory>> _familyFactories;

        public AdvancedEquipmentFactory(
            ILogger<AdvancedEquipmentFactory> logger,
            IEquipmentFactory basicFactory,
            IExtensibleEquipmentFactory extensibleFactory,
            IEquipmentTypeRegistry typeRegistry)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _basicFactory = basicFactory ?? throw new ArgumentNullException(nameof(basicFactory));
            _extensibleFactory = extensibleFactory ?? throw new ArgumentNullException(nameof(extensibleFactory));
            _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
            
            _familyFactories = new Dictionary<string, Func<ILogger, IEquipmentFamilyFactory>>(StringComparer.OrdinalIgnoreCase);
            RegisterFamilyFactories();
        }

        public BaseEquipmentData CreateEquipment(string type)
        {
            _logger.LogDebug($"Creating equipment using basic factory: {type}");
            return _basicFactory.CreateEquipment(type);
        }

        public T CreateEquipment<T>() where T : BaseEquipmentData, new()
        {
            _logger.LogDebug($"Creating equipment of type {typeof(T).Name}");
            return _basicFactory.CreateEquipment<T>();
        }

        public BaseEquipmentData CreateEquipmentFamily(string familyType, string role = "PRIMARY")
        {
            _logger.LogInformation($"Creating equipment family {familyType} with role {role}");
            
            var familyFactory = GetFamilyFactory(familyType);
            
            return role.ToUpperInvariant() switch
            {
                "PRIMARY" => familyFactory.CreatePrimaryEquipment(),
                "BACKUP" => familyFactory.CreateBackupEquipment(),
                "MONITOR" or "MONITORING" => familyFactory.CreateMonitoringEquipment(),
                _ => throw new ArgumentException($"Unknown family role: {role}", nameof(role))
            };
        }

        public IEquipmentFamilyFactory GetFamilyFactory(string familyType)
        {
            if (!_familyFactories.TryGetValue(familyType, out var factoryCreator))
            {
                throw new ArgumentException($"No family factory registered for type: {familyType}", nameof(familyType));
            }

            var factoryLogger = _logger; // In a real implementation, you might create a specific logger
            return factoryCreator(factoryLogger);
        }

        public IEquipmentBuilder CreateBuilder()
        {
            _logger.LogDebug("Creating equipment builder");
            var loggerFactory = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
            var builderLogger = loggerFactory.CreateLogger<EquipmentBuilder>();
            return new EquipmentBuilder(builderLogger);
        }

        public BaseEquipmentData CreateWithBuilder(Action<IEquipmentBuilder> builderAction)
        {
            if (builderAction == null)
                throw new ArgumentNullException(nameof(builderAction));

            _logger.LogDebug("Creating equipment with builder action");
            
            var builder = CreateBuilder();
            builderAction(builder);
            return builder.Build();
        }

        public BaseEquipmentData CreateWithParameters(string typeName, Dictionary<string, object> parameters)
        {
            _logger.LogDebug($"Creating equipment with parameters: {typeName}");
            return _extensibleFactory.CreateEquipment(typeName, parameters);
        }

        public void RegisterCustomType(string typeName, Func<BaseEquipmentData> factory)
        {
            _logger.LogInformation($"Registering custom equipment type: {typeName}");
            _typeRegistry.RegisterFactory(typeName, factory);
        }

        public IEnumerable<string> GetSupportedTypes()
        {
            return _extensibleFactory.GetSupportedTypes();
        }

        public IEnumerable<string> GetSupportedFamilies()
        {
            return _familyFactories.Keys;
        }

        public bool SupportsType(string typeName)
        {
            return _extensibleFactory.SupportsType(typeName);
        }

        public bool SupportsFamilyType(string familyType)
        {
            return _familyFactories.ContainsKey(familyType);
        }

        private void RegisterFamilyFactories()
        {
            // Register built-in family factories
            _familyFactories["SERVER"] = (logger) => new ServerEquipmentFactory(logger as ILogger<ServerEquipmentFactory> ?? 
                Microsoft.Extensions.Logging.Abstractions.NullLogger<ServerEquipmentFactory>.Instance);
                
            _familyFactories["WORKSTATION"] = (logger) => new WorkstationEquipmentFactory(logger as ILogger<WorkstationEquipmentFactory> ?? 
                Microsoft.Extensions.Logging.Abstractions.NullLogger<WorkstationEquipmentFactory>.Instance);

            _logger.LogInformation("Registered family factories: SERVER, WORKSTATION");
        }
    }
}
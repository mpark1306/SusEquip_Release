using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SusEquip.Data.Factories.Methods
{
    /// <summary>
    /// Thread-safe registry for equipment type factories
    /// Enables runtime registration of new equipment types
    /// </summary>
    public class EquipmentTypeRegistry : IEquipmentTypeRegistry
    {
        private readonly ILogger<EquipmentTypeRegistry> _logger;
        private readonly ConcurrentDictionary<string, Func<BaseEquipmentData>> _factories;

        public EquipmentTypeRegistry(ILogger<EquipmentTypeRegistry> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _factories = new ConcurrentDictionary<string, Func<BaseEquipmentData>>(StringComparer.OrdinalIgnoreCase);
            RegisterDefaultFactories();
        }

        public void RegisterFactory(string typeName, Func<BaseEquipmentData> factory)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));
            
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _factories.AddOrUpdate(typeName, factory, (key, existing) => factory);
            _logger.LogInformation($"Registered factory for equipment type: {typeName}");
        }

        public void RegisterFactory<T>(string typeName, Func<T> factory) where T : BaseEquipmentData
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            RegisterFactory(typeName, () => factory());
        }

        public void UnregisterFactory(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return;

            if (_factories.TryRemove(typeName, out _))
            {
                _logger.LogInformation($"Unregistered factory for equipment type: {typeName}");
            }
        }

        public bool IsRegistered(string typeName)
        {
            return !string.IsNullOrWhiteSpace(typeName) && _factories.ContainsKey(typeName);
        }

        public IEnumerable<string> GetRegisteredTypes()
        {
            return _factories.Keys.ToList();
        }

        public BaseEquipmentData CreateEquipment(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));

            if (!_factories.TryGetValue(typeName, out var factory))
            {
                throw new ArgumentException($"No factory registered for equipment type: {typeName}", nameof(typeName));
            }

            try
            {
                var equipment = factory();
                if (equipment == null)
                {
                    throw new InvalidOperationException($"Factory for type {typeName} returned null");
                }
                
                _logger.LogDebug($"Created equipment of type {typeName}: {equipment.GetType().Name}");
                return equipment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create equipment of type {typeName}");
                throw new InvalidOperationException($"Failed to create equipment of type {typeName}", ex);
            }
        }

        public T CreateEquipment<T>(string typeName) where T : BaseEquipmentData
        {
            var equipment = CreateEquipment(typeName);
            
            if (equipment is T typedEquipment)
            {
                return typedEquipment;
            }

            throw new InvalidCastException($"Equipment of type {typeName} cannot be cast to {typeof(T).Name}");
        }

        private void RegisterDefaultFactories()
        {
            // Register built-in equipment types
            RegisterFactory("STANDARD", () => new EquipmentData());
            RegisterFactory("EQUIPMENT", () => new EquipmentData());
            RegisterFactory("OLD", () => new OLDEquipmentData());
            RegisterFactory("LEGACY", () => new OLDEquipmentData());
            RegisterFactory("MACHINE", () => new MachineData());
            
            // Additional default types
            RegisterFactory("SERVER", () => new EquipmentData { MachineType = "Server" });
            RegisterFactory("WORKSTATION", () => new EquipmentData { MachineType = "Workstation" });
            RegisterFactory("LAPTOP", () => new EquipmentData { MachineType = "Laptop" });
            RegisterFactory("PRINTER", () => new EquipmentData { MachineType = "Printer" });
            RegisterFactory("SCANNER", () => new EquipmentData { MachineType = "Scanner" });
            RegisterFactory("NETWORK", () => new EquipmentData { MachineType = "Network Device" });

            _logger.LogInformation("Registered default equipment type factories");
        }
    }
}
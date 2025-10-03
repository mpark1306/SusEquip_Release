using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SusEquip.Data.Builders
{
    /// <summary>
    /// Fluent builder for constructing equipment with step-by-step validation
    /// Ensures data integrity and provides clear error messages
    /// </summary>
    public class EquipmentBuilder : IEquipmentBuilder
    {
        private readonly ILogger<EquipmentBuilder> _logger;
        private readonly List<(Func<BaseEquipmentData, bool> Validator, string ErrorMessage)> _validators;
        
        // Build state
        private string _pcName = string.Empty;
        private string _serialNumber = string.Empty;
        private string _appOwner = string.Empty;
        private string _creatorInitials = string.Empty;
        private string _status = "Active";
        private string _type = "Standard";
        private DateTime? _entryDate;
        private DateTime? _warrantyDate;
        private string _location = string.Empty;
        private string _processor = string.Empty;
        private string _memory = string.Empty;
        private string _storage = string.Empty;

        public EquipmentBuilder(ILogger<EquipmentBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validators = new List<(Func<BaseEquipmentData, bool>, string)>();
            AddDefaultValidators();
        }

        public IEquipmentBuilder SetBasicInfo(string pcName, string serialNumber)
        {
            _pcName = pcName ?? throw new ArgumentNullException(nameof(pcName));
            _serialNumber = serialNumber ?? throw new ArgumentNullException(nameof(serialNumber));
            
            _logger.LogDebug($"Set basic info: PC={pcName}, Serial={serialNumber}");
            return this;
        }

        public IEquipmentBuilder SetOwnership(string appOwner, string creatorInitials)
        {
            _appOwner = appOwner ?? throw new ArgumentNullException(nameof(appOwner));
            _creatorInitials = creatorInitials ?? throw new ArgumentNullException(nameof(creatorInitials));
            
            _logger.LogDebug($"Set ownership: Owner={appOwner}, Creator={creatorInitials}");
            return this;
        }

        public IEquipmentBuilder SetStatus(string status)
        {
            _status = status ?? "Active";
            _logger.LogDebug($"Set status: {status}");
            return this;
        }

        public IEquipmentBuilder SetType(string type)
        {
            _type = type ?? "Standard";
            _logger.LogDebug($"Set type: {type}");
            return this;
        }

        public IEquipmentBuilder SetDates(DateTime? entryDate = null, DateTime? warrantyDate = null)
        {
            _entryDate = entryDate ?? DateTime.Now;
            _warrantyDate = warrantyDate;
            
            _logger.LogDebug($"Set dates: Entry={_entryDate}, Warranty={_warrantyDate}");
            return this;
        }

        public IEquipmentBuilder SetLocation(string location)
        {
            _location = location ?? string.Empty;
            _logger.LogDebug($"Set location: {location}");
            return this;
        }

        public IEquipmentBuilder SetSpecs(string? processor = null, string? memory = null, string? storage = null)
        {
            _processor = processor ?? string.Empty;
            _memory = memory ?? string.Empty;
            _storage = storage ?? string.Empty;
            
            _logger.LogDebug($"Set specs: CPU={processor}, RAM={memory}, Storage={storage}");
            return this;
        }

        public IEquipmentBuilder AddValidation(Func<BaseEquipmentData, bool> validator, string errorMessage)
        {
            if (validator != null)
            {
                _validators.Add((validator, errorMessage ?? "Custom validation failed"));
                _logger.LogDebug($"Added custom validator: {errorMessage}");
            }
            return this;
        }

        public IEquipmentBuilder Reset()
        {
            _pcName = string.Empty;
            _serialNumber = string.Empty;
            _appOwner = string.Empty;
            _creatorInitials = string.Empty;
            _status = "Active";
            _type = "Standard";
            _entryDate = null;
            _warrantyDate = null;
            _location = string.Empty;
            _processor = string.Empty;
            _memory = string.Empty;
            _storage = string.Empty;
            
            // Keep default validators, clear custom ones
            var defaultValidators = _validators.Take(GetDefaultValidatorCount()).ToList();
            _validators.Clear();
            _validators.AddRange(defaultValidators);
            
            _logger.LogDebug("Builder reset to initial state");
            return this;
        }

        public BaseEquipmentData Build()
        {
            return Build<EquipmentData>();
        }

        public T Build<T>() where T : BaseEquipmentData, new()
        {
            _logger.LogInformation($"Building equipment of type {typeof(T).Name}");
            
            // Create equipment instance
            var equipment = new T
            {
                PC_Name = _pcName,
                Serial_No = _serialNumber,
                App_Owner = _appOwner,
                Creator_Initials = _creatorInitials,
                Status = _status,
                MachineType = _type,
                Entry_Date = (_entryDate ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
                Note = _location ?? string.Empty
            };

            // Set warranty date if specified
            if (_warrantyDate.HasValue)
            {
                // Assuming there's a warranty field, if not available, this would be skipped
                if (equipment is EquipmentData standardEquipment)
                {
                    // standardEquipment.WarrantyDate = _warrantyDate.Value.ToString("yyyy-MM-dd");
                }
            }

            // Set specifications for equipment that supports them
            if (equipment is EquipmentData equipmentWithSpecs)
            {
                // If the model has these fields, set them
                // equipmentWithSpecs.Processor = _processor;
                // equipmentWithSpecs.Memory = _memory;
                // equipmentWithSpecs.Storage = _storage;
            }

            // Validate before returning
            var (isValid, errors) = ValidateEquipment(equipment);
            if (!isValid)
            {
                var errorMessage = $"Equipment validation failed: {string.Join(", ", errors)}";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            _logger.LogInformation($"Successfully built equipment: {equipment.PC_Name} ({equipment.Serial_No})");
            return equipment;
        }

        public (bool IsValid, List<string> Errors) Validate()
        {
            // Create a temporary equipment for validation
            var tempEquipment = new EquipmentData
            {
                PC_Name = _pcName,
                Serial_No = _serialNumber,
                App_Owner = _appOwner,
                Creator_Initials = _creatorInitials,
                Status = _status,
                MachineType = _type,
                Entry_Date = (_entryDate ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
                Note = _location ?? string.Empty
            };

            return ValidateEquipment(tempEquipment);
        }

        private (bool IsValid, List<string> Errors) ValidateEquipment(BaseEquipmentData equipment)
        {
            var errors = new List<string>();

            foreach (var (validator, errorMessage) in _validators)
            {
                try
                {
                    if (!validator(equipment))
                    {
                        errors.Add(errorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Validation error: {errorMessage}");
                    errors.Add($"Validation exception: {errorMessage}");
                }
            }

            return (errors.Count == 0, errors);
        }

        private void AddDefaultValidators()
        {
            // Required field validators
            _validators.Add((eq => !string.IsNullOrWhiteSpace(eq.PC_Name), "PC Name is required"));
            _validators.Add((eq => !string.IsNullOrWhiteSpace(eq.Serial_No), "Serial Number is required"));
            _validators.Add((eq => !string.IsNullOrWhiteSpace(eq.App_Owner), "App Owner is required"));
            _validators.Add((eq => !string.IsNullOrWhiteSpace(eq.Creator_Initials), "Creator Initials is required"));
            
            // Format validators
            _validators.Add((eq => eq.PC_Name?.Length <= 50, "PC Name must be 50 characters or less"));
            _validators.Add((eq => eq.Serial_No?.Length <= 50, "Serial Number must be 50 characters or less"));
            _validators.Add((eq => eq.Creator_Initials?.Length <= 10, "Creator Initials must be 10 characters or less"));
            
            // Business rule validators
            _validators.Add((eq => IsValidStatus(eq.Status), "Status must be valid (Active, Inactive, Maintenance, Disposed)"));
        }

        private bool IsValidStatus(string status)
        {
            var validStatuses = new[] { "Active", "Inactive", "Maintenance", "Disposed", "Standby", "Monitoring" };
            return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }

        private int GetDefaultValidatorCount() => 8; // Number of default validators added
    }
}
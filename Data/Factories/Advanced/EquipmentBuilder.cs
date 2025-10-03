using Microsoft.Extensions.Logging;
using SusEquip.Data.Models;
using System;
using System.Collections.Generic;

namespace SusEquip.Data.Factories.Advanced
{
    /// <summary>
    /// Builder interface for constructing complex equipment objects
    /// </summary>
    public interface IEquipmentBuilder
    {
        /// <summary>
        /// Sets basic equipment information
        /// </summary>
        IEquipmentBuilder SetBasicInfo(string pcName, string serialNo);

        /// <summary>
        /// Sets network-related information
        /// </summary>
        IEquipmentBuilder SetNetworkInfo(string mac1, string mac2, string uuid);

        /// <summary>
        /// Sets service period information
        /// </summary>
        IEquipmentBuilder SetServiceInfo(DateTime start, DateTime end);

        /// <summary>
        /// Sets service period information with string dates
        /// </summary>
        IEquipmentBuilder SetServiceInfo(string start, string end);

        /// <summary>
        /// Sets department and machine type information
        /// </summary>
        IEquipmentBuilder SetDepartmentInfo(string department, string machineType);

        /// <summary>
        /// Sets audit and ownership information
        /// </summary>
        IEquipmentBuilder SetAuditInfo(string creator, string appOwner);

        /// <summary>
        /// Sets product information
        /// </summary>
        IEquipmentBuilder SetProductInfo(string productNo, string modelName);

        /// <summary>
        /// Sets instance number (for standard equipment)
        /// </summary>
        IEquipmentBuilder SetInstanceNumber(int instNo);

        /// <summary>
        /// Sets instance number (for old equipment)
        /// </summary>
        IEquipmentBuilder SetInstanceNumber(string instNo);

        /// <summary>
        /// Sets equipment status
        /// </summary>
        IEquipmentBuilder SetStatus(string status);

        /// <summary>
        /// Adds a note to the equipment
        /// </summary>
        IEquipmentBuilder AddNote(string note);

        /// <summary>
        /// Validates the current configuration
        /// </summary>
        IEquipmentBuilder Validate();

        /// <summary>
        /// Builds the equipment object
        /// </summary>
        BaseEquipmentData Build();

        /// <summary>
        /// Resets the builder to start fresh
        /// </summary>
        IEquipmentBuilder Reset();
    }

    /// <summary>
    /// Concrete builder for standard equipment
    /// </summary>
    public class StandardEquipmentBuilder : IEquipmentBuilder
    {
        private readonly ILogger<StandardEquipmentBuilder> _logger;
        private EquipmentData _equipment;
        private readonly List<string> _validationErrors;

        public StandardEquipmentBuilder(ILogger<StandardEquipmentBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _equipment = new EquipmentData();
            _validationErrors = new List<string>();
        }

        public IEquipmentBuilder SetBasicInfo(string pcName, string serialNo)
        {
            if (string.IsNullOrWhiteSpace(pcName))
                _validationErrors.Add("PC Name cannot be empty");
            if (string.IsNullOrWhiteSpace(serialNo))
                _validationErrors.Add("Serial Number cannot be empty");

            _equipment.PC_Name = pcName ?? string.Empty;
            _equipment.Serial_No = serialNo ?? string.Empty;
            _equipment.Entry_Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _logger.LogDebug("Set basic info: PC={PCName}, Serial={SerialNo}", pcName, serialNo);
            return this;
        }

        public IEquipmentBuilder SetNetworkInfo(string mac1, string mac2, string uuid)
        {
            _equipment.Mac_Address1 = mac1 ?? string.Empty;
            _equipment.Mac_Address2 = mac2 ?? string.Empty;
            _equipment.UUID = uuid ?? string.Empty;

            _logger.LogDebug("Set network info: MAC1={MAC1}, MAC2={MAC2}, UUID={UUID}", mac1, mac2, uuid);
            return this;
        }

        public IEquipmentBuilder SetServiceInfo(DateTime start, DateTime end)
        {
            return SetServiceInfo(start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
        }

        public IEquipmentBuilder SetServiceInfo(string start, string end)
        {
            if (DateTime.TryParse(start, out DateTime startDate) && DateTime.TryParse(end, out DateTime endDate))
            {
                if (endDate < startDate)
                    _validationErrors.Add("Service end date cannot be before start date");
            }

            _equipment.Service_Start = start ?? string.Empty;
            _equipment.Service_Ends = end ?? string.Empty;

            _logger.LogDebug("Set service info: Start={Start}, End={End}", start, end);
            return this;
        }

        public IEquipmentBuilder SetDepartmentInfo(string department, string machineType)
        {
            _equipment.Department = department ?? string.Empty;
            _equipment.MachineType = machineType ?? string.Empty;

            _logger.LogDebug("Set department info: Department={Department}, MachineType={MachineType}", department, machineType);
            return this;
        }

        public IEquipmentBuilder SetAuditInfo(string creator, string appOwner)
        {
            if (string.IsNullOrWhiteSpace(creator))
                _validationErrors.Add("Creator initials cannot be empty");

            _equipment.Creator_Initials = creator ?? string.Empty;
            _equipment.App_Owner = appOwner ?? string.Empty;

            _logger.LogDebug("Set audit info: Creator={Creator}, AppOwner={AppOwner}", creator, appOwner);
            return this;
        }

        public IEquipmentBuilder SetProductInfo(string productNo, string modelName)
        {
            _equipment.Product_No = productNo ?? string.Empty;
            _equipment.Model_Name_and_No = modelName ?? string.Empty;

            _logger.LogDebug("Set product info: ProductNo={ProductNo}, ModelName={ModelName}", productNo, modelName);
            return this;
        }

        public IEquipmentBuilder SetInstanceNumber(int instNo)
        {
            if (instNo <= 0)
                _validationErrors.Add("Instance number must be greater than 0");

            _equipment.Inst_No = instNo;

            _logger.LogDebug("Set instance number: {InstNo}", instNo);
            return this;
        }

        public IEquipmentBuilder SetInstanceNumber(string instNo)
        {
            if (int.TryParse(instNo, out int parsedInstNo))
            {
                return SetInstanceNumber(parsedInstNo);
            }
            else
            {
                _validationErrors.Add($"Cannot parse instance number: {instNo}");
                return this;
            }
        }

        public IEquipmentBuilder SetStatus(string status)
        {
            var validStatuses = new[] { "Active", "Inactive", "Karantæne", "Kasseret", "Brugt", "Ny", "Out of Service" };
            if (!string.IsNullOrWhiteSpace(status) && Array.IndexOf(validStatuses, status) == -1)
                _validationErrors.Add($"Invalid status: {status}. Valid statuses are: {string.Join(", ", validStatuses)}");

            _equipment.Status = status ?? "Active"; // Default to Active

            _logger.LogDebug("Set status: {Status}", status);
            return this;
        }

        public IEquipmentBuilder AddNote(string note)
        {
            if (!string.IsNullOrWhiteSpace(note))
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                var noteToAdd = $"[{timestamp}] {note}";
                
                _equipment.Note = string.IsNullOrWhiteSpace(_equipment.Note) 
                    ? noteToAdd 
                    : $"{_equipment.Note}\n{noteToAdd}";

                _logger.LogDebug("Added note: {Note}", note);
            }
            return this;
        }

        public IEquipmentBuilder Validate()
        {
            if (_validationErrors.Count > 0)
            {
                var errors = string.Join(", ", _validationErrors);
                _logger.LogWarning("Validation errors found: {Errors}", errors);
            }
            else
            {
                _logger.LogDebug("Validation passed for equipment builder");
            }
            return this;
        }

        public BaseEquipmentData Build()
        {
            if (_validationErrors.Count > 0)
            {
                var errors = string.Join(", ", _validationErrors);
                throw new InvalidOperationException($"Cannot build equipment with validation errors: {errors}");
            }

            _logger.LogInformation("Built standard equipment: PC={PCName}, Serial={SerialNo}, InstNo={InstNo}", 
                _equipment.PC_Name, _equipment.Serial_No, _equipment.Inst_No);

            var result = _equipment;
            Reset(); // Reset for next build
            return result;
        }

        public IEquipmentBuilder Reset()
        {
            _equipment = new EquipmentData();
            _validationErrors.Clear();
            _logger.LogDebug("Builder reset for new equipment construction");
            return this;
        }
    }

    /// <summary>
    /// Concrete builder for old equipment
    /// </summary>
    public class OldEquipmentBuilder : IEquipmentBuilder
    {
        private readonly ILogger<OldEquipmentBuilder> _logger;
        private OLDEquipmentData _equipment;
        private readonly List<string> _validationErrors;

        public OldEquipmentBuilder(ILogger<OldEquipmentBuilder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _equipment = new OLDEquipmentData();
            _validationErrors = new List<string>();
        }

        public IEquipmentBuilder SetBasicInfo(string pcName, string serialNo)
        {
            if (string.IsNullOrWhiteSpace(pcName))
                _validationErrors.Add("PC Name cannot be empty");
            if (string.IsNullOrWhiteSpace(serialNo))
                _validationErrors.Add("Serial Number cannot be empty");

            _equipment.PC_Name = pcName ?? string.Empty;
            _equipment.Serial_No = serialNo ?? string.Empty;
            _equipment.Entry_Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _logger.LogDebug("Set basic info for old equipment: PC={PCName}, Serial={SerialNo}", pcName, serialNo);
            return this;
        }

        public IEquipmentBuilder SetNetworkInfo(string mac1, string mac2, string uuid)
        {
            _equipment.Mac_Address1 = mac1 ?? string.Empty;
            _equipment.Mac_Address2 = mac2 ?? string.Empty;
            _equipment.UUID = uuid ?? string.Empty;

            _logger.LogDebug("Set network info for old equipment: MAC1={MAC1}, MAC2={MAC2}, UUID={UUID}", mac1, mac2, uuid);
            return this;
        }

        public IEquipmentBuilder SetServiceInfo(DateTime start, DateTime end)
        {
            return SetServiceInfo(start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));
        }

        public IEquipmentBuilder SetServiceInfo(string start, string end)
        {
            if (DateTime.TryParse(start, out DateTime startDate) && DateTime.TryParse(end, out DateTime endDate))
            {
                if (endDate < startDate)
                    _validationErrors.Add("Service end date cannot be before start date");
            }

            _equipment.Service_Start = start ?? string.Empty;
            _equipment.Service_Ends = end ?? string.Empty;

            _logger.LogDebug("Set service info for old equipment: Start={Start}, End={End}", start, end);
            return this;
        }

        public IEquipmentBuilder SetDepartmentInfo(string department, string machineType)
        {
            _equipment.Department = department ?? string.Empty;
            _equipment.MachineType = machineType ?? string.Empty;

            _logger.LogDebug("Set department info for old equipment: Department={Department}, MachineType={MachineType}", department, machineType);
            return this;
        }

        public IEquipmentBuilder SetAuditInfo(string creator, string appOwner)
        {
            if (string.IsNullOrWhiteSpace(creator))
                _validationErrors.Add("Creator initials cannot be empty");

            _equipment.Creator_Initials = creator ?? string.Empty;
            _equipment.App_Owner = appOwner ?? string.Empty;

            _logger.LogDebug("Set audit info for old equipment: Creator={Creator}, AppOwner={AppOwner}", creator, appOwner);
            return this;
        }

        public IEquipmentBuilder SetProductInfo(string productNo, string modelName)
        {
            _equipment.Product_No = productNo ?? string.Empty;
            _equipment.Model_Name_and_No = modelName ?? string.Empty;

            _logger.LogDebug("Set product info for old equipment: ProductNo={ProductNo}, ModelName={ModelName}", productNo, modelName);
            return this;
        }

        public IEquipmentBuilder SetInstanceNumber(int instNo)
        {
            _equipment.Inst_No = instNo.ToString();

            _logger.LogDebug("Set instance number for old equipment: {InstNo}", instNo);
            return this;
        }

        public IEquipmentBuilder SetInstanceNumber(string instNo)
        {
            if (string.IsNullOrWhiteSpace(instNo))
                _validationErrors.Add("Instance number cannot be empty for old equipment");

            _equipment.Inst_No = instNo ?? string.Empty;

            _logger.LogDebug("Set instance number for old equipment: {InstNo}", instNo);
            return this;
        }

        public IEquipmentBuilder SetStatus(string status)
        {
            var validStatuses = new[] { "Active", "Inactive", "Karantæne", "Kasseret", "Brugt", "Ny", "Out of Service" };
            if (!string.IsNullOrWhiteSpace(status) && Array.IndexOf(validStatuses, status) == -1)
                _validationErrors.Add($"Invalid status: {status}. Valid statuses are: {string.Join(", ", validStatuses)}");

            _equipment.Status = status ?? "Active"; // Default to Active

            _logger.LogDebug("Set status for old equipment: {Status}", status);
            return this;
        }

        public IEquipmentBuilder AddNote(string note)
        {
            if (!string.IsNullOrWhiteSpace(note))
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                var noteToAdd = $"[{timestamp}] {note}";
                
                _equipment.Note = string.IsNullOrWhiteSpace(_equipment.Note) 
                    ? noteToAdd 
                    : $"{_equipment.Note}\n{noteToAdd}";

                _logger.LogDebug("Added note to old equipment: {Note}", note);
            }
            return this;
        }

        public IEquipmentBuilder Validate()
        {
            if (_validationErrors.Count > 0)
            {
                var errors = string.Join(", ", _validationErrors);
                _logger.LogWarning("Validation errors found for old equipment: {Errors}", errors);
            }
            else
            {
                _logger.LogDebug("Validation passed for old equipment builder");
            }
            return this;
        }

        public BaseEquipmentData Build()
        {
            if (_validationErrors.Count > 0)
            {
                var errors = string.Join(", ", _validationErrors);
                throw new InvalidOperationException($"Cannot build old equipment with validation errors: {errors}");
            }

            _logger.LogInformation("Built old equipment: PC={PCName}, Serial={SerialNo}, InstNo={InstNo}", 
                _equipment.PC_Name, _equipment.Serial_No, _equipment.Inst_No);

            var result = _equipment;
            Reset(); // Reset for next build
            return result;
        }

        public IEquipmentBuilder Reset()
        {
            _equipment = new OLDEquipmentData();
            _validationErrors.Clear();
            _logger.LogDebug("Old equipment builder reset for new equipment construction");
            return this;
        }
    }

    /// <summary>
    /// Director class that knows how to build common equipment configurations
    /// </summary>
    public class EquipmentDirector
    {
        private readonly ILogger<EquipmentDirector> _logger;

        public EquipmentDirector(ILogger<EquipmentDirector> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Builds a minimal equipment configuration
        /// </summary>
        public BaseEquipmentData BuildMinimalEquipment(IEquipmentBuilder builder, string pcName, string serialNo, string creator)
        {
            _logger.LogDebug("Building minimal equipment configuration");
            
            return builder
                .SetBasicInfo(pcName, serialNo)
                .SetAuditInfo(creator, creator)
                .SetStatus("Active")
                .AddNote("Created via builder pattern")
                .Validate()
                .Build();
        }

        /// <summary>
        /// Builds a complete equipment configuration
        /// </summary>
        public BaseEquipmentData BuildCompleteEquipment(
            IEquipmentBuilder builder,
            string pcName,
            string serialNo,
            string creator,
            string department,
            string machineType,
            string mac1 = "",
            string mac2 = "",
            string uuid = "",
            string productNo = "",
            string modelName = "",
            DateTime? serviceStart = null,
            DateTime? serviceEnd = null)
        {
            _logger.LogDebug("Building complete equipment configuration");

            builder = builder
                .SetBasicInfo(pcName, serialNo)
                .SetAuditInfo(creator, creator)
                .SetDepartmentInfo(department, machineType)
                .SetStatus("Active");

            if (!string.IsNullOrWhiteSpace(mac1) || !string.IsNullOrWhiteSpace(mac2) || !string.IsNullOrWhiteSpace(uuid))
            {
                builder = builder.SetNetworkInfo(mac1, mac2, uuid);
            }

            if (!string.IsNullOrWhiteSpace(productNo) || !string.IsNullOrWhiteSpace(modelName))
            {
                builder = builder.SetProductInfo(productNo, modelName);
            }

            if (serviceStart.HasValue && serviceEnd.HasValue)
            {
                builder = builder.SetServiceInfo(serviceStart.Value, serviceEnd.Value);
            }

            return builder
                .AddNote("Created with complete configuration")
                .Validate()
                .Build();
        }
    }
}
using System;
using System.Collections.Generic;

namespace SusEquip.Data.Exceptions
{
    /// <summary>
    /// Exception thrown when equipment cannot be found in the system.
    /// Used for missing equipment records, invalid IDs, and data access failures.
    /// </summary>
    public class EquipmentNotFoundException : SusEquipException
    {
        /// <summary>
        /// The identifier used to search for the equipment
        /// </summary>
        public object? SearchCriteria { get; }
        
        /// <summary>
        /// The type of search that was performed
        /// </summary>
        public string SearchType { get; }

        public EquipmentNotFoundException(
            string searchType,
            object? searchCriteria,
            string? customMessage = null,
            string? customUserMessage = null,
            Exception? innerException = null)
            : base(
                errorCode: "EQUIPMENT_NOT_FOUND",
                message: customMessage ?? $"Equipment not found using {searchType}: {searchCriteria}",
                userMessage: customUserMessage ?? CreateDefaultUserMessage(searchType, searchCriteria),
                severity: ErrorSeverity.Warning,
                innerException: innerException,
                errorContext: CreateContext(searchType, searchCriteria))
        {
            SearchCriteria = searchCriteria;
            SearchType = searchType;
        }

        /// <summary>
        /// Creates exception for equipment not found by ID
        /// </summary>
        public static EquipmentNotFoundException ById(int equipmentId)
        {
            return new EquipmentNotFoundException(
                searchType: "ID",
                searchCriteria: equipmentId,
                customUserMessage: $"No equipment found with ID {equipmentId}. The equipment may have been deleted or the ID is incorrect.");
        }

        /// <summary>
        /// Creates exception for equipment not found by serial number
        /// </summary>
        public static EquipmentNotFoundException BySerialNumber(string serialNumber)
        {
            return new EquipmentNotFoundException(
                searchType: "Serial Number",
                searchCriteria: serialNumber,
                customUserMessage: $"No equipment found with serial number '{serialNumber}'. Please verify the serial number is correct.");
        }

        /// <summary>
        /// Creates exception for equipment not found by PC name
        /// </summary>
        public static EquipmentNotFoundException ByPCName(string pcName)
        {
            return new EquipmentNotFoundException(
                searchType: "PC Name",
                searchCriteria: pcName,
                customUserMessage: $"No equipment found with PC name '{pcName}'. Please check the name and try again.");
        }

        /// <summary>
        /// Creates exception for equipment not found by MAC address
        /// </summary>
        public static EquipmentNotFoundException ByMacAddress(string macAddress)
        {
            return new EquipmentNotFoundException(
                searchType: "MAC Address",
                searchCriteria: macAddress,
                customUserMessage: $"No equipment found with MAC address '{macAddress}'. Please verify the MAC address format.");
        }

        /// <summary>
        /// Creates exception for equipment not found by UUID
        /// </summary>
        public static EquipmentNotFoundException ByUuid(string uuid)
        {
            return new EquipmentNotFoundException(
                searchType: "UUID",
                searchCriteria: uuid,
                customUserMessage: $"No equipment found with UUID '{uuid}'. The equipment may not be registered in the system.");
        }

        /// <summary>
        /// Creates exception for equipment not found by multiple criteria
        /// </summary>
        public static EquipmentNotFoundException ByMultipleCriteria(Dictionary<string, object> criteria)
        {
            var criteriaString = string.Join(", ", criteria.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            
            return new EquipmentNotFoundException(
                searchType: "Multiple Criteria",
                searchCriteria: criteria,
                customMessage: $"Equipment not found using multiple criteria: {criteriaString}",
                customUserMessage: "No equipment found matching the specified criteria. Please refine your search parameters.");
        }

        /// <summary>
        /// Creates exception for equipment that exists but is in wrong state
        /// </summary>
        public static EquipmentNotFoundException InWrongState(
            object searchCriteria,
            string currentState,
            string expectedState)
        {
            var exception = new EquipmentNotFoundException(
                searchType: "State Check",
                searchCriteria: searchCriteria,
                customMessage: $"Equipment found but in wrong state. Current: {currentState}, Expected: {expectedState}",
                customUserMessage: $"The equipment was found but is currently {currentState}. This operation requires the equipment to be {expectedState}.");
            
            exception.AddContext("CurrentState", currentState);
            exception.AddContext("ExpectedState", expectedState);
            return exception;
        }

        /// <summary>
        /// Creates exception for equipment in deleted/archived state
        /// </summary>
        public static EquipmentNotFoundException Deleted(object searchCriteria, DateTime? deletedDate = null)
        {
            var exception = new EquipmentNotFoundException(
                searchType: "Deleted Equipment",
                searchCriteria: searchCriteria,
                customMessage: "Equipment found but has been deleted",
                customUserMessage: "The requested equipment has been deleted and is no longer available.");
            
            if (deletedDate.HasValue)
            {
                exception.AddContext("DeletedDate", deletedDate.Value);
            }
            
            return exception;
        }

        private static string CreateDefaultUserMessage(string searchType, object? searchCriteria)
        {
            return $"No equipment found using {searchType}: {searchCriteria}. Please check your search criteria and try again.";
        }

        private static Dictionary<string, object> CreateContext(string searchType, object? searchCriteria)
        {
            var context = new Dictionary<string, object>
            {
                ["SearchType"] = searchType
            };
            
            if (searchCriteria != null)
            {
                context["SearchCriteria"] = searchCriteria;
                context["SearchCriteriaType"] = searchCriteria.GetType().Name;
            }
            
            return context;
        }
    }
}
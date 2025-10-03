using System;
using System.Collections.Generic;
using SusEquip.Data.Models;

namespace SusEquip.Data.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to create equipment with a serial number that already exists.
    /// Used for duplicate constraint violations and uniqueness enforcement.
    /// </summary>
    public class DuplicateSerialNumberException : SusEquipException
    {
        /// <summary>
        /// The duplicate serial number that caused the conflict
        /// </summary>
        public string SerialNumber { get; }
        
        /// <summary>
        /// The existing equipment that has the conflicting serial number
        /// </summary>
        public BaseEquipmentData? ExistingEquipment { get; }
        
        /// <summary>
        /// The new equipment that was being created/updated
        /// </summary>
        public BaseEquipmentData? NewEquipment { get; }

        public DuplicateSerialNumberException(
            string serialNumber,
            BaseEquipmentData? existingEquipment = null,
            BaseEquipmentData? newEquipment = null,
            string? customMessage = null,
            string? customUserMessage = null,
            Exception? innerException = null)
            : base(
                errorCode: "DUPLICATE_SERIAL_NUMBER",
                message: customMessage ?? $"Duplicate serial number detected: {serialNumber}",
                userMessage: customUserMessage ?? CreateDefaultUserMessage(serialNumber, existingEquipment),
                severity: ErrorSeverity.Error,
                innerException: innerException,
                errorContext: CreateContext(serialNumber, existingEquipment, newEquipment))
        {
            SerialNumber = serialNumber;
            ExistingEquipment = existingEquipment;
            NewEquipment = newEquipment;
        }

        /// <summary>
        /// Creates exception for duplicate serial number during equipment creation
        /// </summary>
        public static DuplicateSerialNumberException ForCreation(
            string serialNumber,
            BaseEquipmentData newEquipment,
            BaseEquipmentData? existingEquipment = null)
        {
            return new DuplicateSerialNumberException(
                serialNumber: serialNumber,
                existingEquipment: existingEquipment,
                newEquipment: newEquipment,
                customMessage: $"Cannot create equipment: serial number '{serialNumber}' already exists",
                customUserMessage: $"The serial number '{serialNumber}' is already assigned to another piece of equipment. Please use a unique serial number.");
        }

        /// <summary>
        /// Creates exception for duplicate serial number during equipment update
        /// </summary>
        public static DuplicateSerialNumberException ForUpdate(
            string serialNumber,
            BaseEquipmentData updatedEquipment,
            BaseEquipmentData? conflictingEquipment = null)
        {
            return new DuplicateSerialNumberException(
                serialNumber: serialNumber,
                existingEquipment: conflictingEquipment,
                newEquipment: updatedEquipment,
                customMessage: $"Cannot update equipment: serial number '{serialNumber}' is already in use by another equipment",
                customUserMessage: $"Cannot update serial number to '{serialNumber}' because it's already assigned to another piece of equipment. Please choose a different serial number.");
        }

        /// <summary>
        /// Creates exception for duplicate serial number during bulk import
        /// </summary>
        public static DuplicateSerialNumberException ForBulkImport(
            string serialNumber,
            int importRowNumber,
            BaseEquipmentData? existingEquipment = null)
        {
            var exception = new DuplicateSerialNumberException(
                serialNumber: serialNumber,
                existingEquipment: existingEquipment,
                customMessage: $"Bulk import failed: duplicate serial number '{serialNumber}' found in row {importRowNumber}",
                customUserMessage: $"Import failed at row {importRowNumber}: The serial number '{serialNumber}' is already in use. Please correct the import data and try again.");
            
            exception.AddContext("ImportRowNumber", importRowNumber);
            exception.AddContext("OperationType", "BulkImport");
            return exception;
        }

        /// <summary>
        /// Creates exception for duplicate serial number within the same import batch
        /// </summary>
        public static DuplicateSerialNumberException ForImportBatchDuplicate(
            string serialNumber,
            int firstRowNumber,
            int duplicateRowNumber)
        {
            var exception = new DuplicateSerialNumberException(
                serialNumber: serialNumber,
                customMessage: $"Duplicate serial number '{serialNumber}' found within import batch: first occurrence at row {firstRowNumber}, duplicate at row {duplicateRowNumber}",
                customUserMessage: $"The serial number '{serialNumber}' appears multiple times in your import data (rows {firstRowNumber} and {duplicateRowNumber}). Each serial number must be unique.");
            
            exception.AddContext("FirstRowNumber", firstRowNumber);
            exception.AddContext("DuplicateRowNumber", duplicateRowNumber);
            exception.AddContext("OperationType", "ImportBatchValidation");
            return exception;
        }

        /// <summary>
        /// Creates exception when serial number conflicts with different equipment types
        /// </summary>
        public static DuplicateSerialNumberException CrossTypeConflict(
            string serialNumber,
            string newEquipmentType,
            string existingEquipmentType,
            BaseEquipmentData? existingEquipment = null)
        {
            var exception = new DuplicateSerialNumberException(
                serialNumber: serialNumber,
                existingEquipment: existingEquipment,
                customMessage: $"Serial number '{serialNumber}' conflict: attempting to create {newEquipmentType} but serial already exists for {existingEquipmentType}",
                customUserMessage: $"The serial number '{serialNumber}' is already assigned to a {existingEquipmentType}. Serial numbers must be unique across all equipment types.");
            
            exception.AddContext("NewEquipmentType", newEquipmentType);
            exception.AddContext("ExistingEquipmentType", existingEquipmentType);
            return exception;
        }

        /// <summary>
        /// Creates exception for MAC address conflicts (when serial numbers are based on MAC)
        /// </summary>
        public static DuplicateSerialNumberException MacAddressConflict(
            string serialNumber,
            string macAddress,
            BaseEquipmentData? existingEquipment = null)
        {
            var exception = new DuplicateSerialNumberException(
                serialNumber: serialNumber,
                existingEquipment: existingEquipment,
                customMessage: $"Serial number '{serialNumber}' derived from MAC address '{macAddress}' already exists",
                customUserMessage: $"Equipment with MAC address '{macAddress}' is already registered in the system. Each device's MAC address must be unique.");
            
            exception.AddContext("MACAddress", macAddress);
            exception.AddContext("ConflictType", "MACAddressBased");
            return exception;
        }

        private static string CreateDefaultUserMessage(string serialNumber, BaseEquipmentData? existingEquipment)
        {
            if (existingEquipment != null)
            {
                return $"The serial number '{serialNumber}' is already assigned to equipment '{existingEquipment.PC_Name}' (ID: {existingEquipment.EntryId}). Please use a unique serial number.";
            }
            
            return $"The serial number '{serialNumber}' is already in use. Please choose a different serial number.";
        }

        private static Dictionary<string, object> CreateContext(
            string serialNumber,
            BaseEquipmentData? existingEquipment,
            BaseEquipmentData? newEquipment)
        {
            var context = new Dictionary<string, object>
            {
                ["DuplicateSerialNumber"] = serialNumber
            };
            
            if (existingEquipment != null)
            {
                context["ExistingEquipmentId"] = existingEquipment.EntryId;
                context["ExistingEquipmentPCName"] = existingEquipment.PC_Name ?? "N/A";
                context["ExistingEquipmentType"] = existingEquipment.GetType().Name;
                context["ExistingEquipmentStatus"] = existingEquipment.Status ?? "Unknown";
            }
            
            if (newEquipment != null)
            {
                context["NewEquipmentPCName"] = newEquipment.PC_Name ?? "N/A";
                context["NewEquipmentType"] = newEquipment.GetType().Name;
                
                if (newEquipment.EntryId > 0)
                {
                    context["NewEquipmentId"] = newEquipment.EntryId;
                }
            }
            
            return context;
        }
    }
}
using System;
using System.Collections.Generic;
using SusEquip.Data.Models;

namespace SusEquip.Data.Exceptions
{
    /// <summary>
    /// Exception thrown when equipment validation fails.
    /// Used for data validation errors, business rule violations, and constraint failures.
    /// </summary>
    public class EquipmentValidationException : SusEquipException
    {
        /// <summary>
        /// The equipment data that failed validation
        /// </summary>
        public BaseEquipmentData? Equipment { get; }
        
        /// <summary>
        /// List of specific validation errors
        /// </summary>
        public List<ValidationError> ValidationErrors { get; }
        
        /// <summary>
        /// The field that failed validation (if specific to one field)
        /// </summary>
        public string? FieldName { get; }

        public EquipmentValidationException(
            string message,
            string userMessage = "The equipment data provided is not valid. Please check the highlighted fields and try again.",
            List<ValidationError>? validationErrors = null,
            BaseEquipmentData? equipment = null,
            string? fieldName = null,
            Exception? innerException = null)
            : base(
                errorCode: "EQUIPMENT_VALIDATION_FAILED",
                message: message,
                userMessage: userMessage,
                severity: ErrorSeverity.Warning,
                innerException: innerException,
                errorContext: CreateContext(equipment, fieldName, validationErrors))
        {
            Equipment = equipment;
            ValidationErrors = validationErrors ?? new List<ValidationError>();
            FieldName = fieldName;
        }

        /// <summary>
        /// Creates validation exception for a specific field
        /// </summary>
        public static EquipmentValidationException ForField(
            string fieldName,
            string message,
            string userMessage,
            BaseEquipmentData? equipment = null,
            object? fieldValue = null)
        {
            var validationError = new ValidationError
            {
                FieldName = fieldName,
                ErrorMessage = message,
                AttemptedValue = fieldValue?.ToString(),
                ErrorCode = "FIELD_VALIDATION_FAILED"
            };

            return new EquipmentValidationException(
                message: $"Validation failed for field '{fieldName}': {message}",
                userMessage: userMessage,
                validationErrors: new List<ValidationError> { validationError },
                equipment: equipment,
                fieldName: fieldName);
        }

        /// <summary>
        /// Creates validation exception for required field missing
        /// </summary>
        public static EquipmentValidationException RequiredFieldMissing(
            string fieldName,
            BaseEquipmentData? equipment = null)
        {
            return ForField(
                fieldName: fieldName,
                message: $"Required field '{fieldName}' is missing or empty",
                userMessage: $"The {fieldName.Replace("_", " ")} field is required and cannot be empty.",
                equipment: equipment);
        }

        /// <summary>
        /// Creates validation exception for invalid field format
        /// </summary>
        public static EquipmentValidationException InvalidFormat(
            string fieldName,
            string expectedFormat,
            object? actualValue = null,
            BaseEquipmentData? equipment = null)
        {
            return ForField(
                fieldName: fieldName,
                message: $"Field '{fieldName}' has invalid format. Expected: {expectedFormat}",
                userMessage: $"The {fieldName.Replace("_", " ")} field format is incorrect. Expected format: {expectedFormat}",
                equipment: equipment,
                fieldValue: actualValue);
        }

        /// <summary>
        /// Creates validation exception for duplicate values
        /// </summary>
        public static EquipmentValidationException DuplicateValue(
            string fieldName,
            object? value,
            BaseEquipmentData? equipment = null)
        {
            return ForField(
                fieldName: fieldName,
                message: $"Duplicate value '{value}' found for field '{fieldName}'",
                userMessage: $"The {fieldName.Replace("_", " ")} '{value}' is already in use. Please choose a different value.",
                equipment: equipment,
                fieldValue: value);
        }

        /// <summary>
        /// Creates validation exception for business rule violations
        /// </summary>
        public static EquipmentValidationException BusinessRuleViolation(
            string ruleName,
            string message,
            string userMessage,
            BaseEquipmentData? equipment = null)
        {
            var exception = new EquipmentValidationException(
                message: $"Business rule violation: {ruleName} - {message}",
                userMessage: userMessage,
                equipment: equipment);
            
            exception.AddContext("BusinessRule", ruleName);
            return exception;
        }

        private static Dictionary<string, object> CreateContext(
            BaseEquipmentData? equipment,
            string? fieldName,
            List<ValidationError>? validationErrors)
        {
            var context = new Dictionary<string, object>();
            
            if (equipment != null)
            {
                context["EquipmentType"] = equipment.GetType().Name;
                context["EquipmentId"] = equipment.EntryId;
                context["PCName"] = equipment.PC_Name ?? "N/A";
                context["SerialNumber"] = equipment.Serial_No ?? "N/A";
            }
            
            if (!string.IsNullOrEmpty(fieldName))
            {
                context["FieldName"] = fieldName;
            }
            
            if (validationErrors?.Count > 0)
            {
                context["ValidationErrorCount"] = validationErrors.Count;
                context["ValidationErrors"] = validationErrors;
            }
            
            return context;
        }
    }

    /// <summary>
    /// Represents a specific validation error
    /// </summary>
    public class ValidationError
    {
        public string FieldName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? AttemptedValue { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public Dictionary<string, object> Context { get; set; } = new();
    }
}
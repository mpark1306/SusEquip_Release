using SusEquip.Data.Models;
using System.Collections.Generic;
using SusEquip.Data.Commands.Equipment;

namespace SusEquip.Data.Commands
{
    /// <summary>
    /// Result of bulk import operations
    /// </summary>
    public class ImportResult
    {
        public List<EquipmentData> SuccessfulItems { get; set; } = new();
        public List<ImportFailure> FailedItems { get; set; } = new();
        
        public int TotalProcessed => SuccessfulItems.Count + FailedItems.Count;
        public double SuccessRate => TotalProcessed > 0 ? (double)SuccessfulItems.Count / TotalProcessed * 100 : 0;
        
        public bool HasFailures => FailedItems.Count > 0;
        public bool IsCompleteSuccess => FailedItems.Count == 0 && SuccessfulItems.Count > 0;
    }

    /// <summary>
    /// Details of a failed import item
    /// </summary>
    public class ImportFailure
    {
        public EquipmentData Equipment { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
        public System.Exception? Exception { get; set; }
        public int LineNumber { get; set; }
    }

    /// <summary>
    /// Result of equipment operations that return an ID
    /// </summary>
    public class EquipmentOperationResult
    {
        public int EquipmentId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public EquipmentData? Equipment { get; set; }
        public List<EquipmentChange> Changes { get; set; } = new();

        // Properties for bulk operations
        public List<object> FailedItems { get; set; } = new();
        public bool IsCompleteSuccess => Success && FailedItems.Count == 0;

        // Static factory methods
        public static EquipmentOperationResult CreateSuccess(string message, int equipmentId = 0, EquipmentData? equipment = null)
        {
            return new EquipmentOperationResult
            {
                Success = true,
                Message = message,
                EquipmentId = equipmentId,
                Equipment = equipment
            };
        }

        public static EquipmentOperationResult CreateFailure(string message)
        {
            return new EquipmentOperationResult
            {
                Success = false,
                Message = message,
                EquipmentId = 0
            };
        }
    }

    /// <summary>
    /// Result of delete operations
    /// </summary>
    public class DeleteResult
    {
        public bool Success { get; set; }
        public int DeletedCount { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Represents a change made to equipment data
    /// </summary>
    public class EquipmentChange
    {
        public string FieldName { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public System.DateTime ChangedAt { get; set; }
    }
}
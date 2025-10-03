using SusEquip.Data.Models;
using System;

namespace SusEquip.Data.Events.Equipment
{
    /// <summary>
    /// Event raised when equipment is successfully created
    /// </summary>
    public class EquipmentCreatedEvent : DomainEventBase
    {
        public EquipmentCreatedEvent(
            BaseEquipmentData equipment, 
            string triggeredBy, 
            string? correlationId = null,
            string? reason = null) 
            : base(triggeredBy, correlationId)
        {
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            EquipmentType = equipment.GetType().Name;
            EquipmentId = equipment.EntryId;
            InstNo = equipment.GetInstNoAsString();
            PCName = equipment.PC_Name ?? "Unknown";
            Reason = reason ?? "Equipment created through system";
        }

        /// <summary>
        /// The equipment that was created
        /// </summary>
        public BaseEquipmentData Equipment { get; }
        
        /// <summary>
        /// Type of equipment (EquipmentData, OLDEquipmentData, MachineData)
        /// </summary>
        public string EquipmentType { get; }
        
        /// <summary>
        /// Primary key of the created equipment
        /// </summary>
        public int EquipmentId { get; }
        
        /// <summary>
        /// Instance number of the equipment
        /// </summary>
        public string InstNo { get; }
        
        /// <summary>
        /// PC name of the equipment
        /// </summary>
        public string PCName { get; }
        
        /// <summary>
        /// Reason for creation
        /// </summary>
        public string Reason { get; }
    }

    /// <summary>
    /// Event raised when equipment status is updated
    /// </summary>
    public class EquipmentUpdatedEvent : DomainEventBase
    {
        public EquipmentUpdatedEvent(
            BaseEquipmentData equipment,
            string? previousStatus,
            string? newStatus,
            string triggeredBy,
            string? correlationId = null,
            string? reason = null) 
            : base(triggeredBy, correlationId)
        {
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            EquipmentType = equipment.GetType().Name;
            EquipmentId = equipment.EntryId;
            InstNo = equipment.GetInstNoAsString();
            PCName = equipment.PC_Name ?? "Unknown";
            PreviousStatus = previousStatus ?? "Unknown";
            NewStatus = newStatus ?? "Unknown";
            Reason = reason ?? "Equipment status updated";
        }

        /// <summary>
        /// The equipment that was updated
        /// </summary>
        public BaseEquipmentData Equipment { get; }
        
        /// <summary>
        /// Type of equipment
        /// </summary>
        public string EquipmentType { get; }
        
        /// <summary>
        /// Primary key of the updated equipment
        /// </summary>
        public int EquipmentId { get; }
        
        /// <summary>
        /// Instance number of the equipment
        /// </summary>
        public string InstNo { get; }
        
        /// <summary>
        /// PC name of the equipment
        /// </summary>
        public string PCName { get; }
        
        /// <summary>
        /// Previous status before update
        /// </summary>
        public string PreviousStatus { get; }
        
        /// <summary>
        /// New status after update
        /// </summary>
        public string NewStatus { get; }
        
        /// <summary>
        /// Reason for update
        /// </summary>
        public string Reason { get; }
    }

    /// <summary>
    /// Event raised when equipment is deleted
    /// </summary>
    public class EquipmentDeletedEvent : DomainEventBase
    {
        public EquipmentDeletedEvent(
            int equipmentId,
            string instNo,
            string pcName,
            string equipmentType,
            bool isHardDelete,
            string triggeredBy,
            string? correlationId = null,
            string? reason = null) 
            : base(triggeredBy, correlationId)
        {
            EquipmentId = equipmentId;
            InstNo = instNo ?? "Unknown";
            PCName = pcName ?? "Unknown";
            EquipmentType = equipmentType ?? "Unknown";
            IsHardDelete = isHardDelete;
            Reason = reason ?? "Equipment deleted";
        }

        /// <summary>
        /// Primary key of the deleted equipment
        /// </summary>
        public int EquipmentId { get; }
        
        /// <summary>
        /// Instance number of the equipment
        /// </summary>
        public string InstNo { get; }
        
        /// <summary>
        /// PC name of the equipment
        /// </summary>
        public string PCName { get; }
        
        /// <summary>
        /// Type of equipment that was deleted
        /// </summary>
        public string EquipmentType { get; }
        
        /// <summary>
        /// Whether this was a hard delete (permanent) or soft delete
        /// </summary>
        public bool IsHardDelete { get; }
        
        /// <summary>
        /// Reason for deletion
        /// </summary>
        public string Reason { get; }
    }

    /// <summary>
    /// Event raised when equipment validation fails
    /// </summary>
    public class EquipmentValidationFailedEvent : DomainEventBase
    {
        public EquipmentValidationFailedEvent(
            BaseEquipmentData equipment,
            string[] validationErrors,
            string triggeredBy,
            string? correlationId = null) 
            : base(triggeredBy, correlationId)
        {
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            ValidationErrors = validationErrors ?? throw new ArgumentNullException(nameof(validationErrors));
            EquipmentType = equipment.GetType().Name;
            EquipmentId = equipment.EntryId;
            InstNo = equipment.GetInstNoAsString();
            PCName = equipment.PC_Name ?? "Unknown";
        }

        /// <summary>
        /// The equipment that failed validation
        /// </summary>
        public BaseEquipmentData Equipment { get; }
        
        /// <summary>
        /// List of validation error messages
        /// </summary>
        public string[] ValidationErrors { get; }
        
        /// <summary>
        /// Type of equipment
        /// </summary>
        public string EquipmentType { get; }
        
        /// <summary>
        /// Primary key of the equipment
        /// </summary>
        public int EquipmentId { get; }
        
        /// <summary>
        /// Instance number of the equipment
        /// </summary>
        public string InstNo { get; }
        
        /// <summary>
        /// PC name of the equipment
        /// </summary>
        public string PCName { get; }
    }

    /// <summary>
    /// Event raised when bulk equipment operations complete
    /// </summary>
    public class BulkEquipmentOperationCompletedEvent : DomainEventBase
    {
        public BulkEquipmentOperationCompletedEvent(
            string operationType,
            int totalItems,
            int successfulItems,
            int failedItems,
            string triggeredBy,
            string? correlationId = null,
            string[]? errors = null) 
            : base(triggeredBy, correlationId)
        {
            OperationType = operationType ?? throw new ArgumentNullException(nameof(operationType));
            TotalItems = totalItems;
            SuccessfulItems = successfulItems;
            FailedItems = failedItems;
            Errors = errors ?? Array.Empty<string>();
        }

        /// <summary>
        /// Type of bulk operation (Import, Export, Update, Delete)
        /// </summary>
        public string OperationType { get; }
        
        /// <summary>
        /// Total number of items processed
        /// </summary>
        public int TotalItems { get; }
        
        /// <summary>
        /// Number of successfully processed items
        /// </summary>
        public int SuccessfulItems { get; }
        
        /// <summary>
        /// Number of failed items
        /// </summary>
        public int FailedItems { get; }
        
        /// <summary>
        /// List of errors encountered during bulk operation
        /// </summary>
        public string[] Errors { get; }
    }
}
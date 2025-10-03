using SusEquip.Data.Models;
using System;
using System.Collections.Generic;

namespace SusEquip.Data.Builders
{
    /// <summary>
    /// Builder interface for constructing equipment with validation
    /// Provides fluent interface for step-by-step equipment creation
    /// </summary>
    public interface IEquipmentBuilder
    {
        IEquipmentBuilder SetBasicInfo(string pcName, string serialNumber);
        IEquipmentBuilder SetOwnership(string appOwner, string creatorInitials);
        IEquipmentBuilder SetStatus(string status);
        IEquipmentBuilder SetType(string type);
        IEquipmentBuilder SetDates(DateTime? entryDate = null, DateTime? warrantyDate = null);
        IEquipmentBuilder SetLocation(string location);
        IEquipmentBuilder SetSpecs(string? processor = null, string? memory = null, string? storage = null);
        IEquipmentBuilder AddValidation(Func<BaseEquipmentData, bool> validator, string errorMessage);
        IEquipmentBuilder Reset();
        
        BaseEquipmentData Build();
        T Build<T>() where T : BaseEquipmentData, new();
        
        /// <summary>
        /// Validates the current state without building
        /// </summary>
        (bool IsValid, List<string> Errors) Validate();
    }
}
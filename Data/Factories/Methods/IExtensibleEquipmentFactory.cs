using SusEquip.Data.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace SusEquip.Data.Factories.Methods
{
    /// <summary>
    /// Registration system for equipment type factories
    /// Supports plugin architecture for custom equipment creation
    /// </summary>
    public interface IEquipmentTypeRegistry
    {
        void RegisterFactory(string typeName, Func<BaseEquipmentData> factory);
        void RegisterFactory<T>(string typeName, Func<T> factory) where T : BaseEquipmentData;
        void UnregisterFactory(string typeName);
        bool IsRegistered(string typeName);
        IEnumerable<string> GetRegisteredTypes();
        BaseEquipmentData CreateEquipment(string typeName);
        T CreateEquipment<T>(string typeName) where T : BaseEquipmentData;
    }

    /// <summary>
    /// Factory method interface for extensible equipment creation
    /// Supports dynamic registration of new equipment types
    /// </summary>
    public interface IExtensibleEquipmentFactory
    {
        BaseEquipmentData CreateEquipment(string typeName, Dictionary<string, object>? parameters = null);
        T CreateEquipment<T>(Dictionary<string, object>? parameters = null) where T : BaseEquipmentData, new();
        bool SupportsType(string typeName);
        IEnumerable<string> GetSupportedTypes();
    }
}
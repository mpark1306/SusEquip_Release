using SusEquip.Data.Models;

namespace SusEquip.Data.Interfaces.Services
{
    /// <summary>
    /// Interface for equipment service operations
    /// </summary>
    public interface IEquipmentService
    {
        // Equipment management operations
        Task AddEntryAsync(EquipmentData equipmentData);
        Task InsertEntryAsync(EquipmentData equipmentData);
        Task UpdateLatestEntryAsync(EquipmentData equipmentData);
        Task DeleteEntryAsync(int instNo, int entryId);
        Task DeleteEquipmentAsync(int instNo); // New method for command pattern
        Task UpdateEquipmentStatusAsync(int instNo, string newStatus); // New method for command pattern

        // Equipment retrieval operations
        Task<List<EquipmentData>> GetEquipmentAsync();
        Task<List<EquipmentData>> GetEquipmentSortedAsync(int instNo);
        Task<List<EquipmentData>> GetEquipmentSortedByEntryAsync(int instNo);
        Task<EquipmentData?> GetByInstNoAsync(int instNo); // New method for command pattern

        // Machine operations
        Task<List<MachineData>> GetMachinesAsync();
        Task<List<MachineData>> GetActiveMachinesAsync();
        Task<List<MachineData>> GetNewMachinesAsync();
        Task<List<MachineData>> GetUsedMachinesAsync();
        Task<List<MachineData>> GetQuarantineMachinesAsync();
        Task<List<MachineData>> GetMachinesOutOfServiceSinceJuneAsync();

        // Utility operations
        Task<int> GetNextInstNoAsync();
        Task<bool> IsInstNoTakenAsync(int instNo);
        Task<bool> IsSerialNoTakenInMachinesAsync(string serialNo);

        // Dashboard and statistics
        Task<(int activeCount, int newCount, int usedCount, int quarantinedCount)> GetDashboardStatisticsAsync();
    }
}
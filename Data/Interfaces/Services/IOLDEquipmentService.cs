using SusEquip.Data.Models;

namespace SusEquip.Data.Interfaces.Services
{
    /// <summary>
    /// Interface for OLD equipment service operations
    /// </summary>
    public interface IOLDEquipmentService
    {
        // Equipment management operations
        Task AddEntryAsync(OLDEquipmentData equipmentData);
        Task UpdateEntryAsync(OLDEquipmentData equipmentData);
        Task DeleteEntryAsync(string instNo);

        // Equipment retrieval operations
        Task<List<OLDEquipmentData>> GetOLDEquipmentAsync();
        Task<List<OLDEquipmentData>> GetOLDMachinesAsync();
        Task<OLDEquipmentData?> GetOLDEquipmentByInstNoAsync(string instNo);

        // Utility operations
        Task<bool> IsOLDMachineAsync(string pcName, string department);
        Task<bool> IsInstNoTakenAsync(string instNo);
        Task<bool> IsSerialNoTakenAsync(string serialNo);

        // Statistics
        Task<int> GetTotalCountAsync();
        Task<int> GetActiveCountAsync();
    }
}
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;

namespace SusEquip.Data.Services
{
    /// <summary>
    /// Async adapter for the existing OLDEquipmentService to make it compatible with IOLDEquipmentService interface
    /// </summary>
    public class OLDEquipmentServiceAsyncAdapter : IOLDEquipmentService
    {
        private readonly OLDEquipmentService _oldEquipmentService;

        public OLDEquipmentServiceAsyncAdapter(OLDEquipmentService oldEquipmentService)
        {
            _oldEquipmentService = oldEquipmentService ?? throw new ArgumentNullException(nameof(oldEquipmentService));
        }

        // Equipment management operations
        public async Task AddEntryAsync(OLDEquipmentData equipmentData)
        {
            await Task.Run(() => _oldEquipmentService.AddEntry(equipmentData));
        }

        public async Task UpdateEntryAsync(OLDEquipmentData equipmentData)
        {
            await Task.Run(() => 
            {
                // The existing service doesn't have an update method, so we'll add as new entry
                // In a real implementation, you'd add an Update method to the original service
                _oldEquipmentService.AddEntry(equipmentData);
            });
        }

        public async Task DeleteEntryAsync(string instNo)
        {
            await Task.Run(() => 
            {
                // The existing service doesn't have a delete method
                // In a real implementation, you'd add a Delete method to the original service
                // For now, this is a placeholder
                throw new NotImplementedException("Delete functionality not implemented in underlying service");
            });
        }

        // Equipment retrieval operations
        public async Task<List<OLDEquipmentData>> GetOLDEquipmentAsync()
        {
            return await Task.Run(() => _oldEquipmentService.GetOLDEquipment());
        }

        public async Task<List<OLDEquipmentData>> GetOLDMachinesAsync()
        {
            return await Task.Run(() => _oldEquipmentService.GetOLDMachines());
        }

        public async Task<OLDEquipmentData?> GetOLDEquipmentByInstNoAsync(string instNo)
        {
            var allEquipment = await GetOLDEquipmentAsync();
            return allEquipment.FirstOrDefault(e => string.Equals(e.Inst_No, instNo, StringComparison.OrdinalIgnoreCase));
        }

        // Utility operations
        public async Task<bool> IsOLDMachineAsync(string pcName, string department)
        {
            return await Task.Run(() => OLDEquipmentService.IsOLDMachine(pcName, department));
        }

        public async Task<bool> IsInstNoTakenAsync(string instNo)
        {
            return await Task.Run(() => _oldEquipmentService.IsOLDInstNoTaken(instNo));
        }

        public async Task<bool> IsSerialNoTakenAsync(string serialNo)
        {
            var allEquipment = await GetOLDEquipmentAsync();
            return allEquipment.Any(e => string.Equals(e.Serial_No, serialNo, StringComparison.OrdinalIgnoreCase));
        }

        // Statistics
        public async Task<int> GetTotalCountAsync()
        {
            var allEquipment = await GetOLDEquipmentAsync();
            return allEquipment.Count;
        }

        public async Task<int> GetActiveCountAsync()
        {
            var allEquipment = await GetOLDEquipmentAsync();
            return allEquipment.Count(e => !string.Equals(e.Status, "Inactive", StringComparison.OrdinalIgnoreCase) &&
                                          !string.Equals(e.Status, "Retired", StringComparison.OrdinalIgnoreCase) &&
                                          !string.Equals(e.Status, "Disposed", StringComparison.OrdinalIgnoreCase));
        }
    }
}
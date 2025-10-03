using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;
using System.Linq;

namespace SusEquip.Data.Services
{
    /// <summary>
    /// Adapter to make any IEquipmentServiceSync implementation work with the async IEquipmentService interface
    /// This allows decorated sync services to be used where async interface is expected
    /// </summary>
    public class EquipmentServiceSyncToAsyncAdapter : IEquipmentService
    {
        private readonly IEquipmentServiceSync _syncService;

        public EquipmentServiceSyncToAsyncAdapter(IEquipmentServiceSync syncService)
        {
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        }

        public Task AddEntryAsync(EquipmentData equipmentData)
        {
            return Task.Run(() => _syncService.AddEntry(equipmentData));
        }

        public Task InsertEntryAsync(EquipmentData equipmentData)
        {
            return Task.Run(() => _syncService.InsertEntry(equipmentData));
        }

        public Task UpdateLatestEntryAsync(EquipmentData equipmentData)
        {
            return Task.Run(() => _syncService.UpdateLatestEntry(equipmentData));
        }

        public Task DeleteEntryAsync(int instNo, int entryId)
        {
            return Task.Run(() => _syncService.DeleteEntry(instNo, entryId));
        }

        public Task<List<EquipmentData>> GetEquipmentAsync()
        {
            return Task.Run(() => _syncService.GetEquipment());
        }

        public Task<List<EquipmentData>> GetEquipmentSortedAsync(int instNo)
        {
            return Task.Run(() => _syncService.GetEquipmentSorted(instNo));
        }

        public Task<List<EquipmentData>> GetEquipmentSortedByEntryAsync(int instNo)
        {
            return Task.Run(() => _syncService.GetEquipSortedByEntry(instNo));
        }

        public Task<List<MachineData>> GetMachinesAsync()
        {
            return Task.Run(() => _syncService.GetMachines());
        }

        public Task<List<MachineData>> GetNewMachinesAsync()
        {
            return Task.Run(() => _syncService.GetNewMachines());
        }

        public Task<List<MachineData>> GetUsedMachinesAsync()
        {
            return Task.Run(() => _syncService.GetUsedMachines());
        }

        public Task<List<MachineData>> GetQuarantineMachinesAsync()
        {
            return Task.Run(() => _syncService.GetQuarantineMachines());
        }

        public Task<List<MachineData>> GetActiveMachinesAsync()
        {
            return Task.Run(() => _syncService.GetActiveMachines());
        }

        public Task<List<MachineData>> GetMachinesOutOfServiceSinceJuneAsync()
        {
            return Task.Run(() => _syncService.GetMachinesOutOfServiceSinceJune());
        }

        public Task<int> GetNextInstNoAsync()
        {
            return Task.Run(() => _syncService.GetNextInstNo());
        }

        public Task<bool> IsInstNoTakenAsync(int instNo)
        {
            return Task.Run(() => _syncService.IsInstNoTaken(instNo));
        }

        public Task<bool> IsSerialNoTakenInMachinesAsync(string serialNo)
        {
            return Task.Run(() => _syncService.IsSerialNoTakenInMachines(serialNo));
        }

        public Task<(int activeCount, int newCount, int usedCount, int quarantinedCount)> GetDashboardStatisticsAsync()
        {
            return Task.Run(() => _syncService.GetDashboardStatistics());
        }

        // Command pattern support methods
        public Task DeleteEquipmentAsync(int instNo)
        {
            return Task.Run(() => 
            {
                // For now, use the existing DeleteEntry method with a dummy entry ID
                // This is a workaround until we have proper equipment deletion
                _syncService.DeleteEntry(instNo, 0);
            });
        }

        public Task UpdateEquipmentStatusAsync(int instNo, string newStatus)
        {
            return Task.Run(() => 
            {
                // Get current equipment
                var equipment = _syncService.GetEquipmentSorted(instNo).FirstOrDefault();
                if (equipment != null)
                {
                    equipment.Status = newStatus;
                    equipment.Entry_Date = DateTime.Now.ToString("yyyy-MM-dd");
                    _syncService.UpdateLatestEntry(equipment);
                }
            });
        }

        public Task<EquipmentData?> GetByInstNoAsync(int instNo)
        {
            return Task.Run(() => 
            {
                var equipmentList = _syncService.GetEquipmentSorted(instNo);
                return equipmentList.FirstOrDefault();
            });
        }
    }
}
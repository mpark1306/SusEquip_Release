using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Models;
using System.Linq;

namespace SusEquip.Data.Services
{
    /// <summary>
    /// Adapter to make the existing synchronous EquipmentService work with the async IEquipmentService interface
    /// This allows us to implement Phase 3 decorators while maintaining compatibility with existing code
    /// </summary>
    public class EquipmentServiceAsyncAdapter : IEquipmentService
    {
        private readonly EquipmentService _equipmentService;

        public EquipmentServiceAsyncAdapter(EquipmentService equipmentService)
        {
            _equipmentService = equipmentService ?? throw new ArgumentNullException(nameof(equipmentService));
        }

        public Task AddEntryAsync(EquipmentData equipmentData)
        {
            return Task.Run(() => _equipmentService.AddEntry(equipmentData));
        }

        public Task InsertEntryAsync(EquipmentData equipmentData)
        {
            return Task.Run(() => _equipmentService.InsertEntry(equipmentData));
        }

        public Task UpdateLatestEntryAsync(EquipmentData equipmentData)
        {
            return Task.Run(() => _equipmentService.UpdateLatestEntry(equipmentData));
        }

        public Task DeleteEntryAsync(int instNo, int entryId)
        {
            return Task.Run(() => _equipmentService.DeleteEntry(instNo, entryId));
        }

        public Task<List<EquipmentData>> GetEquipmentAsync()
        {
            return Task.Run(() => _equipmentService.GetEquipment());
        }

        public Task<List<EquipmentData>> GetEquipmentSortedAsync(int instNo)
        {
            return Task.Run(() => _equipmentService.GetEquipmentSorted(instNo));
        }

        public Task<List<EquipmentData>> GetEquipmentSortedByEntryAsync(int instNo)
        {
            return Task.Run(() => _equipmentService.GetEquipSortedByEntry(instNo));
        }

        public Task<List<MachineData>> GetMachinesAsync()
        {
            return Task.Run(() => _equipmentService.GetMachines());
        }

        public Task<List<MachineData>> GetActiveMachinesAsync()
        {
            return Task.Run(() => _equipmentService.GetActiveMachines());
        }

        public Task<List<MachineData>> GetNewMachinesAsync()
        {
            return Task.Run(() => _equipmentService.GetNewMachines());
        }

        public Task<List<MachineData>> GetUsedMachinesAsync()
        {
            return Task.Run(() => _equipmentService.GetUsedMachines());
        }

        public Task<List<MachineData>> GetQuarantineMachinesAsync()
        {
            return Task.Run(() => _equipmentService.GetQuarantineMachines());
        }

        public Task<List<MachineData>> GetMachinesOutOfServiceSinceJuneAsync()
        {
            return Task.Run(() => _equipmentService.GetMachinesOutOfServiceSinceJune());
        }

        public Task<int> GetNextInstNoAsync()
        {
            return Task.Run(() => _equipmentService.GetNextInstNo());
        }

        public Task<bool> IsInstNoTakenAsync(int instNo)
        {
            return Task.Run(() => _equipmentService.IsInstNoTaken(instNo));
        }

        public Task<bool> IsSerialNoTakenInMachinesAsync(string serialNo)
        {
            return Task.Run(() => _equipmentService.IsSerialNoTakenInMachines(serialNo));
        }

        public Task<(int activeCount, int newCount, int usedCount, int quarantinedCount)> GetDashboardStatisticsAsync()
        {
            return Task.Run(() => _equipmentService.GetDashboardStatistics());
        }

        // Command pattern support methods
        public Task DeleteEquipmentAsync(int instNo)
        {
            return Task.Run(() => 
            {
                // For now, use the existing DeleteEntry method with a dummy entry ID
                // This is a workaround until we have proper equipment deletion
                _equipmentService.DeleteEntry(instNo, 0);
            });
        }

        public Task UpdateEquipmentStatusAsync(int instNo, string newStatus)
        {
            return Task.Run(() => 
            {
                // Get current equipment
                var equipment = _equipmentService.GetEquipmentSorted(instNo).FirstOrDefault();
                if (equipment != null)
                {
                    equipment.Status = newStatus;
                    equipment.Entry_Date = DateTime.Now.ToString("yyyy-MM-dd");
                    _equipmentService.UpdateLatestEntry(equipment);
                }
            });
        }

        public Task<EquipmentData?> GetByInstNoAsync(int instNo)
        {
            return Task.Run(() => 
            {
                var equipmentList = _equipmentService.GetEquipmentSorted(instNo);
                return equipmentList.FirstOrDefault();
            });
        }
    }
}
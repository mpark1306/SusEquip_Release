using SusEquip.Data.Models;
using System.Collections.Generic;

namespace SusEquip.Data.Services
{
    public interface IEquipmentServiceSync
    {
        void AddEntry(EquipmentData equipmentData);
        void InsertEntry(EquipmentData ed);
        void UpdateLatestEntry(EquipmentData ed);
        int GetNextInstNo();
        void DeleteEntry(int inst_no, int entry_id);
        List<EquipmentData> GetEquipmentSorted(int inst_no);
        List<EquipmentData> GetEquipSortedByEntry(int inst_no);
        List<EquipmentData> GetEquipment();
        List<MachineData> GetMachines();
        List<MachineData> GetNewMachines();
        List<MachineData> GetUsedMachines();
        List<MachineData> GetQuarantineMachines();
        List<MachineData> GetActiveMachines();
        bool IsInstNoTaken(int instNo);
        bool IsSerialNoTakenInMachines(string serialNo);
        (int activeCount, int newCount, int usedCount, int quarantinedCount) GetDashboardStatistics();
        List<MachineData> GetMachinesOutOfServiceSinceJune();
    }
}
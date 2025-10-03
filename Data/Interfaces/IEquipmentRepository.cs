using SusEquip.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SusEquip.Data.Interfaces
{
    /// <summary>
    /// Repository interface for Equipment data operations
    /// </summary>
    public interface IEquipmentRepository : IRepository<EquipmentData>
    {
        /// <summary>
        /// Gets equipment by status
        /// </summary>
        /// <param name="status">The status to filter by</param>
        /// <returns>Collection of equipment with the specified status</returns>
        Task<IEnumerable<EquipmentData>> GetByStatusAsync(string status);

        /// <summary>
        /// Gets equipment by department
        /// </summary>
        /// <param name="department">The department to filter by</param>
        /// <returns>Collection of equipment in the specified department</returns>
        Task<IEnumerable<EquipmentData>> GetByDepartmentAsync(string department);

        /// <summary>
        /// Gets equipment by serial number
        /// </summary>
        /// <param name="serialNumber">The serial number to search for</param>
        /// <returns>Equipment with the specified serial number, or null if not found</returns>
        Task<EquipmentData?> GetBySerialNumberAsync(string serialNumber);

        /// <summary>
        /// Gets equipment by instance number
        /// </summary>
        /// <param name="instNo">The instance number to search for</param>
        /// <returns>Collection of equipment with the specified instance number</returns>
        Task<IEnumerable<EquipmentData>> GetByInstNoAsync(int instNo);

        /// <summary>
        /// Gets the next available instance number
        /// </summary>
        /// <returns>The next instance number to use</returns>
        Task<int> GetNextInstNoAsync();

        /// <summary>
        /// Checks if an instance number is already taken
        /// </summary>
        /// <param name="instNo">The instance number to check</param>
        /// <returns>True if taken, false if available</returns>
        Task<bool> IsInstNoTakenAsync(int instNo);

        /// <summary>
        /// Checks if a serial number is already taken
        /// </summary>
        /// <param name="serialNo">The serial number to check</param>
        /// <returns>True if taken, false if available</returns>
        Task<bool> IsSerialNoTakenAsync(string serialNo);

        /// <summary>
        /// Gets active machines (equipment with active status and valid service dates)
        /// </summary>
        /// <returns>Collection of active machines</returns>
        Task<IEnumerable<MachineData>> GetActiveMachinesAsync();

        /// <summary>
        /// Gets new machines (equipment with "Ny" status)
        /// </summary>
        /// <returns>Collection of new machines</returns>
        Task<IEnumerable<MachineData>> GetNewMachinesAsync();

        /// <summary>
        /// Gets used machines (equipment with "Brugt" status)
        /// </summary>
        /// <returns>Collection of used machines</returns>
        Task<IEnumerable<MachineData>> GetUsedMachinesAsync();

        /// <summary>
        /// Gets quarantine machines (equipment with "Karantæne" status)
        /// </summary>
        /// <returns>Collection of quarantine machines</returns>
        Task<IEnumerable<MachineData>> GetQuarantineMachinesAsync();

        /// <summary>
        /// Deletes equipment by instance number and entry ID
        /// </summary>
        /// <param name="instNo">The instance number</param>
        /// <param name="entryId">The entry ID</param>
        Task DeleteByInstNoAndEntryIdAsync(int instNo, int entryId);

        /// <summary>
        /// Gets dashboard statistics for equipment counts by status
        /// </summary>
        /// <returns>Tuple containing counts of active, new, used, and quarantined equipment</returns>
        Task<(int activeCount, int newCount, int usedCount, int quarantinedCount)> GetDashboardStatisticsAsync();

        /// <summary>
        /// Gets equipment by machine type
        /// </summary>
        /// <param name="machineType">The machine type to filter by</param>
        /// <returns>Collection of equipment with the specified machine type</returns>
        Task<IEnumerable<EquipmentData>> GetByMachineTypeAsync(string machineType);
    }
}
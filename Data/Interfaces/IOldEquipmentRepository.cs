using SusEquip.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SusEquip.Data.Interfaces
{
    /// <summary>
    /// Repository interface for OLD Equipment data operations
    /// </summary>
    public interface IOldEquipmentRepository : IRepository<OLDEquipmentData>
    {
        /// <summary>
        /// Gets all OLD machines from the database
        /// </summary>
        /// <returns>Collection of OLD equipment data</returns>
        Task<IEnumerable<OLDEquipmentData>> GetOldMachinesAsync();

        /// <summary>
        /// Determines if a machine should be classified as an OLD machine based on PC_Name or Department
        /// </summary>
        /// <param name="pcName">PC Name</param>
        /// <param name="department">Department</param>
        /// <returns>True if machine should be treated as OLD machine</returns>
        Task<bool> IsOLDMachineAsync(string pcName, string department);

        /// <summary>
        /// Gets OLD equipment by PC name
        /// </summary>
        /// <param name="pcName">The PC name to search for</param>
        /// <returns>Collection of OLD equipment with the specified PC name</returns>
        Task<IEnumerable<OLDEquipmentData>> GetByPCNameAsync(string pcName);

        /// <summary>
        /// Gets OLD equipment by serial number
        /// </summary>
        /// <param name="serialNumber">The serial number to search for</param>
        /// <returns>OLD equipment with the specified serial number, or null if not found</returns>
        Task<OLDEquipmentData?> GetBySerialNumberAsync(string serialNumber);

        /// <summary>
        /// Gets OLD equipment by department
        /// </summary>
        /// <param name="department">The department to filter by</param>
        /// <returns>Collection of OLD equipment in the specified department</returns>
        Task<IEnumerable<OLDEquipmentData>> GetByDepartmentAsync(string department);

        /// <summary>
        /// Gets OLD equipment by status
        /// </summary>
        /// <param name="status">The status to filter by</param>
        /// <returns>Collection of OLD equipment with the specified status</returns>
        Task<IEnumerable<OLDEquipmentData>> GetByStatusAsync(string status);

        /// <summary>
        /// Gets OLD equipment by instance number (string format)
        /// </summary>
        /// <param name="instNo">The instance number to search for</param>
        /// <returns>Collection of OLD equipment with the specified instance number</returns>
        Task<IEnumerable<OLDEquipmentData>> GetByInstNoAsync(string instNo);

        /// <summary>
        /// Checks if a serial number is already taken in OLD equipment
        /// </summary>
        /// <param name="serialNo">The serial number to check</param>
        /// <returns>True if taken, false if available</returns>
        Task<bool> IsSerialNoTakenAsync(string serialNo);

        /// <summary>
        /// Gets OLD equipment by machine type
        /// </summary>
        /// <param name="machineType">The machine type to filter by</param>
        /// <returns>Collection of OLD equipment with the specified machine type</returns>
        Task<IEnumerable<OLDEquipmentData>> GetByMachineTypeAsync(string machineType);
    }
}
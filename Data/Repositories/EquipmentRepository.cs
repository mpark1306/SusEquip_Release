using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SusEquip.Data.Interfaces;
using SusEquip.Data.Models;
using SusEquip.Data.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SusEquip.Data.Repositories
{
    /// <summary>
    /// Repository implementation for Equipment data operations
    /// </summary>
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly ILogger<EquipmentRepository> _logger;

        public EquipmentRepository(DatabaseHelper dbHelper, ILogger<EquipmentRepository> logger)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Base Repository Implementation

        public async Task<EquipmentData?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw EquipmentValidationException.ForField(
                    "id", 
                    "Equipment ID must be greater than zero", 
                    "Please provide a valid equipment ID.");
            }

            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM Equip WHERE Entry_Id = @id");

                command.Parameters.AddWithValue("@id", id);
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                return reader.Read() ? MapToEquipmentData(reader) : null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error retrieving equipment with ID {Id}", id);
                throw new DatabaseOperationException(ex.Message, "GetById", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment with ID {Id}", id);
                throw new DatabaseOperationException(ex.Message, "GetById", "Equip", ex);
            }
        }

        public async Task<IEnumerable<EquipmentData>> GetAllAsync()
        {
            try
            {
                var equipment = new List<EquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM Equip ORDER BY Entry_Id");

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    equipment.Add(MapToEquipmentData(reader));
                }

                return equipment;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error retrieving all equipment");
                throw new DatabaseOperationException(ex.Message, "GetAll", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all equipment");
                throw new DatabaseOperationException(ex.Message, "GetAll", "Equip", ex);
            }
        }

        public async Task AddAsync(EquipmentData equipment)
        {
            if (equipment == null)
            {
                throw EquipmentValidationException.RequiredFieldMissing("equipment");
            }

            // Check for duplicate serial number before adding
            if (!string.IsNullOrWhiteSpace(equipment.Serial_No))
            {
                var existingEquipment = await GetBySerialNumberAsync(equipment.Serial_No);
                if (existingEquipment != null)
                {
                    throw DuplicateSerialNumberException.ForCreation(
                        equipment.Serial_No, 
                        equipment, 
                        existingEquipment);
                }
            }

            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"INSERT INTO Equip (Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, 
                                        MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, 
                                        Service_Start, Service_Ends, Note, MachineType) 
                      VALUES (@entryDate, @instNo, @creator, @appOwner, @status, @serialNo, @mac1, @mac2, 
                              @uuid, @productNo, @modelName, @department, @pcName, @serviceStart, @serviceEnd, @note, @machineType);
                      SELECT SCOPE_IDENTITY();");

                AddParameters(command, equipment);

                await connection.OpenAsync();
                var newId = await command.ExecuteScalarAsync();
                equipment.EntryId = Convert.ToInt32(newId);

                _logger.LogInformation("Successfully added equipment {PCName} with Entry_Id {EntryId}", 
                    equipment.PC_Name, equipment.EntryId);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // Unique constraint violation
            {
                _logger.LogError(ex, "Duplicate constraint violation adding equipment {PCName}", equipment.PC_Name);
                throw DuplicateSerialNumberException.ForCreation(equipment.Serial_No ?? "Unknown", equipment);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error adding equipment {PCName}", equipment.PC_Name);
                throw new DatabaseOperationException(ex.Message, "Add", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding equipment entry for {PCName}", equipment.PC_Name);
                throw new DatabaseOperationException(ex.Message, "Add", "Equip", ex);
            }
        }

        public async Task UpdateAsync(EquipmentData equipment)
        {
            if (equipment == null) throw new EquipmentValidationException("Equipment data cannot be null", nameof(equipment), null);

            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"UPDATE Equip SET 
                        Entry_Date = @entryDate, 
                        inst_no = @instNo, 
                        creator_initials = @creator, 
                        app_owner = @appOwner, 
                        status = @status, 
                        serial_no = @serialNo, 
                        MAC_Address1 = @mac1, 
                        MAC_Address2 = @mac2, 
                        UUID = @uuid, 
                        Product_no = @productNo, 
                        model_name_and_no = @modelName, 
                        Department = @department, 
                        PC_Name = @pcName, 
                        Service_Start = @serviceStart, 
                        Service_Ends = @serviceEnd, 
                        Note = @note, 
                        MachineType = @machineType 
                      WHERE Entry_Id = @entryId");

                AddParameters(command, equipment);
                command.Parameters.AddWithValue("@entryId", equipment.EntryId);

                await connection.OpenAsync();
                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    throw new EquipmentNotFoundException($"Equipment with Entry_Id {equipment.EntryId} not found for update", equipment.EntryId.ToString());
                }

                _logger.LogInformation("Successfully updated equipment {PCName} with Entry_Id {EntryId}", 
                    equipment.PC_Name, equipment.EntryId);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error updating equipment entry for {PCName}", equipment.PC_Name);
                throw new DatabaseOperationException(ex.Message, "Update", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating equipment entry for {PCName}", equipment.PC_Name);
                throw new DatabaseOperationException(ex.Message, "Update", "Equip", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    "DELETE FROM Equip WHERE Entry_Id = @id");

                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    _logger.LogWarning("No equipment found with Entry_Id {Id} for deletion", id);
                }
                else
                {
                    _logger.LogInformation("Successfully deleted equipment with Entry_Id {Id}", id);
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error deleting equipment with Entry_Id {Id}", id);
                throw new DatabaseOperationException(ex.Message, "Delete", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting equipment with Entry_Id {Id}", id);
                throw new DatabaseOperationException(ex.Message, "Delete", "Equip", ex);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    "SELECT COUNT(1) FROM Equip WHERE Entry_Id = @id");

                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count) > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error checking existence of equipment with Entry_Id {Id}", id);
                throw new DatabaseOperationException(ex.Message, "Exists", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of equipment with Entry_Id {Id}", id);
                throw new DatabaseOperationException(ex.Message, "Exists", "Equip", ex);
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    "SELECT COUNT(*) FROM Equip");

                await connection.OpenAsync();
                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error getting equipment count");
                throw new DatabaseOperationException(ex.Message, "Count", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment count");
                throw new DatabaseOperationException(ex.Message, "Count", "Equip", ex);
            }
        }

        #endregion

        #region Equipment-Specific Methods

        public async Task<IEnumerable<EquipmentData>> GetByStatusAsync(string status)
        {
            try
            {
                var equipment = new List<EquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM Equip WHERE status = @status ORDER BY Entry_Id");

                command.Parameters.AddWithValue("@status", status ?? string.Empty);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    equipment.Add(MapToEquipmentData(reader));
                }

                return equipment;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error retrieving equipment by status {Status}", status);
                throw new DatabaseOperationException(ex.Message, "GetByStatus", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment by status {Status}", status);
                throw new DatabaseOperationException(ex.Message, "GetByStatus", "Equip", ex);
            }
        }

        public async Task<IEnumerable<EquipmentData>> GetByDepartmentAsync(string department)
        {
            try
            {
                var equipment = new List<EquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM Equip WHERE Department = @department ORDER BY Entry_Id");

                command.Parameters.AddWithValue("@department", department ?? string.Empty);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    equipment.Add(MapToEquipmentData(reader));
                }

                return equipment;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error retrieving equipment by department {Department}", department);
                throw new DatabaseOperationException(ex.Message, "GetByDepartment", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment by department {Department}", department);
                throw new DatabaseOperationException(ex.Message, "GetByDepartment", "Equip", ex);
            }
        }

        public async Task<EquipmentData?> GetBySerialNumberAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw EquipmentValidationException.RequiredFieldMissing("serial_no");
            }

            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM Equip WHERE serial_no = @serialNumber");

                command.Parameters.AddWithValue("@serialNumber", serialNumber);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                return reader.Read() ? MapToEquipmentData(reader) : null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error retrieving equipment by serial number {SerialNumber}", serialNumber);
                throw new DatabaseOperationException(ex.Message, "GetBySerialNumber", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment by serial number {SerialNumber}", serialNumber);
                throw new DatabaseOperationException(ex.Message, "GetBySerialNumber", "Equip", ex);
            }
        }

        public async Task<IEnumerable<EquipmentData>> GetByInstNoAsync(int instNo)
        {
            try
            {
                var equipment = new List<EquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM Equip WHERE inst_no = @instNo ORDER BY Entry_Id");

                command.Parameters.AddWithValue("@instNo", instNo);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    equipment.Add(MapToEquipmentData(reader));
                }

                return equipment;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error retrieving equipment by instance number {InstNo}", instNo);
                throw new DatabaseOperationException(ex.Message, "GetByInstNo", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment by instance number {InstNo}", instNo);
                throw new DatabaseOperationException(ex.Message, "GetByInstNo", "Equip", ex);
            }
        }

        public async Task<int> GetNextInstNoAsync()
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection, 
                    "SELECT MAX(inst_no) FROM Equip");

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                return result != DBNull.Value ? Convert.ToInt32(result) + 1 : 1;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error getting next instance number");
                throw new DatabaseOperationException(ex.Message, "GetNextInstNo", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next instance number");
                throw new DatabaseOperationException(ex.Message, "GetNextInstNo", "Equip", ex);
            }
        }

        public async Task<bool> IsInstNoTakenAsync(int instNo)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    "SELECT COUNT(1) FROM Equip WHERE inst_no = @instNo");

                command.Parameters.AddWithValue("@instNo", instNo);

                await connection.OpenAsync();
                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count) > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error checking if instance number {InstNo} is taken", instNo);
                throw new DatabaseOperationException(ex.Message, "IsInstNoTaken", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if instance number {InstNo} is taken", instNo);
                throw new DatabaseOperationException(ex.Message, "IsInstNoTaken", "Equip", ex);
            }
        }

        public async Task<bool> IsSerialNoTakenAsync(string serialNo)
        {
            if (string.IsNullOrWhiteSpace(serialNo)) return false;

            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    "SELECT COUNT(1) FROM Equip WHERE serial_no = @serialNo");

                command.Parameters.AddWithValue("@serialNo", serialNo);

                await connection.OpenAsync();
                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count) > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error checking if serial number {SerialNo} is taken", serialNo);
                throw new DatabaseOperationException(ex.Message, "IsSerialNoTaken", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if serial number {SerialNo} is taken", serialNo);
                throw new DatabaseOperationException(ex.Message, "IsSerialNoTaken", "Equip", ex);
            }
        }

        public async Task<IEnumerable<MachineData>> GetActiveMachinesAsync()
        {
            try
            {
                var machines = new List<MachineData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection, @"
                    SELECT DISTINCT e.Entry_Id, e.Entry_Date, e.inst_no, e.creator_initials, e.app_owner, e.status, 
                           e.serial_no, e.MAC_Address1, e.MAC_Address2, e.UUID, e.Product_no, e.model_name_and_no, 
                           e.Department, e.PC_Name, e.Service_Start, e.Service_Ends, e.Note, e.MachineType
                    FROM Equip e
                    INNER JOIN (
                        SELECT inst_no, MAX(Entry_Id) AS MaxEntryId
                        FROM Equip
                        GROUP BY inst_no
                    ) latest ON e.inst_no = latest.inst_no AND e.Entry_Id = latest.MaxEntryId
                    WHERE e.status NOT IN ('Afhentet af Refurb', 'Kasseret', 'Karantæne', 'OLD') 
                    AND (e.Service_Ends IS NULL OR e.Service_Ends = '' OR 
                         TRY_CAST(e.Service_Ends AS DATE) IS NULL OR 
                         TRY_CAST(e.Service_Ends AS DATE) > GETDATE())");

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    machines.Add(MapToMachineData(reader));
                }

                return machines;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error retrieving active machines");
                throw new DatabaseOperationException(ex.Message, "GetActiveMachines", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active machines");
                throw new DatabaseOperationException(ex.Message, "GetActiveMachines", "Equip", ex);
            }
        }

        public async Task<IEnumerable<MachineData>> GetNewMachinesAsync()
        {
            try
            {
                var machines = new List<MachineData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection, @"
                    SELECT DISTINCT e.Entry_Id, e.Entry_Date, e.inst_no, e.creator_initials, e.app_owner, e.status, 
                           e.serial_no, e.MAC_Address1, e.MAC_Address2, e.UUID, e.Product_no, e.model_name_and_no, 
                           e.Department, e.PC_Name, e.Service_Start, e.Service_Ends, e.Note, e.MachineType
                    FROM Equip e
                    INNER JOIN (
                        SELECT inst_no, MAX(Entry_Id) AS MaxEntryId
                        FROM Equip
                        GROUP BY inst_no
                    ) latest ON e.inst_no = latest.inst_no AND e.Entry_Id = latest.MaxEntryId
                    WHERE e.status = 'Ny'");

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    machines.Add(MapToMachineData(reader));
                }

                return machines;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error retrieving new machines");
                throw new DatabaseOperationException(ex.Message, "GetNewMachines", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving new machines");
                throw new DatabaseOperationException(ex.Message, "GetNewMachines", "Equip", ex);
            }
        }

        public async Task<IEnumerable<MachineData>> GetUsedMachinesAsync()
        {
            try
            {
                var machines = new List<MachineData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection, @"
                    SELECT DISTINCT e.Entry_Id, e.Entry_Date, e.inst_no, e.creator_initials, e.app_owner, e.status, 
                           e.serial_no, e.MAC_Address1, e.MAC_Address2, e.UUID, e.Product_no, e.model_name_and_no, 
                           e.Department, e.PC_Name, e.Service_Start, e.Service_Ends, e.Note, e.MachineType
                    FROM Equip e
                    INNER JOIN (
                        SELECT inst_no, MAX(Entry_Id) AS MaxEntryId
                        FROM Equip
                        GROUP BY inst_no
                    ) latest ON e.inst_no = latest.inst_no AND e.Entry_Id = latest.MaxEntryId
                    WHERE e.status = 'Brugt'");

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    machines.Add(MapToMachineData(reader));
                }

                return machines;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error retrieving used machines");
                throw new DatabaseOperationException(ex.Message, "GetUsedMachines", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving used machines");
                throw new DatabaseOperationException(ex.Message, "GetUsedMachines", "Equip", ex);
            }
        }

        public async Task<IEnumerable<MachineData>> GetQuarantineMachinesAsync()
        {
            try
            {
                var machines = new List<MachineData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection, @"
                    SELECT DISTINCT e.Entry_Id, e.Entry_Date, e.inst_no, e.creator_initials, e.app_owner, e.status, 
                           e.serial_no, e.MAC_Address1, e.MAC_Address2, e.UUID, e.Product_no, e.model_name_and_no, 
                           e.Department, e.PC_Name, e.Service_Start, e.Service_Ends, e.Note, e.MachineType
                    FROM Equip e
                    INNER JOIN (
                        SELECT inst_no, MAX(Entry_Id) AS MaxEntryId
                        FROM Equip
                        GROUP BY inst_no
                    ) latest ON e.inst_no = latest.inst_no AND e.Entry_Id = latest.MaxEntryId
                    WHERE e.status = 'Karantæne'");

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    machines.Add(MapToMachineData(reader));
                }

                return machines;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error retrieving quarantine machines");
                throw new DatabaseOperationException(ex.Message, "GetQuarantineMachines", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quarantine machines");
                throw new DatabaseOperationException(ex.Message, "GetQuarantineMachines", "Equip", ex);
            }
        }

        public async Task DeleteByInstNoAndEntryIdAsync(int instNo, int entryId)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    "DELETE FROM Equip WHERE inst_no = @instNo AND Entry_Id = @entryId");

                command.Parameters.AddWithValue("@instNo", instNo);
                command.Parameters.AddWithValue("@entryId", entryId);

                await connection.OpenAsync();
                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    _logger.LogWarning("No equipment found with inst_no {InstNo} and Entry_Id {EntryId} for deletion", instNo, entryId);
                }
                else
                {
                    _logger.LogInformation("Successfully deleted equipment with inst_no {InstNo} and Entry_Id {EntryId}", instNo, entryId);
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error deleting equipment with inst_no {InstNo} and Entry_Id {EntryId}", instNo, entryId);
                throw new DatabaseOperationException(ex.Message, "DeleteByInstNoAndEntryId", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting equipment with inst_no {InstNo} and Entry_Id {EntryId}", instNo, entryId);
                throw new DatabaseOperationException(ex.Message, "DeleteByInstNoAndEntryId", "Equip", ex);
            }
        }

        public async Task<(int activeCount, int newCount, int usedCount, int quarantinedCount)> GetDashboardStatisticsAsync()
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection, @"
                    WITH LatestEquipment AS (
                        SELECT e.*, ROW_NUMBER() OVER (PARTITION BY e.inst_no ORDER BY e.Entry_Id DESC) as rn
                        FROM Equip e
                    )
                    SELECT 
                        SUM(CASE 
                            WHEN status NOT IN ('Afhentet af Refurb', 'Kasseret', 'Karantæne', 'OLD') 
                                AND (Service_Ends IS NULL OR Service_Ends = '' OR TRY_CAST(Service_Ends AS DATE) IS NULL OR TRY_CAST(Service_Ends AS DATE) > GETDATE())
                            THEN 1 ELSE 0 END) as ActiveCount,
                        SUM(CASE WHEN status = 'Ny' THEN 1 ELSE 0 END) as NewCount,
                        SUM(CASE WHEN status = 'Brugt' THEN 1 ELSE 0 END) as UsedCount,
                        SUM(CASE WHEN status = 'Karantæne' THEN 1 ELSE 0 END) as QuarantinedCount
                    FROM LatestEquipment 
                    WHERE rn = 1");

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (reader.Read())
                {
                    return (
                        activeCount: reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        newCount: reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                        usedCount: reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                        quarantinedCount: reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                    );
                }

                return (0, 0, 0, 0);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error getting dashboard statistics");
                throw new DatabaseOperationException(ex.Message, "GetDashboardStatistics", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                throw new DatabaseOperationException(ex.Message, "GetDashboardStatistics", "Equip", ex);
            }
        }

        public async Task<IEnumerable<EquipmentData>> GetByMachineTypeAsync(string machineType)
        {
            try
            {
                var equipment = new List<EquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM Equip WHERE MachineType = @machineType ORDER BY Entry_Id");

                command.Parameters.AddWithValue("@machineType", machineType ?? string.Empty);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    equipment.Add(MapToEquipmentData(reader));
                }

                return equipment;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving equipment by machine type {MachineType}", machineType);
                throw new DatabaseOperationException(ex.Message, "GetByMachineType", "Equip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment by machine type {MachineType}", machineType);
                throw new DatabaseOperationException(ex.Message, "GetByMachineType", "Equip", ex);
            }
        }

        #endregion

        #region Private Helper Methods

        private void AddParameters(SqlCommand command, EquipmentData equipment)
        {
            command.Parameters.AddWithValue("@entryDate", equipment.Entry_Date ?? string.Empty);
            command.Parameters.AddWithValue("@instNo", equipment.Inst_No);
            command.Parameters.AddWithValue("@creator", equipment.Creator_Initials ?? string.Empty);
            command.Parameters.AddWithValue("@appOwner", equipment.App_Owner ?? string.Empty);
            command.Parameters.AddWithValue("@status", equipment.Status ?? string.Empty);
            command.Parameters.AddWithValue("@serialNo", equipment.Serial_No ?? string.Empty);
            command.Parameters.AddWithValue("@mac1", equipment.Mac_Address1 ?? string.Empty);
            command.Parameters.AddWithValue("@mac2", equipment.Mac_Address2 ?? string.Empty);
            command.Parameters.AddWithValue("@uuid", equipment.UUID ?? string.Empty);
            command.Parameters.AddWithValue("@productNo", equipment.Product_No ?? string.Empty);
            command.Parameters.AddWithValue("@modelName", equipment.Model_Name_and_No ?? string.Empty);
            command.Parameters.AddWithValue("@department", equipment.Department ?? string.Empty);
            command.Parameters.AddWithValue("@pcName", equipment.PC_Name ?? string.Empty);
            command.Parameters.AddWithValue("@serviceStart", equipment.Service_Start ?? string.Empty);
            command.Parameters.AddWithValue("@serviceEnd", equipment.Service_Ends ?? string.Empty);
            command.Parameters.AddWithValue("@note", equipment.Note ?? string.Empty);
            command.Parameters.AddWithValue("@machineType", equipment.MachineType ?? string.Empty);
        }

        private EquipmentData MapToEquipmentData(SqlDataReader reader)
        {
            return new EquipmentData
            {
                EntryId = reader.IsDBNull("Entry_Id") ? 0 : reader.GetInt32("Entry_Id"),
                Entry_Date = reader.IsDBNull("Entry_Date") ? string.Empty : reader.GetString("Entry_Date"),
                Inst_No = reader.IsDBNull("inst_no") ? 0 : reader.GetInt32("inst_no"),
                Creator_Initials = reader.IsDBNull("creator_initials") ? string.Empty : reader.GetString("creator_initials"),
                App_Owner = reader.IsDBNull("app_owner") ? string.Empty : reader.GetString("app_owner"),
                Status = reader.IsDBNull("status") ? string.Empty : reader.GetString("status"),
                Serial_No = reader.IsDBNull("serial_no") ? string.Empty : reader.GetString("serial_no"),
                Mac_Address1 = reader.IsDBNull("MAC_Address1") ? string.Empty : reader.GetString("MAC_Address1"),
                Mac_Address2 = reader.IsDBNull("MAC_Address2") ? string.Empty : reader.GetString("MAC_Address2"),
                UUID = reader.IsDBNull("UUID") ? string.Empty : reader.GetString("UUID"),
                Product_No = reader.IsDBNull("Product_no") ? string.Empty : reader.GetString("Product_no"),
                Model_Name_and_No = reader.IsDBNull("model_name_and_no") ? string.Empty : reader.GetString("model_name_and_no"),
                Department = reader.IsDBNull("Department") ? string.Empty : reader.GetString("Department"),
                PC_Name = reader.IsDBNull("PC_Name") ? string.Empty : reader.GetString("PC_Name"),
                Service_Start = reader.IsDBNull("Service_Start") ? string.Empty : reader.GetString("Service_Start"),
                Service_Ends = reader.IsDBNull("Service_Ends") ? string.Empty : reader.GetString("Service_Ends"),
                Note = reader.IsDBNull("Note") ? string.Empty : reader.GetString("Note"),
                MachineType = reader.IsDBNull("MachineType") ? string.Empty : reader.GetString("MachineType")
            };
        }

        private MachineData MapToMachineData(SqlDataReader reader)
        {
            return new MachineData
            {
                EntryId = reader.IsDBNull("Entry_Id") ? 0 : reader.GetInt32("Entry_Id"),
                Entry_Date = reader.IsDBNull("Entry_Date") ? string.Empty : reader.GetString("Entry_Date"),
                Inst_No = reader.IsDBNull("inst_no") ? 0 : reader.GetInt32("inst_no"),
                Creator_Initials = reader.IsDBNull("creator_initials") ? string.Empty : reader.GetString("creator_initials"),
                App_Owner = reader.IsDBNull("app_owner") ? string.Empty : reader.GetString("app_owner"),
                Status = reader.IsDBNull("status") ? string.Empty : reader.GetString("status"),
                Serial_No = reader.IsDBNull("serial_no") ? string.Empty : reader.GetString("serial_no"),
                Mac_Address1 = reader.IsDBNull("MAC_Address1") ? string.Empty : reader.GetString("MAC_Address1"),
                Mac_Address2 = reader.IsDBNull("MAC_Address2") ? string.Empty : reader.GetString("MAC_Address2"),
                UUID = reader.IsDBNull("UUID") ? string.Empty : reader.GetString("UUID"),
                Product_No = reader.IsDBNull("Product_no") ? string.Empty : reader.GetString("Product_no"),
                Model_Name_and_No = reader.IsDBNull("model_name_and_no") ? string.Empty : reader.GetString("model_name_and_no"),
                Department = reader.IsDBNull("Department") ? string.Empty : reader.GetString("Department"),
                PC_Name = reader.IsDBNull("PC_Name") ? string.Empty : reader.GetString("PC_Name"),
                Service_Start = reader.IsDBNull("Service_Start") ? string.Empty : reader.GetString("Service_Start"),
                Service_Ends = reader.IsDBNull("Service_Ends") ? string.Empty : reader.GetString("Service_Ends"),
                Note = reader.IsDBNull("Note") ? string.Empty : reader.GetString("Note"),
                MachineType = reader.IsDBNull("MachineType") ? string.Empty : reader.GetString("MachineType")
            };
        }

        #endregion
    }
}
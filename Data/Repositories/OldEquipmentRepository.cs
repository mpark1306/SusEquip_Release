using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SusEquip.Data.Exceptions;
using SusEquip.Data.Interfaces;
using SusEquip.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SusEquip.Data.Repositories
{
    /// <summary>
    /// Repository implementation for OLD Equipment data operations
    /// </summary>
    public class OldEquipmentRepository : IOldEquipmentRepository
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly ILogger<OldEquipmentRepository> _logger;

        public OldEquipmentRepository(DatabaseHelper dbHelper, ILogger<OldEquipmentRepository> logger)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Base Repository Implementation

        public async Task<OLDEquipmentData?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, PC_Name, Inst_No, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM OLD_Equip WHERE Entry_Id = @id");

                command.Parameters.AddWithValue("@id", id);
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                return reader.Read() ? MapToOldEquipmentData(reader) : null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving OLD equipment with ID {Id}", id);
                throw new DatabaseOperationException(ex.Message, "GetById", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OLD equipment with ID {Id}", id);
                throw new DatabaseOperationException(ex.Message, "GetById", "OLDEquip", ex);
            }
        }

        public async Task<IEnumerable<OLDEquipmentData>> GetAllAsync()
        {
            try
            {
                var equipment = new List<OLDEquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, PC_Name, Inst_No, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM OLD_Equip ORDER BY Entry_Id");

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    equipment.Add(MapToOldEquipmentData(reader));
                }

                return equipment;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving all OLD equipment");
                throw new DatabaseOperationException(ex.Message, "GetAll", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all OLD equipment");
                throw new DatabaseOperationException(ex.Message, "GetAll", "OLDEquip", ex);
            }
        }

        public async Task AddAsync(OLDEquipmentData equipment)
        {
            if (equipment == null) throw new EquipmentValidationException("Equipment cannot be null", "equipment");

            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"INSERT INTO OLD_Equip (Entry_Date, PC_Name, Inst_No, creator_initials, app_owner, status, serial_no, 
                                           MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, 
                                           Service_Start, Service_Ends, Note, MachineType) 
                      VALUES (@entryDate, @pcName, @instNo, @creator, @appOwner, @status, @serialNo, @mac1, @mac2, 
                              @uuid, @productNo, @modelName, @department, @serviceStart, @serviceEnd, @note, @machineType);
                      SELECT SCOPE_IDENTITY();");

                AddParameters(command, equipment);

                await connection.OpenAsync();
                var newId = await command.ExecuteScalarAsync();
                equipment.EntryId = Convert.ToInt32(newId);

                _logger.LogInformation("Successfully added OLD equipment {PCName} with Entry_Id {EntryId}", 
                    equipment.PC_Name, equipment.EntryId);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error adding OLD equipment entry for {PCName}", equipment.PC_Name);
                throw new DatabaseOperationException(ex.Message, "Add", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding OLD equipment entry for {PCName}", equipment.PC_Name);
                throw new DatabaseOperationException(ex.Message, "Add", "OLDEquip", ex);
            }
        }

        public async Task UpdateAsync(OLDEquipmentData equipment)
        {
            if (equipment == null) throw new EquipmentValidationException("Equipment cannot be null", "equipment");

            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"UPDATE OLD_Equip SET 
                        Entry_Date = @entryDate, 
                        PC_Name = @pcName, 
                        Inst_No = @instNo, 
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
                    throw new EquipmentNotFoundException($"OLD Equipment with Entry_Id {equipment.EntryId} not found for update", equipment.EntryId.ToString());
                }

                _logger.LogInformation("Successfully updated OLD equipment {PCName} with Entry_Id {EntryId}", 
                    equipment.PC_Name, equipment.EntryId);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error updating OLD equipment entry for {PCName}", equipment.PC_Name);
                throw new DatabaseOperationException(ex.Message, "Update", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating OLD equipment entry for {PCName}", equipment.PC_Name);
                throw new DatabaseOperationException(ex.Message, "Update", "OLDEquip", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    "DELETE FROM OLD_Equip WHERE Entry_Id = @id");

                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    _logger.LogWarning("No OLD equipment found with Entry_Id {Id} for deletion", id);
                }
                else
                {
                    _logger.LogInformation("Successfully deleted OLD equipment with Entry_Id {Id}", id);
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error deleting OLD equipment with Entry_Id {Id}", id);
                throw new DatabaseOperationException(ex.Message, "Delete", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting OLD equipment with Entry_Id {Id}", id);
                throw new DatabaseOperationException(ex.Message, "Delete", "OLDEquip", ex);
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    "SELECT COUNT(1) FROM OLD_Equip WHERE Entry_Id = @id");

                command.Parameters.AddWithValue("@id", id);

                await connection.OpenAsync();
                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count) > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error checking existence of OLD equipment with Entry_Id {Id}", id);
                throw new DatabaseOperationException(ex.Message, "Exists", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of OLD equipment with Entry_Id {Id}", id);
                throw new DatabaseOperationException(ex.Message, "Exists", "OLDEquip", ex);
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    "SELECT COUNT(*) FROM OLD_Equip");

                await connection.OpenAsync();
                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error getting OLD equipment count");
                throw new DatabaseOperationException(ex.Message, "Count", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OLD equipment count");
                throw new DatabaseOperationException(ex.Message, "Count", "OLDEquip", ex);
            }
        }

        #endregion

        #region OLD Equipment-Specific Methods

        public async Task<IEnumerable<OLDEquipmentData>> GetOldMachinesAsync()
        {
            try
            {
                var machines = new List<OLDEquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, PC_Name, Inst_No, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM OLD_Equip ORDER BY Entry_Id");

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    machines.Add(MapToOldEquipmentData(reader));
                }

                return machines;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving OLD machines");
                throw new DatabaseOperationException(ex.Message, "GetOldMachines", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OLD machines");
                throw new DatabaseOperationException(ex.Message, "GetOldMachines", "OLDEquip", ex);
            }
        }

        public async Task<bool> IsOLDMachineAsync(string pcName, string department)
        {
            // This method implements the logic FROM OLD_EquipmentService.IsOLDMachine
            if (string.IsNullOrWhiteSpace(pcName) && string.IsNullOrWhiteSpace(department))
                return false;

            return await Task.FromResult(
                (pcName?.ToUpper().Contains("OLD") == true) || 
                (department?.ToUpper().Contains("OLD") == true)
            );
        }

        public async Task<IEnumerable<OLDEquipmentData>> GetByPCNameAsync(string pcName)
        {
            try
            {
                var equipment = new List<OLDEquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, PC_Name, Inst_No, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM OLD_Equip WHERE PC_Name = @pcName ORDER BY Entry_Id");

                command.Parameters.AddWithValue("@pcName", pcName ?? string.Empty);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    equipment.Add(MapToOldEquipmentData(reader));
                }

                return equipment;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving OLD equipment by PC name {PCName}", pcName);
                throw new DatabaseOperationException(ex.Message, "GetByPCName", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OLD equipment by PC name {PCName}", pcName);
                throw new DatabaseOperationException(ex.Message, "GetByPCName", "OLDEquip", ex);
            }
        }

        public async Task<OLDEquipmentData?> GetBySerialNumberAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber)) return null;

            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, PC_Name, Inst_No, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM OLD_Equip WHERE serial_no = @serialNumber");

                command.Parameters.AddWithValue("@serialNumber", serialNumber);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                return reader.Read() ? MapToOldEquipmentData(reader) : null;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving OLD equipment by serial number {SerialNumber}", serialNumber);
                throw new DatabaseOperationException(ex.Message, "GetBySerialNumber", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OLD equipment by serial number {SerialNumber}", serialNumber);
                throw new DatabaseOperationException(ex.Message, "GetBySerialNumber", "OLDEquip", ex);
            }
        }

        public async Task<IEnumerable<OLDEquipmentData>> GetByDepartmentAsync(string department)
        {
            try
            {
                var equipment = new List<OLDEquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, PC_Name, Inst_No, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM OLD_Equip WHERE Department = @department ORDER BY Entry_Id");

                command.Parameters.AddWithValue("@department", department ?? string.Empty);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    equipment.Add(MapToOldEquipmentData(reader));
                }

                return equipment;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving OLD equipment by department {Department}", department);
                throw new DatabaseOperationException(ex.Message, "GetByDepartment", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OLD equipment by department {Department}", department);
                throw new DatabaseOperationException(ex.Message, "GetByDepartment", "OLDEquip", ex);
            }
        }

        public async Task<IEnumerable<OLDEquipmentData>> GetByStatusAsync(string status)
        {
            try
            {
                var equipment = new List<OLDEquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, PC_Name, Inst_No, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM OLD_Equip WHERE status = @status ORDER BY Entry_Id");

                command.Parameters.AddWithValue("@status", status ?? string.Empty);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    equipment.Add(MapToOldEquipmentData(reader));
                }

                return equipment;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving OLD equipment by status {Status}", status);
                throw new DatabaseOperationException(ex.Message, "GetByStatus", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OLD equipment by status {Status}", status);
                throw new DatabaseOperationException(ex.Message, "GetByStatus", "OLDEquip", ex);
            }
        }

        public async Task<IEnumerable<OLDEquipmentData>> GetByInstNoAsync(string instNo)
        {
            try
            {
                var equipment = new List<OLDEquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, PC_Name, Inst_No, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM OLD_Equip WHERE Inst_No = @instNo ORDER BY Entry_Id");

                command.Parameters.AddWithValue("@instNo", instNo ?? string.Empty);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    equipment.Add(MapToOldEquipmentData(reader));
                }

                return equipment;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving OLD equipment by instance number {InstNo}", instNo);
                throw new DatabaseOperationException(ex.Message, "GetByInstNo", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OLD equipment by instance number {InstNo}", instNo);
                throw new DatabaseOperationException(ex.Message, "GetByInstNo", "OLDEquip", ex);
            }
        }

        public async Task<bool> IsSerialNoTakenAsync(string serialNo)
        {
            if (string.IsNullOrWhiteSpace(serialNo)) return false;

            try
            {
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    "SELECT COUNT(1) FROM OLD_Equip WHERE serial_no = @serialNo");

                command.Parameters.AddWithValue("@serialNo", serialNo);

                await connection.OpenAsync();
                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count) > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error checking if serial number {SerialNo} is taken in OLD equipment", serialNo);
                throw new DatabaseOperationException(ex.Message, "IsSerialNoTaken", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if serial number {SerialNo} is taken in OLD equipment", serialNo);
                throw new DatabaseOperationException(ex.Message, "IsSerialNoTaken", "OLDEquip", ex);
            }
        }

        public async Task<IEnumerable<OLDEquipmentData>> GetByMachineTypeAsync(string machineType)
        {
            try
            {
                var equipment = new List<OLDEquipmentData>();
                using var connection = _dbHelper.GetConnection();
                using var command = _dbHelper.CreateCommand(connection,
                    @"SELECT Entry_Id, Entry_Date, PC_Name, Inst_No, creator_initials, app_owner, status, serial_no, 
                             MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, 
                             Service_Start, Service_Ends, Note, MachineType 
                      FROM OLD_Equip WHERE MachineType = @machineType ORDER BY Entry_Id");

                command.Parameters.AddWithValue("@machineType", machineType ?? string.Empty);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (reader.Read())
                {
                    equipment.Add(MapToOldEquipmentData(reader));
                }

                return equipment;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error retrieving OLD equipment by machine type {MachineType}", machineType);
                throw new DatabaseOperationException(ex.Message, "GetByMachineType", "OLDEquip", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OLD equipment by machine type {MachineType}", machineType);
                throw new DatabaseOperationException(ex.Message, "GetByMachineType", "OLDEquip", ex);
            }
        }

        #endregion

        #region Private Helper Methods

        private void AddParameters(SqlCommand command, OLDEquipmentData equipment)
        {
            command.Parameters.AddWithValue("@entryDate", equipment.Entry_Date ?? string.Empty);
            command.Parameters.AddWithValue("@pcName", equipment.PC_Name ?? string.Empty);
            command.Parameters.AddWithValue("@instNo", equipment.Inst_No ?? string.Empty);
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
            command.Parameters.AddWithValue("@serviceStart", equipment.Service_Start ?? string.Empty);
            command.Parameters.AddWithValue("@serviceEnd", equipment.Service_Ends ?? string.Empty);
            command.Parameters.AddWithValue("@note", equipment.Note ?? string.Empty);
            command.Parameters.AddWithValue("@machineType", equipment.MachineType ?? string.Empty);
        }

        private OLDEquipmentData MapToOldEquipmentData(SqlDataReader reader)
        {
            return new OLDEquipmentData
            {
                EntryId = reader.IsDBNull(reader.GetOrdinal("Entry_Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Entry_Id")),
                Entry_Date = reader.IsDBNull(reader.GetOrdinal("Entry_Date")) ? string.Empty : reader.GetString(reader.GetOrdinal("Entry_Date")),
                PC_Name = reader.IsDBNull(reader.GetOrdinal("PC_Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("PC_Name")),
                Inst_No = reader.IsDBNull(reader.GetOrdinal("Inst_No")) ? string.Empty : reader.GetString(reader.GetOrdinal("Inst_No")),
                Creator_Initials = reader.IsDBNull(reader.GetOrdinal("creator_initials")) ? string.Empty : reader.GetString(reader.GetOrdinal("creator_initials")),
                App_Owner = reader.IsDBNull(reader.GetOrdinal("app_owner")) ? string.Empty : reader.GetString(reader.GetOrdinal("app_owner")),
                Status = reader.IsDBNull(reader.GetOrdinal("status")) ? string.Empty : reader.GetString(reader.GetOrdinal("status")),
                Serial_No = reader.IsDBNull(reader.GetOrdinal("serial_no")) ? string.Empty : reader.GetString(reader.GetOrdinal("serial_no")),
                Mac_Address1 = reader.IsDBNull(reader.GetOrdinal("MAC_Address1")) ? string.Empty : reader.GetString(reader.GetOrdinal("MAC_Address1")),
                Mac_Address2 = reader.IsDBNull(reader.GetOrdinal("MAC_Address2")) ? string.Empty : reader.GetString(reader.GetOrdinal("MAC_Address2")),
                UUID = reader.IsDBNull(reader.GetOrdinal("UUID")) ? string.Empty : reader.GetString(reader.GetOrdinal("UUID")),
                Product_No = reader.IsDBNull(reader.GetOrdinal("Product_no")) ? string.Empty : reader.GetString(reader.GetOrdinal("Product_no")),
                Model_Name_and_No = reader.IsDBNull(reader.GetOrdinal("model_name_and_no")) ? string.Empty : reader.GetString(reader.GetOrdinal("model_name_and_no")),
                Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? string.Empty : reader.GetString(reader.GetOrdinal("Department")),
                Service_Start = reader.IsDBNull(reader.GetOrdinal("Service_Start")) ? string.Empty : reader.GetString(reader.GetOrdinal("Service_Start")),
                Service_Ends = reader.IsDBNull(reader.GetOrdinal("Service_Ends")) ? string.Empty : reader.GetString(reader.GetOrdinal("Service_Ends")),
                Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? string.Empty : reader.GetString(reader.GetOrdinal("Note")),
                MachineType = reader.IsDBNull(reader.GetOrdinal("MachineType")) ? string.Empty : reader.GetString(reader.GetOrdinal("MachineType"))
            };
        }

        #endregion
    }
}

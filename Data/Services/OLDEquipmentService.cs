using SusEquip.Data.Exceptions;
using SusEquip.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SusEquip.Data.Services
{
    public class OLDEquipmentService
    {
        private readonly DatabaseHelper _dbHelper;

        public OLDEquipmentService(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public void AddEntry(OLDEquipmentData equipmentData)
        {
            if (equipmentData == null) throw new EquipmentValidationException("Equipment data cannot be null", "equipmentData");
            
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                "INSERT INTO OLD_Equip (Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, " +
                "MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, Service_Start, Service_Ends, Note, MachineType) " +
                "VALUES (@_entryDate, @_instNo, @_creator, @_appOwner, @_status, @_serialNr, @_mac1, @_mac2, @_uUID, @_productNo, @_modelNameAndNr, @_department, @_pcName, @_serviceStart, @_serviceEnd, @_note, @_machineType)");

            command.Parameters.AddWithValue("@_entryDate", equipmentData.Entry_Date);
            command.Parameters.AddWithValue("@_instNo", equipmentData.Inst_No);
            command.Parameters.AddWithValue("@_creator", equipmentData.Creator_Initials);
            command.Parameters.AddWithValue("@_appOwner", equipmentData.App_Owner);
            command.Parameters.AddWithValue("@_status", equipmentData.Status);
            command.Parameters.AddWithValue("@_serialNr", equipmentData.Serial_No);
            command.Parameters.AddWithValue("@_mac1", equipmentData.Mac_Address1);
            command.Parameters.AddWithValue("@_mac2", equipmentData.Mac_Address2);
            command.Parameters.AddWithValue("@_uUID", equipmentData.UUID);
            command.Parameters.AddWithValue("@_productNo", equipmentData.Product_No);
            command.Parameters.AddWithValue("@_modelNameAndNr", equipmentData.Model_Name_and_No);
            command.Parameters.AddWithValue("@_department", equipmentData.Department);
            command.Parameters.AddWithValue("@_pcName", equipmentData.PC_Name);
            command.Parameters.AddWithValue("@_serviceStart", equipmentData.Service_Start);
            command.Parameters.AddWithValue("@_serviceEnd", equipmentData.Service_Ends);
            command.Parameters.AddWithValue("@_note", equipmentData.Note);
            command.Parameters.AddWithValue("@_machineType", equipmentData.MachineType);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException(ex.Message, "AddEntry", "OLDEquip", ex);
            }
        }

        public string GetNextOLDInstNo()
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, 
                "SELECT MAX(CAST(SUBSTRING(inst_no, 3, LEN(inst_no) - 2) AS INT)) FROM OLD_Equip WHERE inst_no LIKE 'O-%'");

            connection.Open();
            object result = command.ExecuteScalar();
            
            int nextNumber = 1;
            if (result != DBNull.Value && result != null)
            {
                nextNumber = Convert.ToInt32(result) + 1;
            }
            
            return $"O-{nextNumber}";
        }

        public bool IsOLDInstNoTaken(string instNo)
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, "SELECT COUNT(*) FROM OLD_Equip WHERE inst_no = @instNo");
            command.Parameters.AddWithValue("@instNo", instNo);

            connection.Open();
            int count = (int)command.ExecuteScalar();
            return count > 0;
        }

        public List<OLDEquipmentData> GetOLDEquipment()
        {
            List<OLDEquipmentData> devices = new List<OLDEquipmentData>();
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                "SELECT Entry_Id, Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, " +
                "MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Department, PC_Name, " +
                "Service_Start, Service_Ends, Note, MachineType " +
                "FROM OLD_Equip"
            );

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                OLDEquipmentData item = new OLDEquipmentData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                    Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                    Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                    MachineType = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
                };
                devices.Add(item);
            }
            return devices;
        }

        public List<OLDEquipmentData> GetOLDMachines()
        {
            List<OLDEquipmentData> machines = new List<OLDEquipmentData>();
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection,
                @"
                ;WITH temp AS (
                    SELECT e.inst_no, e.Entry_Id,
                           ROW_NUMBER() OVER (PARTITION BY e.inst_no ORDER BY e.Entry_Id DESC) AS rank
                    FROM OLD_Equip e
                ),
                ranked AS (
                    SELECT inst_no, Entry_Id FROM temp WHERE rank = 1
                )
                SELECT 
                    e.Entry_Id, 
                    e.Entry_Date, 
                    e.inst_no, 
                    e.creator_initials, 
                    e.app_owner, 
                    e.status, 
                    e.serial_no, 
                    e.MAC_Address1, 
                    e.MAC_Address2, 
                    e.UUID, 
                    e.Product_no, 
                    e.model_name_and_no, 
                    e.Department, 
                    e.PC_Name, 
                    e.Service_Start, 
                    e.Service_Ends, 
                    e.Note,
                    e.MachineType
                FROM OLD_Equip e
                JOIN ranked ON e.Entry_Id = ranked.Entry_Id;"
            );

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                OLDEquipmentData item = new OLDEquipmentData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                    Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                    Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                    PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                    Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                    Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                    MachineType = reader.IsDBNull(17) ? string.Empty : reader.GetString(17)
                };
                machines.Add(item);
            }

            return machines;
        }

        /// <summary>
        /// Determines if a machine should be classified as an OLD machine based on PC_Name or Department
        /// </summary>
        /// <param name="pcName">PC Name</param>
        /// <param name="department">Department</param>
        /// <returns>True if machine should be treated as OLD machine</returns>
        public static bool IsOLDMachine(string pcName, string department)
        {
            if (string.IsNullOrWhiteSpace(pcName) && string.IsNullOrWhiteSpace(department))
                return false;

            return (pcName?.ToUpper().Contains("OLD") == true) || 
                   (department?.ToUpper().Contains("OLD") == true);
        }
    }
}

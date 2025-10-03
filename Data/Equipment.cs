using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using SusEquip.Data.Models;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Text.RegularExpressions;


namespace SusEquip.Data
{
    public class Equipment
    {
        private readonly string _connectionString;
        private readonly string scriptPath;

        //// SqlConnectionStringBuilder instance for building the connection string
        //private SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder
        //{
        //    UserID = "sa", // Consider using a less privileged account for security reasons.
        //    Password = "", // Avoid hardcoding passwords.
        //    DataSource = "SUS-EL-MPARK1\\MSSQLSERVER01", // Ensure this server is accessible and the name is correct.
        //    InitialCatalog = "MarkParkingDB", // Ensure this database exists.
        //    IntegratedSecurity = false, // For production, consider using integrated security.
        //    TrustServerCertificate = true // Ensure this setting matches your security requirements.
        //};


        // Constructor to initialize the connection string
        /// <summary>
        /// In the constructor of Equipment, the IOptions<ConnectionString> interface is used as a parameter.
        /// ASP.NET Core's dependency injection system automatically provides an instance of ConnectionString
        /// to the constructor when creating an instance of PersonManager. 
        /// This process is known as dependency injection.
        /// </summary>
        /// <param name="connectionString"></param>
        public Equipment(IOptions<ConnectionString> connectionString) => _connectionString = connectionString.Value.DefaultConnection;
        /* connectionString is an instance of IOptions<ConnectionString> injected into the constructor. 
            *By accessing connectionString.Value.DefaultConnection, the actual connection string value from the configuration file 
            *(such as appsettings.json in an ASP.NET Core application) is retrieved and stored in the private _connectionString field. 
            *This connection string can then be used to establish a connection to the database.
            */

        // Method to add a new entry to the Equip table
        public void AddEntry(string _entryDate, int _instNo, string _creator, string _appOwner, string _status, string _serialNr, string _mac1, string _mac2, string _uUID, string _productNo, string _modelNameAndNr, string _desc, string _dep, string _serviceStart, string _serviceEnd, string _note)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = connection.CreateCommand();

            // SQL command to insert a new record into the Equip table
            command.CommandText = "INSERT INTO Equip (Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, Mac_Address1, Mac_Address2, UUID, Product_no, model_name_and_no, Pc_Name, Department, Service_Start, Service_Ends, Note) " +
                                  "VALUES (@_entryDate, @_instNo, @_creator, @_appOwner, @_status, @_serialNr, @_mac1, @_mac2, @_uUID, @_productNo, @_modelNameAndNr, @_desc, @_dep, @_serviceStart, @_serviceEnd, @_note)";

            command.Parameters.AddWithValue("@_entryDate", _entryDate);
            command.Parameters.AddWithValue("@_instNo", _instNo);
            command.Parameters.AddWithValue("@_creator", _creator);
            command.Parameters.AddWithValue("@_appOwner", _appOwner);
            command.Parameters.AddWithValue("@_status", _status);
            command.Parameters.AddWithValue("@_serialNr", _serialNr);
            command.Parameters.AddWithValue("@_mac1", _mac1);
            command.Parameters.AddWithValue("@_mac2", _mac2);
            command.Parameters.AddWithValue("@_uUID", _uUID);
            command.Parameters.AddWithValue("@_productNo", _productNo);
            command.Parameters.AddWithValue("@_modelNameAndNr", _modelNameAndNr);
            command.Parameters.AddWithValue("@_desc", _desc);
            command.Parameters.AddWithValue("@_dep", _dep);
            command.Parameters.AddWithValue("@_serviceStart", _serviceStart);
            command.Parameters.AddWithValue("@_serviceEnd", _serviceEnd);
            command.Parameters.AddWithValue("@_note", _note);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                throw;
            }
        }

        // Method to retrieve the next instance number based on the maximum existing instance number
        public int GetNextInstNo()
        {
            int nextInstNo = 1; // Default to  1 if the table is empty
            string query = "SELECT MAX(Inst_No) FROM Equip";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != DBNull.Value)
                    {
                        nextInstNo = Convert.ToInt32(result) + 1;
                    }
                }
            }
            return nextInstNo;
        }

        public void DeleteEntry(int inst_no, int entry_id)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = connection.CreateCommand();

            // SQL command to delete the entry with the given inst_no and entry_id
            command.CommandText = @"
                        DELETE FROM Equip
                        WHERE inst_no = @inst_no AND Entry_id = @entry_id";

            command.Parameters.AddWithValue("@inst_no", inst_no);
            command.Parameters.AddWithValue("@entry_id", entry_id);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                throw;
            }
        }

        // Method for updating an entry in the Equip table
        public void InsertEntry(EquipmentData ed)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = connection.CreateCommand();

            // SQL command to insert a new record into the Equip table
            command.CommandText = "INSERT INTO Equip (Entry_Date, inst_no, creator_initials, app_owner, status, serial_no, MAC_Address1, MAC_Address2, UUID, Product_no, model_name_and_no, Pc_Name, Department, Service_Start, Service_Ends, Note) VALUES (@EntryDate, @InstNo, @Creator, @AppOwner, @Status, @SerialNo, @Mac1, @Mac2, @UUID, @ProductNo, @ModelNameAndNo, @Pc_Name, @Department, @ServiceStart, @ServiceEnd, @Note)";

            // Adds the parameters to the SQL command
            command.Parameters.AddWithValue("@EntryDate", ed.Entry_Date);
            command.Parameters.AddWithValue("@InstNo", ed.Inst_No);
            command.Parameters.AddWithValue("@Creator", ed.Creator_Initials);
            command.Parameters.AddWithValue("@AppOwner", ed.App_Owner);
            command.Parameters.AddWithValue("@Status", ed.Status);
            command.Parameters.AddWithValue("@SerialNo", ed.Serial_No);
            command.Parameters.AddWithValue("@Mac1", ed.Mac_Address1);
            command.Parameters.AddWithValue("@Mac2", ed.Mac_Address2);
            command.Parameters.AddWithValue("@UUID", ed.UUID);
            command.Parameters.AddWithValue("@ProductNo", ed.Product_No);
            command.Parameters.AddWithValue("@ModelNameAndNo", ed.Model_Name_and_No);
            command.Parameters.AddWithValue("@Department", ed.Department);
            command.Parameters.AddWithValue("@Pc_Name", ed.PC_Name);
            command.Parameters.AddWithValue("@ServiceStart", ed.Service_Start);
            command.Parameters.AddWithValue("@ServiceEnd", ed.Service_Ends);
            command.Parameters.AddWithValue("@Note", ed.Note);

            try
            {
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                throw;
            }
        }


        // Method to retrieve the current date and time in a specific format.
        public string CurrentDayDisplay()
        {
            DateTime date = DateTime.Now;
            string currentDay = date.ToString("dd-MMMM-yyyy HH:mm");
            return currentDay;
        }

        // Method to retrieve equipment records sorted by instance number.
        public List<EquipmentData> GetEquipmentSorted(int inst_no)
        {
            List<EquipmentData> equipment = new List<EquipmentData>();
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = connection.CreateCommand();

            // Define the SQL command to select equipment records for a specific instance number.
            command.CommandText = "SELECT Entry_id, Entry_date, inst_no, creator_initials, app_owner, status, serial_no, " +
                                  "Mac_Address1, Mac_Address2, UUID, product_no, model_name_and_no, Pc_Name, Department, " +
                                  "Service_Start, Service_Ends, Note FROM Equip WHERE inst_no = @inst_no";
            command.Parameters.AddWithValue("@inst_no", inst_no);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();

            NullCheckGES(equipment, reader);
            return equipment;
        }

        // Method to retrieve the most recent equipment record for a specific instance number.
        public List<EquipmentData> GetEquipSortedByEntry(int inst_no)
        {
            List<EquipmentData> equipmentList = new List<EquipmentData>();
            EquipmentData equipment = new EquipmentData();
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM Equip " +
                                  "WHERE Inst_No = @inst_no AND Entry_id = (SELECT MAX(Entry_id) FROM Equip WHERE Inst_No = @inst_no);";

            command.Parameters.AddWithValue("@inst_no", inst_no);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();

            // Process the retrieved data.
            NullCheckGES(equipmentList, reader);
            equipmentList.Add(equipment);
            return equipmentList;
        }
        private static void NullCheckGES(List<EquipmentData> equipment, SqlDataReader reader) //GES = GetEquipmentSorted //DEBUG VERSION DO NOT PUSH TO PRODUCTION
        {
            while (reader.Read())
            {// Check for NULL values before reading and assign them to string properties
                EquipmentData item = new EquipmentData();
                item.EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                item.Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                item.Inst_No = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                item.Creator_Initials = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                item.App_Owner = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                item.Status = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                item.Serial_No = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                item.Mac_Address1 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);
                item.Mac_Address2 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);
                item.UUID = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
                item.Product_No = reader.IsDBNull(10) ? string.Empty : reader.GetString(10);
                item.Model_Name_and_No = reader.IsDBNull(11) ? string.Empty : reader.GetString(11);
                item.Department = reader.IsDBNull(12) ? string.Empty : reader.GetString(12);
                item.PC_Name = reader.IsDBNull(13) ? string.Empty : reader.GetString(13);
                item.Service_Start = reader.IsDBNull(14) ? string.Empty : reader.GetString(14);
                item.Service_Ends = reader.IsDBNull(15) ? string.Empty : reader.GetString(15);
                item.Note = reader.IsDBNull(16) ? string.Empty : reader.GetString(16);
                equipment.Add(item);
            }
        }
        // Method to retrieve a list of machines from the database.
        public List<MachineData> GetMachines() //DEBUG VERSION DO NOT PUSH TO PRODUCTION
        {
            List<MachineData> machines = new List<MachineData>();
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = connection.CreateCommand();
             //command.CommandText = @"
             //                SELECT E1.*
             //                FROM Equip E1
             //                WHERE E1.Entry_id = (
             //                    SELECT MAX(E2.Entry_id)
             //                    FROM Equip E2
             //                    WHERE E2.Inst_No = E1.Inst_No
             //                );";
             command.CommandText = @"
                 ;with temp as(
                     select e.inst_no, Entry_Id,
                         ROW_NUMBER() over (partition by inst_no order by entry_id desc) as rank
                     from equip e
                 ),
                 ranked as (
                     select inst_no, Entry_Id from temp where rank = 1
                 )
                 select e.* from Equip e
                 join ranked on e.Entry_Id = ranked.Entry_Id;";
            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                MachineData item = new MachineData
                {
                    EntryId = reader.IsDBNull(reader.GetOrdinal("Entry_Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Entry_Id")),
                    Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? string.Empty : reader.GetString(reader.GetOrdinal("Department")),
                    Inst_No = reader.IsDBNull(reader.GetOrdinal("Inst_No")) ? 0 : reader.GetInt32(reader.GetOrdinal("Inst_No")),
                    PC_Name = reader.IsDBNull(reader.GetOrdinal("Pc_Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Pc_Name")),
                    App_Owner = reader.IsDBNull(reader.GetOrdinal("App_Owner")) ? string.Empty : reader.GetString(reader.GetOrdinal("App_Owner")),
                    Serial_No = reader.IsDBNull(reader.GetOrdinal("Serial_No")) ? string.Empty : reader.GetString(reader.GetOrdinal("Serial_No")),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? string.Empty : reader.GetString(reader.GetOrdinal("Status")),
                    Mac_Address1 = reader.IsDBNull(reader.GetOrdinal("Mac_Address1")) ? string.Empty : reader.GetString(reader.GetOrdinal("Mac_Address1")),
                    Mac_Address2 = reader.IsDBNull(reader.GetOrdinal("Mac_Address2")) ? null : reader.GetString(reader.GetOrdinal("Mac_Address2")),
                    UUID = reader.IsDBNull(reader.GetOrdinal("UUID")) ? string.Empty : reader.GetString(reader.GetOrdinal("UUID")),
                    Product_No = reader.IsDBNull(reader.GetOrdinal("Product_No")) ? string.Empty : reader.GetString(reader.GetOrdinal("Product_No")),
                    Model_Name_and_No = reader.IsDBNull(reader.GetOrdinal("Model_Name_and_No")) ? string.Empty : reader.GetString(reader.GetOrdinal("Model_Name_and_No")),
                    Service_Start = reader.IsDBNull(reader.GetOrdinal("Service_Start")) ? string.Empty : reader.GetString(reader.GetOrdinal("Service_Start")),
                    Service_Ends = reader.IsDBNull(reader.GetOrdinal("Service_Ends")) ? string.Empty : reader.GetString(reader.GetOrdinal("Service_Ends"))
                };
                machines.Add(item);
            }
            return machines;
        }

        public List<EquipmentData> GetEquipment() //DEBUG VERSION DO NOT PUSH TO PRODUCTION
        {
            List<EquipmentData> devices = new List<EquipmentData>();
            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = @"
                                SELECT *
                                FROM Equip;";

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();
            int rowCount = 0;
            while (reader.Read())
            {
                rowCount++;
                EquipmentData item = new EquipmentData
                {
                    EntryId = reader.IsDBNull(reader.GetOrdinal("Entry_Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Entry_Id")),
                    Entry_Date = reader.IsDBNull(reader.GetOrdinal("Entry_Date")) ? string.Empty : reader.GetString(reader.GetOrdinal("Entry_Date")),
                    PC_Name = reader.IsDBNull(reader.GetOrdinal("Pc_Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Pc_Name")),
                    Inst_No = reader.IsDBNull(reader.GetOrdinal("Inst_No")) ? 0 : reader.GetInt32(reader.GetOrdinal("Inst_No")),
                    Creator_Initials = reader.IsDBNull(reader.GetOrdinal("Creator_Initials")) ? string.Empty : reader.GetString(reader.GetOrdinal("Creator_Initials")),
                    App_Owner = reader.IsDBNull(reader.GetOrdinal("App_Owner")) ? string.Empty : reader.GetString(reader.GetOrdinal("App_Owner")),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? string.Empty : reader.GetString(reader.GetOrdinal("Status")),
                    Serial_No = reader.IsDBNull(reader.GetOrdinal("Serial_No")) ? string.Empty : reader.GetString(reader.GetOrdinal("Serial_No")),
                    Mac_Address1 = reader.IsDBNull(reader.GetOrdinal("Mac_Address1")) ? string.Empty : reader.GetString(reader.GetOrdinal("Mac_Address1")),
                    Mac_Address2 = reader.IsDBNull(reader.GetOrdinal("Mac_Address2")) ? null : reader.GetString(reader.GetOrdinal("Mac_Address2")),
                    UUID = reader.IsDBNull(reader.GetOrdinal("UUID")) ? string.Empty : reader.GetString(reader.GetOrdinal("UUID")),
                    Product_No = reader.IsDBNull(reader.GetOrdinal("Product_No")) ? string.Empty : reader.GetString(reader.GetOrdinal("Product_No")),
                    Model_Name_and_No = reader.IsDBNull(reader.GetOrdinal("Model_Name_and_No")) ? string.Empty : reader.GetString(reader.GetOrdinal("Model_Name_and_No")),
                    Service_Start = reader.IsDBNull(reader.GetOrdinal("Service_Start")) ? string.Empty : reader.GetString(reader.GetOrdinal("Service_Start")),
                    Service_Ends = reader.IsDBNull(reader.GetOrdinal("Service_Ends")) ? string.Empty : reader.GetString(reader.GetOrdinal("Service_Ends")),
                    Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? string.Empty : reader.GetString(reader.GetOrdinal("Department")),
                    Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? string.Empty : reader.GetString(reader.GetOrdinal("Note"))
                };
                devices.Add(item);
            }
            return devices;
        }

        public bool ValidateEquipmentData(EquipmentData equipmentData)
        {
            // Define the required number of characters for Mac addresses
            int macAddressLength = 17; // Assuming MAC addresses are in the format "XX:XX:XX:XX:XX:XX"
            // Regular expression for UUID validation
            Regex uuidRegex = new Regex(@"^[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}$");

            // Validation rules
            if (string.IsNullOrWhiteSpace(equipmentData.PC_Name))
                return false;

            if (equipmentData.Inst_No <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(equipmentData.Creator_Initials))
                return false;

            if (string.IsNullOrWhiteSpace(equipmentData.Status))
                return false;

            if (!string.IsNullOrEmpty(equipmentData.Serial_No) && equipmentData.Serial_No.Length < macAddressLength)
                return false;

            if (!string.IsNullOrEmpty(equipmentData.Mac_Address1) && equipmentData.Mac_Address1.Length < macAddressLength)
                return false;

            if (!string.IsNullOrEmpty(equipmentData.UUID) && !uuidRegex.IsMatch(equipmentData.UUID))
                return false;

            return true;
        }

        public void ExportTableToExcel() //TODO: Test this method
        {
            string tableName = "Equip";
            string formattedDateTime = DateTime.Now.ToString("dd-MM-yyyy-hh_mm");
            // Set the license context to NonCommercial or Commercial as per your usage
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Define the output file name
            string fileName = $"{tableName}_{formattedDateTime}.xlsx";

            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Get the worksheet
                var worksheet = package.Workbook.Worksheets.Add(tableName);

                // Define the query to select all records from the table
                string query = $"SELECT * FROM {tableName}";

                // Execute the query and load the result set
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            // Load the data into the worksheet starting from row 2 (row 1 for headers)
                            int rowIndex = 2;
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    worksheet.Cells[rowIndex, i + 1].Value = reader[i];
                                }
                                rowIndex++;
                            }
                        }
                    }
                }

                // Optionally, set headers explicitly
                worksheet.Cells[1, 1].Value = "Entry ID";
                worksheet.Cells[1, 2].Value = "Entry Date";
                worksheet.Cells[1, 3].Value = "Inst_No";
                worksheet.Cells[1, 4].Value = "Creator Initials";
                worksheet.Cells[1, 5].Value = "App Owner";
                worksheet.Cells[1, 6].Value = "Status";
                worksheet.Cells[1, 7].Value = "Serial Number";
                worksheet.Cells[1, 8].Value = "MAC Address 1";
                worksheet.Cells[1, 9].Value = "MAC Address 2";
                worksheet.Cells[1, 10].Value = "UUID";
                worksheet.Cells[1, 11].Value = "Product Number";
                worksheet.Cells[1, 12].Value = "Model Name and Number";
                worksheet.Cells[1, 13].Value = "PC Name";
                worksheet.Cells[1, 14].Value = "Department";
                worksheet.Cells[1, 15].Value = "Service Starts";
                worksheet.Cells[1, 16].Value = "Service Ends";
                worksheet.Cells[1, 17].Value = "Note";



                // Save the Excel file to the specified output path
                package.SaveAs(new FileInfo(fileName));
            }
        }

    }

}

using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using SusEquip.Data.Models;
using System.Threading.Tasks;

namespace SusEquip.Data
{
    public class DatabaseHelper
    {

        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public SqlCommand CreateCommand(SqlConnection connection, string query)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;
            return command;
        }

        public async Task ExecuteQueryAsync(string query, SqlParameter[] parameters)
        {
            using var connection = GetConnection();
            using var command = CreateCommand(connection, query);
            command.Parameters.AddRange(parameters);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public void LogDTUPCEntry(DTUPC_Log logEntry)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "INSERT INTO DTUPC_Log (EntryDate, CreatorInitials, PCName, MacAddress1, SerialNo, UUID) " +
                            "VALUES (@EntryDate, @CreatorInitials, @PCName, @MacAddress1, @SerialNo, @UUID)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EntryDate", logEntry.EntryDate);
                    command.Parameters.AddWithValue("@CreatorInitials", logEntry.CreatorInitials);
                    command.Parameters.AddWithValue("@PCName", logEntry.PCName);
                    command.Parameters.AddWithValue("@MacAddress1", logEntry.MacAddress1);
                    command.Parameters.AddWithValue("@SerialNo", logEntry.SerialNo);
                    command.Parameters.AddWithValue("@UUID", logEntry.UUID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<DTUPC_Log> GetAllDTUPCLogEntries()
        {
            var logEntries = new List<DTUPC_Log>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM DTUPC_Log";
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var logEntry = new DTUPC_Log
                            {
                                LogId = reader.GetInt32(0),
                                EntryDate = reader.GetString(1), // Read as string
                                CreatorInitials = reader.GetString(2),
                                PCName = reader.GetString(3),
                                MacAddress1 = reader.GetString(4),
                                SerialNo = reader.GetString(5),
                                UUID = reader.GetString(6)
                            };
                            logEntries.Add(logEntry);
                        }
                    }
                }
            }

            return logEntries;
        }

        public DTUPC_Log GetLogEntryBySerialNo(string serialNo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM DTUPC_Log WHERE SerialNo = @SerialNo";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SerialNo", serialNo);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new DTUPC_Log
                            {
                                LogId = reader.GetInt32(0),
                                EntryDate = reader.GetString(1),
                                CreatorInitials = reader.GetString(2),
                                PCName = reader.GetString(3),
                                MacAddress1 = reader.GetString(4),
                                SerialNo = reader.GetString(5),
                                UUID = reader.GetString(6)
                            };
                        }
                    }
                }
            }
            return null;
        }

    }
}
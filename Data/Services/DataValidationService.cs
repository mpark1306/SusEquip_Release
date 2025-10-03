using Microsoft.Data.SqlClient;
using SusEquip.Data.Models;
using SusEquip.Data.Interfaces.Services;
using System.Text.RegularExpressions;

namespace SusEquip.Data.Services
{
    public class DataValidationService : IDataValidationService
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly string[] _validStatuses = {
            "Modtaget (Ny)", "Hos Bruger", "Kasseret", "Stjålet", 
            "På Lager (Brugt)", "I bur/kasse", "Karantæne", "Afhentet af Refurb"
        };

        public DataValidationService(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            EnsureTablesExist();
        }

        private void EnsureTablesExist()
        {
            try
            {
                // Check and create IgnoredIssues table if it doesn't exist
                if (!TableExists("IgnoredIssues"))
                {
                    CreateIgnoredIssuesTable();
                }

                // Check and create SolvedIssues table if it doesn't exist
                if (!TableExists("SolvedIssues"))
                {
                    CreateSolvedIssuesTable();
                }
            }
            catch (Exception)
            {
                // Log the error but don't prevent the service from initializing
                // Tables creation failed, continue without them
            }
        }

        private bool TableExists(string tableName)
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = @tableName");
            
            command.Parameters.AddWithValue("@tableName", tableName);
            
            try
            {
                connection.Open();
                var result = (int)command.ExecuteScalar();
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public List<ValidationIssue> DetectValidationIssues()
        {
            var issues = new List<ValidationIssue>();
            
            // Get the latest entry for each equipment
            var latestEquipment = GetLatestEquipmentEntries();
            
            foreach (var equipment in latestEquipment)
            {
                issues.AddRange(ValidateEquipment(equipment));
            }

            // Filter out ignored and solved issues
            var filteredIssues = issues.Where(issue => 
                !IsIssueIgnored(issue.InstNo, issue.FieldName, issue.IssueType) &&
                !IsIssueSolved(issue.InstNo, issue.FieldName, issue.IssueType)).ToList();
            
            return filteredIssues.OrderByDescending(i => GetSeverityWeight(i.Severity))
                        .ThenBy(i => i.InstNo)
                        .ToList();
        }

        private List<EquipmentData> GetLatestEquipmentEntries()
        {
            var equipment = new List<EquipmentData>();
            
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                WITH latest_entries AS (
                    SELECT e.inst_no, MAX(e.Entry_Id) as max_entry_id
                    FROM Equip e
                    GROUP BY e.inst_no
                )
                SELECT e.Entry_Id, e.Entry_Date, e.inst_no, e.creator_initials, e.app_owner, 
                       e.status, e.serial_no, e.MAC_Address1, e.MAC_Address2, e.UUID, 
                       e.Product_no, e.model_name_and_no, e.Department, e.PC_Name, 
                       e.Service_Start, e.Service_Ends, e.Note, e.MachineType
                FROM Equip e
                INNER JOIN latest_entries le ON e.inst_no = le.inst_no AND e.Entry_Id = le.max_entry_id
                ORDER BY e.inst_no");

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                equipment.Add(new EquipmentData
                {
                    EntryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    Entry_Date = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Inst_No = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
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
                });
            }

            return equipment;
        }

        private List<ValidationIssue> ValidateEquipment(EquipmentData equipment)
        {
            var issues = new List<ValidationIssue>();

            // 1. Invalid Status Check
            if (!_validStatuses.Contains(equipment.Status) && !string.IsNullOrEmpty(equipment.Status))
            {
                // Skip legacy formats that should be left as-is
                if (!IsLegacyFormat(equipment.Status))
                {
                    var suggestedStatus = SuggestCorrectStatus(equipment.Status);
                    issues.Add(new ValidationIssue
                    {
                        EntryId = equipment.EntryId,
                        InstNo = equipment.Inst_No,
                        PCName = equipment.PC_Name,
                        IssueType = "Invalid Status",
                        FieldName = "Status",
                        CurrentValue = equipment.Status,
                        SuggestedValue = suggestedStatus,
                        Reason = "Status value does not match any valid status options. Use one of: Hos Bruger, Modtaget (Ny), Kasseret, På Lager (Brugt), Karantæne, Stjålet",
                        Severity = "Medium",
                        EquipmentData = equipment
                    });
                }
            }

            // 2. Service End Date Check for "Kasseret" status
            if (equipment.Status == "Kasseret" && 
                (string.IsNullOrEmpty(equipment.Service_Ends) || 
                 !DateTime.TryParse(equipment.Service_Ends, out var serviceEnd) ||
                 serviceEnd > DateTime.Now))
            {
                issues.Add(new ValidationIssue
                {
                    EntryId = equipment.EntryId,
                    InstNo = equipment.Inst_No,
                    PCName = equipment.PC_Name,
                    IssueType = "Invalid Service End Date",
                    FieldName = "Service_Ends",
                    CurrentValue = equipment.Service_Ends,
                    SuggestedValue = DateTime.Now.ToString("yyyy-MM-dd"),
                    Reason = "Equipment marked as 'Kasseret' should have a valid service end date in the past",
                    Severity = "Medium",
                    EquipmentData = equipment
                });
            }

            // 3. Active equipment with past service end date (ignore for Lab machines)
            if ((equipment.Status == "Hos Bruger" || equipment.Status == "Modtaget (Ny)") &&
                !string.IsNullOrEmpty(equipment.Service_Ends) &&
                DateTime.TryParse(equipment.Service_Ends, out var endDate) &&
                endDate < DateTime.Now &&
                !string.Equals(equipment.MachineType, "Lab", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ValidationIssue
                {
                    EntryId = equipment.EntryId,
                    InstNo = equipment.Inst_No,
                    PCName = equipment.PC_Name,
                    IssueType = "Expired Service Date",
                    FieldName = "Status",
                    CurrentValue = equipment.Status,
                    SuggestedValue = "Kasseret",
                    Reason = "Equipment has passed service end date but is still marked as active",
                    Severity = "Low",
                    EquipmentData = equipment
                });
            }

            // 4. Missing PC Name for active equipment (only check "Hos Bruger", not "Modtaget (Ny)")
            if (equipment.Status == "Hos Bruger" && string.IsNullOrWhiteSpace(equipment.PC_Name))
            {
                // Option 1: Add PC Name
                issues.Add(new ValidationIssue
                {
                    EntryId = equipment.EntryId,
                    InstNo = equipment.Inst_No,
                    PCName = equipment.PC_Name,
                    IssueType = "Missing PC Name",
                    FieldName = "PC_Name",
                    CurrentValue = equipment.PC_Name,
                    SuggestedValue = $"PC-{equipment.Inst_No:D6}",
                    Reason = "Equipment marked as 'Hos Bruger' requires a PC name. Use naming convention: PC-XXXXXX (where X is 6-digit Inst_No) or department-specific naming",
                    Severity = "Medium",
                    EquipmentData = equipment
                });

                // Option 2: Update Status if not actually in use
                issues.Add(new ValidationIssue
                {
                    EntryId = equipment.EntryId,
                    InstNo = equipment.Inst_No,
                    PCName = equipment.PC_Name,
                    IssueType = "Status May Be Incorrect",
                    FieldName = "Status",
                    CurrentValue = equipment.Status,
                    SuggestedValue = "På Lager (Brugt)",
                    Reason = "If equipment is not actually in use, status should be updated instead of adding PC name",
                    Severity = "Medium",
                    EquipmentData = equipment
                });
            }

            // 5. Invalid MAC Address format
            if (!string.IsNullOrEmpty(equipment.Mac_Address1) && !IsValidMacAddress(equipment.Mac_Address1))
            {
                var formattedMac = FormatMacAddress(equipment.Mac_Address1);
                var suggestion = formattedMac != equipment.Mac_Address1 ? formattedMac : "Please enter valid MAC address (e.g., 00:1A:2B:3C:4D:5E)";
                
                issues.Add(new ValidationIssue
                {
                    EntryId = equipment.EntryId,
                    InstNo = equipment.Inst_No,
                    PCName = equipment.PC_Name,
                    IssueType = "Invalid MAC Address",
                    FieldName = "Mac_Address1",
                    CurrentValue = equipment.Mac_Address1,
                    SuggestedValue = suggestion,
                    Reason = "MAC address must be in format XX:XX:XX:XX:XX:XX using hexadecimal characters (0-9, A-F)",
                    Severity = "High",
                    EquipmentData = equipment
                });
            }

            // 6. Invalid UUID format
            if (!string.IsNullOrEmpty(equipment.UUID) && !IsValidUuid(equipment.UUID))
            {
                issues.Add(new ValidationIssue
                {
                    EntryId = equipment.EntryId,
                    InstNo = equipment.Inst_No,
                    PCName = equipment.PC_Name,
                    IssueType = "Invalid UUID",
                    FieldName = "UUID",
                    CurrentValue = equipment.UUID,
                    SuggestedValue = "Generate new UUID (e.g., 12345678-90AB-CDEF-1234-567890ABCDEF)",
                    Reason = "UUID must be in standard format: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX using hexadecimal characters",
                    Severity = "High",
                    EquipmentData = equipment
                });
            }

            // 7. Missing App Owner for active equipment
            if ((equipment.Status == "Hos Bruger") && string.IsNullOrWhiteSpace(equipment.App_Owner))
            {
                issues.Add(new ValidationIssue
                {
                    EntryId = equipment.EntryId,
                    InstNo = equipment.Inst_No,
                    PCName = equipment.PC_Name,
                    IssueType = "Missing App Owner",
                    FieldName = "App_Owner",
                    CurrentValue = equipment.App_Owner,
                    SuggestedValue = "N/A",
                    Reason = "Equipment in use should have an app owner assigned. Use 'N/A' if no specific owner or department",
                    Severity = "Low",
                    EquipmentData = equipment
                });
            }

            return issues;
        }

        private string SuggestCorrectStatus(string currentStatus)
        {
            // Simple fuzzy matching for common misspellings
            var statusLower = currentStatus.ToLowerInvariant();
            
            if (statusLower.Contains("bruger") || statusLower.Contains("user")) return "Hos Bruger";
            if (statusLower.Contains("ny") || statusLower.Contains("new")) return "Modtaget (Ny)";
            if (statusLower.Contains("kasseret") || statusLower.Contains("retired")) return "Kasseret";
            if (statusLower.Contains("lager") || statusLower.Contains("storage")) return "På Lager (Brugt)";
            if (statusLower.Contains("karantæne") || statusLower.Contains("quarantine")) return "Karantæne";
            if (statusLower.Contains("stjålet") || statusLower.Contains("stolen")) return "Stjålet";
            
            return "Modtaget (Ny)"; // Default suggestion
        }

        private bool IsLegacyFormat(string status)
        {
            // Check for legacy formats that should be left as-is
            if (string.IsNullOrWhiteSpace(status))
                return false;
                
            var statusLower = status.ToLowerInvariant();
            
            // Legacy format: "I bur/kasse uden BIOS pw"
            if (statusLower.Contains("i bur") || statusLower.Contains("kasse uden bios"))
                return true;
                
            // Legacy format: "På lager uden BIOS pw"
            if (statusLower.Contains("på lager uden bios"))
                return true;
                
            // Add other legacy formats here if needed
            
            return false;
        }

        private bool IsValidMacAddress(string macAddress)
        {
            var macRegex = new Regex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
            return macRegex.IsMatch(macAddress);
        }

        private string FormatMacAddress(string macAddress)
        {
            // Remove any existing separators and spaces
            var cleaned = Regex.Replace(macAddress, @"[:\-\s]", "");
            
            // If it's 12 hex characters, format it properly
            if (cleaned.Length == 12 && Regex.IsMatch(cleaned, @"^[0-9A-Fa-f]{12}$"))
            {
                return string.Join(":", Enumerable.Range(0, 6)
                    .Select(i => cleaned.Substring(i * 2, 2).ToUpperInvariant()));
            }
            
            return macAddress; // Return original if can't format
        }

        private bool IsValidUuid(string uuid)
        {
            return Guid.TryParse(uuid, out _);
        }

        private int GetSeverityWeight(string severity)
        {
            return severity switch
            {
                "High" => 3,
                "Medium" => 2,
                "Low" => 1,
                _ => 0
            };
        }

        public void LogCorrection(DataCorrection correction)
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                INSERT INTO DataCorrections 
                (EntryId, InstNo, FieldName, OldValue, NewValue, CorrectorInitials, AppOwner, Reason, CorrectionDate, IssueType, IsApproved)
                VALUES 
                (@EntryId, @InstNo, @FieldName, @OldValue, @NewValue, @CorrectorInitials, @AppOwner, @Reason, @CorrectionDate, @IssueType, @IsApproved)");

            command.Parameters.AddWithValue("@EntryId", correction.EntryId);
            command.Parameters.AddWithValue("@InstNo", correction.InstNo);
            command.Parameters.AddWithValue("@FieldName", correction.FieldName);
            command.Parameters.AddWithValue("@OldValue", correction.OldValue);
            command.Parameters.AddWithValue("@NewValue", correction.NewValue);
            command.Parameters.AddWithValue("@CorrectorInitials", correction.CorrectorInitials);
            command.Parameters.AddWithValue("@AppOwner", correction.AppOwner);
            command.Parameters.AddWithValue("@Reason", correction.Reason);
            command.Parameters.AddWithValue("@CorrectionDate", correction.CorrectionDate);
            command.Parameters.AddWithValue("@IssueType", correction.IssueType);
            command.Parameters.AddWithValue("@IsApproved", correction.IsApproved);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Handle the case where the table doesn't exist yet
                if (ex.Message.Contains("Invalid object name 'DataCorrections'"))
                {
                    CreateDataCorrectionsTable();
                    // Retry the insert
                    connection.Close();
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                else
                {
                    throw;
                }
            }
        }

        private void CreateDataCorrectionsTable()
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                CREATE TABLE DataCorrections (
                    CorrectionId INT IDENTITY(1,1) PRIMARY KEY,
                    EntryId INT NOT NULL,
                    InstNo INT NOT NULL,
                    FieldName NVARCHAR(100) NOT NULL,
                    OldValue NVARCHAR(500),
                    NewValue NVARCHAR(500),
                    CorrectorInitials NVARCHAR(10) NOT NULL,
                    AppOwner NVARCHAR(100) NOT NULL,
                    Reason NVARCHAR(500),
                    CorrectionDate DATETIME NOT NULL,
                    IssueType NVARCHAR(100),
                    IsApproved BIT DEFAULT 0
                )");

            connection.Open();
            command.ExecuteNonQuery();
        }

        public List<DataCorrection> GetCorrectionHistory(int? instNo = null)
        {
            var corrections = new List<DataCorrection>();
            
            var query = @"
                SELECT CorrectionId, EntryId, InstNo, FieldName, OldValue, NewValue, 
                       CorrectorInitials, AppOwner, Reason, CorrectionDate, IssueType, IsApproved
                FROM DataCorrections";
            
            if (instNo.HasValue)
            {
                query += " WHERE InstNo = @InstNo";
            }
            
            query += " ORDER BY CorrectionDate DESC";

            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, query);

            if (instNo.HasValue)
            {
                command.Parameters.AddWithValue("@InstNo", instNo.Value);
            }

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    corrections.Add(new DataCorrection
                    {
                        CorrectionId = reader.GetInt32(0),
                        EntryId = reader.GetInt32(1),
                        InstNo = reader.GetInt32(2),
                        FieldName = reader.GetString(3),
                        OldValue = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                        NewValue = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                        CorrectorInitials = reader.GetString(6),
                        AppOwner = reader.GetString(7),
                        Reason = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                        CorrectionDate = reader.GetDateTime(9),
                        IssueType = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                        IsApproved = reader.GetBoolean(11)
                    });
                }
            }
            catch (SqlException ex) when (ex.Message.Contains("Invalid object name 'DataCorrections'"))
            {
                // Table doesn't exist yet, return empty list
                return new List<DataCorrection>();
            }

            return corrections;
        }

        public void IgnoreIssue(ValidationIssue issue, string ignoredBy, string ignoreReason)
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                INSERT INTO IgnoredIssues 
                (EntryId, InstNo, PCName, IssueType, FieldName, CurrentValue, SuggestedValue, Reason, Severity, IgnoredBy, IgnoreReason, IgnoredDate)
                VALUES 
                (@EntryId, @InstNo, @PCName, @IssueType, @FieldName, @CurrentValue, @SuggestedValue, @Reason, @Severity, @IgnoredBy, @IgnoreReason, @IgnoredDate)");

            command.Parameters.AddWithValue("@EntryId", issue.EntryId);
            command.Parameters.AddWithValue("@InstNo", issue.InstNo);
            command.Parameters.AddWithValue("@PCName", issue.PCName);
            command.Parameters.AddWithValue("@IssueType", issue.IssueType);
            command.Parameters.AddWithValue("@FieldName", issue.FieldName);
            command.Parameters.AddWithValue("@CurrentValue", issue.CurrentValue);
            command.Parameters.AddWithValue("@SuggestedValue", issue.SuggestedValue);
            command.Parameters.AddWithValue("@Reason", issue.Reason);
            command.Parameters.AddWithValue("@Severity", issue.Severity);
            command.Parameters.AddWithValue("@IgnoredBy", ignoredBy);
            command.Parameters.AddWithValue("@IgnoreReason", ignoreReason);
            command.Parameters.AddWithValue("@IgnoredDate", DateTime.Now);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Handle the case where the table doesn't exist yet
                if (ex.Message.Contains("Invalid object name 'IgnoredIssues'"))
                {
                    CreateIgnoredIssuesTable();
                    // Retry the insert
                    connection.Close();
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                else
                {
                    throw;
                }
            }
        }

        public void CreateMissingTables()
        {
            try
            {
                if (!TableExists("IgnoredIssues"))
                {
                    CreateIgnoredIssuesTable();
                }

                if (!TableExists("SolvedIssues"))
                {
                    CreateSolvedIssuesTable();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating missing tables: {ex.Message}", ex);
            }
        }

        public void CreateIgnoredIssuesTable()
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                CREATE TABLE IgnoredIssues (
                    IgnoredIssueId INT IDENTITY(1,1) PRIMARY KEY,
                    EntryId INT NOT NULL,
                    InstNo INT NOT NULL,
                    PCName NVARCHAR(100),
                    IssueType NVARCHAR(100) NOT NULL,
                    FieldName NVARCHAR(100) NOT NULL,
                    CurrentValue NVARCHAR(500),
                    SuggestedValue NVARCHAR(500),
                    Reason NVARCHAR(500),
                    Severity NVARCHAR(20) NOT NULL,
                    IgnoredBy NVARCHAR(10) NOT NULL,
                    IgnoreReason NVARCHAR(500),
                    IgnoredDate DATETIME NOT NULL
                )");

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void CreateSolvedIssuesTable()
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                CREATE TABLE SolvedIssues (
                    SolvedIssueId INT IDENTITY(1,1) PRIMARY KEY,
                    EntryId INT NOT NULL,
                    InstNo INT NOT NULL,
                    PCName NVARCHAR(100),
                    IssueType NVARCHAR(100) NOT NULL,
                    FieldName NVARCHAR(100) NOT NULL,
                    CurrentValue NVARCHAR(500),
                    SuggestedValue NVARCHAR(500),
                    Reason NVARCHAR(500),
                    Severity NVARCHAR(20) NOT NULL,
                    SolvedBy NVARCHAR(10) NOT NULL,
                    SolutionMethod NVARCHAR(100),
                    SolutionNotes NVARCHAR(500),
                    SolvedDate DATETIME NOT NULL
                )");

            connection.Open();
            command.ExecuteNonQuery();
        }

        public List<IgnoredIssue> GetIgnoredIssues()
        {
            var ignoredIssues = new List<IgnoredIssue>();
            
            var query = @"
                SELECT IgnoredIssueId, EntryId, InstNo, PCName, IssueType, FieldName, CurrentValue, 
                       SuggestedValue, Reason, Severity, IgnoredBy, IgnoreReason, IgnoredDate
                FROM IgnoredIssues
                ORDER BY IgnoredDate DESC";

            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, query);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ignoredIssues.Add(new IgnoredIssue
                    {
                        IgnoredIssueId = reader.GetInt32(0),
                        EntryId = reader.GetInt32(1),
                        InstNo = reader.GetInt32(2),
                        PCName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        IssueType = reader.GetString(4),
                        FieldName = reader.GetString(5),
                        CurrentValue = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        SuggestedValue = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        Reason = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                        Severity = reader.GetString(9),
                        IgnoredBy = reader.GetString(10),
                        IgnoreReason = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                        IgnoredDate = reader.GetDateTime(12)
                    });
                }
            }
            catch (SqlException ex) when (ex.Message.Contains("Invalid object name 'IgnoredIssues'"))
            {
                // Table doesn't exist yet, return empty list
                return new List<IgnoredIssue>();
            }

            return ignoredIssues;
        }

        public void UnignoreIssue(int ignoredIssueId)
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                DELETE FROM IgnoredIssues WHERE IgnoredIssueId = @IgnoredIssueId");

            command.Parameters.AddWithValue("@IgnoredIssueId", ignoredIssueId);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException ex) when (ex.Message.Contains("Invalid object name 'IgnoredIssues'"))
            {
                // Table doesn't exist, nothing to delete
            }
        }

        public bool IsIssueIgnored(int instNo, string fieldName, string issueType)
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                SELECT COUNT(*) FROM IgnoredIssues 
                WHERE InstNo = @InstNo AND FieldName = @FieldName AND IssueType = @IssueType");

            command.Parameters.AddWithValue("@InstNo", instNo);
            command.Parameters.AddWithValue("@FieldName", fieldName);
            command.Parameters.AddWithValue("@IssueType", issueType);

            try
            {
                connection.Open();
                var count = (int)command.ExecuteScalar();
                return count > 0;
            }
            catch (SqlException ex) when (ex.Message.Contains("Invalid object name 'IgnoredIssues'"))
            {
                // Table doesn't exist yet
                return false;
            }
        }

        public void LogSolvedIssue(ValidationIssue issue, string solvedBy, string solutionMethod, string solutionNotes)
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                INSERT INTO SolvedIssues 
                (EntryId, InstNo, PCName, IssueType, FieldName, CurrentValue, SuggestedValue, Reason, Severity, SolvedBy, SolutionMethod, SolutionNotes, SolvedDate)
                VALUES 
                (@EntryId, @InstNo, @PCName, @IssueType, @FieldName, @CurrentValue, @SuggestedValue, @Reason, @Severity, @SolvedBy, @SolutionMethod, @SolutionNotes, @SolvedDate)");

            command.Parameters.AddWithValue("@EntryId", issue.EntryId);
            command.Parameters.AddWithValue("@InstNo", issue.InstNo);
            command.Parameters.AddWithValue("@PCName", issue.PCName);
            command.Parameters.AddWithValue("@IssueType", issue.IssueType);
            command.Parameters.AddWithValue("@FieldName", issue.FieldName);
            command.Parameters.AddWithValue("@CurrentValue", issue.CurrentValue);
            command.Parameters.AddWithValue("@SuggestedValue", issue.SuggestedValue);
            command.Parameters.AddWithValue("@Reason", issue.Reason);
            command.Parameters.AddWithValue("@Severity", issue.Severity);
            command.Parameters.AddWithValue("@SolvedBy", solvedBy);
            command.Parameters.AddWithValue("@SolutionMethod", solutionMethod);
            command.Parameters.AddWithValue("@SolutionNotes", solutionNotes);
            command.Parameters.AddWithValue("@SolvedDate", DateTime.Now);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Handle the case where the table doesn't exist yet
                if (ex.Message.Contains("Invalid object name 'SolvedIssues'"))
                {
                    CreateSolvedIssuesTable();
                    // Retry the insert
                    connection.Close();
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                else
                {
                    throw;
                }
            }
        }

        public List<SolvedIssue> GetSolvedIssues()
        {
            var solvedIssues = new List<SolvedIssue>();
            
            var query = @"
                SELECT SolvedIssueId, EntryId, InstNo, PCName, IssueType, FieldName, CurrentValue, 
                       SuggestedValue, Reason, Severity, SolvedBy, SolutionMethod, SolutionNotes, SolvedDate
                FROM SolvedIssues
                ORDER BY SolvedDate DESC";

            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, query);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    solvedIssues.Add(new SolvedIssue
                    {
                        SolvedIssueId = reader.GetInt32(0),
                        EntryId = reader.GetInt32(1),
                        InstNo = reader.GetInt32(2),
                        PCName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        IssueType = reader.GetString(4),
                        FieldName = reader.GetString(5),
                        CurrentValue = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        SuggestedValue = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        Reason = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                        Severity = reader.GetString(9),
                        SolvedBy = reader.GetString(10),
                        SolutionMethod = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                        SolutionNotes = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                        SolvedDate = reader.GetDateTime(13)
                    });
                }
            }
            catch (SqlException ex) when (ex.Message.Contains("Invalid object name 'SolvedIssues'"))
            {
                // Table doesn't exist yet, return empty list
                return new List<SolvedIssue>();
            }

            return solvedIssues;
        }

        public bool IsIssueSolved(int instNo, string fieldName, string issueType)
        {
            using var connection = _dbHelper.GetConnection();
            using var command = _dbHelper.CreateCommand(connection, @"
                SELECT COUNT(*) FROM SolvedIssues 
                WHERE InstNo = @InstNo AND FieldName = @FieldName AND IssueType = @IssueType");

            command.Parameters.AddWithValue("@InstNo", instNo);
            command.Parameters.AddWithValue("@FieldName", fieldName);
            command.Parameters.AddWithValue("@IssueType", issueType);

            try
            {
                connection.Open();
                var count = (int)command.ExecuteScalar();
                return count > 0;
            }
            catch (SqlException ex) when (ex.Message.Contains("Invalid object name 'SolvedIssues'"))
            {
                // Table doesn't exist yet
                return false;
            }
        }

        /// <summary>
        /// Validates equipment data before saving and returns any potential issues
        /// </summary>
        /// <param name="equipment">The equipment data to validate</param>
        /// <returns>List of validation issues that would be created</returns>
        public List<ValidationIssue> ValidateEquipmentBeforeSaving(EquipmentData equipment)
        {
            return ValidateEquipment(equipment);
        }

        /// <summary>
        /// Gets a user-friendly summary of validation issues for display in dialogs
        /// </summary>
        /// <param name="issues">List of validation issues</param>
        /// <returns>Formatted string summary of issues with proper line breaks</returns>
        public string GetIssuesSummary(List<ValidationIssue> issues)
        {
            if (!issues.Any())
                return string.Empty;

            var summary = new List<string>();
            
            foreach (var issue in issues)
            {
                var severity = issue.Severity switch
                {
                    "High" => "⚠️ HIGH",
                    "Medium" => "⚡ MEDIUM",
                    "Low" => "ℹ️ LOW",
                    _ => "❓ UNKNOWN"
                };

                summary.Add($"• {severity}: {issue.IssueType}");
                summary.Add($"  Reason: {issue.Reason}");
                
                if (!string.IsNullOrEmpty(issue.SuggestedValue))
                {
                    summary.Add($"  Suggested: {issue.SuggestedValue}");
                }
                
                summary.Add(""); // Add blank line between issues
            }

            return string.Join(Environment.NewLine, summary);
        }
    }
}

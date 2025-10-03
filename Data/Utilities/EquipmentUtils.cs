using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Microsoft.Data.SqlClient;
using SusEquip.Data.Models;
using SusEquip.Data.Services;
using SusEquip.Data.Interfaces.Services;
using System.Threading.Tasks;

namespace SusEquip.Data.Utilities
{
    public static class EquipmentUtils
    {
        private static ICookieService? _cookieService;

        public static void Initialize(ICookieService cookieService)
        {
            _cookieService = cookieService;
        }

        public static bool ValidateEquipmentData(EquipmentData equipmentData)
        {
            // Example validation logic
            if (string.IsNullOrEmpty(equipmentData.Entry_Date) ||
                string.IsNullOrEmpty(equipmentData.Creator_Initials) ||
                string.IsNullOrEmpty(equipmentData.App_Owner) ||
                string.IsNullOrEmpty(equipmentData.Status) ||
                string.IsNullOrEmpty(equipmentData.Serial_No))
            {
                return false;
            }
            return true;
        }

        public static byte[] ExportTableToExcel(string connectionString, string? folderPath = "\\Sus-pequip01\\EQ_DB\\Equip_Expo")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT * FROM Equip", connection))
                using (var reader = command.ExecuteReader())
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Equipment");
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = reader.GetName(i);
                    }

                    int row = 2;
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            worksheet.Cells[row, i + 1].Value = reader.GetValue(i);
                        }
                        row++;
                    }

                    // Save a copy to the specified folder if path is provided and accessible
                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        try
                        {
                            // Ensure directory exists
                            Directory.CreateDirectory(folderPath);
                            var filePath = Path.Combine(folderPath, $"EquipmentData-{CurrentDayDisplay()}.xlsx");
                            var file = new FileInfo(filePath);
                            package.SaveAs(file);
                        }
                        catch (Exception)
                        {
                            // Continue anyway - the browser download will still work
                        }
                    }

                    // Return the file as a byte array for download
                    return package.GetAsByteArray();
                }
            }
        }

        public static byte[] ExportDTUPCLogToExcel(List<DTUPC_Log> logEntries, string? folderPath = "\\Sus-pequip01\\EQ_DB\\DTUPC_Log")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("DTUPC Log");

                // Add headers
                worksheet.Cells[1, 1].Value = "LogId";
                worksheet.Cells[1, 2].Value = "EntryDate";
                worksheet.Cells[1, 3].Value = "CreatorInitials";
                worksheet.Cells[1, 4].Value = "PCName";
                worksheet.Cells[1, 5].Value = "MacAddress1";
                worksheet.Cells[1, 6].Value = "SerialNo";
                worksheet.Cells[1, 7].Value = "UUID";

                // Add data
                for (int i = 0; i < logEntries.Count; i++)
                {
                    var logEntry = logEntries[i];
                    worksheet.Cells[i + 2, 1].Value = logEntry.LogId;
                    worksheet.Cells[i + 2, 2].Value = logEntry.EntryDate;
                    worksheet.Cells[i + 2, 3].Value = logEntry.CreatorInitials;
                    worksheet.Cells[i + 2, 4].Value = logEntry.PCName;
                    worksheet.Cells[i + 2, 5].Value = logEntry.MacAddress1;
                    worksheet.Cells[i + 2, 6].Value = logEntry.SerialNo;
                    worksheet.Cells[i + 2, 7].Value = logEntry.UUID;
                }

                // Save a copy to the specified folder if path is provided and accessible
                if (!string.IsNullOrEmpty(folderPath))
                {
                    try
                    {
                        // Ensure directory exists
                        Directory.CreateDirectory(folderPath);
                        var filePath = Path.Combine(folderPath, $"DTUPC_Log-{CurrentDayDisplay()}.xlsx");
                        var file = new FileInfo(filePath);
                        package.SaveAs(file);
                    }
                    catch (Exception)
                    {
                        // Continue anyway - the browser download will still work
                    }
                }

                // Return the file as a byte array for download
                return package.GetAsByteArray();
            }
        }

        public static string CurrentDayDisplay()
        {
            DateTime date = DateTime.Now;
            return date.ToString("dd-MMMM-yyyy_kl-HHmm");
        }

        public static async Task<string> GetCreatorInitialsAsync()
        {
            return _cookieService != null ? await _cookieService.GetCookieAsync("Creator_Initials") : string.Empty;
        }

        /// <summary>
        /// Automatically determines the machine type based on PC_Name and Status
        /// </summary>
        /// <param name="pcName">The PC name to parse</param>
        /// <param name="status">The equipment status</param>
        /// <returns>The determined machine type, "Error" if naming convention not met, or "General" if cannot be determined</returns>
        public static string DetermineMachineType(string pcName, string status)
        {
            // If PC_Name is "N/A", return "Waiting for PC Name"
            if (!string.IsNullOrWhiteSpace(pcName) && pcName.Trim().Equals("N/A", StringComparison.OrdinalIgnoreCase))
            {
                return "Waiting for PC Name";
            }
            
            // If PC_Name is null, empty, or whitespace, return based on status
            if (string.IsNullOrWhiteSpace(pcName))
            {
                return DetermineMachineTypeFromStatus(status);
            }

            // Parse PC name to extract machine type code
            string machineTypeCode = ExtractMachineTypeFromPcName(pcName);
            
            if (string.IsNullOrEmpty(machineTypeCode))
            {
                // If PC name doesn't match naming convention and it's not N/A, return Error
                return "Error";
            }

            return ConvertMachineTypeCodeToFullName(machineTypeCode);
        }

        /// <summary>
        /// Extracts machine type code from PC name based on naming conventions
        /// </summary>
        /// <param name="pcName">The PC name to parse</param>
        /// <returns>Machine type code (e.g., "EL", "ED", etc.) or empty string if not found</returns>
        private static string ExtractMachineTypeFromPcName(string pcName)
        {
            if (string.IsNullOrWhiteSpace(pcName))
                return string.Empty;

            // Split by dash to get parts
            string[] parts = pcName.Split('-');
            
            if (parts.Length < 2)
                return string.Empty;

            // Second part should contain the machine type
            string secondPart = parts[1].Trim().ToUpper();
            
            // Check for pattern like "EL-MPARK1" (department-machinetype-username+number)
            if (secondPart.Length >= 2)
            {
                string possibleMachineType = secondPart.Substring(0, 2);
                if (IsValidMachineTypeCode(possibleMachineType))
                {
                    return possibleMachineType;
                }
            }

            // Check for pattern like "EL1MPARK" (department-machinetype+number+username)
            if (secondPart.Length >= 3)
            {
                string possibleMachineType = secondPart.Substring(0, 2);
                if (IsValidMachineTypeCode(possibleMachineType) && char.IsDigit(secondPart[2]))
                {
                    return possibleMachineType;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Checks if the given code is a valid machine type code
        /// </summary>
        /// <param name="code">The code to validate</param>
        /// <returns>True if valid machine type code</returns>
        private static bool IsValidMachineTypeCode(string code)
        {
            var validCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "EL", "ED", "EX", "EM", "SX", "LL", "LD", "LX", "GL", "GD", "GM", "GX"
            };
            
            return validCodes.Contains(code);
        }

        /// <summary>
        /// Converts machine type code to full machine type name
        /// </summary>
        /// <param name="code">Machine type code</param>
        /// <returns>Full machine type name</returns>
        private static string ConvertMachineTypeCodeToFullName(string code)
        {
            return code.ToUpper() switch
            {
                "EL" => "Employee Laptop",
                "ED" => "Employee Desktop",
                "EX" => "Employee Linux",
                "EM" => "Employee Mac(Apple)",
                "SX" => "Student",
                "LL" => "LAB",
                "LD" => "LAB",
                "LX" => "LAB",
                "GL" => "General",
                "GD" => "General",
                "GM" => "General",
                "GX" => "General",
                _ => "General"
            };
        }

        /// <summary>
        /// Determines machine type based on status when PC name doesn't provide clear indication
        /// </summary>
        /// <param name="status">Equipment status</param>
        /// <returns>Machine type based on status</returns>
        private static string DetermineMachineTypeFromStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "General";

            return status.ToLower() switch
            {
                "modtaget (ny)" => "General",
                "på lager (brugt)" => "General",
                "i bur/kasse" => "General",
                "karantæne" => "General",
                "afhentet af refurb" => "General",
                _ => "General"
            };
        }
    }
}

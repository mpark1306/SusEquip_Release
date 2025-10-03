using SusEquip.Data.Models;

namespace SusEquip.Tests.Infrastructure
{
    /// <summary>
    /// Provides test data fixtures and scenarios for integration testing
    /// </summary>
    public static class TestDataFixtures
    {
        /// <summary>
        /// Creates a valid EquipmentData instance for testing
        /// </summary>
        public static EquipmentData CreateValidEquipmentData(int instNo = 1001, string suffix = "")
        {
            return new EquipmentData
            {
                Entry_Date = DateTime.Now.ToString("yyyy-MM-dd"),
                Inst_No = instNo,
                Creator_Initials = "TEST",
                App_Owner = $"Test Owner{suffix}",
                Status = "Active",
                Serial_No = $"SN{instNo:D6}{suffix}",
                Mac_Address1 = $"00:11:22:33:44:{instNo % 256:X2}",
                Mac_Address2 = $"66:77:88:99:AA:{instNo % 256:X2}",
                UUID = Guid.NewGuid().ToString(),
                Product_No = $"P{instNo:D3}",
                Model_Name_and_No = $"TestModel-{instNo}{suffix}",
                Department = "IT Test",
                PC_Name = $"TEST-PC-{instNo:D4}{suffix}",
                Service_Start = DateTime.Now.ToString("yyyy-MM-dd"),
                Service_Ends = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd"),
                Note = $"Test equipment {instNo}{suffix}",
                MachineType = "Test Laptop"
            };
        }

        /// <summary>
        /// Creates multiple equipment data instances for bulk testing
        /// </summary>
        public static List<EquipmentData> CreateMultipleEquipmentData(int count, int startingInstNo = 2000)
        {
            var equipmentList = new List<EquipmentData>();
            for (int i = 0; i < count; i++)
            {
                equipmentList.Add(CreateValidEquipmentData(startingInstNo + i, $"-Bulk{i}"));
            }
            return equipmentList;
        }

        /// <summary>
        /// Creates equipment data with invalid properties for error testing
        /// </summary>
        public static EquipmentData CreateInvalidEquipmentData()
        {
            return new EquipmentData
            {
                Entry_Date = "", // Invalid empty date
                Inst_No = -1,    // Invalid negative InstNo
                Creator_Initials = "",
                App_Owner = "",
                Status = "",
                Serial_No = "",
                Mac_Address1 = "invalid-mac",
                Mac_Address2 = "invalid-mac",
                UUID = "invalid-uuid",
                Product_No = "",
                Model_Name_and_No = "",
                Department = "",
                PC_Name = "", // Invalid empty PC name
                Service_Start = "invalid-date",
                Service_Ends = "invalid-date",
                Note = "",
                MachineType = ""
            };
        }

        /// <summary>
        /// Creates equipment data for stress testing scenarios
        /// </summary>
        public static EquipmentData CreateStressTestEquipmentData(int instNo)
        {
            return new EquipmentData
            {
                Entry_Date = DateTime.Now.ToString("yyyy-MM-dd"),
                Inst_No = instNo,
                Creator_Initials = "STRESS",
                App_Owner = new string('A', 100), // Long string
                Status = "Stress Test",
                Serial_No = $"STRESS-{instNo}-{Guid.NewGuid():N}",
                Mac_Address1 = $"00:11:22:33:44:{instNo % 256:X2}",
                Mac_Address2 = $"66:77:88:99:AA:{instNo % 256:X2}",
                UUID = Guid.NewGuid().ToString(),
                Product_No = $"STRESS-{instNo}",
                Model_Name_and_No = $"StressModel-{instNo}-{new string('X', 50)}",
                Department = "Stress Test Department",
                PC_Name = $"STRESS-PC-{instNo}-{DateTime.Now.Ticks}",
                Service_Start = DateTime.Now.ToString("yyyy-MM-dd"),
                Service_Ends = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd"),
                Note = $"Stress test equipment {instNo} - {new string('N', 200)}",
                MachineType = "Stress Test Machine"
            };
        }

        /// <summary>
        /// Creates equipment data with edge case values
        /// </summary>
        public static EquipmentData CreateEdgeCaseEquipmentData()
        {
            return new EquipmentData
            {
                Entry_Date = DateTime.MaxValue.ToString("yyyy-MM-dd"),
                Inst_No = int.MaxValue,
                Creator_Initials = "EDGE",
                App_Owner = "Edge Case Owner",
                Status = "Edge",
                Serial_No = new string('9', 50),
                Mac_Address1 = "FF:FF:FF:FF:FF:FF",
                Mac_Address2 = "00:00:00:00:00:00",
                UUID = new Guid().ToString(),
                Product_No = new string('Z', 20),
                Model_Name_and_No = new string('M', 100),
                Department = new string('D', 100),
                PC_Name = new string('P', 100),
                Service_Start = DateTime.MinValue.ToString("yyyy-MM-dd"),
                Service_Ends = DateTime.MaxValue.ToString("yyyy-MM-dd"),
                Note = new string('N', 500),
                MachineType = new string('T', 50)
            };
        }
    }

    /// <summary>
    /// Provides test scenarios for error handling and fault tolerance testing
    /// </summary>
    public static class TestScenarios
    {
        /// <summary>
        /// Scenario for testing circuit breaker failure threshold
        /// </summary>
        public static class CircuitBreakerScenarios
        {
            public const int FailureThreshold = 5;
            public const int TimeoutMilliseconds = 1000;
            public const int RecoveryTimeoutMilliseconds = 5000;
        }

        /// <summary>
        /// Scenario for testing retry policies
        /// </summary>
        public static class RetryScenarios
        {
            public const int MaxRetryAttempts = 3;
            public static readonly TimeSpan InitialDelay = TimeSpan.FromMilliseconds(100);
            public static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Scenario for testing compensation patterns
        /// </summary>
        public static class CompensationScenarios
        {
            public const int MultiStepOperationCount = 5;
            public const int FailAtStep = 3; // Fail at step 3 to test compensation
        }

        /// <summary>
        /// Performance testing thresholds
        /// </summary>
        public static class PerformanceThresholds
        {
            public const int MaxOperationTimeMs = 1000;
            public const int BulkOperationCount = 100;
            public const int ConcurrentOperationCount = 10;
            public const int StressTestDurationMs = 30000; // 30 seconds
        }
    }
}
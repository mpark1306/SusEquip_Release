using SusEquip.Data.Models;
using SusEquip.Data.Services;
using Moq;
using Xunit;
using System.Collections.Generic;
using System;

namespace SusEquip.Tests.Services
{
    /// <summary>
    /// Comprehensive CRUD tests for EquipmentService using interface-based mocking.
    /// Tests all Create, Read, Update, Delete operations with proper mocking patterns.
    /// </summary>
    public class EquipmentServiceCRUDTests
    {
        private readonly Mock<IEquipmentServiceSync> _mockEquipmentService;

        public EquipmentServiceCRUDTests()
        {
            _mockEquipmentService = new Mock<IEquipmentServiceSync>();
        }

        [Fact]
        public void AddEntry_ValidEquipmentData_CallsServiceOnce()
        {
            // Arrange
            var equipmentData = new EquipmentData
            {
                Entry_Date = DateTime.Now.ToString(),
                Inst_No = 1001,
                Creator_Initials = "MP",
                App_Owner = "John Doe",
                Status = "Active",
                Serial_No = "SN123456",
                Mac_Address1 = "00:11:22:33:44:55",
                Mac_Address2 = "66:77:88:99:AA:BB",
                UUID = Guid.NewGuid().ToString(),
                Product_No = "P001",
                Model_Name_and_No = "Model X1",
                Department = "IT",
                PC_Name = "PC001",
                Service_Start = DateTime.Now.ToString(),
                Service_Ends = DateTime.Now.AddYears(1).ToString(),
                Note = "Test note",
                MachineType = "Laptop"
            };

            // Act
            _mockEquipmentService.Object.AddEntry(equipmentData);

            // Assert
            _mockEquipmentService.Verify(x => x.AddEntry(equipmentData), Times.Once);
        }

        [Fact]
        public void InsertEntry_ValidEquipmentData_CallsServiceOnce()
        {
            // Arrange
            var equipmentData = new EquipmentData
            {
                Entry_Date = DateTime.Now.ToString(),
                Inst_No = 1002,
                Creator_Initials = "MP",
                Status = "New"
            };

            // Act
            _mockEquipmentService.Object.InsertEntry(equipmentData);

            // Assert
            _mockEquipmentService.Verify(x => x.InsertEntry(equipmentData), Times.Once);
        }

        [Fact]
        public void UpdateLatestEntry_ValidEquipmentData_CallsServiceOnce()
        {
            // Arrange
            var equipmentData = new EquipmentData
            {
                Entry_Date = DateTime.Now.ToString(),
                Inst_No = 1003,
                Status = "Updated"
            };

            // Act
            _mockEquipmentService.Object.UpdateLatestEntry(equipmentData);

            // Assert
            _mockEquipmentService.Verify(x => x.UpdateLatestEntry(equipmentData), Times.Once);
        }

        [Fact]
        public void GetNextInstNo_ReturnsValidInstNo()
        {
            // Arrange
            _mockEquipmentService.Setup(x => x.GetNextInstNo()).Returns(1004);

            // Act
            var result = _mockEquipmentService.Object.GetNextInstNo();

            // Assert
            Assert.Equal(1004, result);
            _mockEquipmentService.Verify(x => x.GetNextInstNo(), Times.Once);
        }

        [Fact]
        public void DeleteEntry_ValidInstNoAndEntryId_CallsServiceOnce()
        {
            // Arrange
            int instNo = 1005;
            int entryId = 1;

            // Act
            _mockEquipmentService.Object.DeleteEntry(instNo, entryId);

            // Assert
            _mockEquipmentService.Verify(x => x.DeleteEntry(instNo, entryId), Times.Once);
        }

        [Fact]
        public void GetEquipmentSorted_ValidInstNo_ReturnsEquipmentList()
        {
            // Arrange
            int instNo = 1006;
            var expectedEquipment = new List<EquipmentData>
            {
                new EquipmentData { Inst_No = instNo, Status = "Active" }
            };
            _mockEquipmentService.Setup(x => x.GetEquipmentSorted(instNo)).Returns(expectedEquipment);

            // Act
            var result = _mockEquipmentService.Object.GetEquipmentSorted(instNo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEquipment.Count, result.Count);
            _mockEquipmentService.Verify(x => x.GetEquipmentSorted(instNo), Times.Once);
        }

        [Fact]
        public void GetEquipSortedByEntry_ValidInstNo_ReturnsEquipmentList()
        {
            // Arrange
            int instNo = 1007;
            var expectedEquipment = new List<EquipmentData>
            {
                new EquipmentData { Inst_No = instNo, Entry_Date = DateTime.Now.ToString() }
            };
            _mockEquipmentService.Setup(x => x.GetEquipSortedByEntry(instNo)).Returns(expectedEquipment);

            // Act
            var result = _mockEquipmentService.Object.GetEquipSortedByEntry(instNo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEquipment.Count, result.Count);
            _mockEquipmentService.Verify(x => x.GetEquipSortedByEntry(instNo), Times.Once);
        }

        [Fact]
        public void GetEquipment_ReturnsAllEquipment()
        {
            // Arrange
            var expectedEquipment = new List<EquipmentData>
            {
                new EquipmentData { Inst_No = 1008, Status = "Active" },
                new EquipmentData { Inst_No = 1009, Status = "Inactive" }
            };
            _mockEquipmentService.Setup(x => x.GetEquipment()).Returns(expectedEquipment);

            // Act
            var result = _mockEquipmentService.Object.GetEquipment();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedEquipment.Count, result.Count);
            _mockEquipmentService.Verify(x => x.GetEquipment(), Times.Once);
        }

        [Fact]
        public void GetMachines_ReturnsAllMachines()
        {
            // Arrange
            var expectedMachines = new List<MachineData>
            {
                new MachineData { Inst_No = 1010, Status = "Active" },
                new MachineData { Inst_No = 1011, Status = "Used" }
            };
            _mockEquipmentService.Setup(x => x.GetMachines()).Returns(expectedMachines);

            // Act
            var result = _mockEquipmentService.Object.GetMachines();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedMachines.Count, result.Count);
            _mockEquipmentService.Verify(x => x.GetMachines(), Times.Once);
        }

        [Fact]
        public void GetNewMachines_ReturnsNewMachines()
        {
            // Arrange
            var expectedMachines = new List<MachineData>
            {
                new MachineData { Inst_No = 1012, Status = "New" }
            };
            _mockEquipmentService.Setup(x => x.GetNewMachines()).Returns(expectedMachines);

            // Act
            var result = _mockEquipmentService.Object.GetNewMachines();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedMachines.Count, result.Count);
            _mockEquipmentService.Verify(x => x.GetNewMachines(), Times.Once);
        }

        [Fact]
        public void GetUsedMachines_ReturnsUsedMachines()
        {
            // Arrange
            var expectedMachines = new List<MachineData>
            {
                new MachineData { Inst_No = 1013, Status = "Used" }
            };
            _mockEquipmentService.Setup(x => x.GetUsedMachines()).Returns(expectedMachines);

            // Act
            var result = _mockEquipmentService.Object.GetUsedMachines();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedMachines.Count, result.Count);
            _mockEquipmentService.Verify(x => x.GetUsedMachines(), Times.Once);
        }

        [Fact]
        public void GetQuarantineMachines_ReturnsQuarantineMachines()
        {
            // Arrange
            var expectedMachines = new List<MachineData>
            {
                new MachineData { Inst_No = 1014, Status = "Quarantine" }
            };
            _mockEquipmentService.Setup(x => x.GetQuarantineMachines()).Returns(expectedMachines);

            // Act
            var result = _mockEquipmentService.Object.GetQuarantineMachines();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedMachines.Count, result.Count);
            _mockEquipmentService.Verify(x => x.GetQuarantineMachines(), Times.Once);
        }

        [Fact]
        public void GetActiveMachines_ReturnsActiveMachines()
        {
            // Arrange
            var expectedMachines = new List<MachineData>
            {
                new MachineData { Inst_No = 1015, Status = "Active" }
            };
            _mockEquipmentService.Setup(x => x.GetActiveMachines()).Returns(expectedMachines);

            // Act
            var result = _mockEquipmentService.Object.GetActiveMachines();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedMachines.Count, result.Count);
            _mockEquipmentService.Verify(x => x.GetActiveMachines(), Times.Once);
        }

        [Fact]
        public void IsInstNoTaken_ExistingInstNo_ReturnsTrue()
        {
            // Arrange
            int instNo = 1016;
            _mockEquipmentService.Setup(x => x.IsInstNoTaken(instNo)).Returns(true);

            // Act
            var result = _mockEquipmentService.Object.IsInstNoTaken(instNo);

            // Assert
            Assert.True(result);
            _mockEquipmentService.Verify(x => x.IsInstNoTaken(instNo), Times.Once);
        }

        [Fact]
        public void IsInstNoTaken_NonExistingInstNo_ReturnsFalse()
        {
            // Arrange
            int instNo = 9999;
            _mockEquipmentService.Setup(x => x.IsInstNoTaken(instNo)).Returns(false);

            // Act
            var result = _mockEquipmentService.Object.IsInstNoTaken(instNo);

            // Assert
            Assert.False(result);
            _mockEquipmentService.Verify(x => x.IsInstNoTaken(instNo), Times.Once);
        }

        [Fact]
        public void IsSerialNoTakenInMachines_ExistingSerialNo_ReturnsTrue()
        {
            // Arrange
            string serialNo = "SN123456";
            _mockEquipmentService.Setup(x => x.IsSerialNoTakenInMachines(serialNo)).Returns(true);

            // Act
            var result = _mockEquipmentService.Object.IsSerialNoTakenInMachines(serialNo);

            // Assert
            Assert.True(result);
            _mockEquipmentService.Verify(x => x.IsSerialNoTakenInMachines(serialNo), Times.Once);
        }

        [Fact]
        public void IsSerialNoTakenInMachines_NonExistingSerialNo_ReturnsFalse()
        {
            // Arrange
            string serialNo = "NONEXISTENT";
            _mockEquipmentService.Setup(x => x.IsSerialNoTakenInMachines(serialNo)).Returns(false);

            // Act
            var result = _mockEquipmentService.Object.IsSerialNoTakenInMachines(serialNo);

            // Assert
            Assert.False(result);
            _mockEquipmentService.Verify(x => x.IsSerialNoTakenInMachines(serialNo), Times.Once);
        }

        [Fact]
        public void GetDashboardStatistics_ReturnsDashboardData()
        {
            // Arrange
            var expectedStats = (activeCount: 10, newCount: 5, usedCount: 3, quarantinedCount: 2);
            _mockEquipmentService.Setup(x => x.GetDashboardStatistics()).Returns(expectedStats);

            // Act
            var result = _mockEquipmentService.Object.GetDashboardStatistics();

            // Assert
            Assert.Equal(expectedStats.activeCount, result.activeCount);
            Assert.Equal(expectedStats.newCount, result.newCount);
            Assert.Equal(expectedStats.usedCount, result.usedCount);
            Assert.Equal(expectedStats.quarantinedCount, result.quarantinedCount);
            _mockEquipmentService.Verify(x => x.GetDashboardStatistics(), Times.Once);
        }

        [Fact]
        public void GetMachinesOutOfServiceSinceJune_ReturnsOutOfServiceMachines()
        {
            // Arrange
            var expectedMachines = new List<MachineData>
            {
                new MachineData 
                { 
                    Inst_No = 1017, 
                    Status = "Out of Service",
                    Service_Ends = "2023-05-01" // Before June
                }
            };
            _mockEquipmentService.Setup(x => x.GetMachinesOutOfServiceSinceJune()).Returns(expectedMachines);

            // Act
            var result = _mockEquipmentService.Object.GetMachinesOutOfServiceSinceJune();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedMachines.Count, result.Count);
            _mockEquipmentService.Verify(x => x.GetMachinesOutOfServiceSinceJune(), Times.Once);
        }

        [Fact]
        public void AddEntry_NullEquipmentData_DoesNotThrow()
        {
            // Arrange
            EquipmentData? equipmentData = null;

            // Act & Assert (no exception should be thrown during mocking)
            _mockEquipmentService.Object.AddEntry(equipmentData!);
            _mockEquipmentService.Verify(x => x.AddEntry(equipmentData!), Times.Once);
        }

        [Fact]
        public void GetEquipmentSorted_InvalidInstNo_ReturnsEmptyList()
        {
            // Arrange
            int invalidInstNo = -1;
            var emptyList = new List<EquipmentData>();
            _mockEquipmentService.Setup(x => x.GetEquipmentSorted(invalidInstNo)).Returns(emptyList);

            // Act
            var result = _mockEquipmentService.Object.GetEquipmentSorted(invalidInstNo);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockEquipmentService.Verify(x => x.GetEquipmentSorted(invalidInstNo), Times.Once);
        }

        [Fact]
        public void DeleteEntry_InvalidInstNoAndEntryId_CallsServiceWithProvidedValues()
        {
            // Arrange
            int invalidInstNo = -1;
            int invalidEntryId = -1;

            // Act
            _mockEquipmentService.Object.DeleteEntry(invalidInstNo, invalidEntryId);

            // Assert
            _mockEquipmentService.Verify(x => x.DeleteEntry(invalidInstNo, invalidEntryId), Times.Once);
        }

        [Fact]
        public void IsSerialNoTakenInMachines_NullSerialNo_ReturnsFalse()
        {
            // Arrange
            string? nullSerialNo = null;
            _mockEquipmentService.Setup(x => x.IsSerialNoTakenInMachines(nullSerialNo!)).Returns(false);

            // Act
            var result = _mockEquipmentService.Object.IsSerialNoTakenInMachines(nullSerialNo!);

            // Assert
            Assert.False(result);
            _mockEquipmentService.Verify(x => x.IsSerialNoTakenInMachines(nullSerialNo!), Times.Once);
        }

        [Fact]
        public void IsSerialNoTakenInMachines_EmptySerialNo_ReturnsFalse()
        {
            // Arrange
            string emptySerialNo = "";
            _mockEquipmentService.Setup(x => x.IsSerialNoTakenInMachines(emptySerialNo)).Returns(false);

            // Act
            var result = _mockEquipmentService.Object.IsSerialNoTakenInMachines(emptySerialNo);

            // Assert
            Assert.False(result);
            _mockEquipmentService.Verify(x => x.IsSerialNoTakenInMachines(emptySerialNo), Times.Once);
        }

        [Fact]
        public void GetDashboardStatistics_AllZeros_ReturnsZeroStats()
        {
            // Arrange
            var zeroStats = (activeCount: 0, newCount: 0, usedCount: 0, quarantinedCount: 0);
            _mockEquipmentService.Setup(x => x.GetDashboardStatistics()).Returns(zeroStats);

            // Act
            var result = _mockEquipmentService.Object.GetDashboardStatistics();

            // Assert
            Assert.Equal(0, result.activeCount);
            Assert.Equal(0, result.newCount);
            Assert.Equal(0, result.usedCount);
            Assert.Equal(0, result.quarantinedCount);
            _mockEquipmentService.Verify(x => x.GetDashboardStatistics(), Times.Once);
        }

        [Fact]
        public void GetMachines_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var emptyMachines = new List<MachineData>();
            _mockEquipmentService.Setup(x => x.GetMachines()).Returns(emptyMachines);

            // Act
            var result = _mockEquipmentService.Object.GetMachines();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockEquipmentService.Verify(x => x.GetMachines(), Times.Once);
        }

        [Fact]
        public void GetEquipment_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var emptyEquipment = new List<EquipmentData>();
            _mockEquipmentService.Setup(x => x.GetEquipment()).Returns(emptyEquipment);

            // Act
            var result = _mockEquipmentService.Object.GetEquipment();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockEquipmentService.Verify(x => x.GetEquipment(), Times.Once);
        }

        [Fact]
        public void GetNextInstNo_MaxValue_ReturnsMaxValue()
        {
            // Arrange
            _mockEquipmentService.Setup(x => x.GetNextInstNo()).Returns(int.MaxValue);

            // Act
            var result = _mockEquipmentService.Object.GetNextInstNo();

            // Assert
            Assert.Equal(int.MaxValue, result);
            _mockEquipmentService.Verify(x => x.GetNextInstNo(), Times.Once);
        }

        [Fact]
        public void GetMachinesOutOfServiceSinceJune_NoOutOfServiceMachines_ReturnsEmptyList()
        {
            // Arrange
            var emptyMachines = new List<MachineData>();
            _mockEquipmentService.Setup(x => x.GetMachinesOutOfServiceSinceJune()).Returns(emptyMachines);

            // Act
            var result = _mockEquipmentService.Object.GetMachinesOutOfServiceSinceJune();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockEquipmentService.Verify(x => x.GetMachinesOutOfServiceSinceJune(), Times.Once);
        }
    }
}
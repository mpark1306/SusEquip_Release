using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SusEquip.Data.Models;
using SusEquip.Data.Services;
using SusEquip.Data.Services.Decorators;
using Xunit;

namespace SusEquip.Tests.Services.Decorators
{
    public class SyncCachingEquipmentServiceTests : IDisposable
    {
        private readonly Mock<IEquipmentServiceSync> _mockService;
        private readonly Mock<ILogger<CachingEquipmentService>> _mockLogger;
        private readonly IMemoryCache _cache;
        private readonly CachingEquipmentService _cachingService;

        public SyncCachingEquipmentServiceTests()
        {
            _mockService = new Mock<IEquipmentServiceSync>();
            _mockLogger = new Mock<ILogger<CachingEquipmentService>>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _cachingService = new CachingEquipmentService(_mockService.Object, _cache, _mockLogger.Object);
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }

        [Fact]
        public void GetEquipment_FirstCall_FetchesFromServiceAndCaches()
        {
            // Arrange
            var equipment = new List<EquipmentData>
            {
                new EquipmentData { EntryId = 1, PC_Name = "Test Equipment", Serial_No = "12345" }
            };
            
            _mockService.Setup(s => s.GetEquipment()).Returns(equipment);

            // Act
            var result = _cachingService.GetEquipment();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].EntryId);
            Assert.Equal("Test Equipment", result[0].PC_Name);
            _mockService.Verify(s => s.GetEquipment(), Times.Once);
        }

        [Fact]
        public void GetMachines_FirstCall_FetchesFromServiceAndCaches()
        {
            // Arrange
            var machines = new List<MachineData>
            {
                new MachineData { EntryId = 1, PC_Name = "Test Machine", Inst_No = 100 }
            };
            
            _mockService.Setup(s => s.GetMachines()).Returns(machines);

            // Act
            var result = _cachingService.GetMachines();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].EntryId);
            Assert.Equal("Test Machine", result[0].PC_Name);
            _mockService.Verify(s => s.GetMachines(), Times.Once);
        }

        [Fact]
        public void GetEquipment_SecondCall_ReturnsFromCache()
        {
            // Arrange
            var equipment = new List<EquipmentData>
            {
                new EquipmentData { EntryId = 1, PC_Name = "Test Equipment", Serial_No = "12345" }
            };
            
            _mockService.Setup(s => s.GetEquipment()).Returns(equipment);

            // Act - First call to populate cache
            var firstResult = _cachingService.GetEquipment();
            
            // Act - Second call should hit cache
            var secondResult = _cachingService.GetEquipment();

            // Assert
            Assert.NotNull(secondResult);
            Assert.Single(secondResult);
            Assert.Equal(1, secondResult[0].EntryId);
            _mockService.Verify(s => s.GetEquipment(), Times.Once); // Should only be called once
        }

        [Fact]
        public void IsSerialNoTakenInMachines_FirstCall_FetchesFromServiceAndCaches()
        {
            // Arrange
            var serialNo = "TEST123";
            _mockService.Setup(s => s.IsSerialNoTakenInMachines(serialNo)).Returns(true);

            // Act
            var result = _cachingService.IsSerialNoTakenInMachines(serialNo);

            // Assert
            Assert.True(result);
            _mockService.Verify(s => s.IsSerialNoTakenInMachines(serialNo), Times.Once);
        }

        [Fact]
        public void AddEntry_ClearsEquipmentCache()
        {
            // Arrange
            var equipment = new List<EquipmentData>
            {
                new EquipmentData { EntryId = 1, PC_Name = "Test Equipment", Serial_No = "12345" }
            };
            
            var newEntry = new EquipmentData { EntryId = 2, PC_Name = "New Equipment", Serial_No = "67890" };
            
            _mockService.Setup(s => s.GetEquipment()).Returns(equipment);

            // First call to populate cache
            _cachingService.GetEquipment();

            // Act
            _cachingService.AddEntry(newEntry);

            // Act - Second call after cache invalidation
            _cachingService.GetEquipment();

            // Assert
            _mockService.Verify(s => s.GetEquipment(), Times.Exactly(2)); // Called twice due to cache invalidation
            _mockService.Verify(s => s.AddEntry(newEntry), Times.Once);
        }

        [Fact]
        public void UpdateLatestEntry_ClearsEquipmentCache()
        {
            // Arrange
            var equipment = new List<EquipmentData>
            {
                new EquipmentData { EntryId = 1, PC_Name = "Test Equipment", Serial_No = "12345" }
            };
            
            var updatedEntry = new EquipmentData { EntryId = 1, PC_Name = "Updated Equipment", Serial_No = "12345" };
            
            _mockService.Setup(s => s.GetEquipment()).Returns(equipment);

            // First call to populate cache
            _cachingService.GetEquipment();

            // Act
            _cachingService.UpdateLatestEntry(updatedEntry);

            // Act - Second call after cache invalidation
            _cachingService.GetEquipment();

            // Assert
            _mockService.Verify(s => s.GetEquipment(), Times.Exactly(2)); // Called twice due to cache invalidation
            _mockService.Verify(s => s.UpdateLatestEntry(updatedEntry), Times.Once);
        }

        [Fact]
        public void DeleteEntry_ClearsEquipmentCache()
        {
            // Arrange
            var equipment = new List<EquipmentData>
            {
                new EquipmentData { EntryId = 1, PC_Name = "Test Equipment", Serial_No = "12345" }
            };
            
            var entryId = 1;
            var machineInstNo = 100;
            
            _mockService.Setup(s => s.GetEquipment()).Returns(equipment);

            // First call to populate cache
            _cachingService.GetEquipment();

            // Act
            _cachingService.DeleteEntry(entryId, machineInstNo);

            // Act - Second call after cache invalidation
            _cachingService.GetEquipment();

            // Assert
            _mockService.Verify(s => s.GetEquipment(), Times.Exactly(2)); // Called twice due to cache invalidation
            _mockService.Verify(s => s.DeleteEntry(entryId, machineInstNo), Times.Once);
        }

        [Fact]
        public void CacheInvalidation_IntegrationTest()
        {
            // Arrange - Setup data for both equipment and machines cache
            var equipment = new List<EquipmentData>
            {
                new EquipmentData { EntryId = 1, PC_Name = "Test Equipment", Serial_No = "12345" }
            };
            var machines = new List<MachineData>
            {
                new MachineData { EntryId = 1, PC_Name = "Test Machine", Inst_No = 100 }
            };
            
            _mockService.Setup(s => s.GetEquipment()).Returns(equipment);
            _mockService.Setup(s => s.GetMachines()).Returns(machines);

            // Populate both caches
            _cachingService.GetEquipment();
            _cachingService.GetMachines();

            // Act - Add new entry should invalidate all caches
            var newEntry = new EquipmentData { EntryId = 2, PC_Name = "New Equipment", Serial_No = "67890" };
            _cachingService.AddEntry(newEntry);

            // Act - Access both caches again
            _cachingService.GetEquipment();
            _cachingService.GetMachines();

            // Assert - Both should have been called twice (once for initial population, once after invalidation)
            _mockService.Verify(s => s.GetEquipment(), Times.Exactly(2));
            _mockService.Verify(s => s.GetMachines(), Times.Exactly(2));
        }
    }
}
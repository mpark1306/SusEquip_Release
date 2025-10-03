using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SusEquip.Data.Models;
using SusEquip.Data.Services;
using SusEquip.Data.Services.Decorators;
using Xunit;

namespace SusEquip.Tests.Services.Decorators;

public class SyncDecoratorIntegrationTests
{
    private readonly Mock<IEquipmentServiceSync> _mockCoreService;
    private readonly Mock<ILogger<LoggingEquipmentService>> _mockLoggingLogger;
    private readonly Mock<ILogger<CachingEquipmentService>> _mockCachingLogger;
    private readonly IMemoryCache _memoryCache;

    public SyncDecoratorIntegrationTests()
    {
        _mockCoreService = new Mock<IEquipmentServiceSync>();
        _mockLoggingLogger = new Mock<ILogger<LoggingEquipmentService>>();
        _mockCachingLogger = new Mock<ILogger<CachingEquipmentService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public void DecoratorChain_GetEquipment_ShouldExecuteAllDecorators()
    {
        // Arrange - Build decorator chain: Caching -> Logging -> Core
        var expectedEquipment = new List<EquipmentData>
        {
            new EquipmentData { PC_Name = "TEST-PC-001", Serial_No = "SN001" }
        };

        // Mock core service
        _mockCoreService.Setup(x => x.GetEquipment()).Returns(expectedEquipment);

        // Build decorator chain
        var loggingService = new LoggingEquipmentService(_mockCoreService.Object, _mockLoggingLogger.Object);
        var cachingService = new CachingEquipmentService(loggingService, _memoryCache, _mockCachingLogger.Object);

        // Act
        var result = cachingService.GetEquipment();

        // Assert
        Assert.Equal(expectedEquipment, result);

        // Verify core service was called
        _mockCoreService.Verify(x => x.GetEquipment(), Times.Once);
        
        // Verify logging occurred (Debug level as per actual implementation)
        _mockLoggingLogger.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Starting GetEquipment operation")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public void DecoratorChain_AddEntry_ShouldExecuteBothDecorators()
    {
        // Arrange
        var equipmentData = new EquipmentData { PC_Name = "TEST-PC-001", Serial_No = "SN001" };

        _mockCoreService.Setup(x => x.AddEntry(equipmentData));

        var loggingService = new LoggingEquipmentService(_mockCoreService.Object, _mockLoggingLogger.Object);
        var cachingService = new CachingEquipmentService(loggingService, _memoryCache, _mockCachingLogger.Object);

        // Act
        cachingService.AddEntry(equipmentData);

        // Assert
        _mockCoreService.Verify(x => x.AddEntry(equipmentData), Times.Once);
    }

    [Fact]
    public void DecoratorChain_GetMachines_WithCaching_ShouldUseCacheOnSecondCall()
    {
        // Arrange
        var machines = new List<MachineData>
        {
            new MachineData { PC_Name = "TEST-PC-001", Serial_No = "SN001" }
        };

        _mockCoreService.Setup(x => x.GetMachines()).Returns(machines);

        var loggingService = new LoggingEquipmentService(_mockCoreService.Object, _mockLoggingLogger.Object);
        var cachingService = new CachingEquipmentService(loggingService, _memoryCache, _mockCachingLogger.Object);

        // Act - First call should hit the service
        var result1 = cachingService.GetMachines();
        
        // Act - Second call should use cache
        var result2 = cachingService.GetMachines();

        // Assert
        Assert.Equal(machines, result1);
        Assert.Equal(machines, result2);
        
        // Core service should only be called once due to caching
        _mockCoreService.Verify(x => x.GetMachines(), Times.Once);
    }

    [Fact]
    public void DecoratorChain_CacheInvalidation_AddEntryShouldInvalidateCache()
    {
        // Arrange
        var machines = new List<MachineData>
        {
            new MachineData { PC_Name = "TEST-PC-001", Serial_No = "SN001" }
        };
        var equipmentData = new EquipmentData { PC_Name = "TEST-PC-001", Serial_No = "SN001" };

        _mockCoreService.Setup(x => x.GetMachines()).Returns(machines);
        _mockCoreService.Setup(x => x.AddEntry(equipmentData));

        var loggingService = new LoggingEquipmentService(_mockCoreService.Object, _mockLoggingLogger.Object);
        var cachingService = new CachingEquipmentService(loggingService, _memoryCache, _mockCachingLogger.Object);

        // Act
        // First call caches the result
        var result1 = cachingService.GetMachines();
        
        // Add entry should invalidate cache
        cachingService.AddEntry(equipmentData);
        
        // Second call should hit the service again
        var result2 = cachingService.GetMachines();

        // Assert
        Assert.Equal(machines, result1);
        Assert.Equal(machines, result2);
        
        // Core service should be called twice - once before cache, once after invalidation
        _mockCoreService.Verify(x => x.GetMachines(), Times.Exactly(2));
        _mockCoreService.Verify(x => x.AddEntry(equipmentData), Times.Once);
    }

    [Fact]
    public void DecoratorChain_IsSerialNoTakenInMachines_ShouldWork()
    {
        // Arrange
        _mockCoreService.Setup(x => x.IsSerialNoTakenInMachines("SN001")).Returns(true);

        var loggingService = new LoggingEquipmentService(_mockCoreService.Object, _mockLoggingLogger.Object);
        var cachingService = new CachingEquipmentService(loggingService, _memoryCache, _mockCachingLogger.Object);

        // Act
        var result = cachingService.IsSerialNoTakenInMachines("SN001");

        // Assert
        Assert.True(result);
        _mockCoreService.Verify(x => x.IsSerialNoTakenInMachines("SN001"), Times.Once);
    }

    [Fact]
    public void DecoratorChain_UpdateLatestEntry_ShouldWork()
    {
        // Arrange
        var equipmentData = new EquipmentData { PC_Name = "TEST-PC-001", Serial_No = "SN001" };

        _mockCoreService.Setup(x => x.UpdateLatestEntry(equipmentData));

        var loggingService = new LoggingEquipmentService(_mockCoreService.Object, _mockLoggingLogger.Object);
        var cachingService = new CachingEquipmentService(loggingService, _memoryCache, _mockCachingLogger.Object);

        // Act
        cachingService.UpdateLatestEntry(equipmentData);

        // Assert
        _mockCoreService.Verify(x => x.UpdateLatestEntry(equipmentData), Times.Once);
    }

    [Fact]
    public void DecoratorChain_DeleteEntry_ShouldWork()
    {
        // Arrange
        _mockCoreService.Setup(x => x.DeleteEntry(1, 1));

        var loggingService = new LoggingEquipmentService(_mockCoreService.Object, _mockLoggingLogger.Object);
        var cachingService = new CachingEquipmentService(loggingService, _memoryCache, _mockCachingLogger.Object);

        // Act
        cachingService.DeleteEntry(1, 1);

        // Assert
        _mockCoreService.Verify(x => x.DeleteEntry(1, 1), Times.Once);
    }
}
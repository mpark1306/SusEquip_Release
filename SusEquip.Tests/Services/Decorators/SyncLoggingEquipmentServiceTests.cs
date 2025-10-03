using Microsoft.Extensions.Logging;
using Moq;
using SusEquip.Data.Models;
using SusEquip.Data.Services;
using SusEquip.Data.Services.Decorators;
using Xunit;

namespace SusEquip.Tests.Services.Decorators;

public class SyncLoggingEquipmentServiceTests
{
    private readonly Mock<IEquipmentServiceSync> _mockEquipmentService;
    private readonly Mock<ILogger<LoggingEquipmentService>> _mockLogger;
    private readonly LoggingEquipmentService _loggingService;

    public SyncLoggingEquipmentServiceTests()
    {
        _mockEquipmentService = new Mock<IEquipmentServiceSync>();
        _mockLogger = new Mock<ILogger<LoggingEquipmentService>>();
        _loggingService = new LoggingEquipmentService(_mockEquipmentService.Object, _mockLogger.Object);
    }

    [Fact]
    public void GetEquipment_ShouldLogEntry()
    {
        // Arrange
        var testData = new List<EquipmentData>
        {
            new EquipmentData { PC_Name = "TEST-PC-001" }
        };
        _mockEquipmentService.Setup(x => x.GetEquipment()).Returns(testData);

        // Act
        var result = _loggingService.GetEquipment();

        // Assert
        Assert.Equal(testData, result);
        _mockEquipmentService.Verify(x => x.GetEquipment(), Times.Once);
        
        // Verify logging occurred with correct message (checking actual implementation pattern)
        _mockLogger.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Starting GetEquipment operation")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void AddEntry_ShouldLogEntry()
    {
        // Arrange
        var equipmentData = new EquipmentData { PC_Name = "TEST-PC-001", Serial_No = "SN001" };
        _mockEquipmentService.Setup(x => x.AddEntry(equipmentData));

        // Act
        _loggingService.AddEntry(equipmentData);

        // Assert
        _mockEquipmentService.Verify(x => x.AddEntry(equipmentData), Times.Once);
        
        // Verify logging occurred
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Starting AddEntry for equipment")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void GetMachines_ShouldLogEntry()
    {
        // Arrange
        var testData = new List<MachineData>
        {
            new MachineData { PC_Name = "TEST-PC-001" }
        };
        _mockEquipmentService.Setup(x => x.GetMachines()).Returns(testData);

        // Act
        var result = _loggingService.GetMachines();

        // Assert
        Assert.Equal(testData, result);
        _mockEquipmentService.Verify(x => x.GetMachines(), Times.Once);
        
        // Verify logging occurred
        _mockLogger.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Starting GetMachines operation")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void UpdateLatestEntry_ShouldLogEntry()
    {
        // Arrange
        var equipmentData = new EquipmentData { PC_Name = "TEST-PC-001", Serial_No = "SN001" };
        _mockEquipmentService.Setup(x => x.UpdateLatestEntry(equipmentData));

        // Act
        _loggingService.UpdateLatestEntry(equipmentData);

        // Assert
        _mockEquipmentService.Verify(x => x.UpdateLatestEntry(equipmentData), Times.Once);
        
        // Verify logging occurred
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Starting UpdateLatestEntry for equipment")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void DeleteEntry_ShouldLogEntry()
    {
        // Arrange
        _mockEquipmentService.Setup(x => x.DeleteEntry(1, 1));

        // Act
        _loggingService.DeleteEntry(1, 1);

        // Assert
        _mockEquipmentService.Verify(x => x.DeleteEntry(1, 1), Times.Once);
        
        // Verify logging occurred
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Starting DeleteEntry for Inst_No:")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void IsSerialNoTakenInMachines_ShouldLogEntry()
    {
        // Arrange
        _mockEquipmentService.Setup(x => x.IsSerialNoTakenInMachines("SN001")).Returns(true);

        // Act
        var result = _loggingService.IsSerialNoTakenInMachines("SN001");

        // Assert
        Assert.True(result);
        _mockEquipmentService.Verify(x => x.IsSerialNoTakenInMachines("SN001"), Times.Once);
        
        // Verify logging occurred
        _mockLogger.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Checking if SerialNo SN001 is taken")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }



    [Fact]
    public void ExceptionHandling_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        _mockEquipmentService.Setup(x => x.GetEquipment()).Throws(exception);

        // Act & Assert
        var thrownException = Assert.Throws<InvalidOperationException>(() => _loggingService.GetEquipment());
        Assert.Equal(exception, thrownException);
        
        // Verify error logging occurred
        _mockLogger.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Failed to get equipment")),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
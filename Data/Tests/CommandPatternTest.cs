using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SusEquip.Data.Commands.Equipment;
using SusEquip.Data.Commands.Handlers;
using SusEquip.Data.Commands;
using SusEquip.Data.Models;
using SusEquip.Data.Interfaces.Services;

namespace SusEquip.Data.Tests
{
    /// <summary>
    /// Simple test to verify Command Pattern infrastructure is working
    /// Run this to validate all 4 working commands: Add, Update, UpdateStatus, Delete, BulkImport
    /// </summary>
    public class CommandPatternTest
    {
        public static async Task<bool> TestCommands()
        {
            try
            {
                // Create a mock logger (in a real test, you'd use a proper mock or test logger)
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                var addLogger = loggerFactory.CreateLogger<AddEquipmentCommand>();
                var deleteLogger = loggerFactory.CreateLogger<DeleteEquipmentCommand>();
                var statusLogger = loggerFactory.CreateLogger<UpdateEquipmentStatusCommand>();
                var updateLogger = loggerFactory.CreateLogger<UpdateEquipmentCommand>();
                var bulkLogger = loggerFactory.CreateLogger<BulkImportEquipmentCommand>();
                
                // Create mock equipment service (you'd use your DI container in real app)
                var mockService = new MockEquipmentService();

                // Test 1: AddEquipmentCommand
                var equipmentData = new EquipmentData 
                { 
                    PC_Name = "TestPC001", 
                    Status = "New",
                    Entry_Date = DateTime.Now.ToString("yyyy-MM-dd")
                };

                var addCommand = new AddEquipmentCommand(equipmentData, mockService, addLogger);
                var addResult = await addCommand.ExecuteAsync();
                
                if (!addResult.Success)
                {
                    Console.WriteLine($"AddEquipmentCommand failed: {addResult.Message}");
                    return false;
                }
                Console.WriteLine($"✓ AddEquipmentCommand succeeded: {addResult.Message}");

                // Test 2: UpdateEquipmentStatusCommand  
                var statusCommand = new UpdateEquipmentStatusCommand(12345, "Active", "TestUser", mockService, statusLogger);
                var statusResult = await statusCommand.ExecuteAsync();
                
                if (!statusResult.Success)
                {
                    Console.WriteLine($"UpdateEquipmentStatusCommand failed: {statusResult.Message}");
                    return false;
                }
                Console.WriteLine($"✓ UpdateEquipmentStatusCommand succeeded: {statusResult.Message}");

                // Test 3: UpdateEquipmentCommand
                var updateEquipmentData = new EquipmentData 
                { 
                    Inst_No = 12345,
                    PC_Name = "UpdatedTestPC", 
                    Status = "Active",
                    App_Owner = "TestOwner",
                    Department = "IT",
                    Note = "Updated via command pattern"
                };

                var updateCommand = new UpdateEquipmentCommand(updateEquipmentData, "TestUser", mockService, updateLogger);
                var updateResult = await updateCommand.ExecuteAsync();
                
                if (!updateResult.Success)
                {
                    Console.WriteLine($"UpdateEquipmentCommand failed: {updateResult.Message}");
                    return false;
                }
                Console.WriteLine($"✓ UpdateEquipmentCommand succeeded: {updateResult.Message}");

                // Test 4: DeleteEquipmentCommand
                var deleteCommand = new DeleteEquipmentCommand(12345, "TestUser", mockService, deleteLogger);
                var deleteResult = await deleteCommand.ExecuteAsync();
                
                if (!deleteResult.Success)
                {
                    Console.WriteLine($"DeleteEquipmentCommand failed: {deleteResult.Message}");
                    return false;
                }
                Console.WriteLine($"✓ DeleteEquipmentCommand succeeded: {deleteResult.Message}");

                // Test 5: BulkImportEquipmentCommand
                var equipmentList = new List<EquipmentData>
                {
                    new EquipmentData { PC_Name = "BulkPC001", Status = "New" },
                    new EquipmentData { PC_Name = "BulkPC002", Status = "New" }
                };

                var bulkCommand = new BulkImportEquipmentCommand(equipmentList, "TestUser", mockService, bulkLogger);
                var bulkResult = await bulkCommand.ExecuteAsync();
                
                if (!bulkResult.Success)
                {
                    Console.WriteLine($"BulkImportEquipmentCommand failed: {bulkResult.Message}");
                    return false;
                }
                Console.WriteLine($"✓ BulkImportEquipmentCommand succeeded: {bulkResult.Message}");

                Console.WriteLine("\n🎉 All Command Pattern tests passed! Infrastructure is working correctly.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Command Pattern test failed with exception: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Mock equipment service for testing
    /// </summary>
    public class MockEquipmentService : IEquipmentService
    {
        public Task AddEntryAsync(EquipmentData equipmentData)
        {
            // Simulate success
            return Task.CompletedTask;
        }

        public Task DeleteEquipmentAsync(int instNo)
        {
            // Simulate success
            return Task.CompletedTask;
        }

        public Task<EquipmentData?> GetByInstNoAsync(int instNo)
        {
            // Return mock equipment
            return Task.FromResult<EquipmentData?>(new EquipmentData 
            { 
                Inst_No = instNo, 
                PC_Name = "MockPC", 
                Status = "Active" 
            });
        }

        public Task UpdateEquipmentStatusAsync(int instNo, string newStatus)
        {
            // Simulate success
            return Task.CompletedTask;
        }

        // Implement other required interface methods with basic mocks
        public Task InsertEntryAsync(EquipmentData equipmentData) => Task.CompletedTask;
        public Task UpdateLatestEntryAsync(EquipmentData equipmentData) => Task.CompletedTask;
        public Task DeleteEntryAsync(int instNo, int entryId) => Task.CompletedTask;
        public Task<List<EquipmentData>> GetEquipmentAsync() => Task.FromResult(new List<EquipmentData>());
        public Task<List<EquipmentData>> GetEquipmentSortedAsync(int instNo) => Task.FromResult(new List<EquipmentData>());
        public Task<List<EquipmentData>> GetEquipmentSortedByEntryAsync(int instNo) => Task.FromResult(new List<EquipmentData>());
        public Task<List<MachineData>> GetMachinesAsync() => Task.FromResult(new List<MachineData>());
        public Task<List<MachineData>> GetActiveMachinesAsync() => Task.FromResult(new List<MachineData>());
        public Task<List<MachineData>> GetNewMachinesAsync() => Task.FromResult(new List<MachineData>());
        public Task<List<MachineData>> GetUsedMachinesAsync() => Task.FromResult(new List<MachineData>());
        public Task<List<MachineData>> GetQuarantineMachinesAsync() => Task.FromResult(new List<MachineData>());
        public Task<List<MachineData>> GetMachinesOutOfServiceSinceJuneAsync() => Task.FromResult(new List<MachineData>());
        public Task<int> GetNextInstNoAsync() => Task.FromResult(12345);
        public Task<bool> IsInstNoTakenAsync(int instNo) => Task.FromResult(false);
        public Task<bool> IsSerialNoTakenInMachinesAsync(string serialNo) => Task.FromResult(false);
        public Task<(int activeCount, int newCount, int usedCount, int quarantinedCount)> GetDashboardStatisticsAsync() 
            => Task.FromResult((10, 5, 3, 2));
    }
}
using Microsoft.Extensions.Logging;
using SusEquip.Data.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SusEquip.Data.Repositories
{
    /// <summary>
    /// Implements the Unit of Work pattern to coordinate repositories and manage transactions
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly ILogger<UnitOfWork> _logger;
        private bool _disposed = false;
        private SqlTransaction? _transaction;
        private SqlConnection? _connection;

        // Lazy-loaded repositories
        private IEquipmentRepository? _equipmentRepository;
        private IOldEquipmentRepository? _oldEquipmentRepository;

        public UnitOfWork(
            DatabaseHelper dbHelper,
            ILogger<UnitOfWork> logger,
            IEquipmentRepository equipmentRepository,
            IOldEquipmentRepository oldEquipmentRepository)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
            _oldEquipmentRepository = oldEquipmentRepository ?? throw new ArgumentNullException(nameof(oldEquipmentRepository));
        }

        #region Repository Properties

        public IEquipmentRepository Equipment 
        { 
            get 
            { 
                if (_disposed)
                    throw new ObjectDisposedException(nameof(UnitOfWork));
                
                return _equipmentRepository ?? throw new InvalidOperationException("EquipmentRepository not initialized");
            } 
        }

        public IOldEquipmentRepository OldEquipment 
        { 
            get 
            { 
                if (_disposed)
                    throw new ObjectDisposedException(nameof(UnitOfWork));
                
                return _oldEquipmentRepository ?? throw new InvalidOperationException("OldEquipmentRepository not initialized");
            } 
        }

        // Keep backward compatibility properties
        public IEquipmentRepository EquipmentRepository => Equipment;
        public IOldEquipmentRepository OldEquipmentRepository => OldEquipment;

        /// <summary>
        /// Checks if there is an active transaction
        /// </summary>
        public bool HasActiveTransaction => _transaction != null;

        #endregion

        #region Transaction Management

        public async Task BeginTransactionAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));

            if (_transaction != null)
                throw new InvalidOperationException("Transaction already started");

            try
            {
                _connection = _dbHelper.GetConnection();
                await _connection.OpenAsync();
                _transaction = _connection.BeginTransaction();

                _logger.LogDebug("Transaction started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting transaction");
                await DisposeConnectionAndTransaction();
                throw;
            }
        }

        public async Task CommitAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));

            if (_transaction == null)
                throw new InvalidOperationException("No transaction to commit");

            try
            {
                _transaction.Commit();
                _logger.LogDebug("Transaction committed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing transaction");
                await RollbackAsync();
                throw;
            }
            finally
            {
                await DisposeConnectionAndTransaction();
            }
        }

        public async Task RollbackAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));

            if (_transaction == null)
            {
                _logger.LogWarning("Attempted to rollback but no transaction exists");
                return;
            }

            try
            {
                _transaction.Rollback();
                _logger.LogDebug("Transaction rolled back");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back transaction");
                throw;
            }
            finally
            {
                await DisposeConnectionAndTransaction();
            }
        }

        public async Task CommitTransactionAsync()
        {
            await CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await RollbackAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnitOfWork));

            // If we have an active transaction, commit it and return affected rows
            if (_transaction != null)
            {
                await CommitAsync();
                return 1; // Transaction-based operations don't return row counts
            }
            else
            {
                // If no transaction is active, changes are already saved
                _logger.LogDebug("SaveChanges called but no active transaction - changes already persisted");
                return 0;
            }
        }

        #endregion

        #region Transaction Helpers

        /// <summary>
        /// Executes an action within a transaction, automatically handling commit/rollback
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var wasTransactionActive = _transaction != null;

            try
            {
                if (!wasTransactionActive)
                {
                    await BeginTransactionAsync();
                }

                await action();

                if (!wasTransactionActive)
                {
                    await CommitAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action in transaction");
                
                if (!wasTransactionActive && _transaction != null)
                {
                    await RollbackAsync();
                }
                
                throw;
            }
        }

        /// <summary>
        /// Executes a function within a transaction, automatically handling commit/rollback
        /// </summary>
        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var wasTransactionActive = _transaction != null;

            try
            {
                if (!wasTransactionActive)
                {
                    await BeginTransactionAsync();
                }

                var result = await function();

                if (!wasTransactionActive)
                {
                    await CommitAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing function in transaction");
                
                if (!wasTransactionActive && _transaction != null)
                {
                    await RollbackAsync();
                }
                
                throw;
            }
        }

        #endregion

        #region Business Operations

        /// <summary>
        /// Transfer equipment from Equipment table to OldEquipment table
        /// This is a common business operation that requires transaction coordination
        /// </summary>
        public async Task TransferEquipmentToOldAsync(int equipmentId, string reason = "")
        {
            await ExecuteInTransactionAsync(async () =>
            {
                // Get the equipment to transfer
                var equipment = await EquipmentRepository.GetByIdAsync(equipmentId);
                if (equipment == null)
                {
                    throw new InvalidOperationException($"Equipment with ID {equipmentId} not found");
                }

                // Create OLD equipment entry
                var oldEquipment = MapToOldEquipmentData(equipment, reason);
                await OldEquipmentRepository.AddAsync(oldEquipment);

                // Remove from main equipment table
                await EquipmentRepository.DeleteAsync(equipmentId);

                _logger.LogInformation("Successfully transferred equipment {PCName} (ID: {Id}) to OLD equipment", 
                    equipment.PC_Name, equipmentId);
            });
        }

        /// <summary>
        /// Batch operation to transfer multiple equipment items to OLD equipment
        /// </summary>
        public async Task BulkTransferEquipmentToOldAsync(int[] equipmentIds, string reason = "")
        {
            if (equipmentIds == null || equipmentIds.Length == 0)
                throw new ArgumentException("Equipment IDs cannot be null or empty", nameof(equipmentIds));

            await ExecuteInTransactionAsync(async () =>
            {
                var transferredCount = 0;
                
                foreach (var id in equipmentIds)
                {
                    try
                    {
                        // Get the equipment to transfer
                        var equipment = await EquipmentRepository.GetByIdAsync(id);
                        if (equipment == null)
                        {
                            _logger.LogWarning("Equipment with ID {Id} not found during bulk transfer", id);
                            continue;
                        }

                        // Create OLD equipment entry
                        var oldEquipment = MapToOldEquipmentData(equipment, reason);
                        await OldEquipmentRepository.AddAsync(oldEquipment);

                        // Remove from main equipment table
                        await EquipmentRepository.DeleteAsync(id);

                        transferredCount++;
                        _logger.LogDebug("Transferred equipment {PCName} (ID: {Id}) to OLD", equipment.PC_Name, id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error transferring equipment with ID {Id} during bulk operation", id);
                        throw; // This will cause the entire transaction to rollback
                    }
                }

                _logger.LogInformation("Successfully transferred {Count} equipment items to OLD equipment in bulk operation", 
                    transferredCount);
            });
        }

        #endregion

        #region Private Helper Methods

        private async Task DisposeConnectionAndTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
                _connection = null;
            }
        }

        private Models.OLDEquipmentData MapToOldEquipmentData(Models.EquipmentData equipment, string reason)
        {
            return new Models.OLDEquipmentData
            {
                Entry_Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                PC_Name = equipment.PC_Name,
                Inst_No = equipment.Inst_No.ToString(),
                Creator_Initials = equipment.Creator_Initials,
                App_Owner = equipment.App_Owner,
                Status = equipment.Status,
                Serial_No = equipment.Serial_No,
                Mac_Address1 = equipment.Mac_Address1,
                Mac_Address2 = equipment.Mac_Address2,
                UUID = equipment.UUID,
                Product_No = equipment.Product_No,
                Model_Name_and_No = equipment.Model_Name_and_No,
                Department = equipment.Department,
                Service_Start = equipment.Service_Start,
                Service_Ends = equipment.Service_Ends,
                Note = string.IsNullOrEmpty(reason) ? equipment.Note : $"{equipment.Note} | Transferred: {reason}",
                MachineType = equipment.MachineType
            };
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    if (_transaction != null)
                    {
                        _transaction.Rollback();
                        _logger.LogWarning("UnitOfWork disposed with active transaction - transaction rolled back");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during UnitOfWork disposal");
                }
                finally
                {
                    DisposeConnectionAndTransaction().GetAwaiter().GetResult();
                    _disposed = true;
                }
            }
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }

        #endregion
    }
}
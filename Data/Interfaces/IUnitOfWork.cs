using System;
using System.Threading.Tasks;

namespace SusEquip.Data.Interfaces
{
    /// <summary>
    /// Unit of Work pattern interface for coordinating multiple repositories
    /// and managing database transactions
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Equipment repository instance
        /// </summary>
        IEquipmentRepository Equipment { get; }

        /// <summary>
        /// OLD Equipment repository instance
        /// </summary>
        IOldEquipmentRepository OldEquipment { get; }

        /// <summary>
        /// Saves all changes made in this unit of work to the database
        /// </summary>
        /// <returns>The number of affected records</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        Task RollbackTransactionAsync();

        /// <summary>
        /// Checks if there is an active transaction
        /// </summary>
        bool HasActiveTransaction { get; }
    }
}
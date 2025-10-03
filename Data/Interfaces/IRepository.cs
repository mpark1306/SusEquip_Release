using System.Collections.Generic;
using System.Threading.Tasks;

namespace SusEquip.Data.Interfaces
{
    /// <summary>
    /// Base repository interface providing common CRUD operations for entities
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its ID
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>Collection of all entities</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity by ID
        /// </summary>
        /// <param name="id">The ID of the entity to delete</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Checks if an entity exists by ID
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <returns>True if entity exists, false otherwise</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Gets the total count of entities
        /// </summary>
        /// <returns>Total count</returns>
        Task<int> CountAsync();
    }
}
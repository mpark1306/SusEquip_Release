using SusEquip.Data.Interfaces;
using SusEquip.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SusEquip.Data.Factories.Advanced
{
    /// <summary>
    /// Adapter to make IEquipmentRepository compatible with IRepository&lt;BaseEquipmentData&gt;
    /// </summary>
    public class EquipmentRepositoryAdapter : IRepository<BaseEquipmentData>
    {
        private readonly IEquipmentRepository _equipmentRepository;

        public EquipmentRepositoryAdapter(IEquipmentRepository equipmentRepository)
        {
            _equipmentRepository = equipmentRepository ?? throw new ArgumentNullException(nameof(equipmentRepository));
        }

        public async Task<BaseEquipmentData?> GetByIdAsync(int id)
        {
            return await _equipmentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<BaseEquipmentData>> GetAllAsync()
        {
            var equipment = await _equipmentRepository.GetAllAsync();
            return equipment.Cast<BaseEquipmentData>();
        }

        public async Task AddAsync(BaseEquipmentData entity)
        {
            if (entity is EquipmentData equipmentData)
            {
                await _equipmentRepository.AddAsync(equipmentData);
            }
            else
            {
                throw new ArgumentException("Entity must be of type EquipmentData for equipment repository", nameof(entity));
            }
        }

        public async Task UpdateAsync(BaseEquipmentData entity)
        {
            if (entity is EquipmentData equipmentData)
            {
                await _equipmentRepository.UpdateAsync(equipmentData);
            }
            else
            {
                throw new ArgumentException("Entity must be of type EquipmentData for equipment repository", nameof(entity));
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _equipmentRepository.DeleteAsync(id);
        }

        public async Task<int> CountAsync()
        {
            var allItems = await _equipmentRepository.GetAllAsync();
            return allItems.Count();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _equipmentRepository.ExistsAsync(id);
        }
    }

    /// <summary>
    /// Adapter to make IOldEquipmentRepository compatible with IRepository&lt;BaseEquipmentData&gt;
    /// </summary>
    public class OldEquipmentRepositoryAdapter : IRepository<BaseEquipmentData>
    {
        private readonly IOldEquipmentRepository _oldEquipmentRepository;

        public OldEquipmentRepositoryAdapter(IOldEquipmentRepository oldEquipmentRepository)
        {
            _oldEquipmentRepository = oldEquipmentRepository ?? throw new ArgumentNullException(nameof(oldEquipmentRepository));
        }

        public async Task<BaseEquipmentData?> GetByIdAsync(int id)
        {
            return await _oldEquipmentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<BaseEquipmentData>> GetAllAsync()
        {
            var oldEquipment = await _oldEquipmentRepository.GetAllAsync();
            return oldEquipment.Cast<BaseEquipmentData>();
        }

        public async Task AddAsync(BaseEquipmentData entity)
        {
            if (entity is OLDEquipmentData oldEquipmentData)
            {
                await _oldEquipmentRepository.AddAsync(oldEquipmentData);
            }
            else
            {
                throw new ArgumentException("Entity must be of type OLDEquipmentData for old equipment repository", nameof(entity));
            }
        }

        public async Task UpdateAsync(BaseEquipmentData entity)
        {
            if (entity is OLDEquipmentData oldEquipmentData)
            {
                await _oldEquipmentRepository.UpdateAsync(oldEquipmentData);
            }
            else
            {
                throw new ArgumentException("Entity must be of type OLDEquipmentData for old equipment repository", nameof(entity));
            }
        }

        public async Task DeleteAsync(int id)
        {
            await _oldEquipmentRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _oldEquipmentRepository.ExistsAsync(id);
        }

        public async Task<int> CountAsync()
        {
            var allItems = await _oldEquipmentRepository.GetAllAsync();
            return allItems.Count();
        }
    }
}
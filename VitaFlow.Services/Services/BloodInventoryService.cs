using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Infrastructure.Repositories.Interfaces;

namespace VitaFlow.Services.Services
{
    // Implementation of the IBloodInventoryService interface.
    public class BloodInventoryService : IBloodInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BloodInventoryService> _logger;

        // Constructor with dependency injection
        // bloodInventoryRepository: The blood inventory repository
        // logger: The logger instance
        public BloodInventoryService(IUnitOfWork unitOfWork, ILogger<BloodInventoryService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Gets the current blood inventory
        /// </summary>
        /// <returns>Collection of blood inventory items</returns>
        public async Task<IEnumerable<BloodInventory>> GetCurrentInventoryAsync()
        {
            try
            {
                _logger.LogInformation("Fetching current blood inventory");
                var repo = _unitOfWork.GetRepository<BloodInventory>();
                return await repo.GetListAsync(predicate: null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current blood inventory");
                throw;
            }
        }

        /// <summary>
        /// Adds an item to the blood inventory
        /// </summary>
        /// <param name="inventory">The inventory item to add</param>
        /// <returns>The added inventory item</returns>
        public async Task<BloodInventory> AddToInventoryAsync(BloodInventory inventory)
        {
            try
            {
                if (inventory == null)
                    throw new ArgumentNullException(nameof(inventory));

                _logger.LogInformation("Adding to blood inventory: {BloodType}", inventory.BloodType);

                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<BloodInventory>();
                    await repo.InsertAsync(inventory);
                });
                return inventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to blood inventory");
                throw;
            }
        }

        public async Task UpdateInventoryAsync(BloodInventory inventory)
        {
            try
            {
                if (inventory == null)
                    throw new ArgumentNullException(nameof(inventory));

                _logger.LogInformation("Updating blood inventory item {Id}", inventory.Id);

                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<BloodInventory>();
                    repo.UpdateAsync(inventory);
                });
                // TODO: Kiểm tra lại logic cập nhật inventory nếu có nghiệp vụ đặc biệt
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blood inventory");
                throw;
            }
        }

        public async Task<bool> CheckAvailabilityAsync(BloodType bloodType, double requiredVolume)
        {
            try
            {
                _logger.LogInformation("Checking availability of {BloodType}, {Volume}ml", bloodType, requiredVolume);
                var repo = _unitOfWork.GetRepository<BloodInventory>();
                var inventories = await repo.GetListAsync(predicate: i => i.BloodType == bloodType);
                var totalVolume = inventories.Sum(i => i.WholeBloodVolume + i.RedCellsVolume + i.PlasmaVolume + i.PlateletsVolume);
                return totalVolume >= requiredVolume;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking blood availability");
                throw;
            }
        }

        public async Task<IEnumerable<BloodInventory>> GetExpiringInventoryAsync()
        {
            try
            {
                _logger.LogInformation("Fetching expiring blood inventory items");
                var repo = _unitOfWork.GetRepository<BloodInventory>();
                var soon = DateTime.UtcNow.AddDays(3); // ví dụ: sắp hết hạn trong 3 ngày tới
                var inventories = await repo.GetListAsync(predicate: i => i.ExpiryDate <= soon);
                return inventories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expiring blood inventory");
                throw;
            }
        }

        // Gets a summary of current inventory levels by blood type
        // Returns: Dictionary mapping blood types to their available volume
        public async Task<Dictionary<BloodType, double>> GetInventorySummaryAsync()
        {
            try
            {
                _logger.LogInformation("Generating blood inventory summary");
                var repo = _unitOfWork.GetRepository<BloodInventory>();
                var inventories = await repo.GetListAsync(predicate: null);
                var summary = inventories
                    .GroupBy(i => i.BloodType)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(i => i.WholeBloodVolume + i.RedCellsVolume + i.PlasmaVolume + i.PlateletsVolume)
                    );
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating blood inventory summary");
                throw;
            }
        }
    }
}

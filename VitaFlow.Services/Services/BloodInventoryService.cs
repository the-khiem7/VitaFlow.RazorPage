using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Interfaces.Repositories;
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
                return await repo.GetListAsync();
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

        public Task<bool> CheckAvailabilityAsync(BloodType bloodType, double requiredVolume)
        {
            try
            {
                _logger.LogInformation("Checking availability of {BloodType}, {Volume}ml", bloodType, requiredVolume);
                
                // In a real implementation, we'd query the repository for the total volume of the specified blood type
                // This is a placeholder implementation that always returns true
                
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking blood availability");
                throw;
            }
        }

        public Task<IEnumerable<BloodInventory>> GetExpiringInventoryAsync()
        {
            try
            {
                _logger.LogInformation("Fetching expiring blood inventory items");
                
                // In a real implementation, we'd query the repository for items close to expiration date
                // This is a placeholder implementation
                
                return Task.FromResult<IEnumerable<BloodInventory>>(new List<BloodInventory>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching expiring blood inventory");
                throw;
            }
        }

        // Gets a summary of current inventory levels by blood type
        // Returns: Dictionary mapping blood types to their available volume
        public Task<Dictionary<BloodType, double>> GetInventorySummaryAsync()
        {
            try
            {
                _logger.LogInformation("Generating blood inventory summary");
                
                // In a real implementation, we'd aggregate data from the repository
                // This is a placeholder implementation with sample data
                
                var summary = new Dictionary<BloodType, double>();
                foreach (BloodType bloodType in Enum.GetValues(typeof(BloodType)))
                {
                    // Sample volumes
                    double sampleVolume = bloodType switch
                    {
                        BloodType.APositive => 3500.0,
                        BloodType.ANegative => 1200.0,
                        BloodType.BPositive => 2800.0,
                        BloodType.BNegative => 1000.0,
                        BloodType.ABPositive => 900.0,
                        BloodType.ABNegative => 500.0,
                        BloodType.OPositive => 4200.0,
                        BloodType.ONegative => 1500.0,
                        _ => 0.0
                    };
                    
                    summary.Add(bloodType, sampleVolume);
                }
                
                return Task.FromResult(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating blood inventory summary");
                throw;
            }
        }
    }
}

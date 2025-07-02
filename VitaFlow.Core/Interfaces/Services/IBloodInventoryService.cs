using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for managing blood inventory.
    /// </summary>
    public interface IBloodInventoryService
    {
        Task<IEnumerable<BloodInventory>> GetCurrentInventoryAsync();
        Task<BloodInventory> AddToInventoryAsync(BloodInventory inventory);
        Task UpdateInventoryAsync(BloodInventory inventory);
        Task<bool> CheckAvailabilityAsync(BloodType bloodType, double requiredVolume);
        Task<IEnumerable<BloodInventory>> GetExpiringInventoryAsync();
        Task<Dictionary<BloodType, double>> GetInventorySummaryAsync();
    }
}

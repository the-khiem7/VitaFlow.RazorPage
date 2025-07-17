using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Interfaces.Repositories
{
    /// <summary>
    /// Represents the repository interface for blood inventory-specific operations.
    /// </summary>
    public interface IBloodInventoryRepository : IBaseRepository<BloodInventory>
    {
        Task<double> GetTotalVolumeByBloodTypeAsync(BloodType bloodType, bool wholeBloodOnly = false);
        Task<IEnumerable<BloodInventory>> GetExpiringInventoryAsync(int daysToExpiry);
        Task UpdateInventoryAsync(BloodType bloodType, double volume, bool isAddition);
    }
}

using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class BloodTypeRepository : IBloodTypeRepository
    {
        private readonly BloodDonationSupportContext _context;

        public BloodTypeRepository(BloodDonationSupportContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BloodType>> GetAllAsync()
        {
            return await _context.BloodTypes.ToListAsync();
        }

        public async Task<BloodType> GetByIdAsync(Guid id)
        {
            return await _context.BloodTypes.FindAsync(id);
        }

        public async Task<int> GetAvailableUnitsCountAsync(Guid bloodTypeId)
        {
            return await _context.BloodUnits
                .Where(u => u.BloodTypeId == bloodTypeId && u.Status == "Available")
                .CountAsync();
        }

        public async Task<int> GetTotalUnitsCountAsync(Guid bloodTypeId)
        {
            return await _context.BloodUnits
                .Where(u => u.BloodTypeId == bloodTypeId)
                .CountAsync();
        }

        public async Task<bool> UpdateAvailableUnitsAsync(Guid bloodTypeId, int availableUnits)
        {
            try
            {
                var units = await _context.BloodUnits
                    .Where(u => u.BloodTypeId == bloodTypeId)
                    .ToListAsync();

                // Update status of blood units based on the desired available units count
                int currentAvailable = units.Count(u => u.Status == "Available");
                
                if (currentAvailable < availableUnits)
                {
                    // Need to make more units available
                    var unitsToMakeAvailable = units
                        .Where(u => u.Status != "Available")
                        .Take(availableUnits - currentAvailable);
                    
                    foreach (var unit in unitsToMakeAvailable)
                    {
                        unit.Status = "Available";
                    }
                }
                else if (currentAvailable > availableUnits)
                {
                    // Need to make some units unavailable
                    var unitsToMakeUnavailable = units
                        .Where(u => u.Status == "Available")
                        .Take(currentAvailable - availableUnits);
                    
                    foreach (var unit in unitsToMakeUnavailable)
                    {
                        unit.Status = "Reserved";
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
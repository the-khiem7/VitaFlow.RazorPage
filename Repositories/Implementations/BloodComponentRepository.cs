using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class BloodComponentRepository : IBloodComponentRepository
    {
        private readonly BloodDonationSupportContext _context;

        public BloodComponentRepository(BloodDonationSupportContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BloodComponent>> GetAllAsync()
        {
            return await _context.BloodComponents.ToListAsync();
        }

        public async Task<BloodComponent> GetByIdAsync(Guid id)
        {
            return await _context.BloodComponents.FindAsync(id);
        }

        public async Task<int> GetAvailableUnitsCountAsync(Guid componentId)
        {
            return await _context.BloodUnits
                .Where(u => u.ComponentType == componentId && u.Status == "Available")
                .CountAsync();
        }

        public async Task<int> GetTotalUnitsCountAsync(Guid componentId)
        {
            return await _context.BloodUnits
                .Where(u => u.ComponentType == componentId)
                .CountAsync();
        }
        public async Task<BloodComponent> GetByNameAsync(string componentName)
        {
            return await _context.BloodComponents
                .FirstOrDefaultAsync(bc => bc.ComponentName == componentName);
        }

        public async Task<bool> UpdateAvailableUnitsAsync(Guid componentId, int availableUnits)
        {
            try
            {
                var units = await _context.BloodUnits
                    .Where(u => u.ComponentType == componentId)
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
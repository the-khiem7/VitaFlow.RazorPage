using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Interfaces.Repositories
{
    /// <summary>
    /// Represents the repository interface for donor-specific operations.
    /// </summary>
    public interface IDonorRepository : IBaseRepository<Donor>
    {
        Task<IEnumerable<Donor>> GetAvailableDonorsByBloodTypeAsync(BloodType bloodType);
        Task<IEnumerable<Donor>> GetDonorsByLocationAsync(double latitude, double longitude, double radiusInKm);
        Task<int> GetDonorCountByBloodTypeAsync(BloodType bloodType);
        Task UpdateDonorAvailabilityAsync(int donorId, DateTime nextAvailableDate);
    }
}

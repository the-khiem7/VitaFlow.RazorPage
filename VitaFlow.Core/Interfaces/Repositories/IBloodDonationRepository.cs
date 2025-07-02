using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;

namespace VitaFlow.Core.Interfaces.Repositories
{
    /// <summary>
    /// Represents the repository interface for blood donation-specific operations.
    /// </summary>
    public interface IBloodDonationRepository : IBaseRepository<BloodDonation>
    {
        Task<IEnumerable<BloodDonation>> GetDonationsByDonorIdAsync(int donorId);
        Task<IEnumerable<BloodDonation>> GetDonationsByRequestIdAsync(int requestId);
        Task<IEnumerable<BloodDonation>> GetDonationsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}

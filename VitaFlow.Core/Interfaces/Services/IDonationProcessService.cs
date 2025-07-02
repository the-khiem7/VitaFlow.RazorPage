using System;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;

namespace VitaFlow.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for managing the donation process.
    /// </summary>
    public interface IDonationProcessService
    {
        Task<BloodDonation> ScheduleDonationAsync(int donorId, DateTime scheduledDate);
        Task<BloodDonation> CompleteDonationAsync(int donationId);
        Task CancelDonationAsync(int donationId, string reason);
        Task<BloodDonation> AssignDonationToRequestAsync(int donationId, int requestId);
        Task<BloodRequest> CreateBloodRequestAsync(BloodRequest request);
        Task<BloodRequest> FulfillRequestAsync(int requestId);
    }
}

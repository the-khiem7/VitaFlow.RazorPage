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
        Task<BloodDonation> ScheduleDonationAsync(Guid donorId, DateTime scheduledDate);
        Task<BloodDonation> CompleteDonationAsync(Guid donationId);
        Task CancelDonationAsync(Guid donationId, string reason);
        Task<BloodDonation> AssignDonationToRequestAsync(Guid donationId, Guid requestId);
        Task<BloodRequest> CreateBloodRequestAsync(BloodRequest request);
        Task<BloodRequest> FulfillRequestAsync(Guid requestId);
    }
}

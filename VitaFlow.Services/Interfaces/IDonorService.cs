using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;

namespace VitaFlow.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing donors with business logic.
    /// </summary>
    public interface IDonorService
    {
        Task<IEnumerable<Donor>> GetAllDonorsAsync();
        Task<Donor> RegisterDonorAsync(Donor donor);
        Task UpdateDonorAsync(Donor donor);
        Task<IEnumerable<Donor>> FindCompatibleDonorsAsync(BloodType recipientBloodType);
        Task<IEnumerable<Donor>> FindNearbyDonorsAsync(double latitude, double longitude, double radiusInKm);
        Task UpdateDonorAvailabilityAsync(int donorId);
        Task<IEnumerable<BloodDonation>> GetDonorHistoryAsync(int donorId);
    }
}

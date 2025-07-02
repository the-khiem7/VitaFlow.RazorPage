using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Interfaces.Repositories
{
    /// <summary>
    /// Represents the repository interface for blood request-specific operations.
    /// </summary>
    public interface IBloodRequestRepository : IBaseRepository<BloodRequest>
    {
        Task<IEnumerable<BloodRequest>> GetActiveRequestsAsync();
        Task<IEnumerable<BloodRequest>> GetEmergencyRequestsAsync();
        Task<IEnumerable<BloodRequest>> GetRequestsByBloodTypeAsync(BloodType bloodType);
        Task UpdateRequestStatusAsync(int requestId, RequestStatus status);
    }
}

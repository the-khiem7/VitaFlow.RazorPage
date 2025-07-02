using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Interfaces.Services;

namespace VitaFlow.Services
{
    /// <summary>
    /// Implementation of the IDashboardService interface.
    /// </summary>
    public class DashboardService : IDashboardService
    {
        public Task<Dictionary<BloodType, int>> GetDonorCountsByBloodTypeAsync()
        {
            // Implementation goes here
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<RequestStatus, int>> GetRequestStatusSummaryAsync()
        {
            // Implementation goes here
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<string, int>> GetDonationsPerMonthAsync(int year)
        {
            // Implementation goes here
            throw new System.NotImplementedException();
        }

        public Task<Dictionary<BloodType, double>> GetInventoryLevelsByBloodTypeAsync()
        {
            // Implementation goes here
            throw new System.NotImplementedException();
        }

        public Task<int> GetActiveEmergencyRequestCountAsync()
        {
            // Implementation goes here
            throw new System.NotImplementedException();
        }
    }
}

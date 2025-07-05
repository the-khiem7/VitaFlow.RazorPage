using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Interfaces.Services
{
    // Service interface for dashboard operations.
    public interface IDashboardService
    {
        Task<Dictionary<BloodType, int>> GetDonorCountsByBloodTypeAsync();
        Task<Dictionary<RequestStatus, int>> GetRequestStatusSummaryAsync();
        Task<Dictionary<string, int>> GetDonationsPerMonthAsync(int year);
        Task<Dictionary<BloodType, double>> GetInventoryLevelsByBloodTypeAsync();
        Task<int> GetActiveEmergencyRequestCountAsync();
    }
}

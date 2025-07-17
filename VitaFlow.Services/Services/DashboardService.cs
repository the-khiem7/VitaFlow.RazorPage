using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Infrastructure.Repositories.Interfaces;

namespace VitaFlow.Services.Services
{
    // Implementation of the IDashboardService interface.
    // Provides analytical data for the dashboard.
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardService> _logger;

        // Constructor with dependency injection
        public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Đếm số donor theo từng nhóm máu
        public async Task<Dictionary<BloodType, int>> GetDonorCountsByBloodTypeAsync()
        {
            try
            {
                _logger.LogInformation("Generating donor counts by blood type");
                var repo = _unitOfWork.GetRepository<Donor>();
                var donors = await repo.GetListAsync(predicate: null);
                // Group by BloodType và đếm số lượng
                return donors.GroupBy(d => d.BloodType)
                             .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating donor counts by blood type");
                throw;
            }
        }

        // Đếm số blood request theo từng trạng thái
        public async Task<Dictionary<RequestStatus, int>> GetRequestStatusSummaryAsync()
        {
            try
            {
                _logger.LogInformation("Generating blood request status summary");
                var repo = _unitOfWork.GetRepository<BloodRequest>();
                var requests = await repo.GetListAsync(predicate: null);
                // Group by Status và đếm số lượng
                return requests.GroupBy(r => r.Status)
                               .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating blood request status summary");
                throw;
            }
        }

        // Đếm số donation theo từng tháng của năm
        public async Task<Dictionary<string, int>> GetDonationsPerMonthAsync(int year)
        {
            try
            {
                _logger.LogInformation($"Generating donations per month for year {year}");
                var repo = _unitOfWork.GetRepository<BloodDonation>();
                var donations = await repo.GetListAsync(predicate: d => d.DonationDate.Year == year);
                // Group by month và đếm số lượng
                return donations.GroupBy(d => d.DonationDate.ToString("MMMM"))
                                .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating donations per month for year {year}");
                throw;
            }
        }

        // Tổng hợp volume máu theo từng nhóm máu (tổng tất cả các loại volume)
        public async Task<Dictionary<BloodType, double>> GetInventoryLevelsByBloodTypeAsync()
        {
            try
            {
                _logger.LogInformation("Generating inventory levels by blood type");
                var repo = _unitOfWork.GetRepository<BloodInventory>();
                var inventories = await repo.GetListAsync(predicate: null);
                // Group by BloodType và tính tổng volume (tất cả các loại)
                return inventories.GroupBy(i => i.BloodType)
                                 .ToDictionary(
                                     g => g.Key,
                                     g => g.Sum(i => i.WholeBloodVolume + i.RedCellsVolume + i.PlasmaVolume + i.PlateletsVolume)
                                 );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating inventory levels by blood type");
                throw;
            }
        }

        // Đếm số blood request khẩn cấp còn hiệu lực (IsEmergency == true và Status là New hoặc InProgress)
        public async Task<int> GetActiveEmergencyRequestCountAsync()
        {
            try
            {
                _logger.LogInformation("Getting active emergency request count");
                var repo = _unitOfWork.GetRepository<BloodRequest>();
                var requests = await repo.GetListAsync(predicate: r => r.IsEmergency && (r.Status == RequestStatus.New || r.Status == RequestStatus.InProgress));
                // Đếm số lượng request khẩn cấp còn hiệu lực
                return requests.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active emergency request count");
                throw;
            }
        }
    }
}

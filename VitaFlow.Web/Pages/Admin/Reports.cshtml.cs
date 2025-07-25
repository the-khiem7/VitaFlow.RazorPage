using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Core.Enums;
using VitaFlow.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace VitaFlow.Web.Pages.Admin
{
    /// <summary>
    /// Page model for admin reports.
    /// </summary>
    public class ReportsModel : PageModel
    {
        private readonly IDashboardService _dashboardService;
        private readonly IUserService _userService;
        private readonly IBloodInventoryService _inventoryService;

        public AdminDashboardViewModel DashboardData { get; set; } = new AdminDashboardViewModel();
        public SystemStatisticsViewModel Statistics { get; set; } = new SystemStatisticsViewModel();
        public Dictionary<BloodType, double> InventorySummary { get; set; } = new Dictionary<BloodType, double>();

        // Report filters
        public int SelectedYear { get; set; } = DateTime.Now.Year;
        public int SelectedMonth { get; set; } = DateTime.Now.Month;

        public ReportsModel(IDashboardService dashboardService, IUserService userService, IBloodInventoryService inventoryService)
        {
            _dashboardService = dashboardService;
            _userService = userService;
            _inventoryService = inventoryService;
        }

        public async Task OnGetAsync(int? year, int? month)
        {
            SelectedYear = year ?? DateTime.Now.Year;
            SelectedMonth = month ?? DateTime.Now.Month;

            try
            {
                // Load dashboard data
                DashboardData.DonorCountsByBloodType = await _dashboardService.GetDonorCountsByBloodTypeAsync();
                DashboardData.RequestStatusSummary = await _dashboardService.GetRequestStatusSummaryAsync();
                DashboardData.DonationsPerMonth = await _dashboardService.GetDonationsPerMonthAsync(SelectedYear);
                DashboardData.InventoryLevels = await _dashboardService.GetInventoryLevelsByBloodTypeAsync();
                DashboardData.EmergencyRequestCount = await _dashboardService.GetActiveEmergencyRequestCountAsync();

                // Load user statistics
                var userSummary = await _userService.GetUserCountSummaryAsync();
                Statistics.TotalUsers = userSummary.Values.Sum();
                Statistics.TotalAdmins = userSummary.GetValueOrDefault(UserRole.Admin, 0);
                Statistics.TotalStaff = userSummary.GetValueOrDefault(UserRole.Staff, 0);
                Statistics.TotalDonors = userSummary.GetValueOrDefault(UserRole.Donor, 0);
                Statistics.TotalRecipients = userSummary.GetValueOrDefault(UserRole.Recipient, 0);
                Statistics.ActiveEmergencyRequests = DashboardData.EmergencyRequestCount;
                Statistics.TotalBloodInventory = DashboardData.InventoryLevels?.Values.Sum() ?? 0;

                // Load inventory summary
                InventorySummary = await _inventoryService.GetInventorySummaryAsync();

                // Calculate monthly statistics
                var thisMonthDonations = DashboardData.DonationsPerMonth?.GetValueOrDefault(
                    new DateTime(SelectedYear, SelectedMonth, 1).ToString("MMMM"), 0) ?? 0;
                Statistics.TotalDonationsThisMonth = thisMonthDonations;
            }
            catch (Exception)
            {
                // Handle errors gracefully
                DashboardData = new AdminDashboardViewModel();
                Statistics = new SystemStatisticsViewModel();
                InventorySummary = new Dictionary<BloodType, double>();
            }
        }
    }
}

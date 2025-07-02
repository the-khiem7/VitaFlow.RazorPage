using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VitaFlow.Web.Pages.Admin
{
    /// <summary>
    /// Page model for admin dashboard.
    /// </summary>
    public class DashboardModel : PageModel
    {
        private readonly IDashboardService _dashboardService;

        public Dictionary<BloodType, int> DonorCountsByBloodType { get; set; }
        public Dictionary<RequestStatus, int> RequestStatusSummary { get; set; }
        public Dictionary<string, int> DonationsPerMonth { get; set; }
        public Dictionary<BloodType, double> InventoryLevels { get; set; }
        public int EmergencyRequestCount { get; set; }

        public DashboardModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task OnGetAsync()
        {
            // Load dashboard statistics
        }
    }
}

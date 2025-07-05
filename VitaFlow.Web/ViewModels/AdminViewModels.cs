using System.Collections.Generic;
using VitaFlow.Core.Enums;

namespace VitaFlow.Web.ViewModels
{
    // ViewModel for admin dashboard.
    public class AdminDashboardViewModel
    {
        public Dictionary<BloodType, int>? DonorCountsByBloodType { get; set; }
        public Dictionary<RequestStatus, int>? RequestStatusSummary { get; set; }
        public Dictionary<string, int>? DonationsPerMonth { get; set; }
        public Dictionary<BloodType, double>? InventoryLevels { get; set; }
        public int EmergencyRequestCount { get; set; }
    }
}

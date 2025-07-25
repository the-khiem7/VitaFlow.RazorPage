using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Web.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace VitaFlow.Web.Pages.Inventory
{
    /// <summary>
    /// Page model for inventory overview and management.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IBloodInventoryService _inventoryService;

        public Dictionary<BloodType, double> InventorySummary { get; set; } = new();
        public IEnumerable<BloodInventory> ExpiringInventory { get; set; } = new List<BloodInventory>();
        public IEnumerable<BloodInventory> CurrentInventory { get; set; } = new List<BloodInventory>();

        // Summary statistics
        public int TotalBloodUnits { get; set; }
        public int AvailableUnits { get; set; }
        public int ExpiringUnits { get; set; }
        public int ExpiredUnits { get; set; }

        public IndexModel(IBloodInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Load inventory summary
                InventorySummary = await _inventoryService.GetInventorySummaryAsync();

                // Load current inventory
                CurrentInventory = await _inventoryService.GetCurrentInventoryAsync();

                // Load expiring inventory
                ExpiringInventory = await _inventoryService.GetExpiringInventoryAsync();

                // Calculate statistics
                CalculateStatistics();
            }
            catch (Exception)
            {
                // Log error and handle gracefully
                InventorySummary = new Dictionary<BloodType, double>();
                ExpiringInventory = new List<BloodInventory>();
                CurrentInventory = new List<BloodInventory>();
            }
        }

        private void CalculateStatistics()
        {
            var currentInventoryList = CurrentInventory.ToList();
            var now = DateTime.UtcNow;

            TotalBloodUnits = currentInventoryList.Count;
            AvailableUnits = currentInventoryList.Count(i => i.IsAvailable && i.ExpiryDate > now);
            ExpiringUnits = currentInventoryList.Count(i => i.IsAvailable && i.ExpiryDate > now && i.ExpiryDate <= now.AddDays(7));
            ExpiredUnits = currentInventoryList.Count(i => i.ExpiryDate <= now);
        }
    }
}

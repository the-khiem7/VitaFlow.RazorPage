using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace VitaFlow.Web.Pages.Inventory
{
    /// <summary>
    /// Page model for inventory overview.
    /// </summary>
    public class OverviewModel : PageModel
    {
        private readonly IBloodInventoryService _inventoryService;

        public Dictionary<BloodType, double> InventorySummary { get; set; } = new();
        public IEnumerable<BloodInventory> ExpiringInventory { get; set; } = new List<BloodInventory>();
        public IEnumerable<BloodInventory> CurrentInventory { get; set; } = new List<BloodInventory>();

        public OverviewModel(IBloodInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Load inventory data for display
                InventorySummary = await _inventoryService.GetInventorySummaryAsync();
                ExpiringInventory = await _inventoryService.GetExpiringInventoryAsync();
                CurrentInventory = await _inventoryService.GetCurrentInventoryAsync();
            }
            catch (Exception)
            {
                // Handle errors gracefully
                InventorySummary = new Dictionary<BloodType, double>();
                ExpiringInventory = new List<BloodInventory>();
                CurrentInventory = new List<BloodInventory>();
            }
        }
    }
}

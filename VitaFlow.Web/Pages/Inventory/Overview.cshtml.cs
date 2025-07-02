using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VitaFlow.Web.Pages.Inventory
{
    /// <summary>
    /// Page model for inventory overview.
    /// </summary>
    public class OverviewModel : PageModel
    {
        private readonly IBloodInventoryService _inventoryService;

        public Dictionary<BloodType, double> InventorySummary { get; set; }
        public IEnumerable<BloodInventory> ExpiringInventory { get; set; }

        public OverviewModel(IBloodInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task OnGetAsync()
        {
            // Load inventory data for display
        }
    }
}

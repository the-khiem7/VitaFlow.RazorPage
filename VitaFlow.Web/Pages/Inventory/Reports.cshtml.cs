using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VitaFlow.Web.Pages.Inventory
{
    /// <summary>
    /// Page model for inventory reports and analytics.
    /// </summary>
    public class ReportsModel : PageModel
    {
        private readonly IBloodInventoryService _inventoryService;

        public Dictionary<BloodType, InventoryStats> BloodTypeStats { get; set; } = new();
        public IEnumerable<DailyInventoryReport> DailyReports { get; set; } = new List<DailyInventoryReport>();

        [BindProperty(SupportsGet = true)]
        public DateTime FromDate { get; set; } = DateTime.UtcNow.AddDays(-30);

        [BindProperty(SupportsGet = true)]
        public DateTime ToDate { get; set; } = DateTime.UtcNow;

        [BindProperty(SupportsGet = true)]
        public string ReportType { get; set; } = "summary";

        // Summary statistics
        public double TotalVolume { get; set; }
        public int AvailableUnits { get; set; }
        public int ExpiringUnits { get; set; }
        public int ExpiredUnits { get; set; }

        // Component totals
        public double TotalWholeBlood { get; set; }
        public double TotalRedCells { get; set; }
        public double TotalPlasma { get; set; }
        public double TotalPlatelets { get; set; }

        public ReportsModel(IBloodInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Load current inventory
                var currentInventory = await _inventoryService.GetCurrentInventoryAsync();
                var inventoryList = currentInventory.ToList();

                // Calculate summary statistics
                CalculateSummaryStatistics(inventoryList);

                // Generate blood type statistics
                GenerateBloodTypeStatistics(inventoryList);

                // Generate daily reports if requested
                if (ReportType == "daily")
                {
                    GenerateDailyReports(inventoryList);
                }
            }
            catch (Exception)
            {
                // Handle errors gracefully
                BloodTypeStats = new Dictionary<BloodType, InventoryStats>();
                DailyReports = new List<DailyInventoryReport>();
            }
        }

        private void CalculateSummaryStatistics(List<BloodInventory> inventory)
        {
            var now = DateTime.UtcNow;

            TotalVolume = inventory.Sum(i => i.WholeBloodVolume + i.RedCellsVolume + i.PlasmaVolume + i.PlateletsVolume);
            AvailableUnits = inventory.Count(i => i.IsAvailable && i.ExpiryDate > now);
            ExpiringUnits = inventory.Count(i => i.IsAvailable && i.ExpiryDate > now && i.ExpiryDate <= now.AddDays(7));
            ExpiredUnits = inventory.Count(i => i.ExpiryDate <= now);

            TotalWholeBlood = inventory.Sum(i => i.WholeBloodVolume);
            TotalRedCells = inventory.Sum(i => i.RedCellsVolume);
            TotalPlasma = inventory.Sum(i => i.PlasmaVolume);
            TotalPlatelets = inventory.Sum(i => i.PlateletsVolume);
        }

        private void GenerateBloodTypeStatistics(List<BloodInventory> inventory)
        {
            var now = DateTime.UtcNow;

            BloodTypeStats = inventory
                .GroupBy(i => i.BloodType)
                .ToDictionary(g => g.Key, g => new InventoryStats
                {
                    BloodType = g.Key,
                    TotalVolume = g.Sum(i => i.WholeBloodVolume + i.RedCellsVolume + i.PlasmaVolume + i.PlateletsVolume),
                    TotalUnits = g.Count(),
                    AvailableUnits = g.Count(i => i.IsAvailable && i.ExpiryDate > now),
                    ExpiringUnits = g.Count(i => i.IsAvailable && i.ExpiryDate > now && i.ExpiryDate <= now.AddDays(7)),
                    WholeBloodVolume = g.Sum(i => i.WholeBloodVolume),
                    ComponentsVolume = g.Sum(i => i.RedCellsVolume + i.PlasmaVolume + i.PlateletsVolume)
                });
        }

        private void GenerateDailyReports(List<BloodInventory> inventory)
        {
            var reports = new List<DailyInventoryReport>();

            // Generate dummy daily reports for demonstration
            // In a real application, this would query actual donation/usage data
            for (var date = FromDate.Date; date <= ToDate.Date; date = date.AddDays(1))
            {
                var random = new Random(date.GetHashCode());
                reports.Add(new DailyInventoryReport
                {
                    Date = date,
                    UnitsAdded = random.Next(0, 10),
                    UnitsUsed = random.Next(0, 8),
                    UnitsExpired = random.Next(0, 3),
                    VolumeAdded = random.Next(0, 10) * 450, // Average blood unit is 450ml
                    VolumeUsed = random.Next(0, 8) * 450,
                    VolumeExpired = random.Next(0, 3) * 450
                });
            }

            DailyReports = reports.OrderBy(r => r.Date).ToList();
        }
    }
}

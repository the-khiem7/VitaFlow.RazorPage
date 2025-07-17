using System.Collections.Generic;
using VitaFlow.Core.Enums;

namespace VitaFlow.Web.ViewModels
{
    // ViewModel for inventory overview.
    public class InventoryOverviewViewModel
    {
        public Dictionary<BloodType, double>? TotalWholeBlood { get; set; }
        public Dictionary<BloodType, double>? TotalComponents { get; set; }
        public IEnumerable<InventoryItemViewModel>? ExpiringItems { get; set; }
    }

    // ViewModel for individual inventory items.
    public class InventoryItemViewModel
    {
        public BloodType BloodType { get; set; }
        public double Volume { get; set; }
        public string? StorageLocation { get; set; }
        public string? BatchNumber { get; set; }
    }
}

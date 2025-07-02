using System.Collections.Generic;
using VitaFlow.Core.Enums;

namespace VitaFlow.Web.ViewModels
{
    /// <summary>
    /// ViewModel for inventory overview.
    /// </summary>
    public class InventoryOverviewViewModel
    {
        public Dictionary<BloodType, double>? TotalWholeBlood { get; set; }
        public Dictionary<BloodType, double>? TotalComponents { get; set; }
        public IEnumerable<InventoryItemViewModel>? ExpiringItems { get; set; }
    }

    /// <summary>
    /// ViewModel for individual inventory items.
    /// </summary>
    public class InventoryItemViewModel
    {
        public BloodType BloodType { get; set; }
        public double Volume { get; set; }
        public string? StorageLocation { get; set; }
        public string? BatchNumber { get; set; }
    }
}

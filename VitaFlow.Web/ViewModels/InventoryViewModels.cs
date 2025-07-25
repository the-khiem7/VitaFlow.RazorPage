using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;

namespace VitaFlow.Web.ViewModels
{
    // ViewModel for inventory overview.
    public class InventoryOverviewViewModel
    {
        public Dictionary<BloodType, double>? TotalWholeBlood { get; set; }
        public Dictionary<BloodType, double>? TotalComponents { get; set; }
        public IEnumerable<InventoryItemViewModel>? ExpiringItems { get; set; }
        public Dictionary<BloodType, double>? InventorySummary { get; set; }
        public int TotalBloodUnits { get; set; }
        public int ExpiringUnitsCount { get; set; }
        public IEnumerable<BloodInventory>? CurrentInventory { get; set; }
    }

    // ViewModel for individual inventory items.
    public class InventoryItemViewModel
    {
        public Guid Id { get; set; }
        public BloodType BloodType { get; set; }
        public double Volume { get; set; }
        public string? StorageLocation { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime CollectionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsAvailable { get; set; }
        public double WholeBloodVolume { get; set; }
        public double RedCellsVolume { get; set; }
        public double PlasmaVolume { get; set; }
        public double PlateletsVolume { get; set; }
        public int DaysToExpiry => (ExpiryDate - DateTime.UtcNow).Days;
        public double TotalVolume => WholeBloodVolume + RedCellsVolume + PlasmaVolume + PlateletsVolume;
    }

    // ViewModel for blood units listing
    public class BloodUnitsViewModel
    {
        public IEnumerable<InventoryItemViewModel>? BloodUnits { get; set; }
        public string? FilterBloodType { get; set; }
        public string? FilterStatus { get; set; }
        public string? SearchTerm { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 20;
    }

    // ViewModel for adding new blood inventory
    public class AddInventoryViewModel
    {
        [Required(ErrorMessage = "Nhóm máu là bắt buộc")]
        public BloodType BloodType { get; set; }

        [Required(ErrorMessage = "Thể tích máu toàn phần là bắt buộc")]
        [Range(0, 5000, ErrorMessage = "Thể tích phải từ 0 đến 5000ml")]
        public double WholeBloodVolume { get; set; }

        [Range(0, 5000, ErrorMessage = "Thể tích hồng cầu phải từ 0 đến 5000ml")]
        public double RedCellsVolume { get; set; }

        [Range(0, 5000, ErrorMessage = "Thể tích huyết tương phải từ 0 đến 5000ml")]
        public double PlasmaVolume { get; set; }

        [Range(0, 5000, ErrorMessage = "Thể tích tiểu cầu phải từ 0 đến 5000ml")]
        public double PlateletsVolume { get; set; }

        [Required(ErrorMessage = "Ngày thu thập là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime CollectionDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Ngày hết hạn là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(35);

        [Required(ErrorMessage = "Vị trí lưu trữ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Vị trí lưu trữ không được vượt quá 100 ký tự")]
        public string StorageLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số lô là bắt buộc")]
        [StringLength(50, ErrorMessage = "Số lô không được vượt quá 50 ký tự")]
        public string BatchNumber { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;
    }

    // ViewModel for inventory reports
    public class InventoryReportsViewModel
    {
        public Dictionary<BloodType, InventoryStats>? BloodTypeStats { get; set; }
        public IEnumerable<DailyInventoryReport>? DailyReports { get; set; }
        public DateTime FromDate { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.UtcNow;
        public string? ReportType { get; set; } = "summary";
    }

    public class InventoryStats
    {
        public BloodType BloodType { get; set; }
        public double TotalVolume { get; set; }
        public int TotalUnits { get; set; }
        public int AvailableUnits { get; set; }
        public int ExpiringUnits { get; set; }
        public double WholeBloodVolume { get; set; }
        public double ComponentsVolume { get; set; }
    }

    public class DailyInventoryReport
    {
        public DateTime Date { get; set; }
        public int UnitsAdded { get; set; }
        public int UnitsUsed { get; set; }
        public int UnitsExpired { get; set; }
        public double VolumeAdded { get; set; }
        public double VolumeUsed { get; set; }
        public double VolumeExpired { get; set; }
    }
}

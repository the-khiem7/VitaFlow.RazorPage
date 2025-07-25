using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Core.Entities;
using VitaFlow.Web.ViewModels;
using System;
using System.Threading.Tasks;

namespace VitaFlow.Web.Pages.Inventory
{
    /// <summary>
    /// Page model for adding new blood inventory.
    /// </summary>
    public class AddModel : PageModel
    {
        private readonly IBloodInventoryService _inventoryService;

        [BindProperty]
        public AddInventoryViewModel InventoryData { get; set; } = new AddInventoryViewModel();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public AddModel(IBloodInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public void OnGet()
        {
            // Initialize default values
            InventoryData.CollectionDate = DateTime.UtcNow;
            InventoryData.ExpiryDate = DateTime.UtcNow.AddDays(35);
            InventoryData.IsAvailable = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Validate dates
                if (InventoryData.ExpiryDate <= InventoryData.CollectionDate)
                {
                    ModelState.AddModelError("InventoryData.ExpiryDate", "Ngày hết hạn phải sau ngày thu thập");
                    return Page();
                }

                if (InventoryData.CollectionDate > DateTime.UtcNow)
                {
                    ModelState.AddModelError("InventoryData.CollectionDate", "Ngày thu thập không thể trong tương lai");
                    return Page();
                }

                // Validate volumes
                var totalVolume = InventoryData.WholeBloodVolume + InventoryData.RedCellsVolume +
                                InventoryData.PlasmaVolume + InventoryData.PlateletsVolume;

                if (totalVolume <= 0)
                {
                    ModelState.AddModelError("", "Tổng thể tích máu phải lớn hơn 0");
                    return Page();
                }

                // Create BloodInventory entity
                var inventory = new BloodInventory
                {
                    Id = Guid.NewGuid(),
                    BloodType = InventoryData.BloodType,
                    WholeBloodVolume = InventoryData.WholeBloodVolume,
                    RedCellsVolume = InventoryData.RedCellsVolume,
                    PlasmaVolume = InventoryData.PlasmaVolume,
                    PlateletsVolume = InventoryData.PlateletsVolume,
                    CollectionDate = InventoryData.CollectionDate,
                    ExpiryDate = InventoryData.ExpiryDate,
                    StorageLocation = InventoryData.StorageLocation,
                    BatchNumber = InventoryData.BatchNumber,
                    IsAvailable = InventoryData.IsAvailable
                };

                // Add to inventory
                await _inventoryService.AddToInventoryAsync(inventory);

                // Set success message
                TempData["SuccessMessage"] = $"Đã thêm thành công đơn vị máu {inventory.BloodType} (Lô: {inventory.BatchNumber}) vào kho!";

                // Redirect to inventory index
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra khi thêm máu vào kho: {ex.Message}");
                return Page();
            }
        }
    }
}

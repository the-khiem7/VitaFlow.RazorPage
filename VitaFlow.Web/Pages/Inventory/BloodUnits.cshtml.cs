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
    /// Page model for blood units listing and management.
    /// </summary>
    public class BloodUnitsModel : PageModel
    {
        private readonly IBloodInventoryService _inventoryService;

        public IEnumerable<InventoryItemViewModel> BloodUnits { get; set; } = new List<InventoryItemViewModel>();

        [BindProperty(SupportsGet = true)]
        public string? FilterBloodType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 20;

        public BloodUnitsModel(IBloodInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Load all inventory
                var allInventory = await _inventoryService.GetCurrentInventoryAsync();

                // Apply filters
                var filteredInventory = ApplyFilters(allInventory);

                // Calculate pagination
                TotalItems = filteredInventory.Count();
                TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

                // Apply pagination
                var pagedInventory = filteredInventory
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize);

                // Convert to view models
                BloodUnits = pagedInventory.Select(ConvertToViewModel).ToList();
            }
            catch (Exception)
            {
                // Log error and handle gracefully
                BloodUnits = new List<InventoryItemViewModel>();
                TotalItems = 0;
                TotalPages = 0;
            }
        }

        private IEnumerable<BloodInventory> ApplyFilters(IEnumerable<BloodInventory> inventory)
        {
            var filtered = inventory.AsEnumerable();

            // Filter by blood type
            if (!string.IsNullOrEmpty(FilterBloodType) && Enum.TryParse<BloodType>(FilterBloodType, out var bloodType))
            {
                filtered = filtered.Where(i => i.BloodType == bloodType);
            }

            // Filter by status
            if (!string.IsNullOrEmpty(FilterStatus))
            {
                var now = DateTime.UtcNow;
                filtered = FilterStatus.ToLower() switch
                {
                    "available" => filtered.Where(i => i.IsAvailable && i.ExpiryDate > now),
                    "expiring" => filtered.Where(i => i.IsAvailable && i.ExpiryDate > now && i.ExpiryDate <= now.AddDays(7)),
                    "expired" => filtered.Where(i => i.ExpiryDate <= now),
                    "unavailable" => filtered.Where(i => !i.IsAvailable),
                    _ => filtered
                };
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var searchLower = SearchTerm.ToLower();
                filtered = filtered.Where(i =>
                    i.BatchNumber.ToLower().Contains(searchLower) ||
                    i.StorageLocation.ToLower().Contains(searchLower));
            }

            // Order by expiry date (expiring first)
            return filtered.OrderBy(i => i.ExpiryDate).ToList();
        }

        private InventoryItemViewModel ConvertToViewModel(BloodInventory inventory)
        {
            return new InventoryItemViewModel
            {
                Id = inventory.Id,
                BloodType = inventory.BloodType,
                BatchNumber = inventory.BatchNumber ?? "",
                StorageLocation = inventory.StorageLocation ?? "",
                CollectionDate = inventory.CollectionDate,
                ExpiryDate = inventory.ExpiryDate,
                IsAvailable = inventory.IsAvailable,
                WholeBloodVolume = inventory.WholeBloodVolume,
                RedCellsVolume = inventory.RedCellsVolume,
                PlasmaVolume = inventory.PlasmaVolume,
                PlateletsVolume = inventory.PlateletsVolume
            };
        }
    }
}

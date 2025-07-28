using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTOs;
using Services.Interfaces;

namespace RazorPage.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class BloodManagementModel : PageModel
    {
        private readonly IBloodManagementService _bloodManagementService;

        public BloodManagementModel(IBloodManagementService bloodManagementService)
        {
            _bloodManagementService = bloodManagementService;
        }

        public IEnumerable<BloodTypeResponseDTO> BloodTypes { get; set; } = new List<BloodTypeResponseDTO>();
        public IEnumerable<BloodComponentResponseDTO> BloodComponents { get; set; } = new List<BloodComponentResponseDTO>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                BloodTypes = await _bloodManagementService.GetAllBloodTypesAsync();
                BloodComponents = await _bloodManagementService.GetAllBloodComponentsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                // Log the error
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostUpdateBloodTypeAsync(Guid id, int availableUnits)
        {
            try
            {
                var result = await _bloodManagementService.UpdateBloodTypeInventoryAsync(id, availableUnits);
                if (!result)
                {
                    TempData["Error"] = "Failed to update blood type inventory.";
                    return RedirectToPage();
                }

                TempData["Success"] = "Blood type inventory updated successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating blood type inventory.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostUpdateComponentAsync(Guid id, int availableUnits)
        {
            try
            {
                var result = await _bloodManagementService.UpdateComponentInventoryAsync(id, availableUnits);
                if (!result)
                {
                    TempData["Error"] = "Failed to update component inventory.";
                    return RedirectToPage();
                }

                TempData["Success"] = "Component inventory updated successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating component inventory.";
                return RedirectToPage();
            }
        }
    }
}
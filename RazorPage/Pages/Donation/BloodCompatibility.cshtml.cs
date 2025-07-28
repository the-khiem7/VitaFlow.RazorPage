using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTOs;
using Services;
using Services.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

namespace RazorPage.Pages.Donation
{
    [AllowAnonymous]
    public class BloodCompatibilityModel : PageModel
    {
        private readonly IBloodManagementService _bloodManagementService;

        public BloodCompatibilityModel(IBloodManagementService bloodManagementService)
        {
            _bloodManagementService = bloodManagementService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Please select a blood type.")]
        public string SelectedBloodType { get; set; }

        public List<string> CanDonateTo { get; set; }
        public List<string> CanReceiveFrom { get; set; }

        public void OnGet()
        {
            // Initial page load
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var compatibilityResult = await _bloodManagementService.GetBloodTypeCompatibilityAsync(SelectedBloodType);
            CanDonateTo = compatibilityResult.CanDonateTo.ToList();
            CanReceiveFrom = compatibilityResult.CanReceiveFrom.ToList();

            return Page();
        }
    }
}
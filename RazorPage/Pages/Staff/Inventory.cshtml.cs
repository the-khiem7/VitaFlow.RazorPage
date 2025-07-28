using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTOs;
using Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RazorPage.Pages.Staff
{
    public class InventoryModel : PageModel
    {
        private readonly IBloodDonationService _bloodDonationService;

        public InventoryModel(IBloodDonationService bloodDonationService)
        {
            _bloodDonationService = bloodDonationService;
        }

        public List<BloodDonationDto> Donations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Donations = (await _bloodDonationService.GetAllAsync()).ToList();
            return Page();
        }

        public async Task<IActionResult> OnGetDonationDetailsAsync(Guid donationId)
        {
            var donationDetails = await _bloodDonationService.GetByIdAsync(donationId);
            return new JsonResult(donationDetails);
        }
    }
}
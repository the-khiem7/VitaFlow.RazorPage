using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Web.ViewModels;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Core.Enums;
using System.Collections.Generic;

namespace VitaFlow.Web.Pages.BloodRequests
{
    /// <summary>
    /// Page model for creating a blood request.
    /// </summary>
    public class CreateRequestModel : PageModel
    {
        private readonly IDonationProcessService _donationProcessService;
        private readonly IDonorService _donorService;
        private readonly INotificationService _notificationService;

        [BindProperty]
        public BloodRequestViewModel RequestVM { get; set; }

        public List<BloodType> BloodTypes { get; set; }

        public CreateRequestModel(IDonationProcessService donationProcessService, IDonorService donorService, INotificationService notificationService)
        {
            _donationProcessService = donationProcessService;
            _donorService = donorService;
            _notificationService = notificationService;
        }

        public void OnGet()
        {
            // Initialize form with blood types
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Create and process blood request logic
            return RedirectToPage("Details");
        }
    }
}

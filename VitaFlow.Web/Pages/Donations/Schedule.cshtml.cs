using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Web.ViewModels;
using VitaFlow.Core.Interfaces.Services;

namespace VitaFlow.Web.Pages.Donations
{
    /// <summary>
    /// Page model for scheduling a donation.
    /// </summary>
    public class ScheduleModel : PageModel
    {
        private readonly IDonationProcessService _donationProcessService;

        [BindProperty]
        public DonationScheduleViewModel ScheduleVM { get; set; }

        public ScheduleModel(IDonationProcessService donationProcessService)
        {
            _donationProcessService = donationProcessService;
        }

        public void OnGet()
        {
            // Initialize any data for the schedule form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Schedule donation logic
            return RedirectToPage("Confirmation");
        }
    }
}

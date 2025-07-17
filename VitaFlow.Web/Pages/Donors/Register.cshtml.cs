using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Web.ViewModels;
using VitaFlow.Core.Interfaces.Services;

namespace VitaFlow.Web.Pages.Donors
{
    /// <summary>
    /// Page model for donor registration.
    /// </summary>
    public class RegisterModel : PageModel
    {
        private readonly IDonorService _donorService;
        private readonly ILocationService _locationService;

        [BindProperty]
        public DonorRegistrationViewModel DonorVM { get; set; }

        public RegisterModel(IDonorService donorService, ILocationService locationService)
        {
            _donorService = donorService;
            _locationService = locationService;
        }

        public void OnGet()
        {
            // Initialize any data for the registration form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Register donor logic
            return RedirectToPage("Confirmation");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Services;
using System.ComponentModel.DataAnnotations;

namespace RazorPage.Pages.Donation
{
    [AllowAnonymous]
    public class LocationSearchModel : PageModel
    {
        private readonly IDonorService _donorService;

        public LocationSearchModel(IDonorService donorService)
        {
            _donorService = donorService;
        }

        [BindProperty]
        public LocationSearchInput SearchLocation { get; set; }

        [BindProperty]
        [Range(1, 100, ErrorMessage = "Search radius must be between 1 and 100 kilometers")]
        public int SearchRadius { get; set; } = 10; // Default radius of 10km

        public List<NearbyDonorViewModel> NearbyDonors { get; set; }

        public bool SearchPerformed { get; set; }

        public void OnGet()
        {
            SearchLocation = new LocationSearchInput();
            NearbyDonors = new List<NearbyDonorViewModel>();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            SearchPerformed = true;

            try
            {
                var donors = await _donorService.GetNearbyDonorsAsync(
                    SearchLocation.Latitude,
                    SearchLocation.Longitude,
                    SearchRadius
                );

                NearbyDonors = donors.Select(d => new NearbyDonorViewModel
                {
                    FullName = d.FullName,
                    BloodType = d.BloodType,
                    Distance = d.Distance,
                    Address = d.Address
                }).OrderBy(d => d.Distance).ToList();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while searching for donors. Please try again.");
            }

            return Page();
        }
    }

    public class LocationSearchInput
    {
        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
        public double Longitude { get; set; }
    }

    public class NearbyDonorViewModel
    {
        public string FullName { get; set; }
        public string BloodType { get; set; }
        public double Distance { get; set; }
        public string Address { get; set; }
    }
}
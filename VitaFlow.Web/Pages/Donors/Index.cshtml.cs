using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VitaFlow.Core.Interfaces.Services;

namespace VitaFlow.Web.Pages.Donors
{
    /// <summary>
    /// Page model for donors listing with search and filter functionality.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IDonorService _donorService;

        public IndexModel(IDonorService donorService)
        {
            _donorService = donorService;
        }

        // Properties for data binding
        public IEnumerable<Donor> Donors { get; set; } = new List<Donor>();

        // Search and filter properties
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public BloodType? SelectedBloodType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedStatus { get; set; }

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // Statistics properties
        public int TotalDonors { get; set; }
        public int ActiveDonors { get; set; }
        public int EmergencyDonors { get; set; }
        public int AvailableDonors { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Get all donors from service
                var allDonors = await _donorService.GetAllDonorsAsync();

                // Calculate statistics
                TotalDonors = allDonors.Count();
                ActiveDonors = allDonors.Count(d => d.IsActive);
                EmergencyDonors = allDonors.Count(d => d.IsEmergencyDonor);
                AvailableDonors = allDonors.Count(d => d.IsActive &&
                    (!d.NextAvailableDate.HasValue || d.NextAvailableDate <= DateTime.Now));

                // Apply filters
                var filteredDonors = allDonors.AsQueryable();

                // Search filter
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    var searchLower = SearchTerm.ToLower();
                    filteredDonors = filteredDonors.Where(d =>
                        d.FirstName.ToLower().Contains(searchLower) ||
                        d.LastName.ToLower().Contains(searchLower) ||
                        d.Email.ToLower().Contains(searchLower) ||
                        d.PhoneNumber.Contains(SearchTerm));
                }

                // Blood type filter
                if (SelectedBloodType.HasValue)
                {
                    filteredDonors = filteredDonors.Where(d => d.BloodType == SelectedBloodType.Value);
                }

                // Status filter
                if (!string.IsNullOrEmpty(SelectedStatus))
                {
                    switch (SelectedStatus.ToLower())
                    {
                        case "active":
                            filteredDonors = filteredDonors.Where(d => d.IsActive);
                            break;
                        case "emergency":
                            filteredDonors = filteredDonors.Where(d => d.IsEmergencyDonor);
                            break;
                        case "available":
                            filteredDonors = filteredDonors.Where(d => d.IsActive &&
                                (!d.NextAvailableDate.HasValue || d.NextAvailableDate <= DateTime.Now));
                            break;
                    }
                }

                // Apply pagination
                var totalFiltered = filteredDonors.Count();
                TotalPages = (int)Math.Ceiling(totalFiltered / (double)PageSize);

                Donors = filteredDonors
                    .OrderBy(d => d.LastName)
                    .ThenBy(d => d.FirstName)
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
            catch (Exception)
            {
                // Log error and show empty result
                // You might want to add proper logging here
                Donors = new List<Donor>();
                TotalDonors = 0;
                ActiveDonors = 0;
                EmergencyDonors = 0;
                AvailableDonors = 0;
            }
        }
    }
}

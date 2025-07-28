using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Services;
using Models;
using Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace RazorPage.Pages.Member
{
    [Authorize(Roles = "Member,Customer")]
    public class HistoryModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IDonorService _donorService;
        private readonly IDonationHistoryService _donationHistoryService;
        
        public HistoryModel(IUserService userService, IDonorService donorService, IDonationHistoryService donationHistoryService)
        {
            _userService = userService;
            _donorService = donorService;
            _donationHistoryService = donationHistoryService;
        }
        
        public int TotalDonations { get; set; }
        public int LivesSaved { get; set; }
        public double TotalBloodDonated { get; set; } // in ml
        public DateTime? NextEligibleDate { get; set; }
        public List<DonationHistoryResponseDto> DonationHistory { get; set; } = new List<DonationHistoryResponseDto>();
        public string BloodType { get; set; } = string.Empty;
        
        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalPages { get; set; }
        
        public async Task OnGetAsync()
        {
            // Get current user from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }
            
            try
            {
                // Get donor information
                var donors = await _donorService.GetAllAsync();
                var donor = donors.FirstOrDefault(d => d.UserId == Guid.Parse(userId));
                
                if (donor != null)
                {
                    // Get donation history
                    var allDonationHistory = await _donationHistoryService.GetAllAsync();
                    var donorHistory = allDonationHistory.Where(h => h.DonorId == donor.DonorId).ToList();
                    
                    // Set summary statistics
                    TotalDonations = donorHistory.Count;
                    LivesSaved = TotalDonations * 3; // Assuming each donation saves 3 lives
                    TotalBloodDonated = donorHistory.Sum(h => h.Quantity);
                    NextEligibleDate = donor.NextEligibleDate?.ToDateTime(new TimeOnly());
                    // Get blood type information
                    if (donor.BloodTypeId.HasValue)
                    {
                        var bloodTypes = await _donorService.GetBloodTypesAsync();
                        var bloodType = bloodTypes.FirstOrDefault(bt => bt.BloodTypeId == donor.BloodTypeId);
                        if (bloodType != null)
                        {
                            BloodType = bloodType.AboType + (bloodType.RhFactor == "+" ? " Positive" : " Negative");
                        }
                    }
                    
                    // Pagination
                    TotalPages = (int)Math.Ceiling(donorHistory.Count / (double)PageSize);
                    if (CurrentPage < 1) CurrentPage = 1;
                    if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;
                    
                    // Get paginated donation history
                    DonationHistory = donorHistory
                        .OrderByDescending(h => h.DonationDate)
                        .Skip((CurrentPage - 1) * PageSize)
                        .Take(PageSize)
                        .ToList();
                }
            }
            catch (Exception)
            {
                // Handle exception
            }
        }
    }
}
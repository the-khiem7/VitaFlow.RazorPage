using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace RazorPage.Pages.Member
{
    [Authorize(Roles = "Member,Customer")]
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public string FirstName { get; set; } = string.Empty;
        
        [BindProperty]
        public string LastName { get; set; } = string.Empty;
        
        [BindProperty]
        public string Email { get; set; } = string.Empty;
        
        [BindProperty]
        public string Phone { get; set; } = string.Empty;
        
        [BindProperty]
        public DateTime DateOfBirth { get; set; }
        
        [BindProperty]
        public string Gender { get; set; } = string.Empty;
        
        [BindProperty]
        public string Address { get; set; } = string.Empty;
        
        [BindProperty]
        public string EmergencyContact { get; set; } = string.Empty;
        
        [BindProperty]
        public string EmergencyPhone { get; set; } = string.Empty;
        
        [BindProperty]
        public bool EmailNotifications { get; set; }
        
        [BindProperty]
        public bool SmsNotifications { get; set; }
        
        [BindProperty]
        public bool AppointmentReminders { get; set; }
        
        [BindProperty]
        public bool DonationEligibility { get; set; }
        
        [BindProperty]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [BindProperty]
        public string NewPassword { get; set; } = string.Empty;
        
        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public void OnGet()
        {
            // TODO: Load user profile data from database
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            // TODO: Implement profile update logic
            // This would integrate with the user service
            
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToPage();
        }
        
        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                TempData["ErrorMessage"] = "All password fields are required.";
                return Page();
            }
            
            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match.";
                return Page();
            }
            
            // TODO: Implement password change logic
            // This would integrate with the authentication service
            
            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToPage();
        }
    }
}
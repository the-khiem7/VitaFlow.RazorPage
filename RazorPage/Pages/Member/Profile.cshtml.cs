using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Services;
using Models;
using Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace RazorPage.Pages.Member
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IDonorService _donorService;
        private readonly IDonationHistoryService _donationHistoryService;

        public ProfileModel(IUserService userService, IDonorService donorService, IDonationHistoryService donationHistoryService)
        {
            _userService = userService;
            _donorService = donorService;
            _donationHistoryService = donationHistoryService;
        }

        public string BloodType { get; set; } = string.Empty;
        public bool IsDonor { get; set; }
        public DateTime MemberSince { get; set; } = DateTime.Now;
        public int TotalDonations { get; set; }
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

        public async Task OnGetAsync()
        {
            // Get current user from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            // Get user details
            var user = await _userService.GetUserDetailAsync(Guid.Parse(userId));
            if (user != null)
            {
                // Map user properties to page model
                var names = user.FullName?.Split(' ', 2) ?? new string[] { "", "" };
                FirstName = names.Length > 0 ? names[0] : "";
                LastName = names.Length > 1 ? names[1] : "";
                Email = user.Email ?? "";
                Phone = user.Phone ?? "";
                DateOfBirth = user.DateOfBirth?.ToDateTime(new TimeOnly()) ?? DateTime.Now;

                // Try to get donor information if available
                try
                {
                    var donors = await _donorService.GetAllAsync();
                    var donor = donors.FirstOrDefault(d => d.UserId == Guid.Parse(userId));

                    if (donor != null)
                    {
                        IsDonor = true;

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

                        // Get donation history count
                        var donationHistories = await _donationHistoryService.GetAllAsync();
                        TotalDonations = donationHistories.Count(dh => dh.DonorId == donor.DonorId);
                    }
                }
                catch (Exception)
                {
                    // Handle exception if donor information is not available
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Get current user ID from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User not found.";
                return Page();
            }

            try
            {
                // Create update DTO
                var updateDto = new UserUpdateDTO
                {
                    FullName = $"{FirstName} {LastName}".Trim(),
                    Email = Email,
                    Phone = Phone,
                    DateOfBirth = DateOnly.FromDateTime(DateOfBirth)
                };

                // Update user profile
                await _userService.UpdateUserAsync(Guid.Parse(userId), updateDto);

                // Update donor information if user is a donor
                var donors = await _donorService.GetAllAsync();
                var donor = donors.FirstOrDefault(d => d.UserId == Guid.Parse(userId));

                if (donor != null)
                {
                    var donorUpdateDto = new DonorDTO.UpdateDonorDto
                    {
                        // Preserve existing values
                        UserId = donor.UserId,
                        BloodTypeId = donor.BloodTypeId,
                        Weight = donor.Weight,
                        Height = donor.Height,
                        MedicalHistory = EmergencyContact, // Using EmergencyContact as MedicalHistory
                        IsAvailable = donor.IsAvailable,
                        LastDonationDate = donor.LastDonationDate,
                        NextEligibleDate = donor.NextEligibleDate,
                        LocationId = donor.LocationId,
                        ClosestFacilityId = donor.ClosestFacilityId
                    };

                    await _donorService.UpdateAsync(donor.DonorId, donorUpdateDto);
                }

                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating profile: {ex.Message}";
            }

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

            // Get current user ID from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User not found.";
                return Page();
            }

            try
            {
                // Get current user
                var user = await _userService.GetUserDetailAsync(Guid.Parse(userId));
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return Page();
                }

                // Verify current password
                if (user.Password != CurrentPassword)
                {
                    TempData["ErrorMessage"] = "Current password is incorrect.";
                    return Page();
                }

                // Update password
                var updateDto = new UserUpdateDTO
                {
                    Password = NewPassword,
                    Email = user.Email,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    DateOfBirth = user.DateOfBirth
                };

                await _userService.UpdateUserAsync(Guid.Parse(userId), updateDto);

                TempData["SuccessMessage"] = "Password changed successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error changing password: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTOs;
using Services;
using System.Security.Claims;

namespace RazorPage.Pages.Donation
{
    [Authorize] // Yêu cầu đăng nhập để truy cập trang
    public class RegisterModel : PageModel
    {
        private readonly IBloodDonationService _donationService;
        private readonly IDonorService _donorService;

        public RegisterModel(IBloodDonationService donationService, IDonorService donorService)
        {
            _donationService = donationService;
            _donorService = donorService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string BloodType { get; set; }
            public DateTime DonationDate { get; set; }
            public string CurrentMedications { get; set; }
            public string Notes { get; set; }
        }

        public void OnGet()
        {
            // Thiết lập ngày mặc định là ngày hiện tại
            Input = new InputModel
            {
                DonationDate = DateTime.Today
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Tạo DTO cho đăng ký hiến máu mới
                var donationDto = new BloodDonationDto
                {
                    DonationDate = DateOnly.FromDateTime(Input.DonationDate),
                    Status = "Pending", // Trạng thái ban đầu
                    Notes = Input.Notes,
                    CurrentMedications = Input.CurrentMedications,
                    BloodType = Input.BloodType
                };

                // Gọi service để tạo đăng ký hiến máu mới
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _donationService.CreateAsync(new CreateBloodDonationDto
                {
                    DonationDate = donationDto.DonationDate,
                    Status = donationDto.Status,
                    Notes = donationDto.Notes,
                    CurrentMedications = donationDto.CurrentMedications,
                    BloodType = donationDto.BloodType
                }, userId);

                // Chuyển hướng đến trang lịch sử hiến máu với thông báo thành công
                TempData["SuccessMessage"] = "Blood donation registration successful!";
                return RedirectToPage("/Member/History");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error registering for blood donation. Please try again.");
                return Page();
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Web.ViewModels;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using System.Threading.Tasks;

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

        [TempData]
        public string SuccessMessage { get; set; }

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
            {
                return Page();
            }

            // Map ViewModel to Entity
            var donor = new Donor
            {
                FirstName = DonorVM.FirstName,
                LastName = DonorVM.LastName,
                DateOfBirth = DonorVM.DateOfBirth,
                Email = DonorVM.Email,
                PhoneNumber = DonorVM.PhoneNumber,
                Address = DonorVM.Address,
                BloodType = DonorVM.BloodType,
                IsEmergencyDonor = DonorVM.IsEmergencyDonor,
                MedicalNotes = DonorVM.MedicalNotes,
                IsActive = true,
                Role = UserRole.Donor // Sử dụng đúng giá trị enum
            };

            // Lưu donor qua service
            await _donorService.RegisterDonorAsync(donor);

            SuccessMessage = "Đăng ký thành công! Cảm ơn bạn đã tham gia hiến máu.";
            return RedirectToPage("Index"); // Nếu chưa có trang này, có thể chuyển về Index hoặc trang khác
        }
    }
}

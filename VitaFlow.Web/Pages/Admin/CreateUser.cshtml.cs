using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Core.Entities;
using VitaFlow.Web.ViewModels;
using System;
using System.Threading.Tasks;

namespace VitaFlow.Web.Pages.Admin
{
    /// <summary>
    /// Page model for creating new users.
    /// </summary>
    public class CreateUserModel : PageModel
    {
        private readonly IUserService _userService;

        [BindProperty]
        public UserFormViewModel UserForm { get; set; } = new UserFormViewModel();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public CreateUserModel(IUserService userService)
        {
            _userService = userService;
        }

        public void OnGet()
        {
            // Initialize form with default values
            UserForm = new UserFormViewModel
            {
                DateOfBirth = DateTime.Now.AddYears(-18) // Default to 18 years old
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
                // Check if email already exists
                var existingUser = await _userService.GetUserByEmailAsync(UserForm.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("UserForm.Email", "Email này đã được sử dụng bởi người dùng khác.");
                    return Page();
                }

                // Create new user
                var user = new User
                {
                    FirstName = UserForm.FirstName,
                    LastName = UserForm.LastName,
                    Email = UserForm.Email,
                    PhoneNumber = UserForm.PhoneNumber,
                    DateOfBirth = UserForm.DateOfBirth,
                    Address = UserForm.Address,
                    Role = UserForm.Role,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userService.CreateUserAsync(user);

                SuccessMessage = $"Tạo người dùng thành công! ID: {createdUser.Id}";
                return RedirectToPage("Users");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Có lỗi xảy ra khi tạo người dùng: {ex.Message}";
                return Page();
            }
        }
    }
}

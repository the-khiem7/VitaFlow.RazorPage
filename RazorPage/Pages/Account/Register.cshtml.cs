using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;
using Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace RazorPage.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(50, MinimumLength = 3)]
            public string Username { get; set; } = string.Empty;

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [Phone]
            public string Phone { get; set; } = string.Empty;

            [Required]
            [StringLength(20)]
            public string UserIdCard { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var registerDto = new RegisterDTO
                    {
                        Email = Input.Email,
                        Username = Input.Username,
                        Password = Input.Password,
                        FullName = Input.FullName,
                        Phone = Input.Phone,
                        UserIdCard = Input.UserIdCard,
                        DateOfBirth = DateOnly.FromDateTime(Input.DateOfBirth)
                    };

                    var result = await _authService.RegisterAsync(registerDto);

                    if (result.success)
                    {
                        TempData["SuccessMessage"] = "Registration successful! Please login with your credentials.";
                        return RedirectToPage("/Account/Login");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, result.message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred during registration.");
                }
            }

            return Page();
        }
    }
}
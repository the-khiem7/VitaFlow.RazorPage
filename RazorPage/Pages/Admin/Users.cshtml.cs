using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTOs;
using Services;
using Services.Implementations;

namespace RazorPage.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        [BindProperty]
        public RegisterDTO NewUser { get; set; }

        [BindProperty]
        public UserUpdateDTO UpdateUser { get; set; }

        public IEnumerable<User> Users { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public UsersModel(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        public async Task OnGetAsync(string searchTerm = "")
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                Users = await _userService.GetAllUsersAsync();
            }
            else
            {
                Users = await _userService.SearchUsersByNameAsync(searchTerm);
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please check your input.";
                await OnGetAsync();
                return Page();
            }

            var result = await _authService.RegisterAsync(NewUser);
            if (!result.success)
            {
                ErrorMessage = result.message;
                await OnGetAsync();
                return Page();
            }

            SuccessMessage = "User created successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please check your input.";
                await OnGetAsync();
                return Page();
            }

            var result = await _userService.UpdateUserAsync(UpdateUser);
            if (!result)
            {
                ErrorMessage = "Failed to update user.";
                await OnGetAsync();
                return Page();
            }

            SuccessMessage = "User updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetSearchAsync(string term)
        {
            await OnGetAsync(term);
            return Page();
        }
    }
}
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PantherPetManagement_Service.Services;

namespace PantherPetManagement_NguyenVanDuyKhiem.Pages.Account
{
    [AllowAnonymous]
    
    public class LoginModel : PageModel
    {
        private readonly PantherAccountService _pantherAccountService;

        public LoginModel() => _pantherAccountService ??= new PantherAccountService();

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;


        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
            var pantherAccount = await _pantherAccountService.GetAccount(Email, Password);

            if (pantherAccount != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Email),
                    new Claim(ClaimTypes.Role, pantherAccount.RoleId.ToString()),
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                Response.Cookies.Append("UserName", pantherAccount.UserName);

                return RedirectToPage("/PantherProfiles/Index");
            }
            else
            {
                TempData["Message"] = "Invalid Email or Password!";
            }

            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Page();
        }
    }
}

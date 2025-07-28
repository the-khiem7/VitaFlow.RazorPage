using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using Models.DTOs;
using System.ComponentModel.DataAnnotations;
using Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Models;

namespace RazorPage.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public LoginModel(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    var loginDto = new LoginDTO
                    {
                        Email = Input.Email,
                        Password = Input.Password
                    };

                    var result = await _authService.LoginAsync(loginDto);
                    
                    if (result.success)
                    {
                        // Get user details for creating claims
                        var user = await _authService.GetUserByEmailAsync(Input.Email);
                        
                        if (user != null)
                        {
                            // Create claims for the authenticated user
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim(ClaimTypes.Name, user.Username ?? user.Email),
                                new Claim(ClaimTypes.Role, user.Role ?? "Member")
                            };

                            // Add full name if available
                            if (!string.IsNullOrEmpty(user.FullName))
                            {
                                claims.Add(new Claim(ClaimTypes.GivenName, user.FullName));
                            }

                            // Create claims identity
                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                            // Configure authentication properties
                            var authProperties = new AuthenticationProperties
                            {
                                IsPersistent = Input.RememberMe,
                                ExpiresUtc = Input.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                            };

                            // Sign in the user with cookie authentication
                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                claimsPrincipal,
                                authProperties);

                            // Redirect to return URL or home page
                            return LocalRedirect(returnUrl ?? "/");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "User not found after successful login.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, result.message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred during login.");
                }
            }

            return Page();
        }
    }
}
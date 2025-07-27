using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace RazorPage.Pages.Member
{
    [Authorize(Roles = "Member,Customer")]
    public class HealthModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
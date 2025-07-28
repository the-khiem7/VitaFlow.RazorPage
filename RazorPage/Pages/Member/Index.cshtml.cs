using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace RazorPage.Pages.Member
{
    [Authorize(Roles = "Member,Customer")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
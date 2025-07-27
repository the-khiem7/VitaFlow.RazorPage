using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace RazorPage.Pages.Staff
{
    [Authorize(Roles = "Staff")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
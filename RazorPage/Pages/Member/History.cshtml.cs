using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace RazorPage.Pages.Member
{
    [Authorize(Roles = "Member,Customer")]
    public class HistoryModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
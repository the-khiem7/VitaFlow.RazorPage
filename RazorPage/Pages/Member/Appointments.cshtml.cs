using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace RazorPage.Pages.Member
{
    [Authorize(Roles = "Member,Customer")]
    public class AppointmentsModel : PageModel
    {
        [BindProperty]
        public DateTime AppointmentDate { get; set; }
        
        [BindProperty]
        public string AppointmentTime { get; set; } = string.Empty;
        
        [BindProperty]
        public string Location { get; set; } = string.Empty;
        
        [BindProperty]
        public string Notes { get; set; } = string.Empty;

        public void OnGet()
        {
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            // TODO: Implement appointment scheduling logic
            // This would integrate with the appointment service
            
            TempData["SuccessMessage"] = "Appointment scheduled successfully!";
            return RedirectToPage();
        }
    }
}
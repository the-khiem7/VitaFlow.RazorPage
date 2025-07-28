using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTOs;
using Models.Enums;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorPage.Pages.Staff
{
    public class BloodRequestsModel : PageModel
    {
        private readonly IBloodRequestService _bloodRequestService;

        public BloodRequestsModel(IBloodRequestService bloodRequestService)
        {
            _bloodRequestService = bloodRequestService;
        }

        public List<BloodRequestResponseDTO> BloodRequests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var requests = await _bloodRequestService.GetAllRequestsAsync();
            BloodRequests = requests.Select(r => new BloodRequestResponseDTO
            {
                RequestId = r.RequestId,
                PatientName = r.Recipient?.User?.FullName ?? "Unknown",
                BloodType = r.BloodTypeRequiredNavigation?.AboType + r.BloodTypeRequiredNavigation?.RhFactor ?? "Unknown",
                Components = "Whole Blood", // Default or you can add logic to determine components
                UrgencyLevel = r.UrgencyLevel ?? "Normal",
                RequestDate = r.RequestDate,
                Status = r.Status ?? "Pending",
                Description = r.Description ?? "",
                QuantityNeeded = r.QuantityNeeded,
                RecipientId = r.RecipientId,
                BloodTypeRequired = r.BloodTypeRequired
            }).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid requestId)
        {
            var staffIdValue = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(staffIdValue) || !Guid.TryParse(staffIdValue, out Guid staffId))
            {
                ModelState.AddModelError(string.Empty, "Staff ID is required");
                return Page();
            }

            var result = await _bloodRequestService.ApproveBloodRequestAsync(requestId, staffId);
            if (!result.success)
            {
                ModelState.AddModelError(string.Empty, result.message);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid requestId, string reason)
        {
            var staffIdValue = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(staffIdValue) || !Guid.TryParse(staffIdValue, out Guid staffId))
            {
                ModelState.AddModelError(string.Empty, "Staff ID is required");
                return Page();
            }

            var rejectDto = new BloodRequestRejectDTO
            {
                RejectionReason = reason
            };

            var result = await _bloodRequestService.RejectBloodRequestAsync(requestId, rejectDto, staffId);
            if (!result.success)
            {
                ModelState.AddModelError(string.Empty, result.message);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetDetailsAsync(Guid requestId)
        {
            var request = await _bloodRequestService.GetRequestByIdAsync(requestId);
            return new JsonResult(request);
        }
    }
}
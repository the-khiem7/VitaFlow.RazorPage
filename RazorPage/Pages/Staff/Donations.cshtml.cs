using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTOs;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorPage.Pages.Staff
{
    public class DonationsModel : PageModel
    {
        private readonly IBloodDonationService _bloodDonationService;
        private readonly IHealthCheckService _healthCheckService;
        private readonly IAppointmentService _appointmentService;

        public DonationsModel(
            IBloodDonationService bloodDonationService,
            IHealthCheckService healthCheckService,
            IAppointmentService appointmentService)
        {
            _bloodDonationService = bloodDonationService;
            _healthCheckService = healthCheckService;
            _appointmentService = appointmentService;
        }

        public List<BloodDonationProcessDTO> DonationProcesses { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var donations = await _bloodDonationService.GetAllAsync();
            
            // Convert BloodDonationDto to BloodDonationProcessDTO
            DonationProcesses = new List<BloodDonationProcessDTO>();
            
            foreach (var donation in donations)
            {
                // Get donation process information using AppointmentService
                var donationProcess = await _appointmentService.GetLatestDonationProcessByDonorIdAsync(donation.DonorId);
                var healthCheckStatus = "Pending";
                DateOnly? healthCheckDate = null;
                
                if (donationProcess != null && donationProcess.HealthCheckStatus != null)
                {
                    healthCheckStatus = donationProcess.HealthCheckStatus;
                    healthCheckDate = donationProcess.HealthCheckDate;
                }
                
                DonationProcesses.Add(new BloodDonationProcessDTO
                {
                    DonationId = donation.DonationId,
                    Status = donation.Status,
                    HealthCheckDate = healthCheckDate,
                    HealthCheckStatus = healthCheckStatus,
                    CertificateId = donation.CertificateId,
                    Notes = donation.Notes,
                    DonorId = donation.DonorId,
                    // Add additional properties needed for display
                    DonorName = donation.DonorName,
                    BloodType = donation.BloodType,
                    AppointmentDate = donation.DonationDate
                });
            }
            
            return Page();
        }

        // get detail
        public async Task<IActionResult> OnGetDonationDetailsAsync(Guid donationId)
        {
            var donationDetails = await _bloodDonationService.GetByIdAsync(donationId);
            if (donationDetails == null)
            {
                return NotFound();
            }
            
            // Get donation process information using AppointmentService
            var donationProcess = await _appointmentService.GetLatestDonationProcessByDonorIdAsync(donationDetails.DonorId);
            
            var processDto = new BloodDonationProcessDTO
            {
                DonationId = donationDetails.DonationId,
                Status = donationDetails.Status,
                CertificateId = donationDetails.CertificateId,
                Notes = donationDetails.Notes,
                DonorId = donationDetails.DonorId,
                DonorName = donationDetails.DonorName,
                BloodType = donationDetails.BloodType,
                AppointmentDate = donationDetails.DonationDate,
                PhoneNumber = donationDetails.PhoneNumber,
                Email = donationDetails.Email,
                Address = donationDetails.Address,
                DateOfBirth = donationDetails.DateOfBirth,
                Quantity = donationDetails.Quantity,
                FullName = donationDetails.FullName,
                CurrentMedications = donationDetails.CurrentMedications,
                LastDonationDate = donationDetails.LastDonationDate
            };
            
            if (donationProcess != null)
            {
                processDto.HealthCheckDate = donationProcess.HealthCheckDate;
                processDto.HealthCheckStatus = donationProcess.HealthCheckStatus;
            }
            else
            {
                processDto.HealthCheckStatus = "Pending";
            }
            
            return new JsonResult(processDto);
        }
    }
}
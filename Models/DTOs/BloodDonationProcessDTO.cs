using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class BloodDonationProcessDTO
    {
        public Guid DonationId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateOnly? HealthCheckDate { get; set; }
        public string HealthCheckStatus { get; set; } = string.Empty;
        public Guid? CertificateId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Guid? DonorId { get; set; }
        
        // Additional properties for display
        public string DonorName { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public DateOnly? AppointmentDate { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public int? Quantity { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string CurrentMedications { get; set; } = string.Empty;
        public DateOnly? LastDonationDate { get; set; }
    }
}

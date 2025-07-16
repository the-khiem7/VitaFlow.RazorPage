using System;
using System.Collections.Generic;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Entities;

namespace VitaFlow.Core.Entities
{
    // Represents a donor in the system.
    public class Donor : User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public BloodType BloodType { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public DateTime? NextAvailableDate { get; set; }
        public bool IsEmergencyDonor { get; set; }
        public List<BloodDonation> DonationHistory { get; set; } = new List<BloodDonation>();
        public string MedicalNotes { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Entities
{
    // Represents a blood request.
    public class BloodRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RecipientId { get; set; }
        public Recipient? Recipient { get; set; }
        public BloodType RequiredBloodType { get; set; }
        public double VolumeNeeded { get; set; }
        public bool IsWholeBloodNeeded { get; set; }
        public bool IsRedCellsNeeded { get; set; }
        public bool IsPlasmaNeeded { get; set; }
        public bool IsPlateletsNeeded { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public bool IsEmergency { get; set; }
        public string? MedicalNotes { get; set; }
        public List<BloodDonation>? AssignedDonations { get; set; }
    }
}

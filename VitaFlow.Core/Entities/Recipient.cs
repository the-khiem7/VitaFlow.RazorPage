using System.Collections.Generic;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Entities;

namespace VitaFlow.Core.Entities
{
    // Represents a recipient in the system.
    public class Recipient : User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public BloodType BloodType { get; set; }
        public string MedicalNotes { get; set; } = string.Empty;
        public List<BloodRequest> RequestHistory { get; set; } = new List<BloodRequest>();
    }
}

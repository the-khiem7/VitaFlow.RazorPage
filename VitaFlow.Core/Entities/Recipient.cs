using System.Collections.Generic;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Entities;

namespace VitaFlow.Core.Entities
{
    /// <summary>
    /// Represents a recipient in the system.
    /// </summary>
    public class Recipient : User
    {
        public BloodType BloodType { get; set; }
        public string MedicalNotes { get; set; } = string.Empty;
        public List<BloodRequest> RequestHistory { get; set; } = new List<BloodRequest>();
    }
}

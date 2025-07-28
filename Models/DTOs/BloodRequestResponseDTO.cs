using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class BloodRequestResponseDTO
    {
        public Guid RequestId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string Components { get; set; } = string.Empty;
        public string UrgencyLevel { get; set; } = string.Empty;
        public DateOnly? RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? QuantityNeeded { get; set; }
        public Guid? RecipientId { get; set; }
        public Guid? BloodTypeRequired { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using VitaFlow.Core.Enums;

namespace VitaFlow.Web.ViewModels
{
    /// <summary>
    /// ViewModel for blood request creation.
    /// </summary>
    public class BloodRequestViewModel
    {
        public int RecipientId { get; set; }

        [Required]
        public BloodType RequiredBloodType { get; set; }

        [Required]
        [Range(1, 5000)]
        public double VolumeNeeded { get; set; }

        public bool IsWholeBloodNeeded { get; set; }
        public bool IsRedCellsNeeded { get; set; }
        public bool IsPlasmaNeeded { get; set; }
        public bool IsPlateletsNeeded { get; set; }

        [DataType(DataType.Date)]
        public DateTime? RequiredByDate { get; set; }

        public bool IsEmergency { get; set; }
        public string MedicalNotes { get; set; }
    }
}

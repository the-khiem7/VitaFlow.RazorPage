using System;
using System.ComponentModel.DataAnnotations;
using VitaFlow.Core.Enums;

namespace VitaFlow.Web.ViewModels
{
    /// <summary>
    /// ViewModel for donor registration.
    /// </summary>
    public class DonorRegistrationViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public BloodType BloodType { get; set; }

        [Display(Name = "Available for emergency donations")]
        public bool IsEmergencyDonor { get; set; }

        public string MedicalNotes { get; set; }
    }
}

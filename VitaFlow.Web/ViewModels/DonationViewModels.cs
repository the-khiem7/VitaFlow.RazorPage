using System;
using System.ComponentModel.DataAnnotations;

namespace VitaFlow.Web.ViewModels
{
    /// <summary>
    /// ViewModel for scheduling a donation.
    /// </summary>
    public class DonationScheduleViewModel
    {
        public int DonorId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; }

        public string Notes { get; set; }
    }
}

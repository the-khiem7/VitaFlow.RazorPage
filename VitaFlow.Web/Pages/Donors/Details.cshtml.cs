using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using VitaFlow.Core.Interfaces.Services;

namespace VitaFlow.Web.Pages.Donors
{
    /// <summary>
    /// Page model for donor details view.
    /// </summary>
    public class DetailsModel : PageModel
    {
        private readonly IDonorService _donorService;

        public DetailsModel(IDonorService donorService)
        {
            _donorService = donorService;
        }

        public Donor Donor { get; set; } = null!;
        public IEnumerable<BloodDonation> RecentDonations { get; set; } = new List<BloodDonation>();
        public int DonationCount { get; set; }
        public double TotalVolumeML { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                // Get all donors and find the one we need (since GetDonorByIdAsync uses int in interface)
                var allDonors = await _donorService.GetAllDonorsAsync();
                Donor = allDonors.FirstOrDefault(d => d.Id == id)!;

                if (Donor == null)
                {
                    return NotFound("Không tìm thấy thông tin người hiến máu.");
                }

                // Use the donor's donation history property instead of service method for now
                RecentDonations = Donor.DonationHistory?.OrderByDescending(d => d.DonationDate).ToList() ?? new List<BloodDonation>();

                // Calculate statistics
                DonationCount = RecentDonations.Count();
                TotalVolumeML = RecentDonations.Where(d => d.Status == DonationStatus.Completed)
                                              .Sum(d => d.Volume);

                return Page();
            }
            catch (Exception)
            {
                return NotFound("Có lỗi xảy ra khi tải thông tin người hiến máu.");
            }
        }

        /// <summary>
        /// Calculate age from date of birth
        /// </summary>
        public int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;

            // Check if birthday has occurred this year
            if (dateOfBirth.Date > today.AddYears(-age))
                age--;

            return age;
        }

        /// <summary>
        /// Calculate days since last donation
        /// </summary>
        public int CalculateDaysSinceLastDonation(DateTime lastDonationDate)
        {
            return (DateTime.Now - lastDonationDate).Days;
        }

        /// <summary>
        /// Calculate days until donor can donate again
        /// </summary>
        public int CalculateDaysUntilAvailable(DateTime nextAvailableDate)
        {
            var days = (nextAvailableDate - DateTime.Now).Days;
            return Math.Max(0, days);
        }

        /// <summary>
        /// Check if donor can donate now
        /// </summary>
        public bool CanDonateNow()
        {
            if (!Donor.IsActive)
                return false;

            if (!Donor.NextAvailableDate.HasValue)
                return true;

            return Donor.NextAvailableDate.Value <= DateTime.Now;
        }

        /// <summary>
        /// Get compatible recipient blood types for this donor
        /// </summary>
        public List<BloodType> GetCompatibleRecipientTypes(BloodType donorBloodType)
        {
            // Blood compatibility rules for whole blood donation
            return donorBloodType switch
            {
                BloodType.ONegative => new List<BloodType>
                {
                    BloodType.OPositive, BloodType.ONegative,
                    BloodType.APositive, BloodType.ANegative,
                    BloodType.BPositive, BloodType.BNegative,
                    BloodType.ABPositive, BloodType.ABNegative
                },
                BloodType.OPositive => new List<BloodType>
                {
                    BloodType.OPositive, BloodType.APositive,
                    BloodType.BPositive, BloodType.ABPositive
                },
                BloodType.ANegative => new List<BloodType>
                {
                    BloodType.APositive, BloodType.ANegative,
                    BloodType.ABPositive, BloodType.ABNegative
                },
                BloodType.APositive => new List<BloodType>
                {
                    BloodType.APositive, BloodType.ABPositive
                },
                BloodType.BNegative => new List<BloodType>
                {
                    BloodType.BPositive, BloodType.BNegative,
                    BloodType.ABPositive, BloodType.ABNegative
                },
                BloodType.BPositive => new List<BloodType>
                {
                    BloodType.BPositive, BloodType.ABPositive
                },
                BloodType.ABNegative => new List<BloodType>
                {
                    BloodType.ABPositive, BloodType.ABNegative
                },
                BloodType.ABPositive => new List<BloodType>
                {
                    BloodType.ABPositive
                },
                _ => new List<BloodType>()
            };
        }

        /// <summary>
        /// Get formatted donation status text
        /// </summary>
        public string GetDonationStatusText(DonationStatus status)
        {
            return status switch
            {
                DonationStatus.Processing => "Đang xử lý",
                DonationStatus.Scheduled => "Đã lên lịch",
                DonationStatus.Completed => "Hoàn thành",
                DonationStatus.Canceled => "Đã hủy",
                DonationStatus.Failed => "Thất bại",
                DonationStatus.Expired => "Đã hết hạn",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Get CSS class for donation status
        /// </summary>
        public string GetDonationStatusClass(DonationStatus status)
        {
            return status switch
            {
                DonationStatus.Completed => "bg-green-100 text-green-800",
                DonationStatus.Processing => "bg-yellow-100 text-yellow-800",
                DonationStatus.Scheduled => "bg-blue-100 text-blue-800",
                DonationStatus.Canceled => "bg-red-100 text-red-800",
                DonationStatus.Failed => "bg-red-100 text-red-800",
                DonationStatus.Expired => "bg-gray-100 text-gray-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }
    }
}

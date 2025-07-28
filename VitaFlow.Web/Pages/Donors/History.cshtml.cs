using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VitaFlow.Core.Interfaces.Services;

namespace VitaFlow.Web.Pages.Donors
{
    /// <summary>
    /// Page model for donor donation history view.
    /// </summary>
    public class HistoryModel : PageModel
    {
        private readonly IDonorService _donorService;

        public HistoryModel(IDonorService donorService)
        {
            _donorService = donorService;
        }

        // Donor information
        public Guid DonorId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public BloodType BloodType { get; set; }

        // All donations for this donor
        public IEnumerable<BloodDonation> AllDonations { get; set; } = new List<BloodDonation>();
        public IEnumerable<BloodDonation> FilteredDonations { get; set; } = new List<BloodDonation>();

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DonationStatus? SelectedStatus { get; set; }

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 15;
        public int TotalPages { get; set; }

        // Statistics properties
        public int TotalDonations { get; set; }
        public double TotalVolumeML { get; set; }
        public int CompletedDonations { get; set; }
        public int ScheduledDonations { get; set; }
        public int ProcessingDonations { get; set; }
        public int CancelledFailedDonations { get; set; }

        // Helper properties
        public bool HasFilters => FromDate.HasValue || ToDate.HasValue || SelectedStatus.HasValue;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                DonorId = id;

                // Get donor information
                var allDonors = await _donorService.GetAllDonorsAsync();
                var donor = allDonors.FirstOrDefault(d => d.Id == id);

                if (donor == null)
                {
                    return NotFound("Không tìm thấy thông tin người hiến máu.");
                }

                DonorName = $"{donor.FirstName} {donor.LastName}";
                BloodType = donor.BloodType;

                // Get donation history from donor's DonationHistory property
                AllDonations = donor.DonationHistory?.OrderByDescending(d => d.DonationDate).ToList() ?? new List<BloodDonation>();

                // Apply filters
                var filteredDonations = AllDonations.AsQueryable();

                if (FromDate.HasValue)
                {
                    filteredDonations = filteredDonations.Where(d => d.DonationDate.Date >= FromDate.Value.Date);
                }

                if (ToDate.HasValue)
                {
                    filteredDonations = filteredDonations.Where(d => d.DonationDate.Date <= ToDate.Value.Date);
                }

                if (SelectedStatus.HasValue)
                {
                    filteredDonations = filteredDonations.Where(d => d.Status == SelectedStatus.Value);
                }

                // Apply pagination
                var totalFiltered = filteredDonations.Count();
                TotalPages = (int)Math.Ceiling(totalFiltered / (double)PageSize);

                FilteredDonations = filteredDonations
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // Calculate statistics
                TotalDonations = AllDonations.Count();
                TotalVolumeML = AllDonations.Where(d => d.Status == DonationStatus.Completed).Sum(d => d.Volume);
                CompletedDonations = AllDonations.Count(d => d.Status == DonationStatus.Completed);
                ScheduledDonations = AllDonations.Count(d => d.Status == DonationStatus.Scheduled);
                ProcessingDonations = AllDonations.Count(d => d.Status == DonationStatus.Processing);
                CancelledFailedDonations = AllDonations.Count(d => d.Status == DonationStatus.Canceled || d.Status == DonationStatus.Failed);

                return Page();
            }
            catch (Exception)
            {
                return NotFound("Có lỗi xảy ra khi tải lịch sử hiến máu.");
            }
        }

        /// <summary>
        /// Get localized status text
        /// </summary>
        public string GetStatusText(string status)
        {
            return status switch
            {
                nameof(DonationStatus.Completed) => "Hoàn thành",
                nameof(DonationStatus.Scheduled) => "Đã lên lịch",
                nameof(DonationStatus.Processing) => "Đang xử lý",
                nameof(DonationStatus.Canceled) => "Đã hủy",
                nameof(DonationStatus.Failed) => "Thất bại",
                nameof(DonationStatus.Expired) => "Đã hết hạn",
                _ => status
            };
        }

        /// <summary>
        /// Get CSS class for donation status
        /// </summary>
        public string GetStatusClass(DonationStatus status)
        {
            return status switch
            {
                DonationStatus.Completed => "bg-green-100 text-green-800",
                DonationStatus.Scheduled => "bg-blue-100 text-blue-800",
                DonationStatus.Processing => "bg-yellow-100 text-yellow-800",
                DonationStatus.Canceled => "bg-red-100 text-red-800",
                DonationStatus.Failed => "bg-red-100 text-red-800",
                DonationStatus.Expired => "bg-gray-100 text-gray-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }

        /// <summary>
        /// Get volume category description
        /// </summary>
        public string GetVolumeCategory(double volume)
        {
            return volume switch
            {
                >= 450 => "Hiến máu toàn phần",
                >= 300 and < 450 => "Hiến máu một phần",
                >= 200 and < 300 => "Hiến máu nhỏ",
                < 200 and > 0 => "Hiến máu thử nghiệm",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Calculate average donation interval in days
        /// </summary>
        public int GetAverageDonationInterval()
        {
            var completedDonations = AllDonations
                .Where(d => d.Status == DonationStatus.Completed)
                .OrderBy(d => d.DonationDate)
                .ToList();

            if (completedDonations.Count < 2)
                return 0;

            var totalDays = 0;
            for (int i = 1; i < completedDonations.Count; i++)
            {
                totalDays += (completedDonations[i].DonationDate - completedDonations[i - 1].DonationDate).Days;
            }

            return totalDays / (completedDonations.Count - 1);
        }

        /// <summary>
        /// Calculate donation frequency per year
        /// </summary>
        public string GetDonationFrequencyPerYear()
        {
            var completedDonations = AllDonations
                .Where(d => d.Status == DonationStatus.Completed)
                .ToList();

            if (!completedDonations.Any())
                return "0";

            var firstDonation = completedDonations.Min(d => d.DonationDate);
            var lastDonation = completedDonations.Max(d => d.DonationDate);
            var totalDays = (lastDonation - firstDonation).TotalDays;

            if (totalDays < 365)
                return completedDonations.Count.ToString();

            var years = totalDays / 365.25;
            var frequency = completedDonations.Count / years;

            return frequency.ToString("F1");
        }

        /// <summary>
        /// Get donation trends by year
        /// </summary>
        public Dictionary<int, int> GetDonationTrendsByYear()
        {
            return AllDonations
                .Where(d => d.Status == DonationStatus.Completed)
                .GroupBy(d => d.DonationDate.Year)
                .ToDictionary(g => g.Key, g => g.Count())
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Get donations by month for the current year
        /// </summary>
        public Dictionary<int, int> GetDonationsByMonthThisYear()
        {
            var currentYear = DateTime.Now.Year;
            return AllDonations
                .Where(d => d.Status == DonationStatus.Completed && d.DonationDate.Year == currentYear)
                .GroupBy(d => d.DonationDate.Month)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Check if donor is due for next donation
        /// </summary>
        public bool IsDueForNextDonation()
        {
            var lastCompletedDonation = AllDonations
                .Where(d => d.Status == DonationStatus.Completed)
                .OrderByDescending(d => d.DonationDate)
                .FirstOrDefault();

            if (lastCompletedDonation == null)
                return true; // Never donated, can donate anytime

            // Typically 8-12 weeks between whole blood donations
            var minimumInterval = TimeSpan.FromDays(56); // 8 weeks
            return DateTime.Now - lastCompletedDonation.DonationDate >= minimumInterval;
        }

        /// <summary>
        /// Get next eligible donation date
        /// </summary>
        public DateTime? GetNextEligibleDonationDate()
        {
            var lastCompletedDonation = AllDonations
                .Where(d => d.Status == DonationStatus.Completed)
                .OrderByDescending(d => d.DonationDate)
                .FirstOrDefault();

            if (lastCompletedDonation == null)
                return DateTime.Now; // Never donated, can donate anytime

            return lastCompletedDonation.DonationDate.AddDays(56); // 8 weeks
        }
    }
}

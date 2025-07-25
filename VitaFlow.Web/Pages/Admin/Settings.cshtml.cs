using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Web.ViewModels;
using System;
using System.Threading.Tasks;

namespace VitaFlow.Web.Pages.Admin
{
    /// <summary>
    /// Page model for application settings and management.
    /// </summary>
    public class SettingsModel : PageModel
    {
        [BindProperty]
        public ApplicationSettingsViewModel Settings { get; set; } = new ApplicationSettingsViewModel();

        public SystemStatusViewModel SystemStatus { get; set; } = new SystemStatusViewModel();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Load current settings (in a real app, these would come from a database or configuration)
            Settings = new ApplicationSettingsViewModel
            {
                ApplicationName = "VitaFlow",
                ContactEmail = "admin@vitaflow.com",
                ContactPhone = "1900 1234",
                EnableEmailNotifications = true,
                EnableSmsNotifications = false,
                EnableMaintenanceMode = false,
                LogRetentionDays = 90,
                DefaultPageSize = 20,
                TimeZone = "Asia/Ho_Chi_Minh",
                DefaultLanguage = "vi-VN"
            };

            // Load system status
            await LoadSystemStatusAsync();
        }

        public async Task<IActionResult> OnPostSaveSettingsAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSystemStatusAsync();
                return Page();
            }

            try
            {
                // In a real app, save settings to database or configuration
                SuccessMessage = "Cài đặt đã được lưu thành công!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Có lỗi xảy ra khi lưu cài đặt: {ex.Message}";
                await LoadSystemStatusAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostBackupDatabaseAsync()
        {
            try
            {
                // In a real app, implement database backup logic
                await Task.Delay(1000); // Simulate backup process
                SuccessMessage = "Sao lưu cơ sở dữ liệu thành công!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Có lỗi xảy ra khi sao lưu: {ex.Message}";
            }

            await LoadSystemStatusAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostClearCacheAsync()
        {
            try
            {
                // In a real app, implement cache clearing logic
                await Task.Delay(500); // Simulate cache clearing
                SuccessMessage = "Xóa cache thành công!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Có lỗi xảy ra khi xóa cache: {ex.Message}";
            }

            await LoadSystemStatusAsync();
            return RedirectToPage();
        }

        private async Task LoadSystemStatusAsync()
        {
            // In a real app, get actual system status
            SystemStatus = new SystemStatusViewModel
            {
                IsDatabaseConnected = true,
                IsEmailServiceRunning = true,
                IsNotificationServiceRunning = true,
                LastBackupDate = DateTime.Now.AddDays(-1),
                DatabaseVersion = "PostgreSQL 14.5",
                ApplicationVersion = "1.0.0",
                Uptime = TimeSpan.FromHours(24),
                MemoryUsage = 65.5,
                CpuUsage = 12.3
            };

            await Task.CompletedTask;
        }
    }
}

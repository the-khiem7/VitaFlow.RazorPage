using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;

namespace VitaFlow.Web.ViewModels
{
    // ViewModel for admin dashboard.
    public class AdminDashboardViewModel
    {
        public Dictionary<BloodType, int>? DonorCountsByBloodType { get; set; }
        public Dictionary<RequestStatus, int>? RequestStatusSummary { get; set; }
        public Dictionary<string, int>? DonationsPerMonth { get; set; }
        public Dictionary<BloodType, double>? InventoryLevels { get; set; }
        public int EmergencyRequestCount { get; set; }
    }

    // ViewModel for user management in admin panel.
    public class UserManagementViewModel
    {
        public IEnumerable<UserListItemViewModel>? Users { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalUsers { get; set; }
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public UserRole? RoleFilter { get; set; }
        public bool? IsActiveFilter { get; set; }
        public Dictionary<UserRole, int>? UserCountSummary { get; set; }
    }

    // ViewModel for individual user in the list.
    public class UserListItemViewModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string RoleDisplayName => Role switch
        {
            UserRole.Admin => "Quản trị viên",
            UserRole.Staff => "Nhân viên",
            UserRole.Donor => "Người hiến máu",
            UserRole.Recipient => "Người nhận máu",
            _ => Role.ToString()
        };
    }

    // ViewModel for creating/editing user.
    public class UserFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Họ là bắt buộc")]
        [StringLength(50, ErrorMessage = "Họ không được vượt quá 50 ký tự")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public UserRole Role { get; set; }

        public bool IsEdit => Id.HasValue;
        public string FormTitle => IsEdit ? "Chỉnh sửa người dùng" : "Tạo người dùng mới";
    }

    // ViewModel for admin application management.
    public class ApplicationManagementViewModel
    {
        public SystemStatusViewModel? SystemStatus { get; set; }
        public IEnumerable<AuditLogViewModel>? RecentAuditLogs { get; set; }
        public ApplicationSettingsViewModel? Settings { get; set; }
        public SystemStatisticsViewModel? Statistics { get; set; }
    }

    // ViewModel for system status information.
    public class SystemStatusViewModel
    {
        public bool IsDatabaseConnected { get; set; }
        public bool IsEmailServiceRunning { get; set; }
        public bool IsNotificationServiceRunning { get; set; }
        public DateTime LastBackupDate { get; set; }
        public string DatabaseVersion { get; set; } = string.Empty;
        public string ApplicationVersion { get; set; } = string.Empty;
        public TimeSpan Uptime { get; set; }
        public double MemoryUsage { get; set; }
        public double CpuUsage { get; set; }
    }

    // ViewModel for audit logs.
    public class AuditLogViewModel
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }

    // ViewModel for application settings.
    public class ApplicationSettingsViewModel
    {
        [Required(ErrorMessage = "Tên ứng dụng là bắt buộc")]
        public string ApplicationName { get; set; } = "VitaFlow";

        [Required(ErrorMessage = "Email liên hệ là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email liên hệ không hợp lệ")]
        public string ContactEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại liên hệ là bắt buộc")]
        public string ContactPhone { get; set; } = string.Empty;

        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnableSmsNotifications { get; set; } = false;
        public bool EnableMaintenanceMode { get; set; } = false;

        [Range(1, 365, ErrorMessage = "Thời gian lưu trữ log phải từ 1 đến 365 ngày")]
        public int LogRetentionDays { get; set; } = 90;

        [Range(1, 1000, ErrorMessage = "Kích thước trang phải từ 1 đến 1000")]
        public int DefaultPageSize { get; set; } = 20;

        public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
        public string DefaultLanguage { get; set; } = "vi-VN";
    }

    // ViewModel for system statistics.
    public class SystemStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalDonors { get; set; }
        public int TotalRecipients { get; set; }
        public int TotalStaff { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalDonationsThisMonth { get; set; }
        public int TotalRequestsThisMonth { get; set; }
        public int ActiveEmergencyRequests { get; set; }
        public double TotalBloodInventory { get; set; }
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public DateTime LastDonationDate { get; set; }
        public DateTime LastRequestDate { get; set; }
    }

    // ViewModel for user role change.
    public class ChangeUserRoleViewModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public UserRole CurrentRole { get; set; }

        [Required(ErrorMessage = "Vai trò mới là bắt buộc")]
        public UserRole NewRole { get; set; }

        public string Reason { get; set; } = string.Empty;
    }

    // ViewModel for bulk user operations.
    public class BulkUserOperationViewModel
    {
        public IList<Guid> SelectedUserIds { get; set; } = new List<Guid>();
        public string Operation { get; set; } = string.Empty; // "delete", "changeRole", "activate", "deactivate"
        public UserRole? NewRole { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}

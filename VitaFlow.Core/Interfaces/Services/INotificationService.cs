using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;

namespace VitaFlow.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for managing notifications.
    /// </summary>
    public interface INotificationService
    {
        // Existing methods
        Task SendDonationReminderAsync(Guid donorId);
        Task SendEmergencyRequestAsync(IEnumerable<Guid> donorIds);
        Task SendDonationCompletedNotificationAsync(Guid donorId, Guid donationId);
        Task SendRequestStatusUpdateAsync(Guid recipientId, Guid requestId);
        Task MarkNotificationAsReadAsync(Guid notificationId);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);

        // New methods for Recipients
        Task SendBloodAvailableNotificationAsync(Guid recipientId, Guid requestId, string bloodType);
        Task SendBloodRequestApprovedNotificationAsync(Guid recipientId, Guid requestId);
        Task SendBloodRequestCompletedNotificationAsync(Guid recipientId, Guid requestId);

        // New methods for Staff
        Task SendNewBloodRequestNotificationToStaffAsync(IEnumerable<Guid> staffIds, Guid requestId, string bloodType, bool isEmergency);
        Task SendBusinessProcessNotificationToStaffAsync(IEnumerable<Guid> staffIds, string title, string message, string type = "BusinessProcess");

        // New methods for Admin
        Task SendSystemAlertToAdminAsync(IEnumerable<Guid> adminIds, string title, string message, string alertLevel = "Info");
        Task SendStatisticsReportToAdminAsync(IEnumerable<Guid> adminIds, string reportTitle, string reportContent);

        // Enhanced notification management
        Task<(IEnumerable<Notification> notifications, int totalCount)> GetUserNotificationsWithPaginationAsync(
            Guid userId, int pageNumber = 1, int pageSize = 20, bool? isRead = null, string type = null);
        Task<int> GetUnreadNotificationCountAsync(Guid userId);
        Task MarkAllNotificationsAsReadAsync(Guid userId);
        Task DeleteNotificationAsync(Guid notificationId);
    }
}

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
        Task SendDonationReminderAsync(int donorId);
        Task SendEmergencyRequestAsync(IEnumerable<int> donorIds);
        Task SendDonationCompletedNotificationAsync(int donorId, int donationId);
        Task SendRequestStatusUpdateAsync(int recipientId, int requestId);
        Task SendBloodRequestStatusNotificationAsync(int recipientId, int requestId, string statusMessage, bool isUrgent = false);
        Task MarkNotificationAsReadAsync(int notificationId);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
    }
}

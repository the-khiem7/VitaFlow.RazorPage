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
        Task SendDonationReminderAsync(Guid donorId);
        Task SendEmergencyRequestAsync(IEnumerable<Guid> donorIds);
        Task SendDonationCompletedNotificationAsync(Guid donorId, Guid donationId);
        Task SendRequestStatusUpdateAsync(Guid recipientId, Guid requestId);
        Task MarkNotificationAsReadAsync(Guid notificationId);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Infrastructure.Repositories.Interfaces;

namespace VitaFlow.Services.Services
{
    // Implementation of the INotificationService interface.
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        
        // Constructor with dependency injection
        // logger: The logger instance
        public NotificationService(ILogger<NotificationService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }
        
        // Gửi nhắc nhở hiến máu cho donor (tạo notification và lưu vào DB)
        public async Task SendDonationReminderAsync(Guid donorId)
        {
            try
            {
                _logger.LogInformation("Sending donation reminder to donor {DonorId}", donorId);
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<Notification>();
                    var notification = new Notification
                    {
                        UserId = donorId,
                        Title = "Nhắc nhở hiến máu",
                        Message = "Đã đến thời gian bạn có thể hiến máu tiếp theo!",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    await repo.InsertAsync(notification);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending donation reminder to donor {DonorId}", donorId);
                throw;
            }
        }

        // Gửi thông báo khẩn cấp cho danh sách donor (tạo notification và lưu vào DB)
        public async Task SendEmergencyRequestAsync(IEnumerable<Guid> donorIds)
        {
            try
            {
                if (donorIds == null)
                {
                    throw new ArgumentNullException(nameof(donorIds));
                }
                var repo = _unitOfWork.GetRepository<Notification>();
                var now = DateTime.UtcNow;
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    foreach (var donorId in donorIds)
                    {
                        var notification = new Notification
                        {
                            UserId = donorId,
                            Title = "Yêu cầu hiến máu khẩn cấp",
                            Message = "Có một trường hợp cần máu khẩn cấp, bạn có thể giúp đỡ?",
                            CreatedAt = now,
                            IsRead = false
                        };
                        await repo.InsertAsync(notification);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending emergency request to donors");
                throw;
            }
        }

        // Gửi thông báo hoàn thành hiến máu cho donor
        public async Task SendDonationCompletedNotificationAsync(Guid donorId, Guid donationId)
        {
            try
            {
                _logger.LogInformation("Sending donation completion notification to donor {DonorId} for donation {DonationId}", donorId, donationId);
                var repo = _unitOfWork.GetRepository<Notification>();
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var notification = new Notification
                    {
                        UserId = donorId,
                        Title = "Cảm ơn bạn đã hiến máu",
                        Message = $"Bạn đã hoàn thành hiến máu (mã: {donationId}). Xin cảm ơn tấm lòng của bạn!",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    await repo.InsertAsync(notification);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending donation completion notification to donor {DonorId} for donation {DonationId}", donorId, donationId);
                throw;
            }
        }

        // Gửi thông báo cập nhật trạng thái request cho recipient
        public async Task SendRequestStatusUpdateAsync(Guid recipientId, Guid requestId)
        {
            try
            {
                _logger.LogInformation("Sending request status update to recipient {RecipientId} for request {RequestId}", recipientId, requestId);
                var repo = _unitOfWork.GetRepository<Notification>();
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var notification = new Notification
                    {
                        UserId = recipientId,
                        Title = "Cập nhật trạng thái yêu cầu máu",
                        Message = $"Yêu cầu máu của bạn (mã: {requestId}) đã được cập nhật trạng thái.",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    await repo.InsertAsync(notification);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending request status update to recipient {RecipientId} for request {RequestId}", recipientId, requestId);
                throw;
            }
        }

        // Đánh dấu notification đã đọc
        public async Task MarkNotificationAsReadAsync(Guid notificationId)
        {
            try
            {
                _logger.LogInformation("Marking notification {NotificationId} as read", notificationId);
                var repo = _unitOfWork.GetRepository<Notification>();
                var notification = await repo.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    _logger.LogWarning($"Notification with id {notificationId} not found.");
                    throw new KeyNotFoundException($"Notification with id {notificationId} not found.");
                }
                notification.IsRead = true;
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    repo.UpdateAsync(notification);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                throw;
            }
        }

        // Lấy tất cả notification của user
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting notifications for user {UserId}", userId);
                var repo = _unitOfWork.GetRepository<Notification>();
                var notifications = await repo.GetListAsync(predicate: n => n.UserId == userId);
                // Có thể sắp xếp theo CreatedAt hoặc độ ưu tiên nếu cần
                return notifications.OrderByDescending(n => n.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                throw;
            }
        }
    }
}

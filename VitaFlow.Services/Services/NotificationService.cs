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
            _logger = logger;
            _unitOfWork = unitOfWork;
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
                        IsRead = false,
                        Type = "Reminder"
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
                            IsRead = false,
                            Type = "Emergency"
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
                        IsRead = false,
                        Type = "DonationCompleted"
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
                        IsRead = false,
                        Type = "RequestStatusUpdate"
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

        // NEW METHODS - Gửi thông báo đến người nhận máu (Recipient)
        
        /// <summary>
        /// Gửi thông báo cho recipient khi có đơn vị máu phù hợp
        /// </summary>
        public async Task SendBloodAvailableNotificationAsync(Guid recipientId, Guid requestId, string bloodType)
        {
            try
            {
                _logger.LogInformation("Sending blood available notification to recipient {RecipientId} for request {RequestId}", recipientId, requestId);
                var repo = _unitOfWork.GetRepository<Notification>();
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var notification = new Notification
                    {
                        UserId = recipientId,
                        Title = "Có máu phù hợp",
                        Message = $"Đã tìm thấy đơn vị máu nhóm {bloodType} phù hợp với yêu cầu của bạn (mã: {requestId}). Vui lòng liên hệ nhân viên y tế để biết thêm chi tiết.",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        Type = "BloodAvailable"
                    };
                    await repo.InsertAsync(notification);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending blood available notification to recipient {RecipientId}", recipientId);
                throw;
            }
        }

        /// <summary>
        /// Gửi thông báo cho recipient khi yêu cầu được duyệt
        /// </summary>
        public async Task SendBloodRequestApprovedNotificationAsync(Guid recipientId, Guid requestId)
        {
            try
            {
                _logger.LogInformation("Sending blood request approved notification to recipient {RecipientId} for request {RequestId}", recipientId, requestId);
                var repo = _unitOfWork.GetRepository<Notification>();
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var notification = new Notification
                    {
                        UserId = recipientId,
                        Title = "Yêu cầu máu được duyệt",
                        Message = $"Yêu cầu máu của bạn (mã: {requestId}) đã được duyệt và đang được xử lý. Chúng tôi sẽ liên hệ với bạn sớm nhất có thể.",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        Type = "RequestApproved"
                    };
                    await repo.InsertAsync(notification);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending blood request approved notification to recipient {RecipientId}", recipientId);
                throw;
            }
        }

        /// <summary>
        /// Gửi thông báo cho recipient khi yêu cầu hoàn thành
        /// </summary>
        public async Task SendBloodRequestCompletedNotificationAsync(Guid recipientId, Guid requestId)
        {
            try
            {
                _logger.LogInformation("Sending blood request completed notification to recipient {RecipientId} for request {RequestId}", recipientId, requestId);
                var repo = _unitOfWork.GetRepository<Notification>();
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var notification = new Notification
                    {
                        UserId = recipientId,
                        Title = "Yêu cầu máu hoàn thành",
                        Message = $"Yêu cầu máu của bạn (mã: {requestId}) đã được hoàn thành thành công. Cảm ơn bạn đã tin tưởng dịch vụ của chúng tôi.",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        Type = "RequestCompleted"
                    };
                    await repo.InsertAsync(notification);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending blood request completed notification to recipient {RecipientId}", recipientId);
                throw;
            }
        }

        // NEW METHODS - Gửi thông báo đến nhân viên (Staff)

        /// <summary>
        /// Gửi thông báo cho staff khi có yêu cầu máu mới
        /// </summary>
        public async Task SendNewBloodRequestNotificationToStaffAsync(IEnumerable<Guid> staffIds, Guid requestId, string bloodType, bool isEmergency)
        {
            try
            {
                if (staffIds == null)
                {
                    throw new ArgumentNullException(nameof(staffIds));
                }

                _logger.LogInformation("Sending new blood request notification to staff for request {RequestId}", requestId);
                var repo = _unitOfWork.GetRepository<Notification>();
                var now = DateTime.UtcNow;
                
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    foreach (var staffId in staffIds)
                    {
                        var notification = new Notification
                        {
                            UserId = staffId,
                            Title = isEmergency ? "Yêu cầu máu khẩn cấp mới" : "Yêu cầu máu mới",
                            Message = $"Có yêu cầu máu {(isEmergency ? "khẩn cấp " : "")}mới cho nhóm máu {bloodType} (mã: {requestId}). Vui lòng xử lý trong thời gian sớm nhất.",
                            CreatedAt = now,
                            IsRead = false,
                            Type = isEmergency ? "EmergencyRequestForStaff" : "NewRequestForStaff"
                        };
                        await repo.InsertAsync(notification);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending new blood request notification to staff");
                throw;
            }
        }

        /// <summary>
        /// Gửi thông báo cho staff khi cần xử lý nghiệp vụ đặc biệt
        /// </summary>
        public async Task SendBusinessProcessNotificationToStaffAsync(IEnumerable<Guid> staffIds, string title, string message, string type = "BusinessProcess")
        {
            try
            {
                if (staffIds == null)
                {
                    throw new ArgumentNullException(nameof(staffIds));
                }

                _logger.LogInformation("Sending business process notification to staff: {Title}", title);
                var repo = _unitOfWork.GetRepository<Notification>();
                var now = DateTime.UtcNow;

                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    foreach (var staffId in staffIds)
                    {
                        var notification = new Notification
                        {
                            UserId = staffId,
                            Title = title,
                            Message = message,
                            CreatedAt = now,
                            IsRead = false,
                            Type = type
                        };
                        await repo.InsertAsync(notification);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending business process notification to staff");
                throw;
            }
        }

        // NEW METHODS - Gửi thông báo đến quản trị viên (Admin)

        /// <summary>
        /// Gửi thông báo hệ thống cho admin
        /// </summary>
        public async Task SendSystemAlertToAdminAsync(IEnumerable<Guid> adminIds, string title, string message, string alertLevel = "Info")
        {
            try
            {
                if (adminIds == null)
                {
                    throw new ArgumentNullException(nameof(adminIds));
                }

                _logger.LogInformation("Sending system alert to admin: {Title} - Level: {AlertLevel}", title, alertLevel);
                var repo = _unitOfWork.GetRepository<Notification>();
                var now = DateTime.UtcNow;

                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    foreach (var adminId in adminIds)
                    {
                        var notification = new Notification
                        {
                            UserId = adminId,
                            Title = $"[{alertLevel.ToUpper()}] {title}",
                            Message = message,
                            CreatedAt = now,
                            IsRead = false,
                            Type = $"SystemAlert_{alertLevel}"
                        };
                        await repo.InsertAsync(notification);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system alert to admin");
                throw;
            }
        }

        /// <summary>
        /// Gửi báo cáo thống kê cho admin
        /// </summary>
        public async Task SendStatisticsReportToAdminAsync(IEnumerable<Guid> adminIds, string reportTitle, string reportContent)
        {
            try
            {
                if (adminIds == null)
                {
                    throw new ArgumentNullException(nameof(adminIds));
                }

                _logger.LogInformation("Sending statistics report to admin: {ReportTitle}", reportTitle);
                var repo = _unitOfWork.GetRepository<Notification>();
                var now = DateTime.UtcNow;

                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    foreach (var adminId in adminIds)
                    {
                        var notification = new Notification
                        {
                            UserId = adminId,
                            Title = $"Báo cáo: {reportTitle}",
                            Message = reportContent,
                            CreatedAt = now,
                            IsRead = false,
                            Type = "StatisticsReport"
                        };
                        await repo.InsertAsync(notification);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending statistics report to admin");
                throw;
            }
        }

        // NEW METHODS - Quản lý lịch sử thông báo

        /// <summary>
        /// Lấy danh sách notification có phân trang
        /// </summary>
        public async Task<(IEnumerable<Notification> notifications, int totalCount)> GetUserNotificationsWithPaginationAsync(
            Guid userId, int pageNumber = 1, int pageSize = 20, bool? isRead = null, string type = null)
        {
            try
            {
                _logger.LogInformation("Getting paginated notifications for user {UserId}, page {PageNumber}, size {PageSize}", userId, pageNumber, pageSize);
                var repo = _unitOfWork.GetRepository<Notification>();
                
                // Build predicate
                System.Linq.Expressions.Expression<Func<Notification, bool>> predicate = n => n.UserId == userId;
                
                if (isRead.HasValue)
                {
                    var isReadValue = isRead.Value;
                    predicate = n => n.UserId == userId && n.IsRead == isReadValue;
                }
                
                if (!string.IsNullOrEmpty(type))
                {
                    var currentPredicate = predicate;
                    predicate = n => currentPredicate.Compile()(n) && n.Type == type;
                }

                var allNotifications = await repo.GetListAsync(predicate: predicate);
                var totalCount = allNotifications.Count();
                
                var notifications = allNotifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                return (notifications, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated notifications for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Lấy số lượng notification chưa đọc
        /// </summary>
        public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Getting unread notification count for user {UserId}", userId);
                var repo = _unitOfWork.GetRepository<Notification>();
                var notifications = await repo.GetListAsync(predicate: n => n.UserId == userId && !n.IsRead);
                return notifications.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Đánh dấu tất cả notification của user là đã đọc
        /// </summary>
        public async Task MarkAllNotificationsAsReadAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Marking all notifications as read for user {UserId}", userId);
                var repo = _unitOfWork.GetRepository<Notification>();
                var unreadNotifications = await repo.GetListAsync(predicate: n => n.UserId == userId && !n.IsRead);
                
                if (unreadNotifications.Any())
                {
                    await _unitOfWork.ProcessInTransactionAsync(async () =>
                    {
                        foreach (var notification in unreadNotifications)
                        {
                            notification.IsRead = true;
                            repo.UpdateAsync(notification);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Xóa notification (soft delete hoặc hard delete tùy yêu cầu)
        /// </summary>
        public async Task DeleteNotificationAsync(Guid notificationId)
        {
            try
            {
                _logger.LogInformation("Deleting notification {NotificationId}", notificationId);
                var repo = _unitOfWork.GetRepository<Notification>();
                var notification = await repo.GetByIdAsync(notificationId);
                
                if (notification == null)
                {
                    throw new KeyNotFoundException($"Notification with id {notificationId} not found.");
                }

                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    repo.DeleteAsync(notification);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
                throw;
            }
        }
    }
}

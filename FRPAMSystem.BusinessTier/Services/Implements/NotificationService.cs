using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Email;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<NotificationResponse> SendAsync(SendNotificationRequest request)
        {
            ValidateSendPayload(
                request.UserId,
                request.Title,
                request.Message,
                request.NotificationType);

            var user = await GetUserAsync(request.UserId);

            var notification = BuildNotification(
                request.UserId,
                request.Title,
                request.Message,
                request.NotificationType,
                request.ReferenceType,
                request.ReferenceId);

            await _unitOfWork.GetRepository<Notification>().InsertAsync(notification);
            await TrySendEmailAsync(user, request.Title, request.Message);

            return MapToResponse(notification);
        }

        public async Task<IReadOnlyList<NotificationResponse>> SendToUsersAsync(
            SendNotificationToUsersRequest request)
        {
            if (request.UserIds == null || request.UserIds.Count == 0)
            {
                throw new Exception("At least one user id is required.");
            }

            var distinctUserIds = request.UserIds.Distinct().ToList();

            ValidateSendPayload(
                distinctUserIds[0],
                request.Title,
                request.Message,
                request.NotificationType);

            var users = new Dictionary<int, User>();

            foreach (var userId in distinctUserIds)
            {
                users[userId] = await GetUserAsync(userId);
            }

            var repository = _unitOfWork.GetRepository<Notification>();
            var results = new List<NotificationResponse>(distinctUserIds.Count);

            foreach (var userId in distinctUserIds)
            {
                var notification = BuildNotification(
                    userId,
                    request.Title,
                    request.Message,
                    request.NotificationType,
                    request.ReferenceType,
                    request.ReferenceId);

                await repository.InsertAsync(notification);
                await TrySendEmailAsync(users[userId], request.Title, request.Message);
                results.Add(MapToResponse(notification));
            }

            return results;
        }
        public async Task<IPaginate<NotificationResponse>> ViewByUserAsync(
            int userId,
            NotificationFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<Notification>()
                .GetQueryable()
                .Where(n => n.UserId == userId)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedAt);

            return await query
                .Select(n => new NotificationResponse
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    Title = n.Title,
                    Message = n.Message,
                    NotificationType = n.NotificationType,
                    ReferenceType = n.ReferenceType,
                    ReferenceId = n.ReferenceId,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt,
                    IsDeleted = n.IsDeleted,
                    DeletedAt = n.DeletedAt,
                    CreatedAt = n.CreatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<NotificationResponse?> GetByIdForUserAsync(int notificationId, int userId)
        {
            var notification = await _unitOfWork
                .GetRepository<Notification>()
                .FirstOrDefaultAsync(
                    predicate: n => n.NotificationId == notificationId
                        && n.UserId == userId
                        && !n.IsDeleted);

            return notification == null ? null : MapToResponse(notification);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _unitOfWork
                .GetRepository<Notification>()
                .GetQueryable()
                .CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await GetOwnedNotificationAsync(notificationId, userId);

            if (notification == null)
            {
                return false;
            }

            if (notification.IsRead)
            {
                return true;
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.Now;

            _unitOfWork.GetRepository<Notification>().Update(notification);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var unreadNotifications = await _unitOfWork
                .GetRepository<Notification>()
                .GetListAsync(
                    predicate: n => n.UserId == userId && !n.IsRead && !n.IsDeleted,
                    asNoTracking: false);

            if (unreadNotifications.Count == 0)
            {
                return 0;
            }

            var now = DateTime.Now;
            var repository = _unitOfWork.GetRepository<Notification>();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
                repository.Update(notification);
            }

            await _unitOfWork.CommitAsync();

            return unreadNotifications.Count;
        }

        public async Task<bool> SoftDeleteAsync(int notificationId, int userId)
        {
            var notification = await GetOwnedNotificationAsync(notificationId, userId);

            if (notification == null)
            {
                return false;
            }

            if (notification.IsDeleted)
            {
                return true;
            }

            notification.IsDeleted = true;
            notification.DeletedAt = DateTime.Now;

            _unitOfWork.GetRepository<Notification>().Update(notification);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task<Notification?> GetOwnedNotificationAsync(int notificationId, int userId)
        {
            return await _unitOfWork
                .GetRepository<Notification>()
                .FirstOrDefaultAsync(
                    predicate: n => n.NotificationId == notificationId
                        && n.UserId == userId
                        && !n.IsDeleted,
                    asNoTracking: false);
        }

        private async Task<User> GetUserAsync(int userId)
        {
            var user = await _unitOfWork
                .GetRepository<User>()
                .FirstOrDefaultAsync(predicate: u => u.UserId == userId);

            if (user == null)
            {
                throw new Exception("User does not exist.");
            }

            return user;
        }

        private async Task TrySendEmailAsync(User user, string title, string message)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning(
                    "Skipped notification email for user {UserId} because email is empty.",
                    user.UserId);
                return;
            }

            try
            {
                await _emailService.SendAsync(new SendEmailRequest
                {
                    ToEmail = user.Email,
                    ToName = user.FullName,
                    Subject = title,
                    Body = message
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to send notification email to user {UserId} ({Email}).",
                    user.UserId,
                    user.Email);
            }
        }

        private static void ValidateSendPayload(
            int userId,
            string title,
            string message,
            string notificationType)
        {
            if (userId <= 0)
            {
                throw new Exception("User id must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new Exception("Notification title is required.");
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new Exception("Notification message is required.");
            }

            if (string.IsNullOrWhiteSpace(notificationType))
            {
                throw new Exception("Notification type is required.");
            }
        }

        private static Notification BuildNotification(
            int userId,
            string title,
            string message,
            string notificationType,
            string? referenceType,
            int? referenceId)
        {
            return new Notification
            {
                UserId = userId,
                Title = title.Trim(),
                Message = message.Trim(),
                NotificationType = notificationType.Trim(),
                ReferenceType = string.IsNullOrWhiteSpace(referenceType) ? null : referenceType.Trim(),
                ReferenceId = referenceId,
                IsRead = false,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };
        }

        private static NotificationResponse MapToResponse(Notification notification)
        {
            return new NotificationResponse
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                NotificationType = notification.NotificationType,
                ReferenceType = notification.ReferenceType,
                ReferenceId = notification.ReferenceId,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                IsDeleted = notification.IsDeleted,
                DeletedAt = notification.DeletedAt,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}

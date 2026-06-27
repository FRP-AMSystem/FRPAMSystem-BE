using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface INotificationService
    {
        /// <summary>
        /// Creates a notification for one user. Does not commit — caller should call UnitOfWork.CommitAsync().
        /// </summary>
        Task<NotificationResponse> SendAsync(SendNotificationRequest request);

        /// <summary>
        /// Creates the same notification for multiple users. Does not commit — caller should call UnitOfWork.CommitAsync().
        /// </summary>
        Task<IReadOnlyList<NotificationResponse>> SendToUsersAsync(SendNotificationToUsersRequest request);

        Task<IPaginate<NotificationResponse>> ViewByUserAsync(
            int userId,
            NotificationFilter filter,
            PagingModel pagingModel);

        Task<NotificationResponse?> GetByIdForUserAsync(int notificationId, int userId);

        Task<int> GetUnreadCountAsync(int userId);

        Task<bool> MarkAsReadAsync(int notificationId, int userId);

        Task<int> MarkAllAsReadAsync(int userId);

        Task<bool> SoftDeleteAsync(int notificationId, int userId);
    }
}

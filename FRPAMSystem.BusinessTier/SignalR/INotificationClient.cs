using FRPAMSystem.BusinessTier.Payload.Notification;

namespace FRPAMSystem.BusinessTier.SignalR
{
    public interface INotificationClient
    {
        Task ReceiveNotification(NotificationResponse notification);
    }
}

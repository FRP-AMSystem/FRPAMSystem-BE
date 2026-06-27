namespace FRPAMSystem.BusinessTier.Payload.Notification
{
    public class NotificationFilter
    {
        public bool? IsRead { get; set; }

        public bool IncludeDeleted { get; set; }

        public string? NotificationType { get; set; }
    }
}

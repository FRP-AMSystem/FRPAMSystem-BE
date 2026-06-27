namespace FRPAMSystem.BusinessTier.Payload.Notification
{
    /// <summary>
    /// Payload for other services to send a notification to one user.
    /// </summary>
    public class SendNotificationRequest
    {
        public int UserId { get; set; }

        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string NotificationType { get; set; } = null!;

        public string? ReferenceType { get; set; }

        public int? ReferenceId { get; set; }
    }
}

namespace FRPAMSystem.BusinessTier.Payload.Notification
{
    /// <summary>
    /// Payload for other services to send the same notification to multiple users.
    /// </summary>
    public class SendNotificationToUsersRequest
    {
        public ICollection<int> UserIds { get; set; } = new List<int>();

        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string NotificationType { get; set; } = null!;

        public string? ReferenceType { get; set; }

        public int? ReferenceId { get; set; }
    }
}

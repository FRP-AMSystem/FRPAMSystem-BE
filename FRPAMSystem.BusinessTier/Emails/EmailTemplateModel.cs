namespace FRPAMSystem.BusinessTier.Emails
{
    public sealed class EmailTemplateModel
    {
        public string FullName { get; init; } = "User";

        public string Title { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;

        public string NotificationType { get; init; } = string.Empty;

        public string? ReferenceType { get; init; }

        public int? ReferenceId { get; init; }
    }
}

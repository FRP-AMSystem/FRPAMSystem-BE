namespace FRPAMSystem.BusinessTier.Payload.Email
{
    public class SendEmailRequest
    {
        public string ToEmail { get; set; } = null!;

        public string? ToName { get; set; }

        public string Subject { get; set; } = null!;

        public string Body { get; set; } = null!;
    }
}

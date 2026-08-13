namespace FRPAMSystem.BusinessTier.Payload.AuditLog
{
    public class CreateAuditLogRequest
    {
        public int? ActorUserId { get; set; }
        public string Module { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public string Severity { get; set; } = "INFO";
        public string Description { get; set; } = null!;
        public string? Metadata { get; set; }
    }
}

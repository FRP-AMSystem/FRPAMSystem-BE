using System;

namespace FRPAMSystem.BusinessTier.Payload.AuditLog
{
    public class AuditLogResponse
    {
        public int AuditLogId { get; set; }
        public int? ActorUserId { get; set; }
        public string? ActorFullName { get; set; }
        public string? ActorUsername { get; set; }
        public string? ActorRoleName { get; set; }

        public string Module { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public string Severity { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

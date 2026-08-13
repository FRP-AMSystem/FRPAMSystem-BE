using System;

namespace FRPAMSystem.BusinessTier.Payload.AuditLog
{
    public class AuditLogFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Module { get; set; }
        public string? Action { get; set; }
        public int? ActorUserId { get; set; }
        public string? Severity { get; set; }
        public string? Search { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class AuditLog
{
    public int AuditLogId { get; set; }

    public int? ActorUserId { get; set; }

    public string Module { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public string Severity { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? ActorUser { get; set; }
}

using System;

namespace FRPAMSystem.DataTier.Models;

public partial class EquipmentExtensionRequest
{
    public int ExtensionRequestId { get; set; }

    public int AllocationEquipmentDetailId { get; set; }

    public int RequestedBy { get; set; }

    public DateTime OriginalEndDate { get; set; }

    public DateTime RequestedEndDate { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AllocationEquipmentDetail AllocationEquipmentDetail { get; set; } = null!;

    public virtual User RequestedByNavigation { get; set; } = null!;

    public virtual User? ReviewedByNavigation { get; set; }
}

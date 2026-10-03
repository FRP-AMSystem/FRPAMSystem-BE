using System;

namespace FRPAMSystem.DataTier.Models;

public partial class EquipmentChangeRequest
{
    public int ChangeRequestId { get; set; }

    public int AllocationEquipmentDetailId { get; set; }

    public int? CurrentEquipmentInstanceId { get; set; }

    public int RequestedEquipmentTypeId { get; set; }

    public int? RequestedEquipmentInstanceId { get; set; }

    public int RequestedBy { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AllocationEquipmentDetail AllocationEquipmentDetail { get; set; } = null!;

    public virtual EquipmentInstance? CurrentEquipmentInstance { get; set; }

    public virtual EquipmentType RequestedEquipmentType { get; set; } = null!;

    public virtual EquipmentInstance? RequestedEquipmentInstance { get; set; }

    public virtual User RequestedByNavigation { get; set; } = null!;

    public virtual User? ReviewedByNavigation { get; set; }
}

using System;

namespace FRPAMSystem.DataTier.Models;

public partial class EquipmentReturn
{
    public int ReturnId { get; set; }

    public int AllocationEquipmentDetailId { get; set; }

    public int? EquipmentInstanceId { get; set; }

    public int ReturnedBy { get; set; }

    public int ReceivedBy { get; set; }

    public DateTime ReturnDate { get; set; }

    public int Quantity { get; set; }

    public string ConditionAfter { get; set; } = null!;

    public bool IsDamaged { get; set; }

    public string? DamageDescription { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ConfirmedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AllocationEquipmentDetail AllocationEquipmentDetail { get; set; } = null!;

    public virtual EquipmentInstance? EquipmentInstance { get; set; }

    public virtual User ReturnedByNavigation { get; set; } = null!;

    public virtual User ReceivedByNavigation { get; set; } = null!;
}

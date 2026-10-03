using System;

namespace FRPAMSystem.DataTier.Models;

public partial class EquipmentHandover
{
    public int HandoverId { get; set; }

    public int AllocationEquipmentDetailId { get; set; }

    public int? EquipmentInstanceId { get; set; }

    public int HandedOverBy { get; set; }

    public int ReceivedBy { get; set; }

    public DateTime HandoverDate { get; set; }

    public int Quantity { get; set; }

    public string? ConditionBefore { get; set; }

    public string? Note { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ConfirmedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AllocationEquipmentDetail AllocationEquipmentDetail { get; set; } = null!;

    public virtual EquipmentInstance? EquipmentInstance { get; set; }

    public virtual User HandedOverByNavigation { get; set; } = null!;

    public virtual User ReceivedByNavigation { get; set; } = null!;
}

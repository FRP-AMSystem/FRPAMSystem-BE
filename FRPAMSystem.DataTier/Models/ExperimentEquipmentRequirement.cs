using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class ExperimentEquipmentRequirement
{
    public int ExpEquipmentReqId { get; set; }

    public int ExperimentId { get; set; }

    public int EquipmentTypeId { get; set; }

    public int Quantity { get; set; }

    public bool AllowSubstitute { get; set; }

    public double? MinAcceptableEfficiency { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllocationEquipmentDetail> AllocationEquipmentDetails { get; set; } = new List<AllocationEquipmentDetail>();

    public virtual ICollection<EquipmentShortageLog> EquipmentShortageLogs { get; set; } = new List<EquipmentShortageLog>();

    public virtual EquipmentType EquipmentType { get; set; } = null!;

    public virtual Experiment Experiment { get; set; } = null!;
}

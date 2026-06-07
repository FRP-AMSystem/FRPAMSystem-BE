using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class PhaseEquipmentRequirement
{
    public int PhaseEquipmentReqId { get; set; }

    public int PhaseId { get; set; }

    public int EquipmentTypeId { get; set; }

    public int Quantity { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllocationEquipmentDetail> AllocationEquipmentDetails { get; set; } = new List<AllocationEquipmentDetail>();

    public virtual ICollection<EquipmentShortageLog> EquipmentShortageLogs { get; set; } = new List<EquipmentShortageLog>();

    public virtual EquipmentType EquipmentType { get; set; } = null!;

    public virtual ExperimentPhase Phase { get; set; } = null!;
}

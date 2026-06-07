using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class EquipmentShortageLog
{
    public int ShortageLogId { get; set; }

    public int AllocationPlanId { get; set; }

    public int? ExpEquipmentReqId { get; set; }

    public int? PhaseEquipmentReqId { get; set; }

    public int ShortageQuantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AllocationPlan AllocationPlan { get; set; } = null!;

    public virtual ExperimentEquipmentRequirement? ExpEquipmentReq { get; set; }

    public virtual PhaseEquipmentRequirement? PhaseEquipmentReq { get; set; }
}

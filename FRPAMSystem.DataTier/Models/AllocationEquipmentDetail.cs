using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class AllocationEquipmentDetail
{
    public int AllocationEquipmentDetailId { get; set; }

    public int AllocationPlanId { get; set; }

    public int? ExpEquipmentReqId { get; set; }

    public int? PhaseEquipmentReqId { get; set; }

    public int AllocatedEquipmentTypeId { get; set; }

    public int? EquipmentInstanceId { get; set; }

    public int Quantity { get; set; }

    public bool IsSubstitute { get; set; }

    public double EfficiencyRate { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual EquipmentType AllocatedEquipmentType { get; set; } = null!;

    public virtual AllocationPlan AllocationPlan { get; set; } = null!;

    public virtual EquipmentInstance? EquipmentInstance { get; set; }

    public virtual ExperimentEquipmentRequirement? ExpEquipmentReq { get; set; }

    public virtual PhaseEquipmentRequirement? PhaseEquipmentReq { get; set; }
}

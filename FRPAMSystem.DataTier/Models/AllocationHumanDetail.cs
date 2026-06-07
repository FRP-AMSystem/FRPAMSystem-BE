using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class AllocationHumanDetail
{
    public int AllocationHumanDetailId { get; set; }

    public int AllocationPlanId { get; set; }

    public int? ExpHumanReqId { get; set; }

    public int? PhaseHumanReqId { get; set; }

    public int HumanResourceId { get; set; }

    public double WorkingHours { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AllocationPlan AllocationPlan { get; set; } = null!;

    public virtual ExperimentHumanRequirement? ExpHumanReq { get; set; }

    public virtual HumanResourceProfile HumanResource { get; set; } = null!;

    public virtual PhaseHumanRequirement? PhaseHumanReq { get; set; }
}

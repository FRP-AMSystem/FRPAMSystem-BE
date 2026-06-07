using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class AllocationLandDetail
{
    public int AllocationLandDetailId { get; set; }

    public int AllocationPlanId { get; set; }

    public int LandId { get; set; }

    public int ExpLandReqId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AllocationPlan AllocationPlan { get; set; } = null!;

    public virtual ExperimentLandRequirement ExpLandReq { get; set; } = null!;

    public virtual LandResource Land { get; set; } = null!;
}

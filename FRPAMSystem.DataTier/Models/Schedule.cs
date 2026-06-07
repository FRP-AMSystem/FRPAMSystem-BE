using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public int AllocationPlanId { get; set; }

    public int? PhaseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public int? AssignedHumanResourceId { get; set; }

    public string? Notes { get; set; }

    public int Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual AllocationPlan AllocationPlan { get; set; } = null!;

    public virtual HumanResourceProfile? AssignedHumanResource { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ExperimentPhase? Phase { get; set; }
}

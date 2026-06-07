using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class AllocationPlan
{
    public int AllocationPlanId { get; set; }

    public int ExperimentId { get; set; }

    public double? FitnessScore { get; set; }

    public int? CreatedBy { get; set; }

    public int? ApproveBy { get; set; }

    public string ApproveStatus { get; set; } = null!;

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllocationEquipmentDetail> AllocationEquipmentDetails { get; set; } = new List<AllocationEquipmentDetail>();

    public virtual ICollection<AllocationHumanDetail> AllocationHumanDetails { get; set; } = new List<AllocationHumanDetail>();

    public virtual ICollection<AllocationLandDetail> AllocationLandDetails { get; set; } = new List<AllocationLandDetail>();

    public virtual User? ApproveByNavigation { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<EquipmentShortageLog> EquipmentShortageLogs { get; set; } = new List<EquipmentShortageLog>();

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}

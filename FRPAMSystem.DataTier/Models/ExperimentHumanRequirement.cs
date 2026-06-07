using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class ExperimentHumanRequirement
{
    public int ExpHumanReqId { get; set; }

    public int ExperimentId { get; set; }

    public int RoleId { get; set; }

    public int Quantity { get; set; }

    public int? RequiredSkillId { get; set; }

    public double? WorkingHoursPerDay { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllocationHumanDetail> AllocationHumanDetails { get; set; } = new List<AllocationHumanDetail>();

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual Skill? RequiredSkill { get; set; }

    public virtual Role Role { get; set; } = null!;
}

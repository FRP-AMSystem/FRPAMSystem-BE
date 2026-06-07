using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class HumanResourceProfile
{
    public int HumanResourceId { get; set; }

    public int UserId { get; set; }

    public double MaxWorkingHoursPerDay { get; set; }

    public double CurrentWorkload { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllocationHumanDetail> AllocationHumanDetails { get; set; } = new List<AllocationHumanDetail>();

    public virtual ICollection<HumanResourceSkill> HumanResourceSkills { get; set; } = new List<HumanResourceSkill>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual User User { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllocationPlan> AllocationPlanApproveByNavigations { get; set; } = new List<AllocationPlan>();

    public virtual ICollection<AllocationPlan> AllocationPlanCreatedByNavigations { get; set; } = new List<AllocationPlan>();

    public virtual ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();

    public virtual HumanResourceProfile? HumanResourceProfile { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}

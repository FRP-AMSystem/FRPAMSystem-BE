using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<ExperimentHumanRequirement> ExperimentHumanRequirements { get; set; } = new List<ExperimentHumanRequirement>();

    public virtual ICollection<PhaseHumanRequirement> PhaseHumanRequirements { get; set; } = new List<PhaseHumanRequirement>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

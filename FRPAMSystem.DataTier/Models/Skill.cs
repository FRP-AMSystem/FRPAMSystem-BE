using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class Skill
{
    public int SkillId { get; set; }

    public string SkillName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<ExperimentHumanRequirement> ExperimentHumanRequirements { get; set; } = new List<ExperimentHumanRequirement>();

    public virtual ICollection<HumanResourceSkill> HumanResourceSkills { get; set; } = new List<HumanResourceSkill>();

    public virtual ICollection<PhaseHumanRequirement> PhaseHumanRequirements { get; set; } = new List<PhaseHumanRequirement>();
}

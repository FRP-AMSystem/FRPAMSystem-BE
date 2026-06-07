using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class HumanResourceSkill
{
    public int HumanResourceSkillId { get; set; }

    public int HumanResourceId { get; set; }

    public int SkillId { get; set; }

    public string SkillLevel { get; set; } = null!;

    public virtual HumanResourceProfile HumanResource { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}

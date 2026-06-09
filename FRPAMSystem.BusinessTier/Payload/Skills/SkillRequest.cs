using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.Skill
{
    public class SkillRequest
    {
        public string SkillName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}

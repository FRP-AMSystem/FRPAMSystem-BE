using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.HumanResourceSkill
{
    public class HumanResourceSkillRequest
    {
        public int HumanResourceId { get; set; }

        public int SkillId { get; set; }

        public SkillLevel SkillLevel { get; set; }
    }
}

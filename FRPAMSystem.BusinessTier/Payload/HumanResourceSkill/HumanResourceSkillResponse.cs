using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.HumanResourceSkill
{
    public class HumanResourceSkillResponse
    {
        public int HumanResourceSkillId { get; set; }

        public int HumanResourceId { get; set; }

        public int UserId { get; set; }

        public string? FullName { get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }

        public int SkillId { get; set; }

        public string? SkillName { get; set; }

        public string? SkillDescription { get; set; }

        public string SkillLevel { get; set; } = string.Empty;
    }
}

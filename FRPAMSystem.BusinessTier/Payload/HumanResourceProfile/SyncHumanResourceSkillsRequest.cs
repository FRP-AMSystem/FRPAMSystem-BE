using FRPAMSystem.BusinessTier.Enums;
using System.Collections.Generic;

namespace FRPAMSystem.BusinessTier.Payload.HumanResourceProfile
{
    public class SyncHumanResourceSkillItemRequest
    {
        public int SkillId { get; set; }
        public SkillLevel SkillLevel { get; set; }
    }

    public class SyncHumanResourceSkillsRequest
    {
        public List<SyncHumanResourceSkillItemRequest> Skills { get; set; } = new();
    }
}

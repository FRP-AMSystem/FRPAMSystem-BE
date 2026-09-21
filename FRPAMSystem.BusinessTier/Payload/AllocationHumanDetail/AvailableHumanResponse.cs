using System;
using System.Collections.Generic;

namespace FRPAMSystem.BusinessTier.Payload.AllocationHumanDetail
{
    public class AvailableHumanResponse
    {
        public int HumanResourceId { get; set; }

        public int? UserId { get; set; }

        public string FullName { get; set; } = null!;

        public string? RoleName { get; set; }

        public bool HasRequiredSkill { get; set; }

        public List<string> Skills { get; set; } = new();

        public double MaxWorkingHoursPerDay { get; set; }

        public double CurrentWorkload { get; set; }

        public double AvailableHoursPerDay { get; set; }

        public string MatchReason { get; set; } = null!;
    }
}

using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.HumanResourceProfile
{
    public class HumanResourceProfileFilter
    {
        public string? Keyword { get; set; }

        public int? UserId { get; set; }

        public int? RoleId { get; set; }

        public HumanResourceStatus? Status { get; set; }

        public double? MinMaxWorkingHoursPerDay { get; set; }

        public double? MaxMaxWorkingHoursPerDay { get; set; }

        public double? MinCurrentWorkload { get; set; }

        public double? MaxCurrentWorkload { get; set; }
    }
}

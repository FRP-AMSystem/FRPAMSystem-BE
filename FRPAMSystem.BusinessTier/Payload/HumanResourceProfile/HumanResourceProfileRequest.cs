using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.HumanResourceProfile
{
    public class HumanResourceProfileRequest
    {
        public int UserId { get; set; }

        public double MaxWorkingHoursPerDay { get; set; }

        public double CurrentWorkload { get; set; }

        public HumanResourceStatus Status { get; set; }
    }
}

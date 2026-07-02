using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationPlan
{
    public class AllocationPlanRequest
    {
        public int ExperimentId { get; set; }

        public double? FitnessScore { get; set; }

        public AllocationPlanStatus ApproveStatus { get; set; } = AllocationPlanStatus.Pending;
    }
}

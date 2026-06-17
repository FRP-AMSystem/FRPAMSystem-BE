using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationPlan
{
    public class AllocationPlanFilter
    {
        public string? Keyword { get; set; }

        public int? ExperimentId { get; set; }

        public int? CreatedBy { get; set; }

        public int? ApproveBy { get; set; }

        public AllocationPlanStatus? ApproveStatus { get; set; }

        public double? MinFitnessScore { get; set; }

        public double? MaxFitnessScore { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedTo { get; set; }

        public DateTime? ApprovedFrom { get; set; }

        public DateTime? ApprovedTo { get; set; }
    }
}

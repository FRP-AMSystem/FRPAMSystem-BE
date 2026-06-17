using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationPlan
{
    public class AllocationPlanResponse
    {
        public int AllocationPlanId { get; set; }

        public int ExperimentId { get; set; }

        public string? ExperimentName { get; set; }

        public double? FitnessScore { get; set; }

        public int? CreatedBy { get; set; }

        public string? CreatedByName { get; set; }

        public int? ApproveBy { get; set; }

        public string? ApproveByName { get; set; }

        public string ApproveStatus { get; set; } = string.Empty;

        public DateTime? ApprovedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int LandDetailCount { get; set; }

        public int EquipmentDetailCount { get; set; }

        public int HumanDetailCount { get; set; }

        public int ScheduleCount { get; set; }
    }
}

using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationHumanDetail
{
    public class AllocationHumanDetailFilter
    {
        public string? Keyword { get; set; }

        public int? AllocationPlanId { get; set; }

        public int? ExperimentId { get; set; }

        public int? ExpHumanReqId { get; set; }

        public int? PhaseHumanReqId { get; set; }

        public int? HumanResourceId { get; set; }

        public int? UserId { get; set; }

        public int? RoleId { get; set; }

        public int? RequiredSkillId { get; set; }

        public AllocationDetailStatus? Status { get; set; }

        public DateTime? StartFrom { get; set; }

        public DateTime? StartTo { get; set; }

        public DateTime? EndFrom { get; set; }

        public DateTime? EndTo { get; set; }

        public double? MinWorkingHours { get; set; }

        public double? MaxWorkingHours { get; set; }
    }
}

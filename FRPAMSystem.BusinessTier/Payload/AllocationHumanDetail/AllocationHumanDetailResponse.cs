using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationHumanDetail
{
    public class AllocationHumanDetailResponse
    {
        public int AllocationHumanDetailId { get; set; }

        public int AllocationPlanId { get; set; }

        public int ExperimentId { get; set; }

        public string? ExperimentName { get; set; }

        public int? ExpHumanReqId { get; set; }

        public int? PhaseHumanReqId { get; set; }

        public int? PhaseId { get; set; }

        public string? PhaseName { get; set; }

        public int HumanResourceId { get; set; }

        public int UserId { get; set; }

        public string? FullName { get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }

        public int? HumanResourceRoleId { get; set; }

        public string? HumanResourceRoleName { get; set; }

        public int? RequiredRoleId { get; set; }

        public string? RequiredRoleName { get; set; }

        public int? RequiredSkillId { get; set; }

        public string? RequiredSkillName { get; set; }

        public int RequiredQuantity { get; set; }

        public double? RequiredWorkingHoursPerDay { get; set; }

        public double WorkingHours { get; set; }

        public double MaxWorkingHoursPerDay { get; set; }

        public double CurrentWorkload { get; set; }

        public string? HumanResourceStatus { get; set; }

        public string? RequirementNote { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

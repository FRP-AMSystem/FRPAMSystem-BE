using FRPAMSystem.BusinessTier.Enums;

namespace FRPAMSystem.BusinessTier.Payload.Schedule
{
    public class ScheduleRequest
    {
        public int AllocationPlanId { get; set; }

        public int? PhaseId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ScheduleStatus Status { get; set; } = ScheduleStatus.Planned;

        public int? CreatedBy { get; set; }

        public int? AssignedHumanResourceId { get; set; }

        public string? Notes { get; set; }

        public int Priority { get; set; } = 2;
    }
}

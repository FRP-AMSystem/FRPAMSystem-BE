namespace FRPAMSystem.BusinessTier.Payload.Schedule
{
    public class ScheduleResponse
    {
        public int ScheduleId { get; set; }

        public int AllocationPlanId { get; set; }

        public int ExperimentId { get; set; }

        public string? ExperimentName { get; set; }

        public int? PhaseId { get; set; }

        public string? PhaseName { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public int? CreatedBy { get; set; }

        public string? CreatedByName { get; set; }

        public int? AssignedHumanResourceId { get; set; }

        public string? AssignedHumanResourceName { get; set; }

        public string? Notes { get; set; }

        public int Priority { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

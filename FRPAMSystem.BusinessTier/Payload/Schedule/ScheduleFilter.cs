using FRPAMSystem.BusinessTier.Enums;

namespace FRPAMSystem.BusinessTier.Payload.Schedule
{
    public class ScheduleFilter
    {
        public string? Keyword { get; set; }

        public int? AllocationPlanId { get; set; }

        public int? PhaseId { get; set; }

        public int? AssignedHumanResourceId { get; set; }

        public int? CreatedBy { get; set; }

        public ScheduleStatus? Status { get; set; }

        public DateTime? StartDateFrom { get; set; }

        public DateTime? StartDateTo { get; set; }

        /// <summary>
        /// Calendar overlap: schedule ends on or after this date.
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Calendar overlap: schedule starts on or before this date.
        /// </summary>
        public DateTime? DateTo { get; set; }
    }
}

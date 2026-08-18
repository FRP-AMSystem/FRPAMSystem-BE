namespace FRPAMSystem.BusinessTier.DomainEvents.Events
{
    public class ScheduleAssignedEvent : IDomainEvent
    {
        public ScheduleAssignedEvent(
            int scheduleId,
            int allocationPlanId,
            int? experimentId,
            string? experimentName,
            string scheduleTitle,
            int assignedHumanResourceId,
            bool isNewAssignment,
            DateTime occurredAt)
        {
            ScheduleId = scheduleId;
            AllocationPlanId = allocationPlanId;
            ExperimentId = experimentId;
            ExperimentName = experimentName;
            ScheduleTitle = scheduleTitle;
            AssignedHumanResourceId = assignedHumanResourceId;
            IsNewAssignment = isNewAssignment;
            OccurredAt = occurredAt;
        }


        public int ScheduleId { get; }

        public int AllocationPlanId { get; }

        public int? ExperimentId { get; }

        public string? ExperimentName { get; }

        public string ScheduleTitle { get; }


        public int AssignedHumanResourceId { get; }

        public bool IsNewAssignment { get; }

        public DateTime OccurredAt { get; }
    }
}

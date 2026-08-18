namespace FRPAMSystem.BusinessTier.DomainEvents.Events
{
    public class AllocationPlanSubmittedEvent : IDomainEvent
    {
        public AllocationPlanSubmittedEvent(
            int allocationPlanId,
            int experimentId,
            string? experimentName,
            int? createdBy,
            DateTime occurredAt)
        {
            AllocationPlanId = allocationPlanId;
            ExperimentId = experimentId;
            ExperimentName = experimentName;
            CreatedBy = createdBy;
            OccurredAt = occurredAt;
        }

        public int AllocationPlanId { get; }

        public int ExperimentId { get; }

        public string? ExperimentName { get; }

        public int? CreatedBy { get; }

        public DateTime OccurredAt { get; }
    }
}

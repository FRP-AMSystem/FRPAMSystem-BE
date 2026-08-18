namespace FRPAMSystem.BusinessTier.DomainEvents.Events
{
    public class AllocationPlanShortageDetectedEvent : IDomainEvent
    {
        public int AllocationPlanId { get; }
        public int ExperimentId { get; }
        public string? ExperimentName { get; }
        public DateTime OccurredAt { get; }

        public AllocationPlanShortageDetectedEvent(
            int allocationPlanId,
            int experimentId,
            string? experimentName,
            DateTime occurredAt)
        {
            AllocationPlanId = allocationPlanId;
            ExperimentId = experimentId;
            ExperimentName = experimentName;
            OccurredAt = occurredAt;
        }
    }
}

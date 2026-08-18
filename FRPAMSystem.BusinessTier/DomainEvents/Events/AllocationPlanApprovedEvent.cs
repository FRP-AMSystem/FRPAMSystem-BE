namespace FRPAMSystem.BusinessTier.DomainEvents.Events
{
    public class AllocationPlanApprovedEvent : IDomainEvent
    {
        public AllocationPlanApprovedEvent(
            int allocationPlanId,
            int experimentId,
            string? experimentName,
            int? createdBy,
            int approvedBy,
            DateTime occurredAt)
        {
            AllocationPlanId = allocationPlanId;
            ExperimentId = experimentId;
            ExperimentName = experimentName;
            CreatedBy = createdBy;
            ApprovedBy = approvedBy;
            OccurredAt = occurredAt;
        }

        public int AllocationPlanId { get; }

        public int ExperimentId { get; }

        public string? ExperimentName { get; }

        public int? CreatedBy { get; }

        public int ApprovedBy { get; }

        public DateTime OccurredAt { get; }
    }
}

namespace FRPAMSystem.BusinessTier.DomainEvents.Events
{
    public class AllocationPlanRejectedEvent : IDomainEvent
    {
        public AllocationPlanRejectedEvent(
            int allocationPlanId,
            int experimentId,
            string? experimentName,
            int? createdBy,
            int rejectedBy)
        {
            AllocationPlanId = allocationPlanId;
            ExperimentId = experimentId;
            ExperimentName = experimentName;
            CreatedBy = createdBy;
            RejectedBy = rejectedBy;
            OccurredAt = DateTime.Now;
        }

        public int AllocationPlanId { get; }

        public int ExperimentId { get; }

        public string? ExperimentName { get; }

        public int? CreatedBy { get; }

        public int RejectedBy { get; }

        public DateTime OccurredAt { get; }
    }
}

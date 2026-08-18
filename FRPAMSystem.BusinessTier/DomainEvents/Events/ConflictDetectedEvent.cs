namespace FRPAMSystem.BusinessTier.DomainEvents.Events
{
    public class ConflictDetectedEvent : IDomainEvent
    {
        public int ShortageLogId { get; }
        public int AllocationPlanId { get; }
        public int ShortageQuantity { get; }
        public DateTime OccurredAt { get; }

        public ConflictDetectedEvent(
            int shortageLogId,
            int allocationPlanId,
            int shortageQuantity)
        {
            ShortageLogId = shortageLogId;
            AllocationPlanId = allocationPlanId;
            ShortageQuantity = shortageQuantity;
            OccurredAt = DateTime.Now;
        }
    }
}

namespace FRPAMSystem.BusinessTier.DomainEvents.Events
{
    public class ExperimentRejectedEvent : IDomainEvent
    {
        public int ExperimentId { get; }
        public string? ExperimentName { get; }
        public int ResearcherId { get; }
        public int? RejectedBy { get; }
        public string? Reason { get; }
        public DateTime OccurredAt { get; }

        public ExperimentRejectedEvent(
            int experimentId,
            string? experimentName,
            int researcherId,
            int? rejectedBy,
            string? reason)
        {
            ExperimentId = experimentId;
            ExperimentName = experimentName;
            ResearcherId = researcherId;
            RejectedBy = rejectedBy;
            Reason = reason;
            OccurredAt = DateTime.Now;
        }
    }
}

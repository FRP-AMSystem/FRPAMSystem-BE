namespace FRPAMSystem.BusinessTier.DomainEvents.Events
{
    public class ExperimentApprovedEvent : IDomainEvent
    {
        public int ExperimentId { get; }
        public string? ExperimentName { get; }
        public int ResearcherId { get; }
        public int? ApprovedBy { get; }
        public DateTime OccurredAt { get; }

        public ExperimentApprovedEvent(
            int experimentId,
            string? experimentName,
            int researcherId,
            int? approvedBy,
            DateTime occurredAt)
        {
            ExperimentId = experimentId;
            ExperimentName = experimentName;
            ResearcherId = researcherId;
            ApprovedBy = approvedBy;
            OccurredAt = occurredAt;
        }
    }
}

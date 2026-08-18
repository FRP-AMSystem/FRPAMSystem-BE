namespace FRPAMSystem.BusinessTier.DomainEvents.Events
{
    public class ExperimentSubmittedEvent : IDomainEvent
    {
        public ExperimentSubmittedEvent(
            int experimentId,
            string experimentName,
            int researcherId,
            DateTime occurredAt)
        {
            ExperimentId = experimentId;
            ExperimentName = experimentName;
            ResearcherId = researcherId;
            OccurredAt = occurredAt;
        }

        public int ExperimentId { get; }

        public string ExperimentName { get; }

        public int ResearcherId { get; }

        public DateTime OccurredAt { get; }
    }
}

namespace FRPAMSystem.BusinessTier.DomainEvents.Events
{
    public class ExperimentCreatedEvent : IDomainEvent
    {
        public ExperimentCreatedEvent(
            int experimentId,
            string experimentName,
            int researcherId)
        {
            ExperimentId = experimentId;
            ExperimentName = experimentName;
            ResearcherId = researcherId;
            OccurredAt = DateTime.Now;
        }

        public int ExperimentId { get; }

        public string ExperimentName { get; }

        public int ResearcherId { get; }

        public DateTime OccurredAt { get; }
    }
}

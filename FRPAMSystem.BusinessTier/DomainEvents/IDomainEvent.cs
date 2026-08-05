namespace FRPAMSystem.BusinessTier.DomainEvents
{
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}

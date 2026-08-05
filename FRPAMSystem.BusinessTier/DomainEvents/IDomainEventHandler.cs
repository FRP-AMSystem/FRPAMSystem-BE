namespace FRPAMSystem.BusinessTier.DomainEvents
{
    public interface IDomainEventHandler
    {
        Type EventType { get; }

        Task HandleAsync(
            IDomainEvent domainEvent,
            CancellationToken cancellationToken = default);
    }
}

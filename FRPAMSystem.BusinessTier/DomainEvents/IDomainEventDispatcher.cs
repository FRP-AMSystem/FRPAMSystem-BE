namespace FRPAMSystem.BusinessTier.DomainEvents
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(
            IDomainEvent domainEvent,
            CancellationToken cancellationToken = default);
    }
}

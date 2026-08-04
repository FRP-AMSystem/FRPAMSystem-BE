namespace FRPAMSystem.BusinessTier.DomainEvents
{
    public abstract class DomainEventHandler<TDomainEvent> : IDomainEventHandler
        where TDomainEvent : IDomainEvent
    {
        public Type EventType => typeof(TDomainEvent);

        public Task HandleAsync(
            IDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            return HandleAsync((TDomainEvent)domainEvent, cancellationToken);
        }

        protected abstract Task HandleAsync(
            TDomainEvent domainEvent,
            CancellationToken cancellationToken);
    }
}

using Microsoft.Extensions.Logging;

namespace FRPAMSystem.BusinessTier.DomainEvents.Dispatcher
{
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IEnumerable<IDomainEventHandler> _handlers;
        private readonly ILogger<DomainEventDispatcher> _logger;

        public DomainEventDispatcher(
            IEnumerable<IDomainEventHandler> handlers,
            ILogger<DomainEventDispatcher> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }

        public async Task DispatchAsync(
            IDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            var matchingHandlers = _handlers
                .Where(handler => handler.EventType == domainEvent.GetType())
                .ToList();

            foreach (var handler in matchingHandlers)
            {
                try
                {
                    await handler.HandleAsync(domainEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Domain event handler {HandlerName} failed for {EventName}. Business transaction was already committed.",
                        handler.GetType().Name,
                        domainEvent.GetType().Name);
                }
            }
        }
    }
}

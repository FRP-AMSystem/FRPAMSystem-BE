using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class ExperimentCreatedHandler : DomainEventHandler<ExperimentCreatedEvent>
    {
        private readonly INotificationService _notificationService;

        public ExperimentCreatedHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        protected override Task HandleAsync(
            ExperimentCreatedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            return _notificationService.SendAsync(new SendNotificationRequest
            {
                UserId = domainEvent.ResearcherId,
                Title = "Experiment created",
                Message = $"Experiment '{domainEvent.ExperimentName}' has been created.",
                NotificationType = NotificationTypes.ExperimentCreated,
                ReferenceType = NotificationReferenceTypes.Experiment,
                ReferenceId = domainEvent.ExperimentId
            });
        }
    }
}

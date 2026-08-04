using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class AllocationPlanGeneratedHandler : DomainEventHandler<AllocationPlanGeneratedEvent>
    {
        private readonly INotificationService _notificationService;

        public AllocationPlanGeneratedHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        protected override Task HandleAsync(
            AllocationPlanGeneratedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            if (!domainEvent.CreatedBy.HasValue)
            {
                return Task.CompletedTask;
            }

            return _notificationService.SendAsync(new SendNotificationRequest
            {
                UserId = domainEvent.CreatedBy.Value,
                Title = "Allocation plan generated",
                Message = $"Allocation plan for experiment '{domainEvent.ExperimentName ?? domainEvent.ExperimentId.ToString()}' has been generated.",
                NotificationType = NotificationTypes.AllocationPlanGenerated,
                ReferenceType = NotificationReferenceTypes.AllocationPlan,
                ReferenceId = domainEvent.AllocationPlanId
            });
        }
    }
}

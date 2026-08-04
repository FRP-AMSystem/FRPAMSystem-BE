using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class AllocationPlanRejectedHandler : DomainEventHandler<AllocationPlanRejectedEvent>
    {
        private readonly INotificationService _notificationService;

        public AllocationPlanRejectedHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        protected override Task HandleAsync(
            AllocationPlanRejectedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            if (!domainEvent.CreatedBy.HasValue)
            {
                return Task.CompletedTask;
            }

            return _notificationService.SendAsync(new SendNotificationRequest
            {
                UserId = domainEvent.CreatedBy.Value,
                Title = "Allocation plan rejected",
                Message = $"Allocation plan for experiment '{domainEvent.ExperimentName ?? domainEvent.ExperimentId.ToString()}' has been rejected.",
                NotificationType = NotificationTypes.AllocationPlanRejected,
                ReferenceType = NotificationReferenceTypes.AllocationPlan,
                ReferenceId = domainEvent.AllocationPlanId
            });
        }
    }
}

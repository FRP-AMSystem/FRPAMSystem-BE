using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class AllocationPlanApprovedHandler : DomainEventHandler<AllocationPlanApprovedEvent>
    {
        private readonly INotificationService _notificationService;

        public AllocationPlanApprovedHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        protected override Task HandleAsync(
            AllocationPlanApprovedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            if (!domainEvent.CreatedBy.HasValue)
            {
                return Task.CompletedTask;
            }

            return _notificationService.SendAsync(new SendNotificationRequest
            {
                UserId = domainEvent.CreatedBy.Value,
                Title = "Allocation plan approved",
                Message = $"Allocation plan for experiment '{domainEvent.ExperimentName ?? domainEvent.ExperimentId.ToString()}' has been approved.",
                NotificationType = NotificationTypes.AllocationPlanApproved,
                ReferenceType = NotificationReferenceTypes.AllocationPlan,
                ReferenceId = domainEvent.AllocationPlanId
            });
        }
    }
}

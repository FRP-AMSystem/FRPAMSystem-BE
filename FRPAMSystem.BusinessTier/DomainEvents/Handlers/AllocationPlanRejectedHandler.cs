using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class AllocationPlanRejectedHandler : DomainEventHandler<AllocationPlanRejectedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;

        public AllocationPlanRejectedHandler(
            INotificationService notificationService,
            IAuditLogService auditLogService)
        {
            _notificationService = notificationService;
            _auditLogService = auditLogService;
        }

        protected override async Task HandleAsync(
            AllocationPlanRejectedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            await _auditLogService.RecordLogAsync(new CreateAuditLogRequest
            {
                ActorUserId = domainEvent.RejectedBy,
                Module = "AllocationPlan",
                Action = "RejectPlan",
                ReferenceType = "AllocationPlan",
                ReferenceId = domainEvent.AllocationPlanId,
                Severity = "WARNING",
                Description = $"Từ chối kế hoạch phân bổ ID #{domainEvent.AllocationPlanId} cho đề tài '{domainEvent.ExperimentName ?? domainEvent.ExperimentId.ToString()}'."
            });

            if (domainEvent.CreatedBy.HasValue)
            {
                await _notificationService.SendAsync(new SendNotificationRequest
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
}

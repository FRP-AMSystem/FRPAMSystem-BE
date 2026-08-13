using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class AllocationPlanApprovedHandler : DomainEventHandler<AllocationPlanApprovedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;

        public AllocationPlanApprovedHandler(
            INotificationService notificationService,
            IAuditLogService auditLogService)
        {
            _notificationService = notificationService;
            _auditLogService = auditLogService;
        }

        protected override async Task HandleAsync(
            AllocationPlanApprovedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            await _auditLogService.RecordLogAsync(new CreateAuditLogRequest
            {
                ActorUserId = domainEvent.ApprovedBy,
                Module = "AllocationPlan",
                Action = "ApprovePlan",
                ReferenceType = "AllocationPlan",
                ReferenceId = domainEvent.AllocationPlanId,
                Severity = "INFO",
                Description = $"Phê duyệt kế hoạch phân bổ ID #{domainEvent.AllocationPlanId} cho đề tài '{domainEvent.ExperimentName ?? domainEvent.ExperimentId.ToString()}'."
            });

            if (domainEvent.CreatedBy.HasValue)
            {
                await _notificationService.SendAsync(new SendNotificationRequest
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
}

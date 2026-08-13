using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class ExperimentCreatedHandler : DomainEventHandler<ExperimentCreatedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;

        public ExperimentCreatedHandler(
            INotificationService notificationService,
            IAuditLogService auditLogService)
        {
            _notificationService = notificationService;
            _auditLogService = auditLogService;
        }

        protected override async Task HandleAsync(
            ExperimentCreatedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            await _auditLogService.RecordLogAsync(new CreateAuditLogRequest
            {
                ActorUserId = domainEvent.ResearcherId,
                Module = "Experiment",
                Action = "CreateExperiment",
                ReferenceType = "Experiment",
                ReferenceId = domainEvent.ExperimentId,
                Severity = "INFO",
                Description = $"Tạo mới đề tài nghiên cứu ID #{domainEvent.ExperimentId} ('{domainEvent.ExperimentName}')."
            });

            await _notificationService.SendAsync(new SendNotificationRequest
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

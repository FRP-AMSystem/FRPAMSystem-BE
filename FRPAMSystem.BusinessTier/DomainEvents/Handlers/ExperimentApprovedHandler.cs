using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;
using System.Threading;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class ExperimentApprovedHandler : DomainEventHandler<ExperimentApprovedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;

        public ExperimentApprovedHandler(
            INotificationService notificationService,
            IAuditLogService auditLogService)
        {
            _notificationService = notificationService;
            _auditLogService = auditLogService;
        }

        protected override async Task HandleAsync(
            ExperimentApprovedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            await _auditLogService.RecordLogAsync(new CreateAuditLogRequest
            {
                ActorUserId = domainEvent.ApprovedBy,
                Module = "Experiment",
                Action = "ApproveExperiment",
                ReferenceType = "Experiment",
                ReferenceId = domainEvent.ExperimentId,
                Severity = "INFO",
                Description = $"Approve Experiment ID #{domainEvent.ExperimentId} ('{domainEvent.ExperimentName}')."
            });

            await _notificationService.SendAsync(new SendNotificationRequest
            {
                UserId = domainEvent.ResearcherId,
                Title = "Experiment approved",
                Message = $"Your experiment '{domainEvent.ExperimentName ?? domainEvent.ExperimentId.ToString()}' has been approved.",
                NotificationType = NotificationTypes.ExperimentApproved,
                ReferenceType = NotificationReferenceTypes.Experiment,
                ReferenceId = domainEvent.ExperimentId
            });
        }
    }
}

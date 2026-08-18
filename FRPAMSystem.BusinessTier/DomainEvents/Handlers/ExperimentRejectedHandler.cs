using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;
using System.Threading;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class ExperimentRejectedHandler : DomainEventHandler<ExperimentRejectedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;

        public ExperimentRejectedHandler(
            INotificationService notificationService,
            IAuditLogService auditLogService)
        {
            _notificationService = notificationService;
            _auditLogService = auditLogService;
        }

        protected override async Task HandleAsync(
            ExperimentRejectedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            await _auditLogService.RecordLogAsync(new CreateAuditLogRequest
            {
                ActorUserId = domainEvent.RejectedBy,
                Module = "Experiment",
                Action = "RejectExperiment",
                ReferenceType = "Experiment",
                ReferenceId = domainEvent.ExperimentId,
                Severity = "WARNING",
                Description = $"Reject Experiment ID #{domainEvent.ExperimentId} ('{domainEvent.ExperimentName}')."
            });

            var reasonText = !string.IsNullOrWhiteSpace(domainEvent.Reason) ? $" Reason: {domainEvent.Reason}" : string.Empty;

            await _notificationService.SendAsync(new SendNotificationRequest
            {
                UserId = domainEvent.ResearcherId,
                Title = "Experiment rejected",
                Message = $"Your experiment '{domainEvent.ExperimentName ?? domainEvent.ExperimentId.ToString()}' has been rejected.{reasonText}",
                NotificationType = "ExperimentRejected",
                ReferenceType = NotificationReferenceTypes.Experiment,
                ReferenceId = domainEvent.ExperimentId
            });
        }
    }
}

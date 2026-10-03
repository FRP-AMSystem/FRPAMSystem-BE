using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class ExperimentCreatedHandler : DomainEventHandler<ExperimentCreatedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;
        private readonly IUnitOfWork _unitOfWork;

        public ExperimentCreatedHandler(
            INotificationService notificationService,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork)
        {
            _notificationService = notificationService;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleAsync(
            ExperimentCreatedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            //audit log
            await _auditLogService.RecordLogAsync(new CreateAuditLogRequest
            {
                ActorUserId = domainEvent.ResearcherId,
                Module = "Experiment",
                Action = "CreateExperiment",
                ReferenceType = "Experiment",
                ReferenceId = domainEvent.ExperimentId,
                Severity = "Information",
                Description = $"Create Experiment ID #{domainEvent.ExperimentId} ('{domainEvent.ExperimentName}')."
            });
            //notification
            var managers = await _unitOfWork
                .GetRepository<User>()
                .GetListAsync(
                    selector: user => user.UserId,
                    predicate: user => user.Role.RoleName == "Manager" && user.UserId != domainEvent.ResearcherId);

            if (managers.Count == 0)
            {
                return;
            }

            await _notificationService.SendToUsersAsync(new SendNotificationToUsersRequest
            {
                UserIds = managers,
                Title = "Experiment created",
                Message = $"Experiment '{domainEvent.ExperimentName}' has been created.",
                NotificationType = NotificationTypes.ExperimentCreated,
                ReferenceType = NotificationReferenceTypes.Experiment,
                ReferenceId = domainEvent.ExperimentId
            });
        }
    }
}

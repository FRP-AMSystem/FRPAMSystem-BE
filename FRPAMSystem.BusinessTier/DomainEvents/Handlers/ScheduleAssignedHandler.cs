using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{

    public class ScheduleAssignedHandler : DomainEventHandler<ScheduleAssignedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ScheduleAssignedHandler> _logger;

        public ScheduleAssignedHandler(
            INotificationService notificationService,
            IUnitOfWork unitOfWork,
            ILogger<ScheduleAssignedHandler> logger)
        {
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected override async Task HandleAsync(
            ScheduleAssignedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            var profile = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceId == domainEvent.AssignedHumanResourceId);

            if (profile == null)
            {
                _logger.LogWarning(
                    "[ScheduleAssignedHandler] Cannot resolve recipient: " +
                    "HumanResourceProfile not found for HumanResourceId={HumanResourceId}. " +
                    "ScheduleId={ScheduleId}. Notification skipped.",
                    domainEvent.AssignedHumanResourceId,
                    domainEvent.ScheduleId);
                return;
            }

            int recipientUserId = profile.UserId;

            _logger.LogInformation(
                "[ScheduleAssignedHandler] Recipient resolved. " +
                "NotificationType={NotificationType}, ReferenceType={ReferenceType}, " +
                "ReferenceId={ReferenceId}, AssignedHumanResourceId={HumanResourceId}, " +
                "ResolvedRecipientUserId={RecipientUserId}, IsNewAssignment={IsNewAssignment}",
                NotificationTypes.ScheduleAssigned,
                NotificationReferenceTypes.Schedule,
                domainEvent.ScheduleId,
                domainEvent.AssignedHumanResourceId,
                recipientUserId,
                domainEvent.IsNewAssignment);

            var experimentContext = domainEvent.ExperimentName ?? domainEvent.ExperimentId?.ToString();
            var message = experimentContext != null
                ? $"You have been assigned to schedule '{domainEvent.ScheduleTitle}' for experiment '{experimentContext}'."
                : $"You have been assigned to schedule '{domainEvent.ScheduleTitle}'.";

            await _notificationService.SendAsync(new SendNotificationRequest
            {
                UserId = recipientUserId,
                Title = "Schedule assigned",
                Message = message,
                NotificationType = NotificationTypes.ScheduleAssigned,
                ReferenceType = NotificationReferenceTypes.Schedule,
                ReferenceId = domainEvent.ScheduleId
            });
        }
    }
}

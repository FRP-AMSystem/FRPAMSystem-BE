using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class ExperimentSubmittedHandler : DomainEventHandler<ExperimentSubmittedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;

        public ExperimentSubmittedHandler(
            INotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleAsync(
            ExperimentSubmittedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            var reviewers = await _unitOfWork
                .GetRepository<User>()
                .GetQueryable()
                .Include(user => user.Role)
                .Where(user => user.Role.RoleName == "Admin"
                    || user.Role.RoleName == "Manager")
                .Select(user => user.UserId)
                .ToListAsync(cancellationToken);

            if (reviewers.Count == 0)
            {
                return;
            }

            await _notificationService.SendToUsersAsync(new SendNotificationToUsersRequest
            {
                UserIds = reviewers,
                Title = "Experiment submitted",
                Message = $"Experiment '{domainEvent.ExperimentName}' has been submitted for review.",
                NotificationType = NotificationTypes.ExperimentSubmitted,
                ReferenceType = NotificationReferenceTypes.Experiment,
                ReferenceId = domainEvent.ExperimentId
            });
        }
    }
}

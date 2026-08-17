using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class AllocationPlanSubmittedHandler : DomainEventHandler<AllocationPlanSubmittedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;

        public AllocationPlanSubmittedHandler(
            INotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleAsync(
            AllocationPlanSubmittedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            var reviewers = await _unitOfWork
                .GetRepository<User>()
                .GetListAsync(
                    selector: user => user.UserId,
                    predicate: user => user.Role.RoleName == "Manager" && user.UserId != domainEvent.CreatedBy);

            if (reviewers.Count == 0)
            {
                return;
            }

            await _notificationService.SendToUsersAsync(new SendNotificationToUsersRequest
            {
                UserIds = reviewers,
                Title = "Allocation plan submitted",
                Message = $"Allocation plan for experiment '{domainEvent.ExperimentName ?? domainEvent.ExperimentId.ToString()}' has been submitted for approval.",
                NotificationType = NotificationTypes.AllocationPlanSubmitted,
                ReferenceType = NotificationReferenceTypes.AllocationPlan,
                ReferenceId = domainEvent.AllocationPlanId
            });
        }
    }
}

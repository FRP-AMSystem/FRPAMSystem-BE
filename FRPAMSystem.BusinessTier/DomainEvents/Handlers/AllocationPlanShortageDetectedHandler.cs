using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class AllocationPlanShortageDetectedHandler : DomainEventHandler<AllocationPlanShortageDetectedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;

        public AllocationPlanShortageDetectedHandler(
            INotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        protected override async Task HandleAsync(
            AllocationPlanShortageDetectedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            var managers = await _unitOfWork
                .GetRepository<User>()
                .GetListAsync(
                    selector: user => user.UserId,
                    predicate: user => user.Role.RoleName == "Manager");

            if (managers.Count == 0)
            {
                return;
            }

            await _notificationService.SendToUsersAsync(new Payload.Notification.SendNotificationToUsersRequest
            {
                UserIds = managers,
                Title = "Equipment Shortage Detected",
                Message = $"Allocation Plan #{domainEvent.AllocationPlanId} for Experiment '{domainEvent.ExperimentName}' has detected equipment shortages. Please review the plan or purchase/rent new equipment.",
                NotificationType = NotificationTypes.AllocationPlanGenerated,
                ReferenceType = NotificationReferenceTypes.AllocationPlan,
                ReferenceId = domainEvent.AllocationPlanId
            });
        }
    }
}

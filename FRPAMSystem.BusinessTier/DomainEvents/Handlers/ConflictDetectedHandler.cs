using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.DomainEvents.Handlers
{
    public class ConflictDetectedHandler : DomainEventHandler<ConflictDetectedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public ConflictDetectedHandler(
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        protected override async Task HandleAsync(
            ConflictDetectedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            var plan = await _unitOfWork.GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == domainEvent.AllocationPlanId,
                    asNoTracking: false
                );

            if (plan == null || !plan.CreatedBy.HasValue)
            {
                // Cannot reliably resolve recipient without CreatedBy, per requirements:
                return;
            }

            await _notificationService.SendAsync(new SendNotificationRequest
            {
                UserId = plan.CreatedBy.Value,
                Title = "Equipment Shortage Detected",
                Message = $"An equipment shortage of {domainEvent.ShortageQuantity} was detected in your allocation plan #{domainEvent.AllocationPlanId}.",
                NotificationType = NotificationTypes.ConflictDetected,
                ReferenceType = NotificationReferenceTypes.AllocationPlan,
                ReferenceId = domainEvent.AllocationPlanId
            });
        }
    }
}

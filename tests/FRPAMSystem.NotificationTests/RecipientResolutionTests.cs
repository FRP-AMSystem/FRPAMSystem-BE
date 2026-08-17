using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.DomainEvents.Handlers;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace FRPAMSystem.NotificationTests
{
    public class RecipientResolutionTests
    {
        private readonly Mock<INotificationService> _notificationServiceMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IAuditLogService> _auditLogServiceMock = new();
        private readonly Mock<ILogger<ScheduleAssignedHandler>> _scheduleHandlerLoggerMock = new();

        [Fact]
        public async Task Case1_ScheduleAssigned_UserACreatesAction_NotificationSentToUserB()
        {
            // Arrange:
            // User A (Creator) assigns schedule to HumanResourceProfile ID=10 (which belongs to User B, ID=99)
            int assignedHumanResourceId = 10;
            int expectedRecipientUserId = 99;

            var hrRepoMock = new Mock<IGenericRepository<HumanResourceProfile>>();
            hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new HumanResourceProfile
                {
                    HumanResourceId = assignedHumanResourceId,
                    UserId = expectedRecipientUserId
                });

            _unitOfWorkMock.Setup(u => u.GetRepository<HumanResourceProfile>())
                .Returns(hrRepoMock.Object);

            var handler = new ScheduleAssignedHandler(
                _notificationServiceMock.Object,
                _unitOfWorkMock.Object,
                _scheduleHandlerLoggerMock.Object);

            var domainEvent = new ScheduleAssignedEvent(
                scheduleId: 1,
                allocationPlanId: 100,
                experimentId: 5,
                experimentName: "Pine Growth Study",
                scheduleTitle: "Field Planting Phase 1",
                assignedHumanResourceId: assignedHumanResourceId,
                isNewAssignment: true);

            SendNotificationRequest? capturedRequest = null;
            _notificationServiceMock.Setup(s => s.SendAsync(It.IsAny<SendNotificationRequest>()))
                .Callback<SendNotificationRequest>(req => capturedRequest = req)
                .ReturnsAsync(new NotificationResponse());

            // Act
            await handler.HandleAsync(domainEvent, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(expectedRecipientUserId, capturedRequest.UserId);
            Assert.Equal(NotificationTypes.ScheduleAssigned, capturedRequest.NotificationType);
            Assert.Equal(NotificationReferenceTypes.Schedule, capturedRequest.ReferenceType);
            Assert.Equal(1, capturedRequest.ReferenceId);
            Assert.Contains("Pine Growth Study", capturedRequest.Message);
        }

        [Fact]
        public async Task Case1_AllocationPlanApproved_UserAApproves_NotificationSentToCreatorUserB()
        {
            // Arrange:
            // Manager (User ID=1) approves plan created by Researcher (User ID=2)
            int approverUserId = 1;
            int creatorUserId = 2;

            var handler = new AllocationPlanApprovedHandler(
                _notificationServiceMock.Object,
                _auditLogServiceMock.Object);

            var domainEvent = new AllocationPlanApprovedEvent(
                allocationPlanId: 42,
                experimentId: 10,
                experimentName: "Acacia Breeding",
                createdBy: creatorUserId,
                approvedBy: approverUserId);

            SendNotificationRequest? capturedRequest = null;
            _notificationServiceMock.Setup(s => s.SendAsync(It.IsAny<SendNotificationRequest>()))
                .Callback<SendNotificationRequest>(req => capturedRequest = req)
                .ReturnsAsync(new NotificationResponse());

            // Act
            await handler.HandleAsync(domainEvent, CancellationToken.None);

            // Assert: Notification is sent to Creator (User B = 2), NOT Approver (User A = 1)
            Assert.NotNull(capturedRequest);
            Assert.Equal(creatorUserId, capturedRequest.UserId);
            Assert.Equal(NotificationTypes.AllocationPlanApproved, capturedRequest.NotificationType);
            Assert.Equal(NotificationReferenceTypes.AllocationPlan, capturedRequest.ReferenceType);
            Assert.Equal(42, capturedRequest.ReferenceId);
        }

        [Fact]
        public async Task Case1_AllocationPlanRejected_UserARejects_NotificationSentToCreatorUserB()
        {
            // Arrange:
            int rejectedByUserId = 1;
            int creatorUserId = 3;

            var handler = new AllocationPlanRejectedHandler(
                _notificationServiceMock.Object,
                _auditLogServiceMock.Object);

            var domainEvent = new AllocationPlanRejectedEvent(
                allocationPlanId: 43,
                experimentId: 11,
                experimentName: "Teak Plantation",
                createdBy: creatorUserId,
                rejectedBy: rejectedByUserId);

            SendNotificationRequest? capturedRequest = null;
            _notificationServiceMock.Setup(s => s.SendAsync(It.IsAny<SendNotificationRequest>()))
                .Callback<SendNotificationRequest>(req => capturedRequest = req)
                .ReturnsAsync(new NotificationResponse());

            // Act
            await handler.HandleAsync(domainEvent, CancellationToken.None);

            // Assert: Notification is sent to Creator (User B = 3)
            Assert.NotNull(capturedRequest);
            Assert.Equal(creatorUserId, capturedRequest.UserId);
            Assert.Equal(NotificationTypes.AllocationPlanRejected, capturedRequest.NotificationType);
        }

        [Fact]
        public async Task Case2_AllocationPlanGenerated_NotifiesCreator_WhenActorIsDefinedRecipient()
        {
            // Arrange:
            // When AI generates plan triggered by User ID=5, confirmation notification is sent to User ID=5
            int triggerUserId = 5;

            var handler = new AllocationPlanGeneratedHandler(_notificationServiceMock.Object);

            var domainEvent = new AllocationPlanGeneratedEvent(
                allocationPlanId: 50,
                experimentId: 12,
                experimentName: "Eucalyptus Trial",
                createdBy: triggerUserId);

            SendNotificationRequest? capturedRequest = null;
            _notificationServiceMock.Setup(s => s.SendAsync(It.IsAny<SendNotificationRequest>()))
                .Callback<SendNotificationRequest>(req => capturedRequest = req)
                .ReturnsAsync(new NotificationResponse());

            // Act
            await handler.HandleAsync(domainEvent, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(triggerUserId, capturedRequest.UserId);
            Assert.Equal(NotificationTypes.AllocationPlanGenerated, capturedRequest.NotificationType);
        }

        [Fact]
        public async Task Case3_AllocationPlanSubmitted_SendsNotificationToAllReviewers()
        {
            // Arrange:
            // When plan is submitted, all users with role "Admin" or "Manager" receive notification
            var reviewerUserIds = new List<int> { 101, 102 };

            var userRepoMock = new Mock<IGenericRepository<User>>();
            userRepoMock.Setup(r => r.GetListAsync<int>(
                    It.IsAny<Expression<Func<User, int>>>(),
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                    It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(reviewerUserIds);

            _unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(userRepoMock.Object);

            var handler = new AllocationPlanSubmittedHandler(
                _notificationServiceMock.Object,
                _unitOfWorkMock.Object);

            var domainEvent = new AllocationPlanSubmittedEvent(
                allocationPlanId: 77,
                experimentId: 20,
                experimentName: "Mangrove Restoration",
                createdBy: 5);

            SendNotificationToUsersRequest? capturedRequest = null;
            _notificationServiceMock.Setup(s => s.SendToUsersAsync(It.IsAny<SendNotificationToUsersRequest>()))
                .Callback<SendNotificationToUsersRequest>(req => capturedRequest = req)
                .ReturnsAsync(new List<NotificationResponse>());

            // Act
            await handler.HandleAsync(domainEvent, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(2, capturedRequest.UserIds.Count);
            Assert.Contains(101, capturedRequest.UserIds);
            Assert.Contains(102, capturedRequest.UserIds);
            Assert.Equal(NotificationTypes.AllocationPlanSubmitted, capturedRequest.NotificationType);
        }

        [Fact]
        public async Task Case1_ExperimentCreated_UserACreatesExperiment_NotifiesManagersExcludingActor()
        {
            // Arrange
            int researcherUserId = 88;
            var manager1 = 101;
            var manager2 = 102;
            
            var notificationServiceMock = new Mock<INotificationService>();
            var auditLogServiceMock = new Mock<IAuditLogService>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var userRepoMock = new Mock<IGenericRepository<User>>();
            
            userRepoMock.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<User, int>>>(),
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new List<int> { manager1, manager2 });

            unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(userRepoMock.Object);

            var handler = new ExperimentCreatedHandler(
                notificationServiceMock.Object,
                auditLogServiceMock.Object,
                unitOfWorkMock.Object);

            var domainEvent = new ExperimentCreatedEvent(
                experimentId: 301,
                experimentName: "Soil Carbon Study",
                researcherId: researcherUserId);

            SendNotificationToUsersRequest? capturedRequest = null;
            notificationServiceMock.Setup(s => s.SendToUsersAsync(It.IsAny<SendNotificationToUsersRequest>()))
                .Callback<SendNotificationToUsersRequest>(req => capturedRequest = req)
                .ReturnsAsync(new List<NotificationResponse>());

            // Act
            await handler.HandleAsync(domainEvent, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(2, capturedRequest.UserIds.Count);
            Assert.Contains(manager1, capturedRequest.UserIds);
            Assert.Contains(manager2, capturedRequest.UserIds);
            Assert.DoesNotContain(researcherUserId, capturedRequest.UserIds);
            Assert.Equal(NotificationTypes.ExperimentCreated, capturedRequest.NotificationType);
            Assert.Equal(NotificationReferenceTypes.Experiment, capturedRequest.ReferenceType);
            Assert.Equal(301, capturedRequest.ReferenceId);
        }

        [Fact]
        public async Task Case5_ScheduleAssigned_CannotResolveRecipient_DoesNotSendNotification()
        {
            // Arrange:
            // HumanResourceProfile does not exist for the assigned ID
            var hrRepoMock = new Mock<IGenericRepository<HumanResourceProfile>>();
            hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((HumanResourceProfile?)null);

            _unitOfWorkMock.Setup(u => u.GetRepository<HumanResourceProfile>())
                .Returns(hrRepoMock.Object);

            var handler = new ScheduleAssignedHandler(
                _notificationServiceMock.Object,
                _unitOfWorkMock.Object,
                _scheduleHandlerLoggerMock.Object);

            var domainEvent = new ScheduleAssignedEvent(
                scheduleId: 999,
                allocationPlanId: 100,
                experimentId: 5,
                experimentName: "Invalid Assignment",
                scheduleTitle: "Task",
                assignedHumanResourceId: 99999, // Non-existent HR
                isNewAssignment: true);

            // Act
            await handler.HandleAsync(domainEvent, CancellationToken.None);

            // Assert: NotificationService.SendAsync should NEVER be called with invalid/unresolved recipient
            _notificationServiceMock.Verify(s => s.SendAsync(It.IsAny<SendNotificationRequest>()), Times.Never);
        }

    }
}

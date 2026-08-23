using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Implements;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.BusinessTier.SignalR;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace FRPAMSystem.NotificationTests
{
    public class NotificationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IEmailService> _emailServiceMock = new();
        private readonly Mock<IHubContext<NotificationHub, INotificationClient>> _hubContextMock = new();
        private readonly Mock<IHubClients<INotificationClient>> _hubClientsMock = new();
        private readonly Mock<INotificationClient> _notificationClientMock = new();
        private readonly Mock<ILogger<NotificationService>> _loggerMock = new();
        private readonly Mock<IClock> _clockMock = new();

        public NotificationServiceTests()
        {
            _hubClientsMock.Setup(c => c.User(It.IsAny<string>()))
                .Returns(_notificationClientMock.Object);
            _hubContextMock.Setup(h => h.Clients)
                .Returns(_hubClientsMock.Object);
        }

        [Fact]
        public async Task Case4_SendToUsersAsync_DeduplicatesUserIds_CreatesOnlyOneNotificationPerUser()
        {
            // Arrange:
            // Input list contains duplicate user IDs: [10, 10, 20, 20, 10]
            var inputUserIds = new List<int> { 10, 10, 20, 20, 10 };

            var userRepoMock = new Mock<IGenericRepository<User>>();
            userRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                    It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Expression<Func<User, bool>>? pred, object? a, object? b, bool c) =>
                {
                    return new User { UserId = 10, FullName = "Test User", Email = "test@example.com" };
                });

            var insertedNotifications = new List<Notification>();
            var notificationRepoMock = new Mock<IGenericRepository<Notification>>();
            notificationRepoMock.Setup(r => r.InsertAsync(It.IsAny<Notification>()))
                .Callback<Notification>(n => insertedNotifications.Add(n))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(userRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<Notification>()).Returns(notificationRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);

            var service = new NotificationService(
                _unitOfWorkMock.Object,
                _emailServiceMock.Object,
                _hubContextMock.Object,
                _loggerMock.Object,
                _clockMock.Object);

            var request = new SendNotificationToUsersRequest
            {
                UserIds = inputUserIds,
                Title = "Plan Submitted",
                Message = "A new plan has been submitted.",
                NotificationType = NotificationTypes.AllocationPlanSubmitted,
                ReferenceType = NotificationReferenceTypes.AllocationPlan,
                ReferenceId = 1
            };

            // Act
            var results = await service.SendToUsersAsync(request);

            // Assert: Exactly 2 notifications created (one for 10, one for 20), not 5
            Assert.Equal(2, results.Count);
            Assert.Equal(2, insertedNotifications.Count);
            Assert.Contains(insertedNotifications, n => n.UserId == 10);
            Assert.Contains(insertedNotifications, n => n.UserId == 20);
        }

        [Fact]
        public async Task Case6_MarkAsReadAsync_UserAStateDoesNotAffectUserB()
        {
            // Arrange:
            // Two separate notifications for User A (ID=1) and User B (ID=2)
            var notificationA = new Notification
            {
                NotificationId = 101,
                UserId = 1,
                Title = "Alert A",
                Message = "Msg",
                NotificationType = NotificationTypes.ScheduleAssigned,
                IsRead = false
            };

            var notificationB = new Notification
            {
                NotificationId = 102,
                UserId = 2,
                Title = "Alert B",
                Message = "Msg",
                NotificationType = NotificationTypes.ScheduleAssigned,
                IsRead = false
            };

            var notificationRepoMock = new Mock<IGenericRepository<Notification>>();
            // Setup finding Notification 101 for User 1
            notificationRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Notification, bool>>>(),
                    It.IsAny<Func<IQueryable<Notification>, IOrderedQueryable<Notification>>>(),
                    It.IsAny<Func<IQueryable<Notification>, IIncludableQueryable<Notification, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Expression<Func<Notification, bool>>? pred, object? a, object? b, bool c) =>
                {
                    if (pred == null) return null;
                    var compiled = pred.Compile();
                    if (compiled(notificationA)) return notificationA;
                    if (compiled(notificationB)) return notificationB;
                    return null;
                });

            _unitOfWorkMock.Setup(u => u.GetRepository<Notification>()).Returns(notificationRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);

            var service = new NotificationService(
                _unitOfWorkMock.Object,
                _emailServiceMock.Object,
                _hubContextMock.Object,
                _loggerMock.Object,
                _clockMock.Object);

            // Act: User 1 marks their notification (101) as read
            var result = await service.MarkAsReadAsync(101, userId: 1);

            // Assert:
            // Notification A is marked as read
            Assert.True(result);
            Assert.True(notificationA.IsRead);
            Assert.NotNull(notificationA.ReadAt);

            // Notification B remains unread!
            Assert.False(notificationB.IsRead);
            Assert.Null(notificationB.ReadAt);
        }

        [Fact]
        public async Task Case6_UserCannotAccessOrMutateOtherUserNotifications()
        {
            // Arrange:
            // Notification 101 belongs to User 1
            var notificationA = new Notification
            {
                NotificationId = 101,
                UserId = 1,
                Title = "Alert A",
                Message = "Msg",
                NotificationType = NotificationTypes.ScheduleAssigned,
                IsRead = false
            };

            var notificationRepoMock = new Mock<IGenericRepository<Notification>>();
            // Predicate filters by both notificationId AND userId
            notificationRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Notification, bool>>>(),
                    It.IsAny<Func<IQueryable<Notification>, IOrderedQueryable<Notification>>>(),
                    It.IsAny<Func<IQueryable<Notification>, IIncludableQueryable<Notification, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Expression<Func<Notification, bool>>? pred, object? a, object? b, bool c) =>
                {
                    if (pred == null) return null;
                    var compiled = pred.Compile();
                    return compiled(notificationA) ? notificationA : null;
                });

            _unitOfWorkMock.Setup(u => u.GetRepository<Notification>()).Returns(notificationRepoMock.Object);

            var service = new NotificationService(
                _unitOfWorkMock.Object,
                _emailServiceMock.Object,
                _hubContextMock.Object,
                _loggerMock.Object,
                _clockMock.Object);

            // Act: User 2 tries to mark User 1's notification as read
            var markResult = await service.MarkAsReadAsync(101, userId: 2);

            // Act: User 2 tries to get User 1's notification
            var getResult = await service.GetByIdForUserAsync(101, userId: 2);

            // Act: User 2 tries to soft delete User 1's notification
            var deleteResult = await service.SoftDeleteAsync(101, userId: 2);

            // Assert: All operations fail for non-owner
            Assert.False(markResult);
            Assert.Null(getResult);
            Assert.False(deleteResult);
            Assert.False(notificationA.IsRead);
            Assert.False(notificationA.IsDeleted);
        }

        [Fact]
        public async Task Case7_SendAsync_ThrowsWhenUserDoesNotExist()
        {
            // Arrange:
            var userRepoMock = new Mock<IGenericRepository<User>>();
            userRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                    It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((User?)null); // User does not exist

            _unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(userRepoMock.Object);

            var service = new NotificationService(
                _unitOfWorkMock.Object,
                _emailServiceMock.Object,
                _hubContextMock.Object,
                _loggerMock.Object,
                _clockMock.Object);

            var request = new SendNotificationRequest
            {
                UserId = 999,
                Title = "Test",
                Message = "Test message",
                NotificationType = NotificationTypes.ScheduleAssigned
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.SendAsync(request));
        }
    }
}

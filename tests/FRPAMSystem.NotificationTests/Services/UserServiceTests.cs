using FRPAMSystem.BusinessTier.Services.Implements;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace FRPAMSystem.NotificationTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        private readonly Mock<IGenericRepository<User>> _userRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationPlan>> _planRepoMock = new();
        private readonly Mock<IGenericRepository<Experiment>> _experimentRepoMock = new();
        private readonly Mock<IGenericRepository<HumanResourceProfile>> _hrRepoMock = new();
        private readonly Mock<IGenericRepository<Notification>> _notificationRepoMock = new();
        private readonly Mock<IGenericRepository<Schedule>> _scheduleRepoMock = new();

        public UserServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationPlan>()).Returns(_planRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<Experiment>()).Returns(_experimentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<HumanResourceProfile>()).Returns(_hrRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<Notification>()).Returns(_notificationRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<Schedule>()).Returns(_scheduleRepoMock.Object);
        }

        // UT148-TC48
        // Normal
        [Fact]
        public async Task DeleteUserAsync_WhenNoDependencies_ShouldDeleteUserAndReturnTrue()
        {
            // Arrange
            int userId = 10;
            var user = new User { UserId = userId, Username = "unlinked_user" };

            _userRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                    It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            _planRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationPlan, bool>>>())).ReturnsAsync(false);
            _experimentRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Experiment, bool>>>())).ReturnsAsync(false);
            _hrRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<HumanResourceProfile, bool>>>())).ReturnsAsync(false);
            _notificationRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Notification, bool>>>())).ReturnsAsync(false);
            _scheduleRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Schedule, bool>>>())).ReturnsAsync(false);

            var service = new UserService(_unitOfWorkMock.Object);

            // Act
            var result = await service.DeleteUserAsync(userId);

            // Assert
            Assert.True(result);
            _userRepoMock.Verify(r => r.Delete(user), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // UT148-TC49
        // Abnormal
        [Fact]
        public async Task DeleteUserAsync_WhenUserDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            _userRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                    It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((User?)null);

            var service = new UserService(_unitOfWorkMock.Object);

            // Act
            var result = await service.DeleteUserAsync(999);

            // Assert
            Assert.False(result);
        }

        // UT148-TC50
        // Abnormal
        [Fact]
        public async Task DeleteUserAsync_WhenUserCreatedAllocationPlans_ShouldThrowException()
        {
            // Arrange
            int userId = 10;
            var user = new User { UserId = userId };

            _userRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                    It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            // User created allocation plans
            _planRepoMock.Setup(r => r.AnyAsync(It.Is<Expression<Func<AllocationPlan, bool>>>(p => p.Compile()(new AllocationPlan { CreatedBy = userId }))))
                .ReturnsAsync(true);

            var service = new UserService(_unitOfWorkMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteUserAsync(userId));
            Assert.Equal("Cannot delete user because they created allocation plans.", ex.Message);
        }

        // UT148-TC51
        // Abnormal
        [Fact]
        public async Task DeleteUserAsync_WhenUserHasHumanResourceProfile_ShouldThrowException()
        {
            // Arrange
            int userId = 10;
            var user = new User { UserId = userId };

            _userRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                    It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(user);

            _planRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationPlan, bool>>>())).ReturnsAsync(false);
            _experimentRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Experiment, bool>>>())).ReturnsAsync(false);
            _hrRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<HumanResourceProfile, bool>>>())).ReturnsAsync(true);

            var service = new UserService(_unitOfWorkMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteUserAsync(userId));
            Assert.Equal("Cannot delete user because they have a human resource profile.", ex.Message);
        }
    }
}

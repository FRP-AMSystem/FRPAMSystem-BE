using FRPAMSystem.BusinessTier.Services.Implements;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using FRPAMSystem.NotificationTests.Helpers;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace FRPAMSystem.NotificationTests.Services
{
    public class HumanResourceProfileServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        private readonly Mock<IGenericRepository<HumanResourceProfile>> _hrRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationHumanDetail>> _allocationRepoMock = new();
        private readonly Mock<IGenericRepository<Schedule>> _scheduleRepoMock = new();
        private readonly Mock<IGenericRepository<HumanResourceSkill>> _skillRepoMock = new();

        public HumanResourceProfileServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<HumanResourceProfile>()).Returns(_hrRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationHumanDetail>()).Returns(_allocationRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<Schedule>()).Returns(_scheduleRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<HumanResourceSkill>()).Returns(_skillRepoMock.Object);
            _allocationRepoMock.Setup(r => r.GetQueryable()).Returns(new List<AllocationHumanDetail>().BuildMockQueryable());
        }

        // UT148-TC52
        // Normal
        [Fact]
        public async Task DeleteHumanResourceProfileAsync_WhenNoDependencies_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            int profileId = 5;
            var profile = new HumanResourceProfile { HumanResourceId = profileId, UserId = 10 };

            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(profile);

            _allocationRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationHumanDetail, bool>>>())).ReturnsAsync(false);
            _scheduleRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Schedule, bool>>>())).ReturnsAsync(false);
            _skillRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<HumanResourceSkill, bool>>>())).ReturnsAsync(false);

            var service = new HumanResourceProfileService(_unitOfWorkMock.Object);

            // Act
            var result = await service.DeleteHumanResourceProfileAsync(profileId);

            // Assert
            Assert.True(result);
            _hrRepoMock.Verify(r => r.Delete(profile), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // UT148-TC53
        // Abnormal
        [Fact]
        public async Task DeleteHumanResourceProfileAsync_WhenProfileDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((HumanResourceProfile?)null);

            var service = new HumanResourceProfileService(_unitOfWorkMock.Object);

            // Act
            var result = await service.DeleteHumanResourceProfileAsync(999);

            // Assert
            Assert.False(result);
        }

        // UT148-TC54
        // Abnormal
        [Fact]
        public async Task DeleteHumanResourceProfileAsync_WhenProfileHasAllocations_ShouldThrowException()
        {
            // Arrange
            int profileId = 5;
            var profile = new HumanResourceProfile { HumanResourceId = profileId };

            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(profile);

            _allocationRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationHumanDetail, bool>>>())).ReturnsAsync(true);

            var service = new HumanResourceProfileService(_unitOfWorkMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteHumanResourceProfileAsync(profileId));
            Assert.Equal("Cannot delete human resource profile because it has allocation records.", ex.Message);
        }

        [Fact]
        public async Task CreateHumanResourceProfileAsync_ShouldAlwaysDefaultToAvailable()
        {
            // Arrange
            var userRepoMock = new Mock<IGenericRepository<User>>();
            _unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(userRepoMock.Object);

            userRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                    It.IsAny<Func<IQueryable<User>, IIncludableQueryable<User, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new User { UserId = 2, FullName = "Dr. Green", Role = new Role { RoleName = "Researcher" } });

            _hrRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<HumanResourceProfile, bool>>>()))
                .ReturnsAsync(false);

            HumanResourceProfile? capturedProfile = null;
            _hrRepoMock.Setup(r => r.InsertAsync(It.IsAny<HumanResourceProfile>()))
                .Callback<HumanResourceProfile>(p =>
                {
                    p.HumanResourceId = 77;
                    capturedProfile = p;
                })
                .Returns(Task.CompletedTask);

            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(() => capturedProfile != null ? new HumanResourceProfile
                {
                    HumanResourceId = capturedProfile.HumanResourceId,
                    UserId = capturedProfile.UserId,
                    Status = capturedProfile.Status,
                    MaxWorkingHoursPerDay = capturedProfile.MaxWorkingHoursPerDay,
                    CurrentWorkload = capturedProfile.CurrentWorkload,
                    User = new User { UserId = 2, FullName = "Dr. Green", Role = new Role { RoleName = "Researcher" } }
                } : null);

            var service = new HumanResourceProfileService(_unitOfWorkMock.Object);

            // Act
            var result = await service.CreateHumanResourceProfileAsync(new FRPAMSystem.BusinessTier.Payload.HumanResourceProfile.HumanResourceProfileRequest
            {
                UserId = 2,
                MaxWorkingHoursPerDay = 8,
                CurrentWorkload = 0,
                Status = FRPAMSystem.BusinessTier.Enums.HumanResourceStatus.Inactive // Attempt to pass Inactive
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(FRPAMSystem.BusinessTier.Enums.HumanResourceStatus.Available.ToString(), result.Status); // Enforced Available
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeactivateHumanResourceProfileAsync_WhenNoActiveAllocations_ShouldSetStatusToInactive()
        {
            // Arrange
            var profile = new HumanResourceProfile
            {
                HumanResourceId = 10,
                UserId = 3,
                Status = FRPAMSystem.BusinessTier.Enums.HumanResourceStatus.Available.ToString()
            };

            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(profile);

            _allocationRepoMock.Setup(r => r.GetQueryable())
                .Returns(new List<AllocationHumanDetail>().BuildMockQueryable());

            _scheduleRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Schedule, bool>>>()))
                .ReturnsAsync(false);

            var service = new HumanResourceProfileService(_unitOfWorkMock.Object);

            // Act
            var result = await service.DeactivateHumanResourceProfileAsync(10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(FRPAMSystem.BusinessTier.Enums.HumanResourceStatus.Inactive.ToString(), profile.Status);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task SetLeaveHumanResourceProfileAsync_WhenNoActiveAllocations_ShouldSetStatusToOnLeave()
        {
            // Arrange
            var profile = new HumanResourceProfile
            {
                HumanResourceId = 11,
                UserId = 4,
                Status = FRPAMSystem.BusinessTier.Enums.HumanResourceStatus.Available.ToString()
            };

            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(profile);

            _allocationRepoMock.Setup(r => r.GetQueryable())
                .Returns(new List<AllocationHumanDetail>().BuildMockQueryable());

            _scheduleRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Schedule, bool>>>()))
                .ReturnsAsync(false);

            var service = new HumanResourceProfileService(_unitOfWorkMock.Object);

            // Act
            var result = await service.SetLeaveHumanResourceProfileAsync(11);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(FRPAMSystem.BusinessTier.Enums.HumanResourceStatus.OnLeave.ToString(), profile.Status);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task ActivateHumanResourceProfileAsync_ShouldSetStatusToAvailable()
        {
            // Arrange
            var profile = new HumanResourceProfile
            {
                HumanResourceId = 12,
                UserId = 5,
                Status = FRPAMSystem.BusinessTier.Enums.HumanResourceStatus.Inactive.ToString()
            };

            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(profile);

            var service = new HumanResourceProfileService(_unitOfWorkMock.Object);

            // Act
            var result = await service.ActivateHumanResourceProfileAsync(12);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(FRPAMSystem.BusinessTier.Enums.HumanResourceStatus.Available.ToString(), profile.Status);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}

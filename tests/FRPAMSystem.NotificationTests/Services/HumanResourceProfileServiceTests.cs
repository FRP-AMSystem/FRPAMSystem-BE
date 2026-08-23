using FRPAMSystem.BusinessTier.Services.Implements;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
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
    }
}

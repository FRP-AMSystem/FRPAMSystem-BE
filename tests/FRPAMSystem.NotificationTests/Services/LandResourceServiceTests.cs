using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.LandResource;
using FRPAMSystem.BusinessTier.Services.Implements;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using FRPAMSystem.NotificationTests.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace FRPAMSystem.NotificationTests.Services
{
    public class LandResourceServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IGenericRepository<LandResource>> _landRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationLandDetail>> _detailRepoMock = new();
        private readonly Mock<IGenericRepository<Area>> _areaRepoMock = new();
        private readonly Mock<IClock> _clockMock = new();

        private readonly DateTime _fixedNow = new(2026, 9, 21, 10, 0, 0);

        public LandResourceServiceTests()
        {
            _clockMock.Setup(c => c.Now).Returns(_fixedNow);
            _unitOfWorkMock.Setup(u => u.GetRepository<LandResource>()).Returns(_landRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationLandDetail>()).Returns(_detailRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<Area>()).Returns(_areaRepoMock.Object);
        }

        [Fact]
        public async Task SyncLandStatusAsync_WhenUpcomingApprovedAllocationExists_ShouldTransitionToReserved()
        {
            // Arrange
            var land = new LandResource
            {
                LandId = 1,
                LandCode = "L-01",
                Status = LandResourceStatus.Available.ToString()
            };

            _landRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<LandResource, bool>>>(),
                    null,
                    null,
                    false))
                .ReturnsAsync(land);

            var allocations = new List<AllocationLandDetail>
            {
                new AllocationLandDetail
                {
                    AllocationLandDetailId = 101,
                    LandId = 1,
                    StartDate = _fixedNow.AddDays(5),
                    EndDate = _fixedNow.AddDays(15),
                    Status = AllocationDetailStatus.Allocated.ToString(),
                    AllocationPlan = new AllocationPlan
                    {
                        AllocationPlanId = 1,
                        ApproveStatus = AllocationPlanStatus.Approved.ToString()
                    }
                }
            };

            _detailRepoMock.Setup(r => r.GetQueryable())
                .Returns(allocations.BuildMockQueryable());

            var service = new LandResourceService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act
            await service.SyncLandStatusAsync(1);

            // Assert
            Assert.Equal(LandResourceStatus.Reserved.ToString(), land.Status);
            _landRepoMock.Verify(r => r.Update(land), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task SyncLandStatusAsync_WhenCurrentApprovedAllocationActive_ShouldTransitionToInUse()
        {
            // Arrange
            var land = new LandResource
            {
                LandId = 1,
                LandCode = "L-01",
                Status = LandResourceStatus.Reserved.ToString()
            };

            _landRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<LandResource, bool>>>(),
                    null,
                    null,
                    false))
                .ReturnsAsync(land);

            var allocations = new List<AllocationLandDetail>
            {
                new AllocationLandDetail
                {
                    AllocationLandDetailId = 101,
                    LandId = 1,
                    StartDate = _fixedNow.AddDays(-2),
                    EndDate = _fixedNow.AddDays(5),
                    Status = AllocationDetailStatus.Allocated.ToString(),
                    AllocationPlan = new AllocationPlan
                    {
                        AllocationPlanId = 1,
                        ApproveStatus = AllocationPlanStatus.Approved.ToString()
                    }
                }
            };

            _detailRepoMock.Setup(r => r.GetQueryable())
                .Returns(allocations.BuildMockQueryable());

            var service = new LandResourceService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act
            await service.SyncLandStatusAsync(1);

            // Assert
            Assert.Equal(LandResourceStatus.InUse.ToString(), land.Status);
            _landRepoMock.Verify(r => r.Update(land), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task SyncLandStatusAsync_WhenNoActiveOrUpcomingAllocation_ShouldTransitionToAvailable()
        {
            // Arrange
            var land = new LandResource
            {
                LandId = 1,
                LandCode = "L-01",
                Status = LandResourceStatus.InUse.ToString()
            };

            _landRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<LandResource, bool>>>(),
                    null,
                    null,
                    false))
                .ReturnsAsync(land);

            // All past or completed
            var allocations = new List<AllocationLandDetail>
            {
                new AllocationLandDetail
                {
                    AllocationLandDetailId = 101,
                    LandId = 1,
                    StartDate = _fixedNow.AddDays(-20),
                    EndDate = _fixedNow.AddDays(-5),
                    Status = AllocationDetailStatus.Completed.ToString(),
                    AllocationPlan = new AllocationPlan
                    {
                        AllocationPlanId = 1,
                        ApproveStatus = AllocationPlanStatus.Approved.ToString()
                    }
                }
            };

            _detailRepoMock.Setup(r => r.GetQueryable())
                .Returns(allocations.BuildMockQueryable());

            var service = new LandResourceService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act
            await service.SyncLandStatusAsync(1);

            // Assert
            Assert.Equal(LandResourceStatus.Available.ToString(), land.Status);
            _landRepoMock.Verify(r => r.Update(land), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task SyncLandStatusAsync_WhenLandIsUnavailable_ShouldNotOverwrite()
        {
            // Arrange
            var land = new LandResource
            {
                LandId = 1,
                LandCode = "L-01",
                Status = LandResourceStatus.Unavailable.ToString()
            };

            _landRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<LandResource, bool>>>(),
                    null,
                    null,
                    false))
                .ReturnsAsync(land);

            var service = new LandResourceService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act
            await service.SyncLandStatusAsync(1);

            // Assert: status remains Unavailable and no update committed
            Assert.Equal(LandResourceStatus.Unavailable.ToString(), land.Status);
            _landRepoMock.Verify(r => r.Update(It.IsAny<LandResource>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateLandResourceAsync_WhenSettingUnavailableWhileActivelyInUse_ShouldThrowException()
        {
            // Arrange
            var request = new LandResourceRequest
            {
                AreaId = 1,
                LandCode = "L-01",
                SoilType = "Loam",
                AreaSize = 100m,
                Status = LandResourceStatus.Unavailable
            };

            _areaRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Area, bool>>>()))
                .ReturnsAsync(true);

            _landRepoMock.Setup(r => r.GetQueryable())
                .Returns(new List<LandResource>().BuildMockQueryable());

            var activeAllocations = new List<AllocationLandDetail>
            {
                new AllocationLandDetail
                {
                    AllocationLandDetailId = 10,
                    LandId = 1,
                    StartDate = _fixedNow.AddDays(-1),
                    EndDate = _fixedNow.AddDays(10),
                    Status = AllocationDetailStatus.InUse.ToString(),
                    AllocationPlan = new AllocationPlan
                    {
                        AllocationPlanId = 1,
                        ApproveStatus = AllocationPlanStatus.Approved.ToString()
                    }
                }
            };

            _detailRepoMock.Setup(r => r.GetQueryable())
                .Returns(activeAllocations.BuildMockQueryable());

            var service = new LandResourceService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateLandResourceAsync(1, request));
            Assert.Contains("actively in use", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}

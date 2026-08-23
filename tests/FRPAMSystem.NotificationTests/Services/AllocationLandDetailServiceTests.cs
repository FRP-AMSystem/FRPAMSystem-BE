using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.AllocationLandDetail;
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
    public class AllocationLandDetailServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IGenericRepository<AllocationLandDetail>> _detailRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationPlan>> _planRepoMock = new();
        private readonly Mock<IGenericRepository<LandResource>> _landRepoMock = new();
        private readonly Mock<IGenericRepository<ExperimentLandRequirement>> _landReqRepoMock = new();

        public AllocationLandDetailServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationLandDetail>()).Returns(_detailRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationPlan>()).Returns(_planRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<LandResource>()).Returns(_landRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<ExperimentLandRequirement>()).Returns(_landReqRepoMock.Object);
        }

        // UT148-TC37
        // Normal
        [Fact]
        public async Task CreateAllocationLandDetailAsync_WithValidRequest_ShouldInsertDetail()
        {
            // Arrange
            var request = new AllocationLandDetailRequest
            {
                AllocationPlanId = 1,
                LandId = 10,
                ExpLandReqId = 5,
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 30),
                Status = AllocationDetailStatus.Reserved
            };

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new AllocationPlan { AllocationPlanId = 1, ExperimentId = 2, ApproveStatus = AllocationPlanStatus.Draft.ToString() });

            _landRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<LandResource, bool>>>(),
                    It.IsAny<Func<IQueryable<LandResource>, IOrderedQueryable<LandResource>>>(),
                    It.IsAny<Func<IQueryable<LandResource>, IIncludableQueryable<LandResource, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new LandResource { LandId = 10, LandCode = "PLOT-A1", SoilType = "Loam", AreaSize = 50.0m });

            _landReqRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentLandRequirement, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentLandRequirement>, IOrderedQueryable<ExperimentLandRequirement>>>(),
                    It.IsAny<Func<IQueryable<ExperimentLandRequirement>, IIncludableQueryable<ExperimentLandRequirement, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new ExperimentLandRequirement { ExpLandReqId = 5, ExperimentId = 2, RequiredSoilType = "Loam", RequiredArea = 10.0m });

            _detailRepoMock.Setup(r => r.GetQueryable()).Returns(new List<AllocationLandDetail>().BuildMockQueryable());

            _detailRepoMock.Setup(r => r.InsertAsync(It.IsAny<AllocationLandDetail>()))
                .Callback<AllocationLandDetail>(d => d.AllocationLandDetailId = 301)
                .Returns(Task.CompletedTask);

            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationLandDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationLandDetail>, IOrderedQueryable<AllocationLandDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationLandDetail>, IIncludableQueryable<AllocationLandDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new AllocationLandDetail
                {
                    AllocationLandDetailId = 301,
                    AllocationPlanId = 1,
                    AllocationPlan = new AllocationPlan { AllocationPlanId = 1, ExperimentId = 2, Experiment = new Experiment { ExperimentId = 2, ExperimentName = "Land Trial" } },
                    LandId = 10,
                    Land = new LandResource { LandId = 10, LandCode = "PLOT-A1", SoilType = "Loam", AreaSize = 50.0m },
                    Status = AllocationDetailStatus.Reserved.ToString()
                });

            var service = new AllocationLandDetailService(_unitOfWorkMock.Object);

            // Act
            var result = await service.CreateAllocationLandDetailAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(301, result.AllocationLandDetailId);
            Assert.Equal("PLOT-A1", result.LandCode);
            _detailRepoMock.Verify(r => r.InsertAsync(It.IsAny<AllocationLandDetail>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // UT148-TC38
        // Abnormal
        [Fact]
        public async Task CreateAllocationLandDetailAsync_WhenLandDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var request = new AllocationLandDetailRequest
            {
                AllocationPlanId = 1,
                LandId = 999,
                ExpLandReqId = 5,
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 30),
                Status = AllocationDetailStatus.Reserved
            };

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new AllocationPlan { AllocationPlanId = 1, ExperimentId = 2, ApproveStatus = AllocationPlanStatus.Draft.ToString() });

            _landRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<LandResource, bool>>>(),
                    It.IsAny<Func<IQueryable<LandResource>, IOrderedQueryable<LandResource>>>(),
                    It.IsAny<Func<IQueryable<LandResource>, IIncludableQueryable<LandResource, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((LandResource?)null);

            var service = new AllocationLandDetailService(_unitOfWorkMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateAllocationLandDetailAsync(request));
            Assert.Equal("Land resource does not exist.", ex.Message);
        }

        // UT148-TC39
        // Abnormal
        [Fact]
        public async Task UpdateAllocationLandDetailAsync_WhenDetailDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationLandDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationLandDetail>, IOrderedQueryable<AllocationLandDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationLandDetail>, IIncludableQueryable<AllocationLandDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((AllocationLandDetail?)null);

            var request = new AllocationLandDetailRequest
            {
                AllocationPlanId = 1,
                LandId = 10,
                ExpLandReqId = 5,
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 30)
            };

            var service = new AllocationLandDetailService(_unitOfWorkMock.Object);

            // Act
            var result = await service.UpdateAllocationLandDetailAsync(999, request);

            // Assert
            Assert.Null(result);
        }
    }
}

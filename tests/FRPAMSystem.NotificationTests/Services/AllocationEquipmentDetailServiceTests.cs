using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.AllocationEquipmentDetail;
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
    public class AllocationEquipmentDetailServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IGenericRepository<AllocationEquipmentDetail>> _detailRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationPlan>> _planRepoMock = new();
        private readonly Mock<IGenericRepository<EquipmentType>> _typeRepoMock = new();
        private readonly Mock<IGenericRepository<EquipmentInstance>> _instanceRepoMock = new();
        private readonly Mock<IGenericRepository<Experiment>> _experimentRepoMock = new();
        private readonly Mock<IGenericRepository<ExperimentEquipmentRequirement>> _expReqRepoMock = new();
        private readonly Mock<IGenericRepository<HumanResourceProfile>> _hrRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationHumanDetail>> _humanDetailRepoMock = new();

        public AllocationEquipmentDetailServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationEquipmentDetail>()).Returns(_detailRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationPlan>()).Returns(_planRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<EquipmentType>()).Returns(_typeRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<EquipmentInstance>()).Returns(_instanceRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<Experiment>()).Returns(_experimentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<ExperimentEquipmentRequirement>()).Returns(_expReqRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<HumanResourceProfile>()).Returns(_hrRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationHumanDetail>()).Returns(_humanDetailRepoMock.Object);

            var experimentsList = new List<Experiment>
            {
                new Experiment { ExperimentId = 1, ResearcherId = 5 },
                new Experiment { ExperimentId = 2, ResearcherId = 5 },
                new Experiment { ExperimentId = 3, ResearcherId = 5 }
            };
            _experimentRepoMock.Setup(r => r.GetQueryable()).Returns(experimentsList.BuildMockQueryable());
            _detailRepoMock.Setup(r => r.GetQueryable()).Returns(new List<AllocationEquipmentDetail>().BuildMockQueryable());
            _humanDetailRepoMock.Setup(r => r.GetQueryable()).Returns(new List<AllocationHumanDetail>().BuildMockQueryable());
        }

        // UT148-TC29
        // Normal
        [Fact]
        public async Task CreateAllocationEquipmentDetailAsync_WithValidRequest_ShouldInsertDetail()
        {
            // Arrange
            var request = new AllocationEquipmentDetailRequest
            {
                AllocationPlanId = 1,
                ExpEquipmentReqId = 5,
                AllocatedEquipmentTypeId = 10,
                Quantity = 2,
                EfficiencyRate = 1.0,
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 10),
                Status = AllocationDetailStatus.Reserved
            };

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new AllocationPlan { AllocationPlanId = 1, ExperimentId = 2, ApproveStatus = AllocationPlanStatus.Draft.ToString() });

            _expReqRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentEquipmentRequirement, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentEquipmentRequirement>, IOrderedQueryable<ExperimentEquipmentRequirement>>>(),
                    It.IsAny<Func<IQueryable<ExperimentEquipmentRequirement>, IIncludableQueryable<ExperimentEquipmentRequirement, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new ExperimentEquipmentRequirement { ExpEquipmentReqId = 5, ExperimentId = 2, EquipmentTypeId = 10, Quantity = 5 });

            _typeRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentType, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentType>, IOrderedQueryable<EquipmentType>>>(),
                    It.IsAny<Func<IQueryable<EquipmentType>, IIncludableQueryable<EquipmentType, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new EquipmentType { EquipmentTypeId = 10, Name = "Tractor", TotalQuantity = 10, AvailableQuantity = 10, TrackingType = EquipmentTrackingType.QuantityBased.ToString() });

            _detailRepoMock.Setup(r => r.InsertAsync(It.IsAny<AllocationEquipmentDetail>()))
                .Callback<AllocationEquipmentDetail>(d => d.AllocationEquipmentDetailId = 101)
                .Returns(Task.CompletedTask);

            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IOrderedQueryable<AllocationEquipmentDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IIncludableQueryable<AllocationEquipmentDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new AllocationEquipmentDetail
                {
                    AllocationEquipmentDetailId = 101,
                    AllocationPlanId = 1,
                    AllocationPlan = new AllocationPlan { AllocationPlanId = 1, ExperimentId = 2, Experiment = new Experiment { ExperimentId = 2, ExperimentName = "Exp" } },
                    AllocatedEquipmentTypeId = 10,
                    AllocatedEquipmentType = new EquipmentType { EquipmentTypeId = 10, Name = "Tractor" },
                    Quantity = 2,
                    Status = AllocationDetailStatus.Reserved.ToString()
                });

            var service = new AllocationEquipmentDetailService(_unitOfWorkMock.Object);

            // Act
            var result = await service.CreateAllocationEquipmentDetailAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(101, result.AllocationEquipmentDetailId);
            Assert.Equal(2, result.Quantity);
            _detailRepoMock.Verify(r => r.InsertAsync(It.IsAny<AllocationEquipmentDetail>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // UT148-TC30
        // Abnormal
        [Fact]
        public async Task CreateAllocationEquipmentDetailAsync_WhenPlanDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var request = new AllocationEquipmentDetailRequest
            {
                AllocationPlanId = 999,
                ExpEquipmentReqId = 5,
                AllocatedEquipmentTypeId = 10,
                Quantity = 1,
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 5)
            };

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((AllocationPlan?)null);

            var service = new AllocationEquipmentDetailService(_unitOfWorkMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateAllocationEquipmentDetailAsync(request));
            Assert.Equal("Allocation plan does not exist.", ex.Message);
        }

        // UT148-TC31
        // Abnormal
        [Fact]
        public async Task UpdateAllocationEquipmentDetailAsync_WhenDetailDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IOrderedQueryable<AllocationEquipmentDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IIncludableQueryable<AllocationEquipmentDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((AllocationEquipmentDetail?)null);

            var request = new AllocationEquipmentDetailRequest
            {
                AllocationPlanId = 1,
                ExpEquipmentReqId = 5,
                AllocatedEquipmentTypeId = 10,
                Quantity = 1,
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 5)
            };

            var service = new AllocationEquipmentDetailService(_unitOfWorkMock.Object);

            // Act
            var result = await service.UpdateAllocationEquipmentDetailAsync(999, request);

            // Assert
            Assert.Null(result);
        }

        // UT148-TC32
        // Normal
        [Fact]
        public async Task HandoverMineAsync_WhenValid_ShouldTransitionStatusToInUse()
        {
            // Arrange
            int detailId = 50;
            int userId = 5;

            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new HumanResourceProfile { HumanResourceId = 500, UserId = userId });

            _detailRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>())).ReturnsAsync(true);

            var detail = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = detailId,
                AllocationPlanId = 10,
                AllocationPlan = new AllocationPlan { AllocationPlanId = 10, ExperimentId = 2, Experiment = new Experiment { ExperimentId = 2, ExperimentName = "Trial" } },
                AllocatedEquipmentTypeId = 10,
                AllocatedEquipmentType = new EquipmentType { EquipmentTypeId = 10, Name = "Sensor" },
                Status = AllocationDetailStatus.Reserved.ToString()
            };

            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IOrderedQueryable<AllocationEquipmentDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IIncludableQueryable<AllocationEquipmentDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(detail);

            var service = new AllocationEquipmentDetailService(_unitOfWorkMock.Object);

            // Act
            var result = await service.HandoverMineAsync(detailId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(AllocationDetailStatus.InUse.ToString(), detail.Status);
            _detailRepoMock.Verify(r => r.Update(detail), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // UT148-TC33
        // Normal
        [Fact]
        public async Task ReturnMineAsync_WhenValid_ShouldTransitionStatusToReturned()
        {
            // Arrange
            int detailId = 55;
            int userId = 5;

            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new HumanResourceProfile { HumanResourceId = 500, UserId = userId });

            _detailRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>())).ReturnsAsync(true);

            var detail = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = detailId,
                AllocationPlanId = 10,
                AllocationPlan = new AllocationPlan { AllocationPlanId = 10, ExperimentId = 2, Experiment = new Experiment { ExperimentId = 2, ExperimentName = "Trial" } },
                AllocatedEquipmentTypeId = 10,
                AllocatedEquipmentType = new EquipmentType { EquipmentTypeId = 10, Name = "Sensor" },
                Status = AllocationDetailStatus.InUse.ToString()
            };

            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IOrderedQueryable<AllocationEquipmentDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IIncludableQueryable<AllocationEquipmentDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(detail);

            var service = new AllocationEquipmentDetailService(_unitOfWorkMock.Object);

            // Act
            var result = await service.ReturnMineAsync(detailId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(AllocationDetailStatus.Completed.ToString(), detail.Status);
            _detailRepoMock.Verify(r => r.Update(detail), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}

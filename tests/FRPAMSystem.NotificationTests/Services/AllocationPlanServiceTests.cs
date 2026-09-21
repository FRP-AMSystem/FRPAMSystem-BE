using FRPAMSystem.BusinessTier.AI.Fitness;
using FRPAMSystem.BusinessTier.AI.Mappers;
using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.BusinessTier.DomainEvents;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.AllocationPlan;
using FRPAMSystem.BusinessTier.Services.Implements;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using FRPAMSystem.NotificationTests.Helpers;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace FRPAMSystem.NotificationTests.Services
{
    public class AllocationPlanServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock = new();
        private readonly Mock<IFitnessCalculator> _fitnessCalculatorMock = new();
        private readonly Mock<IAllocationPlanChromosomeMapper> _chromosomeMapperMock = new();
        private readonly Mock<IClock> _clockMock = new();

        private readonly Mock<IGenericRepository<AllocationPlan>> _planRepoMock = new();
        private readonly Mock<IGenericRepository<Experiment>> _experimentRepoMock = new();
        private readonly Mock<IGenericRepository<User>> _userRepoMock = new();
        private readonly Mock<IGenericRepository<EquipmentShortageLog>> _logRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationLandDetail>> _landDetailRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationHumanDetail>> _humanDetailRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationEquipmentDetail>> _equipmentDetailRepoMock = new();
        private readonly Mock<IGenericRepository<EquipmentInstance>> _equipmentInstanceRepoMock = new();
        private readonly Mock<IGenericRepository<EquipmentType>> _equipmentTypeRepoMock = new();
        private readonly Mock<IGenericRepository<EquipmentHandover>> _equipmentHandoverRepoMock = new();

        public AllocationPlanServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationPlan>()).Returns(_planRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<Experiment>()).Returns(_experimentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<EquipmentShortageLog>()).Returns(_logRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationLandDetail>()).Returns(_landDetailRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationHumanDetail>()).Returns(_humanDetailRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationEquipmentDetail>()).Returns(_equipmentDetailRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<EquipmentInstance>()).Returns(_equipmentInstanceRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<EquipmentType>()).Returns(_equipmentTypeRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<EquipmentHandover>()).Returns(_equipmentHandoverRepoMock.Object);

            _clockMock.Setup(c => c.Now).Returns(new DateTime(2026, 8, 21, 14, 0, 0));
        }

        // UT148-TC13
        // Normal
        [Fact]
        public async Task CreateAllocationPlanAsync_WithValidRequest_ShouldCreatePlanAndDispatchEvent()
        {
            // Arrange
            var request = new AllocationPlanRequest
            {
                ExperimentId = 5,
                ApproveStatus = AllocationPlanStatus.Draft
            };
            int creatorUserId = 10;

            _experimentRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Experiment, bool>>>())).ReturnsAsync(true);
            _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);

            AllocationPlan? insertedPlan = null;
            _planRepoMock.Setup(r => r.InsertAsync(It.IsAny<AllocationPlan>()))
                .Callback<AllocationPlan>(p =>
                {
                    p.AllocationPlanId = 100;
                    insertedPlan = p;
                })
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Expression<Func<AllocationPlan, bool>> pred, object? ord, object? inc, bool tracking) =>
                {
                    return new AllocationPlan
                    {
                        AllocationPlanId = 100,
                        ExperimentId = 5,
                        Experiment = new Experiment { ExperimentId = 5, ExperimentName = "Pine Trial" },
                        FitnessScore = 85.5,
                        CreatedBy = creatorUserId,
                        CreatedByNavigation = new User { UserId = creatorUserId, FullName = "User Ten" },
                        ApproveStatus = AllocationPlanStatus.Draft.ToString()
                    };
                });

            var service = new AllocationPlanService(
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object,
                _fitnessCalculatorMock.Object,
                _chromosomeMapperMock.Object,
                _clockMock.Object);

            // Act
            var result = await service.CreateAllocationPlanAsync(request, creatorUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.AllocationPlanId);
            Assert.Equal(85.5, result.FitnessScore);
            _planRepoMock.Verify(r => r.InsertAsync(It.IsAny<AllocationPlan>()), Times.Once);
            _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<AllocationPlanGeneratedEvent>(), CancellationToken.None), Times.Once);
        }

        // UT148-TC14
        // Abnormal
        [Fact]
        public async Task CreateAllocationPlanAsync_WhenExperimentDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var request = new AllocationPlanRequest
            {
                ExperimentId = 999,
                ApproveStatus = AllocationPlanStatus.Draft
            };

            _experimentRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Experiment, bool>>>())).ReturnsAsync(false);

            var service = new AllocationPlanService(
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object,
                _fitnessCalculatorMock.Object,
                _chromosomeMapperMock.Object,
                _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateAllocationPlanAsync(request, 10));
            Assert.Equal("Experiment does not exist.", ex.Message);
        }

        // UT148-TC15
        // Abnormal
        [Fact]
        public async Task UpdateAllocationPlanAsync_WhenPlanIsApproved_ShouldThrowException()
        {
            // Arrange
            int planId = 50;
            var request = new AllocationPlanRequest
            {
                ExperimentId = 5,
                ApproveStatus = AllocationPlanStatus.Pending
            };

            var approvedPlan = new AllocationPlan
            {
                AllocationPlanId = planId,
                ExperimentId = 5,
                ApproveStatus = AllocationPlanStatus.Approved.ToString()
            };

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(approvedPlan);

            var service = new AllocationPlanService(
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object,
                _fitnessCalculatorMock.Object,
                _chromosomeMapperMock.Object,
                _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateAllocationPlanAsync(planId, request));
            Assert.Equal("Approved allocation plan cannot be updated.", ex.Message);
        }

        // UT148-TC16
        // Normal
        [Fact]
        public async Task SubmitAllocationPlanAsync_WhenDraft_ShouldTransitionToPendingAndDispatchEvent()
        {
            // Arrange
            int planId = 60;
            var plan = new AllocationPlan
            {
                AllocationPlanId = planId,
                ExperimentId = 12,
                Experiment = new Experiment { ExperimentId = 12, ExperimentName = "Teak Project" },
                CreatedBy = 7,
                ApproveStatus = AllocationPlanStatus.Draft.ToString()
            };

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(plan);

            var service = new AllocationPlanService(
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object,
                _fitnessCalculatorMock.Object,
                _chromosomeMapperMock.Object,
                _clockMock.Object);

            // Act
            var result = await service.SubmitAllocationPlanAsync(planId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(AllocationPlanStatus.Pending.ToString(), plan.ApproveStatus);
            _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<AllocationPlanSubmittedEvent>(), CancellationToken.None), Times.Once);
        }

        // UT148-TC17
        // Abnormal
        [Fact]
        public async Task ApproveAllocationPlanAsync_WhenCurrentUserIdIsNull_ShouldThrowException()
        {
            // Arrange
            int planId = 70;
            var plan = new AllocationPlan
            {
                AllocationPlanId = planId,
                ApproveStatus = AllocationPlanStatus.Pending.ToString()
            };

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(plan);

            var service = new AllocationPlanService(
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object,
                _fitnessCalculatorMock.Object,
                _chromosomeMapperMock.Object,
                _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.ApproveAllocationPlanAsync(planId, currentUserId: null));
            Assert.Equal("Current user is required to approve allocation plan.", ex.Message);
        }

        // UT148-TC18
        // Normal
        [Fact]
        public async Task ApproveAllocationPlanAsync_WithValidApprover_ShouldApproveAndDispatchEvent()
        {
            // Arrange
            int planId = 75;
            int approverUserId = 2;

            var plan = new AllocationPlan
            {
                AllocationPlanId = planId,
                ExperimentId = 8,
                Experiment = new Experiment { ExperimentId = 8, ExperimentName = "Eucalyptus Study" },
                CreatedBy = 15,
                ApproveStatus = AllocationPlanStatus.Pending.ToString()
            };

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(plan);

            _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);

            var service = new AllocationPlanService(
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object,
                _fitnessCalculatorMock.Object,
                _chromosomeMapperMock.Object,
                _clockMock.Object);

            // Act
            var result = await service.ApproveAllocationPlanAsync(planId, approverUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(AllocationPlanStatus.Approved.ToString(), plan.ApproveStatus);
            Assert.Equal(approverUserId, plan.ApproveBy);
            Assert.Equal(new DateTime(2026, 8, 21, 14, 0, 0), plan.ApprovedAt);
            _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<AllocationPlanApprovedEvent>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task ApproveAllocationPlanAsync_WithProposedDetails_ShouldTransitionDetailsAndEquipmentInstances()
        {
            // Arrange
            int planId = 88;
            int approverUserId = 5;

            var landDetail = new AllocationLandDetail
            {
                AllocationLandDetailId = 1,
                AllocationPlanId = planId,
                Status = AllocationDetailStatus.Proposed.ToString()
            };

            var humanDetail = new AllocationHumanDetail
            {
                AllocationHumanDetailId = 2,
                AllocationPlanId = planId,
                Status = AllocationDetailStatus.Proposed.ToString()
            };

            var instance = new EquipmentInstance
            {
                EquipmentInstanceId = 101,
                Status = EquipmentInstanceStatus.Available.ToString()
            };

            var eqDetailWithInstance = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = 3,
                AllocationPlanId = planId,
                EquipmentInstanceId = 101,
                EquipmentInstance = instance,
                Status = AllocationDetailStatus.Proposed.ToString()
            };

            var individualType = new EquipmentType
            {
                EquipmentTypeId = 20,
                TrackingType = EquipmentTrackingType.Individual.ToString()
            };

            var eqDetailWithoutInstance = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = 4,
                AllocationPlanId = planId,
                EquipmentInstanceId = null,
                AllocatedEquipmentTypeId = 20,
                AllocatedEquipmentType = individualType,
                Status = AllocationDetailStatus.Proposed.ToString()
            };

            var quantityType = new EquipmentType
            {
                EquipmentTypeId = 30,
                TrackingType = EquipmentTrackingType.QuantityBased.ToString()
            };

            var eqDetailQuantityBased = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = 5,
                AllocationPlanId = planId,
                EquipmentInstanceId = null,
                AllocatedEquipmentTypeId = 30,
                AllocatedEquipmentType = quantityType,
                Status = AllocationDetailStatus.Proposed.ToString()
            };

            var plan = new AllocationPlan
            {
                AllocationPlanId = planId,
                ExperimentId = 9,
                Experiment = new Experiment { ExperimentId = 9, ExperimentName = "Acacia Trial" },
                CreatedBy = 12,
                ApproveStatus = AllocationPlanStatus.Pending.ToString(),
                AllocationLandDetails = new List<AllocationLandDetail> { landDetail },
                AllocationHumanDetails = new List<AllocationHumanDetail> { humanDetail },
                AllocationEquipmentDetails = new List<AllocationEquipmentDetail>
                {
                    eqDetailWithInstance,
                    eqDetailWithoutInstance,
                    eqDetailQuantityBased
                }
            };

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(plan);

            _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);

            var service = new AllocationPlanService(
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object,
                _fitnessCalculatorMock.Object,
                _chromosomeMapperMock.Object,
                _clockMock.Object);

            // Act
            var result = await service.ApproveAllocationPlanAsync(planId, approverUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(AllocationPlanStatus.Approved.ToString(), plan.ApproveStatus);
            Assert.Equal(AllocationDetailStatus.Allocated.ToString(), landDetail.Status);
            Assert.Equal(AllocationDetailStatus.Allocated.ToString(), humanDetail.Status);
            Assert.Equal(AllocationDetailStatus.Allocated.ToString(), eqDetailWithInstance.Status);
            Assert.Equal(EquipmentInstanceStatus.Reserved.ToString(), instance.Status);
            Assert.Equal(AllocationDetailStatus.Reserved.ToString(), eqDetailWithoutInstance.Status);
            Assert.Equal(AllocationDetailStatus.Allocated.ToString(), eqDetailQuantityBased.Status);

            _landDetailRepoMock.Verify(r => r.Update(landDetail), Times.Once);
            _humanDetailRepoMock.Verify(r => r.Update(humanDetail), Times.Once);
            _equipmentInstanceRepoMock.Verify(r => r.Update(instance), Times.Once);
            _equipmentDetailRepoMock.Verify(r => r.Update(It.IsAny<AllocationEquipmentDetail>()), Times.Exactly(3));
            _equipmentHandoverRepoMock.Verify(r => r.InsertAsync(It.Is<EquipmentHandover>(h => h.Status == EquipmentHandoverStatus.Pending.ToString())), Times.Exactly(2));
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // UT148-TC19
        // Abnormal
        [Fact]
        public async Task RejectAllocationPlanAsync_WhenAlreadyApproved_ShouldThrowException()
        {
            // Arrange
            int planId = 80;
            int rejectorUserId = 3;

            var plan = new AllocationPlan
            {
                AllocationPlanId = planId,
                ApproveStatus = AllocationPlanStatus.Approved.ToString()
            };

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(plan);

            var service = new AllocationPlanService(
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object,
                _fitnessCalculatorMock.Object,
                _chromosomeMapperMock.Object,
                _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.RejectAllocationPlanAsync(planId, rejectorUserId));
            Assert.Equal("Approved allocation plan cannot be rejected.", ex.Message);
        }

        // UT148-TC20
        // Normal
        [Fact]
        public async Task CancelAllocationPlanAsync_WhenNotApproved_ShouldSetStatusToRejected()
        {
            // Arrange
            int planId = 90;
            var plan = new AllocationPlan
            {
                AllocationPlanId = planId,
                ApproveStatus = AllocationPlanStatus.Pending.ToString()
            };

            _planRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationPlan, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IOrderedQueryable<AllocationPlan>>>(),
                    It.IsAny<Func<IQueryable<AllocationPlan>, IIncludableQueryable<AllocationPlan, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(plan);

            var service = new AllocationPlanService(
                _unitOfWorkMock.Object,
                _domainEventDispatcherMock.Object,
                _fitnessCalculatorMock.Object,
                _chromosomeMapperMock.Object,
                _clockMock.Object);

            // Act
            var result = await service.CancelAllocationPlanAsync(planId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(AllocationPlanStatus.Rejected.ToString(), plan.ApproveStatus);
            Assert.Null(plan.ApprovedAt);
        }
    }
}

using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.AllocationHumanDetail;
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
    public class AllocationHumanDetailServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IGenericRepository<AllocationHumanDetail>> _detailRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationPlan>> _planRepoMock = new();
        private readonly Mock<IGenericRepository<HumanResourceProfile>> _hrRepoMock = new();
        private readonly Mock<IGenericRepository<ExperimentHumanRequirement>> _expReqRepoMock = new();
        private readonly Mock<IGenericRepository<Schedule>> _scheduleRepoMock = new();

        public AllocationHumanDetailServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationHumanDetail>()).Returns(_detailRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationPlan>()).Returns(_planRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<HumanResourceProfile>()).Returns(_hrRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<ExperimentHumanRequirement>()).Returns(_expReqRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<Schedule>()).Returns(_scheduleRepoMock.Object);

            _detailRepoMock.Setup(r => r.GetQueryable()).Returns(new List<AllocationHumanDetail>().BuildMockQueryable());
            _scheduleRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Schedule, bool>>>())).ReturnsAsync(false);
        }

        // UT148-TC34
        // Normal
        [Fact]
        public async Task CreateAllocationHumanDetailAsync_WithValidRequest_ShouldInsertDetail()
        {
            // Arrange
            var request = new AllocationHumanDetailRequest
            {
                AllocationPlanId = 1,
                ExpHumanReqId = 5,
                HumanResourceId = 5,
                WorkingHours = 4,
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
                    It.IsAny<Expression<Func<ExperimentHumanRequirement, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentHumanRequirement>, IOrderedQueryable<ExperimentHumanRequirement>>>(),
                    It.IsAny<Func<IQueryable<ExperimentHumanRequirement>, IIncludableQueryable<ExperimentHumanRequirement, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new ExperimentHumanRequirement { ExpHumanReqId = 5, ExperimentId = 2, RoleId = 2, Quantity = 1 });

            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new HumanResourceProfile
                {
                    HumanResourceId = 5,
                    MaxWorkingHoursPerDay = 8,
                    Status = "Available",
                    HumanResourceSkills = new List<HumanResourceSkill>(),
                    User = new User { UserId = 50, RoleId = 2, FullName = "John Worker" }
                });

            _detailRepoMock.Setup(r => r.InsertAsync(It.IsAny<AllocationHumanDetail>()))
                .Callback<AllocationHumanDetail>(d => d.AllocationHumanDetailId = 201)
                .Returns(Task.CompletedTask);

            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationHumanDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationHumanDetail>, IOrderedQueryable<AllocationHumanDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationHumanDetail>, IIncludableQueryable<AllocationHumanDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new AllocationHumanDetail
                {
                    AllocationHumanDetailId = 201,
                    AllocationPlanId = 1,
                    AllocationPlan = new AllocationPlan { AllocationPlanId = 1, ExperimentId = 2, Experiment = new Experiment { ExperimentId = 2, ExperimentName = "Forest Study" } },
                    HumanResourceId = 5,
                    HumanResource = new HumanResourceProfile { HumanResourceId = 5, User = new User { FullName = "John Worker" } },
                    WorkingHours = 4,
                    Status = AllocationDetailStatus.Reserved.ToString()
                });

            var service = new AllocationHumanDetailService(_unitOfWorkMock.Object);

            // Act
            var result = await service.CreateAllocationHumanDetailAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.AllocationHumanDetailId);
            Assert.Equal(4, result.WorkingHours);
            _detailRepoMock.Verify(r => r.InsertAsync(It.IsAny<AllocationHumanDetail>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // UT148-TC35
        // Abnormal
        [Fact]
        public async Task CreateAllocationHumanDetailAsync_WhenHoursExceedMaxWorkingHours_ShouldThrowException()
        {
            // Arrange
            var request = new AllocationHumanDetailRequest
            {
                AllocationPlanId = 1,
                ExpHumanReqId = 5,
                HumanResourceId = 5,
                WorkingHours = 10, // Exceeds MaxWorkingHoursPerDay = 8
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
                    It.IsAny<Expression<Func<ExperimentHumanRequirement, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentHumanRequirement>, IOrderedQueryable<ExperimentHumanRequirement>>>(),
                    It.IsAny<Func<IQueryable<ExperimentHumanRequirement>, IIncludableQueryable<ExperimentHumanRequirement, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new ExperimentHumanRequirement { ExpHumanReqId = 5, ExperimentId = 2, RoleId = 2, Quantity = 1 });

            _hrRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<HumanResourceProfile, bool>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IOrderedQueryable<HumanResourceProfile>>>(),
                    It.IsAny<Func<IQueryable<HumanResourceProfile>, IIncludableQueryable<HumanResourceProfile, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new HumanResourceProfile
                {
                    HumanResourceId = 5,
                    MaxWorkingHoursPerDay = 8,
                    Status = "Available",
                    HumanResourceSkills = new List<HumanResourceSkill>(),
                    User = new User { UserId = 50, RoleId = 2, FullName = "John Worker" }
                });

            var service = new AllocationHumanDetailService(_unitOfWorkMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateAllocationHumanDetailAsync(request));
            Assert.Contains("exceed", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // UT148-TC36
        // Abnormal
        [Fact]
        public async Task UpdateAllocationHumanDetailAsync_WhenDetailDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationHumanDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationHumanDetail>, IOrderedQueryable<AllocationHumanDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationHumanDetail>, IIncludableQueryable<AllocationHumanDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((AllocationHumanDetail?)null);

            var request = new AllocationHumanDetailRequest
            {
                AllocationPlanId = 1,
                ExpHumanReqId = 5,
                HumanResourceId = 5,
                WorkingHours = 4,
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 10)
            };

            var service = new AllocationHumanDetailService(_unitOfWorkMock.Object);

            // Act
            var result = await service.UpdateAllocationHumanDetailAsync(999, request);

            // Assert
            Assert.Null(result);
        }
    }
}

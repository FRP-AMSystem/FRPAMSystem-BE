using FRPAMSystem.BusinessTier.DomainEvents;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.Schedule;
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
    public class ScheduleServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock = new();
        private readonly Mock<IClock> _clockMock = new();

        private readonly Mock<IGenericRepository<Schedule>> _scheduleRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationPlan>> _planRepoMock = new();
        private readonly Mock<IGenericRepository<ExperimentPhase>> _phaseRepoMock = new();
        private readonly Mock<IGenericRepository<User>> _userRepoMock = new();
        private readonly Mock<IGenericRepository<HumanResourceProfile>> _hrRepoMock = new();

        public ScheduleServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<Schedule>()).Returns(_scheduleRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationPlan>()).Returns(_planRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<ExperimentPhase>()).Returns(_phaseRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<HumanResourceProfile>()).Returns(_hrRepoMock.Object);

            _clockMock.Setup(c => c.Now).Returns(new DateTime(2026, 8, 21, 15, 0, 0));
        }

        // UT148-TC21
        // Normal
        [Fact]
        public async Task CreateScheduleAsync_WithAssignedHumanResource_ShouldCreateScheduleAndDispatchEvent()
        {
            // Arrange
            var request = new ScheduleRequest
            {
                AllocationPlanId = 10,
                Title = "Field Planting Phase 1",
                Description = "Initial sapling planting",
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 15),
                Status = ScheduleStatus.Planned,
                Priority = 2,
                AssignedHumanResourceId = 5,
                CreatedBy = 1
            };

            _planRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationPlan, bool>>>())).ReturnsAsync(true);
            _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);
            _hrRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<HumanResourceProfile, bool>>>())).ReturnsAsync(true);

            _scheduleRepoMock.Setup(r => r.InsertAsync(It.IsAny<Schedule>()))
                .Callback<Schedule>(s => s.ScheduleId = 101)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);

            _scheduleRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Schedule, bool>>>(),
                    It.IsAny<Func<IQueryable<Schedule>, IOrderedQueryable<Schedule>>>(),
                    It.IsAny<Func<IQueryable<Schedule>, IIncludableQueryable<Schedule, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Expression<Func<Schedule, bool>> pred, object? ord, object? inc, bool tracking) =>
                {
                    return new Schedule
                    {
                        ScheduleId = 101,
                        AllocationPlanId = 10,
                        AllocationPlan = new AllocationPlan
                        {
                            AllocationPlanId = 10,
                            ExperimentId = 3,
                            Experiment = new Experiment { ExperimentId = 3, ExperimentName = "Pine Trial" }
                        },
                        Title = "Field Planting Phase 1",
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        Status = ScheduleStatus.Planned.ToString(),
                        Priority = 2,
                        AssignedHumanResourceId = 5
                    };
                });

            var service = new ScheduleService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.CreateScheduleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(101, result.ScheduleId);
            Assert.Equal("Field Planting Phase 1", result.Title);
            _scheduleRepoMock.Verify(r => r.InsertAsync(It.IsAny<Schedule>()), Times.Once);
            _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<ScheduleAssignedEvent>(), CancellationToken.None), Times.Once);
        }

        // UT148-TC22
        // Normal
        [Fact]
        public async Task CreateScheduleAsync_WithoutAssignedHumanResource_ShouldNotDispatchEvent()
        {
            // Arrange
            var request = new ScheduleRequest
            {
                AllocationPlanId = 10,
                Title = "Soil Testing",
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 5),
                Priority = 1,
                AssignedHumanResourceId = null
            };

            _planRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationPlan, bool>>>())).ReturnsAsync(true);

            _scheduleRepoMock.Setup(r => r.InsertAsync(It.IsAny<Schedule>()))
                .Callback<Schedule>(s => s.ScheduleId = 102)
                .Returns(Task.CompletedTask);

            _scheduleRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Schedule, bool>>>(),
                    It.IsAny<Func<IQueryable<Schedule>, IOrderedQueryable<Schedule>>>(),
                    It.IsAny<Func<IQueryable<Schedule>, IIncludableQueryable<Schedule, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new Schedule
                {
                    ScheduleId = 102,
                    AllocationPlanId = 10,
                    AllocationPlan = new AllocationPlan
                    {
                        AllocationPlanId = 10,
                        ExperimentId = 3,
                        Experiment = new Experiment { ExperimentId = 3, ExperimentName = "Pine Trial" }
                    },
                    Title = "Soil Testing",
                    Priority = 1,
                    AssignedHumanResourceId = null
                });

            var service = new ScheduleService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.CreateScheduleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.AssignedHumanResourceId);
            _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<ScheduleAssignedEvent>(), CancellationToken.None), Times.Never);
        }

        // UT148-TC23
        // Abnormal
        [Fact]
        public async Task CreateScheduleAsync_WhenTitleIsEmpty_ShouldThrowException()
        {
            // Arrange
            var request = new ScheduleRequest
            {
                AllocationPlanId = 10,
                Title = "   "
            };

            var service = new ScheduleService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateScheduleAsync(request));
            Assert.Equal("Schedule title is required.", ex.Message);
        }

        // UT148-TC24
        // Abnormal
        [Fact]
        public async Task CreateScheduleAsync_WhenEndDateBeforeStartDate_ShouldThrowException()
        {
            // Arrange
            var request = new ScheduleRequest
            {
                AllocationPlanId = 10,
                Title = "Testing",
                StartDate = new DateTime(2026, 9, 10),
                EndDate = new DateTime(2026, 9, 1)
            };

            var service = new ScheduleService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateScheduleAsync(request));
            Assert.Equal("End date must be greater than or equal to start date.", ex.Message);
        }

        // UT148-TC25
        // Abnormal
        [Fact]
        public async Task CreateScheduleAsync_WhenAllocationPlanDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var request = new ScheduleRequest
            {
                AllocationPlanId = 999,
                Title = "Watering Schedule",
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 5),
                Priority = 2
            };

            _planRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationPlan, bool>>>())).ReturnsAsync(false);

            var service = new ScheduleService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateScheduleAsync(request));
            Assert.Equal("Allocation plan does not exist.", ex.Message);
        }

        // UT148-TC26
        // Normal
        [Fact]
        public async Task UpdateScheduleAsync_WhenAssignmentChanged_ShouldDispatchEventForNewAssignment()
        {
            // Arrange
            int scheduleId = 200;
            var existingSchedule = new Schedule
            {
                ScheduleId = scheduleId,
                AllocationPlanId = 10,
                AllocationPlan = new AllocationPlan
                {
                    AllocationPlanId = 10,
                    ExperimentId = 3,
                    Experiment = new Experiment { ExperimentId = 3, ExperimentName = "Pine Trial" }
                },
                Title = "Pruning Task",
                AssignedHumanResourceId = 5 // Previous HR
            };

            var request = new ScheduleRequest
            {
                AllocationPlanId = 10,
                Title = "Pruning Task Updated",
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 5),
                Priority = 3,
                AssignedHumanResourceId = 12 // New HR assignment
            };

            _planRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationPlan, bool>>>())).ReturnsAsync(true);
            _hrRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<HumanResourceProfile, bool>>>())).ReturnsAsync(true);

            _scheduleRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Schedule, bool>>>(),
                    It.IsAny<Func<IQueryable<Schedule>, IOrderedQueryable<Schedule>>>(),
                    It.IsAny<Func<IQueryable<Schedule>, IIncludableQueryable<Schedule, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(existingSchedule);

            var service = new ScheduleService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.UpdateScheduleAsync(scheduleId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(12, existingSchedule.AssignedHumanResourceId);
            _scheduleRepoMock.Verify(r => r.Update(existingSchedule), Times.Once);
            _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.Is<ScheduleAssignedEvent>(e => e.AssignedHumanResourceId == 12), CancellationToken.None), Times.Once);
        }

        // UT148-TC27
        // Normal
        [Fact]
        public async Task DeleteScheduleAsync_WithValidId_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            int scheduleId = 300;
            var schedule = new Schedule
            {
                ScheduleId = scheduleId,
                Title = "To Be Deleted"
            };

            _scheduleRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Schedule, bool>>>(),
                    It.IsAny<Func<IQueryable<Schedule>, IOrderedQueryable<Schedule>>>(),
                    It.IsAny<Func<IQueryable<Schedule>, IIncludableQueryable<Schedule, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(schedule);

            var service = new ScheduleService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.DeleteScheduleAsync(scheduleId);

            // Assert
            Assert.True(result);
            _scheduleRepoMock.Verify(r => r.Delete(schedule), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // UT148-TC28
        // Abnormal
        [Fact]
        public async Task DeleteScheduleAsync_WhenDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            _scheduleRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Schedule, bool>>>(),
                    It.IsAny<Func<IQueryable<Schedule>, IOrderedQueryable<Schedule>>>(),
                    It.IsAny<Func<IQueryable<Schedule>, IIncludableQueryable<Schedule, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Schedule?)null);

            var service = new ScheduleService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.DeleteScheduleAsync(999);

            // Assert
            Assert.False(result);
        }
    }
}

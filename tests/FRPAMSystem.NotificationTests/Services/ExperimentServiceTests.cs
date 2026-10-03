using FRPAMSystem.BusinessTier.DomainEvents;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.Experiment;
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
    public class ExperimentServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IDomainEventDispatcher> _domainEventDispatcherMock = new();
        private readonly Mock<IClock> _clockMock = new();
        private readonly Mock<IGenericRepository<Experiment>> _experimentRepoMock = new();
        private readonly Mock<IGenericRepository<User>> _userRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationEquipmentDetail>> _detailRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationPlan>> _planRepoMock = new();
        private readonly Mock<IGenericRepository<ExperimentPhase>> _phaseRepoMock = new();

        public ExperimentServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<Experiment>()).Returns(_experimentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(_userRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationEquipmentDetail>()).Returns(_detailRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationPlan>()).Returns(_planRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<ExperimentPhase>()).Returns(_phaseRepoMock.Object);
            _clockMock.Setup(c => c.Now).Returns(new DateTime(2026, 8, 21, 12, 0, 0));
        }

        // UT148-TC01
        // Normal
        [Fact]
        public async Task CreateExperimentAsync_WithValidRequest_ShouldCreateExperimentAndDispatchEvent()
        {
            // Arrange
            var request = new ExperimentRequest
            {
                ExperimentName = "Pine Forest Research",
                Description = "Study on pine growth rates",
                ResearcherId = 10,
                ExpectStartDate = new DateTime(2026, 9, 1),
                ExpectEndDate = new DateTime(2026, 12, 31),
                Deadline = new DateTime(2027, 1, 15),
                Priority = 2,
                Status = ExperimentStatus.Draft
            };

            _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(true);

            _experimentRepoMock.Setup(r => r.GetQueryable()).Returns(new List<Experiment>().BuildMockQueryable());

            _experimentRepoMock.Setup(r => r.InsertAsync(It.IsAny<Experiment>()))
                .Callback<Experiment>(e => e.ExperimentId = 101)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Expression<Func<Experiment, bool>> pred, object? ord, object? inc, bool tracking) =>
                {
                    return new Experiment
                    {
                        ExperimentId = 101,
                        ExperimentName = "Pine Forest Research",
                        Description = "Study on pine growth rates",
                        ResearcherId = 10,
                        Researcher = new User { UserId = 10, FullName = "Dr. Smith" },
                        ExpectStartDate = request.ExpectStartDate,
                        ExpectEndDate = request.ExpectEndDate,
                        Deadline = request.Deadline,
                        Priority = request.Priority,
                        Status = ExperimentStatus.Draft.ToString()
                    };
                });

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.CreateExperimentAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(101, result.ExperimentId);
            Assert.Equal("Pine Forest Research", result.ExperimentName);
            Assert.Equal("Dr. Smith", result.ResearcherName);
            _experimentRepoMock.Verify(r => r.InsertAsync(It.IsAny<Experiment>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
            _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<ExperimentCreatedEvent>(), CancellationToken.None), Times.Once);
        }

        // UT148-TC02
        // Abnormal
        [Fact]
        public async Task CreateExperimentAsync_WhenNameIsEmpty_ShouldThrowException()
        {
            // Arrange
            var request = new ExperimentRequest
            {
                ExperimentName = "",
                ResearcherId = 10
            };

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateExperimentAsync(request));
            Assert.Equal("Experiment name is required.", ex.Message);
        }

        // UT148-TC03
        // Abnormal
        [Fact]
        public async Task CreateExperimentAsync_WhenEndDateBeforeStartDate_ShouldThrowException()
        {
            // Arrange
            var request = new ExperimentRequest
            {
                ExperimentName = "Soil Study",
                ResearcherId = 10,
                ExpectStartDate = new DateTime(2026, 10, 1),
                ExpectEndDate = new DateTime(2026, 9, 1),
                Priority = 1
            };

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateExperimentAsync(request));
            Assert.Equal("Expect end date must be greater than or equal to expect start date.", ex.Message);
        }

        // UT148-TC04
        // Abnormal
        [Fact]
        public async Task CreateExperimentAsync_WhenPriorityInvalid_ShouldThrowException()
        {
            // Arrange
            var request = new ExperimentRequest
            {
                ExperimentName = "Soil Study",
                ResearcherId = 10,
                ExpectStartDate = new DateTime(2026, 9, 1),
                ExpectEndDate = new DateTime(2026, 10, 1),
                Priority = 5 // Invalid (>4)
            };

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateExperimentAsync(request));
            Assert.Equal("Priority must be between 1 and 4.", ex.Message);
        }

        // UT148-TC05
        // Boundary
        [Fact]
        public async Task CreateExperimentAsync_WhenDeadlineBeforeExpectEndDate_ShouldThrowException()
        {
            // Arrange
            var request = new ExperimentRequest
            {
                ExperimentName = "Soil Study",
                ResearcherId = 10,
                ExpectStartDate = new DateTime(2026, 9, 1),
                ExpectEndDate = new DateTime(2026, 10, 1),
                Deadline = new DateTime(2026, 9, 25), // Before ExpectEndDate
                Priority = 2
            };

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateExperimentAsync(request));
            Assert.Equal("Deadline must be greater than or equal to expect end date.", ex.Message);
        }

        // UT148-TC06
        // Abnormal
        [Fact]
        public async Task CreateExperimentAsync_WhenResearcherDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var request = new ExperimentRequest
            {
                ExperimentName = "Soil Study",
                ResearcherId = 999, // Non-existent
                ExpectStartDate = new DateTime(2026, 9, 1),
                ExpectEndDate = new DateTime(2026, 10, 1),
                Priority = 2
            };

            _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(false);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateExperimentAsync(request));
            Assert.Equal("Researcher does not exist.", ex.Message);
        }

        // UT148-TC07
        // Normal
        [Fact]
        public async Task UpdateExperimentAsync_WithValidRequest_ShouldUpdateAndReturnResponse()
        {
            // Arrange
            int experimentId = 10;
            var request = new ExperimentRequest
            {
                ExperimentName = "Updated Name",
                Description = "Updated desc",
                ResearcherId = 1,
                ExpectStartDate = new DateTime(2026, 9, 1),
                ExpectEndDate = new DateTime(2026, 10, 1),
                Priority = 3,
                Status = ExperimentStatus.Draft
            };

            _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);
            _experimentRepoMock.Setup(r => r.GetQueryable()).Returns(new List<Experiment>().BuildMockQueryable());

            var existingExperiment = new Experiment
            {
                ExperimentId = experimentId,
                ExperimentName = "Old Name",
                ResearcherId = 1,
                Researcher = new User { UserId = 1, FullName = "Dr. Alice" },
                Priority = 1
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(existingExperiment);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.UpdateExperimentAsync(experimentId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.ExperimentName);
            _experimentRepoMock.Verify(r => r.Update(existingExperiment), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // UT148-TC08
        // Abnormal
        [Fact]
        public async Task UpdateExperimentAsync_WhenExperimentDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>())).ReturnsAsync(true);
            _experimentRepoMock.Setup(r => r.GetQueryable()).Returns(new List<Experiment>().BuildMockQueryable());

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((Experiment?)null);

            var request = new ExperimentRequest
            {
                ExperimentName = "Test",
                ResearcherId = 1,
                ExpectStartDate = new DateTime(2026, 9, 1),
                ExpectEndDate = new DateTime(2026, 10, 1),
                Priority = 1
            };

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.UpdateExperimentAsync(999, request);

            // Assert
            Assert.Null(result);
        }

        // UT148-TC09
        // Normal
        [Fact]
        public async Task SubmitExperimentAsync_WithValidId_ShouldUpdateStatusAndDispatchEvent()
        {
            // Arrange
            int experimentId = 25;
            var experiment = new Experiment
            {
                ExperimentId = experimentId,
                ExperimentName = "Eucalyptus Growth",
                ResearcherId = 5,
                Researcher = new User { UserId = 5, FullName = "Researcher B" },
                Status = ExperimentStatus.Draft.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.SubmitExperimentAsync(experimentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ExperimentStatus.Submitted.ToString(), experiment.Status);
            Assert.Equal(ExperimentStatus.Submitted.ToString(), result.Status);
            _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<ExperimentSubmittedEvent>(), CancellationToken.None), Times.Once);
        }

        // UT148-TC10
        // Normal
        [Fact]
        public async Task ApproveExperimentAsync_WithValidId_ShouldTransitionStatusToPlanningAndDispatchEvent()
        {
            // Arrange
            int experimentId = 30;
            int managerUserId = 2;

            var experiment = new Experiment
            {
                ExperimentId = experimentId,
                ExperimentName = "Teak Planting",
                ResearcherId = 8,
                Researcher = new User { UserId = 8, FullName = "Researcher C" },
                Status = ExperimentStatus.Submitted.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.ApproveExperimentAsync(experimentId, managerUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ExperimentStatus.Planning.ToString(), experiment.Status);
            Assert.Equal(ExperimentStatus.Planning.ToString(), result.Status);
            _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<ExperimentApprovedEvent>(), CancellationToken.None), Times.Once);
        }

        // UT148-TC11
        // Normal
        [Fact]
        public async Task RejectExperimentAsync_WithReason_ShouldTransitionStatusToDraftAndDispatchEvent()
        {
            // Arrange
            int experimentId = 40;
            int managerUserId = 2;
            string rejectReason = "Incomplete resource requirements";

            var experiment = new Experiment
            {
                ExperimentId = experimentId,
                ExperimentName = "Acacia Field Trial",
                ResearcherId = 12,
                Researcher = new User { UserId = 12, FullName = "Researcher D" },
                Status = ExperimentStatus.Submitted.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.RejectExperimentAsync(experimentId, managerUserId, rejectReason);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ExperimentStatus.Draft.ToString(), experiment.Status);
            Assert.Equal(ExperimentStatus.Draft.ToString(), result.Status);
            _domainEventDispatcherMock.Verify(d => d.DispatchAsync(It.Is<ExperimentRejectedEvent>(e => e.Reason == rejectReason), CancellationToken.None), Times.Once);
        }

        // UT148-TC12
        // Normal
        [Fact]
        public async Task UpdateExperimentStatusAsync_WithValidRequest_ShouldUpdateStatusAndUpdatedAt()
        {
            // Arrange
            int experimentId = 50;
            var request = new UpdateExperimentStatusRequest
            {
                Status = "Ready"
            };

            var experiment = new Experiment
            {
                ExperimentId = experimentId,
                ExperimentName = "Soil Moisture Monitoring",
                ResearcherId = 15,
                Researcher = new User { UserId = 15, FullName = "Researcher E" },
                Status = "Planning"
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.UpdateExperimentStatusAsync(experimentId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ready", experiment.Status);
            Assert.Equal("Ready", result.Status);
            Assert.Equal(new DateTime(2026, 8, 21, 12, 0, 0), experiment.UpdatedAt);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task SubmitExperimentAsync_WhenNotDraft_ShouldThrowException()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 1,
                Status = ExperimentStatus.Planning.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.SubmitExperimentAsync(1));
            Assert.Equal("Only draft experiments can be submitted.", ex.Message);
        }

        [Fact]
        public async Task ApproveExperimentAsync_WhenNotSubmitted_ShouldThrowException()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 1,
                Status = ExperimentStatus.Draft.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.ApproveExperimentAsync(1, 2));
            Assert.Equal("Only submitted experiments can be approved.", ex.Message);
        }

        [Fact]
        public async Task StartExperimentAsync_WhenReady_ShouldTransitionToRunning()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 10,
                Status = ExperimentStatus.Ready.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.StartExperimentAsync(10, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ExperimentStatus.Running.ToString(), experiment.Status);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task StartExperimentAsync_WhenNotReady_ShouldThrowException()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 10,
                Status = ExperimentStatus.Planning.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.StartExperimentAsync(10, 2));
            Assert.Equal("Only ready experiments can be started.", ex.Message);
        }

        [Fact]
        public async Task CompleteExperimentAsync_WhenEquipmentStillInUse_ShouldThrowException()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 20,
                Status = ExperimentStatus.Running.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            _detailRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>()))
                .ReturnsAsync(true);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CompleteExperimentAsync(20, 2));
            Assert.Contains("equipment is still in use", ex.Message);
        }

        [Fact]
        public async Task CompleteExperimentAsync_WhenNoEquipmentInUse_ShouldTransitionToCompleted()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 20,
                Status = ExperimentStatus.Running.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            _detailRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>()))
                .ReturnsAsync(false);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act
            var result = await service.CompleteExperimentAsync(20, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ExperimentStatus.Completed.ToString(), experiment.Status);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelExperimentAsync_WhenAlreadyCompleted_ShouldThrowException()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 30,
                Status = ExperimentStatus.Completed.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CancelExperimentAsync(30, 2));
            Assert.Equal("Completed or cancelled experiments cannot be cancelled.", ex.Message);
        }

        [Fact]
        public async Task UpdateExperimentAsync_WhenNotDraft_ShouldThrowException()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 40,
                Status = ExperimentStatus.Running.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(true);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateExperimentAsync(40, new ExperimentRequest
            {
                ExperimentName = "Updated Name",
                ResearcherId = 1,
                ExpectStartDate = DateTime.UtcNow,
                ExpectEndDate = DateTime.UtcNow.AddDays(10)
            }));

            Assert.Equal("Only draft experiments can be edited.", ex.Message);
        }

        [Fact]
        public async Task DeleteExperimentAsync_WhenHasAllocationPlan_ShouldThrowException()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 45,
                Status = ExperimentStatus.Draft.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            _planRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationPlan, bool>>>()))
                .ReturnsAsync(true);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteExperimentAsync(45));
            Assert.Equal("Cannot delete an experiment that has associated allocation plans.", ex.Message);
        }

        [Fact]
        public async Task UpdateExperimentStatusAsync_WithInvalidTransition_ShouldThrowException()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 55,
                Status = ExperimentStatus.Draft.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert: jumping directly from Draft to Completed is forbidden
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.UpdateExperimentStatusAsync(55, new UpdateExperimentStatusRequest
                {
                    Status = "Completed"
                }));

            Assert.Contains("Invalid status transition", ex.Message);
        }

        [Fact]
        public async Task CompleteExperimentAsync_WhenPhasesStillInProgress_ShouldThrowException()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 60,
                Status = ExperimentStatus.Running.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            _detailRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>()))
                .ReturnsAsync(false);

            _phaseRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ExperimentPhase, bool>>>()))
                .ReturnsAsync(true); // Still has in-progress phases

            var service = new ExperimentService(_unitOfWorkMock.Object, _domainEventDispatcherMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CompleteExperimentAsync(60, 1));
            Assert.Equal("Cannot complete experiment while phases are still planned or in progress.", ex.Message);
        }
    }
}

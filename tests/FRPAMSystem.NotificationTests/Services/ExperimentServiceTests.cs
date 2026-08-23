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

        public ExperimentServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<Experiment>()).Returns(_experimentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(_userRepoMock.Object);
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
                Status = "InProgress"
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
            Assert.Equal("InProgress", experiment.Status);
            Assert.Equal("InProgress", result.Status);
            Assert.Equal(new DateTime(2026, 8, 21, 12, 0, 0), experiment.UpdatedAt);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}

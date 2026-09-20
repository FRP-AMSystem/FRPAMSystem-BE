using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.ExperimentPhase;
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
    public class ExperimentPhaseServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IClock> _clockMock = new();
        private readonly Mock<IGenericRepository<ExperimentPhase>> _phaseRepoMock = new();
        private readonly Mock<IGenericRepository<Experiment>> _experimentRepoMock = new();
        private readonly Mock<IGenericRepository<PhaseEquipmentRequirement>> _equipmentReqRepoMock = new();
        private readonly Mock<IGenericRepository<PhaseHumanRequirement>> _humanReqRepoMock = new();

        public ExperimentPhaseServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<ExperimentPhase>()).Returns(_phaseRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<Experiment>()).Returns(_experimentRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<PhaseEquipmentRequirement>()).Returns(_equipmentReqRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<PhaseHumanRequirement>()).Returns(_humanReqRepoMock.Object);
            _clockMock.Setup(c => c.Now).Returns(new DateTime(2026, 8, 21, 12, 0, 0));
        }

        [Fact]
        public async Task CreateExperimentPhaseAsync_WhenExperimentIsDraft_ShouldCreatePhaseInPlannedStatus()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 1,
                ExperimentName = "Test Experiment",
                Status = ExperimentStatus.Draft.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            _phaseRepoMock.Setup(r => r.GetQueryable())
                .Returns(new List<ExperimentPhase>().BuildMockQueryable());

            ExperimentPhase? capturedPhase = null;
            _phaseRepoMock.Setup(r => r.InsertAsync(It.IsAny<ExperimentPhase>()))
                .Callback<ExperimentPhase>(p =>
                {
                    p.PhaseId = 10;
                    capturedPhase = p;
                })
                .Returns(Task.CompletedTask);

            _phaseRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentPhase, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IOrderedQueryable<ExperimentPhase>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IIncludableQueryable<ExperimentPhase, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(() => capturedPhase != null ? new ExperimentPhase
                {
                    PhaseId = capturedPhase.PhaseId,
                    ExperimentId = capturedPhase.ExperimentId,
                    PhaseName = capturedPhase.PhaseName,
                    PhaseOrder = capturedPhase.PhaseOrder,
                    ExpectedStartDate = capturedPhase.ExpectedStartDate,
                    ExpectedEndDate = capturedPhase.ExpectedEndDate,
                    Status = capturedPhase.Status,
                    Experiment = experiment
                } : null);

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act
            var result = await service.CreateExperimentPhaseAsync(new ExperimentPhaseRequest
            {
                ExperimentId = 1,
                PhaseName = "Phase 1 - Planting",
                PhaseOrder = 1,
                ExpectedStartDate = new DateTime(2026, 9, 1),
                ExpectedEndDate = new DateTime(2026, 9, 10),
                Status = ExperimentPhaseStatus.Completed // Client attempts to force Completed
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ExperimentPhaseStatus.Planned.ToString(), result.Status); // Enforced Planned
            Assert.Equal("Phase 1 - Planting", result.PhaseName);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateExperimentPhaseAsync_WhenExperimentNotDraft_ShouldThrowException()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 1,
                Status = ExperimentStatus.Running.ToString()
            };

            _experimentRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Experiment, bool>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IOrderedQueryable<Experiment>>>(),
                    It.IsAny<Func<IQueryable<Experiment>, IIncludableQueryable<Experiment, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(experiment);

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateExperimentPhaseAsync(new ExperimentPhaseRequest
            {
                ExperimentId = 1,
                PhaseName = "Phase 1",
                PhaseOrder = 1,
                ExpectedStartDate = DateTime.UtcNow,
                ExpectedEndDate = DateTime.UtcNow.AddDays(5)
            }));

            Assert.Equal("Phases can only be configured when the experiment is in draft status.", ex.Message);
        }

        [Fact]
        public async Task UpdateExperimentPhaseAsync_WhenExperimentNotDraft_ShouldThrowException()
        {
            // Arrange
            var phase = new ExperimentPhase
            {
                PhaseId = 1,
                ExperimentId = 2,
                Status = ExperimentPhaseStatus.Planned.ToString(),
                Experiment = new Experiment
                {
                    ExperimentId = 2,
                    Status = ExperimentStatus.Running.ToString()
                }
            };

            _phaseRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentPhase, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IOrderedQueryable<ExperimentPhase>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IIncludableQueryable<ExperimentPhase, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(phase);

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateExperimentPhaseAsync(1, new ExperimentPhaseRequest
            {
                ExperimentId = 2,
                PhaseName = "Updated Phase",
                PhaseOrder = 1,
                ExpectedStartDate = DateTime.UtcNow,
                ExpectedEndDate = DateTime.UtcNow.AddDays(5)
            }));

            Assert.Equal("Phases can only be edited when the experiment is in draft status.", ex.Message);
        }

        [Fact]
        public async Task DeleteExperimentPhaseAsync_WhenPhaseNotPlanned_ShouldThrowException()
        {
            // Arrange
            var phase = new ExperimentPhase
            {
                PhaseId = 1,
                ExperimentId = 2,
                Status = ExperimentPhaseStatus.InProgress.ToString(),
                Experiment = new Experiment
                {
                    ExperimentId = 2,
                    Status = ExperimentStatus.Draft.ToString()
                }
            };

            _phaseRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentPhase, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IOrderedQueryable<ExperimentPhase>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IIncludableQueryable<ExperimentPhase, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(phase);

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteExperimentPhaseAsync(1));
            Assert.Equal("Only planned phases can be deleted.", ex.Message);
        }

        [Fact]
        public async Task DeleteExperimentPhaseAsync_WhenHasRequirements_ShouldThrowException()
        {
            // Arrange
            var phase = new ExperimentPhase
            {
                PhaseId = 1,
                ExperimentId = 2,
                Status = ExperimentPhaseStatus.Planned.ToString(),
                Experiment = new Experiment
                {
                    ExperimentId = 2,
                    Status = ExperimentStatus.Draft.ToString()
                }
            };

            _phaseRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentPhase, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IOrderedQueryable<ExperimentPhase>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IIncludableQueryable<ExperimentPhase, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(phase);

            _equipmentReqRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<PhaseEquipmentRequirement, bool>>>()))
                .ReturnsAsync(true);

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteExperimentPhaseAsync(1));
            Assert.Equal("Cannot delete phase that has associated equipment or human requirements.", ex.Message);
        }

        [Fact]
        public async Task StartExperimentPhaseAsync_WhenExperimentNotRunning_ShouldThrowException()
        {
            // Arrange
            var phase = new ExperimentPhase
            {
                PhaseId = 1,
                ExperimentId = 2,
                Status = ExperimentPhaseStatus.Planned.ToString(),
                Experiment = new Experiment
                {
                    ExperimentId = 2,
                    Status = ExperimentStatus.Ready.ToString() // Not Running yet
                }
            };

            _phaseRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentPhase, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IOrderedQueryable<ExperimentPhase>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IIncludableQueryable<ExperimentPhase, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(phase);

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.StartExperimentPhaseAsync(1, 2));
            Assert.Equal("Cannot start phase when experiment is not running.", ex.Message);
        }

        [Fact]
        public async Task StartExperimentPhaseAsync_WhenPreviousPhaseNotFinished_ShouldThrowException()
        {
            // Arrange
            var phase2 = new ExperimentPhase
            {
                PhaseId = 2,
                ExperimentId = 5,
                PhaseOrder = 2,
                Status = ExperimentPhaseStatus.Planned.ToString(),
                Experiment = new Experiment
                {
                    ExperimentId = 5,
                    Status = ExperimentStatus.Running.ToString()
                }
            };

            _phaseRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentPhase, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IOrderedQueryable<ExperimentPhase>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IIncludableQueryable<ExperimentPhase, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(phase2);

            _phaseRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ExperimentPhase, bool>>>()))
                .ReturnsAsync(true); // Phase 1 is still planned/in-progress

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.StartExperimentPhaseAsync(2, 2));
            Assert.Equal("Previous phases must be completed or cancelled before starting this phase.", ex.Message);
        }

        [Fact]
        public async Task StartExperimentPhaseAsync_WhenValid_ShouldTransitionToInProgress()
        {
            // Arrange
            var phase = new ExperimentPhase
            {
                PhaseId = 1,
                ExperimentId = 5,
                PhaseOrder = 1,
                Status = ExperimentPhaseStatus.Planned.ToString(),
                Experiment = new Experiment
                {
                    ExperimentId = 5,
                    ExperimentName = "Soil Analysis",
                    Status = ExperimentStatus.Running.ToString()
                }
            };

            _phaseRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentPhase, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IOrderedQueryable<ExperimentPhase>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IIncludableQueryable<ExperimentPhase, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(phase);

            _phaseRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<ExperimentPhase, bool>>>()))
                .ReturnsAsync(false); // No unfinished previous phase

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act
            var result = await service.StartExperimentPhaseAsync(1, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ExperimentPhaseStatus.InProgress.ToString(), phase.Status);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CompleteExperimentPhaseAsync_WhenNotInProgress_ShouldThrowException()
        {
            // Arrange
            var phase = new ExperimentPhase
            {
                PhaseId = 1,
                ExperimentId = 5,
                Status = ExperimentPhaseStatus.Planned.ToString(),
                Experiment = new Experiment
                {
                    ExperimentId = 5,
                    Status = ExperimentStatus.Running.ToString()
                }
            };

            _phaseRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentPhase, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IOrderedQueryable<ExperimentPhase>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IIncludableQueryable<ExperimentPhase, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(phase);

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CompleteExperimentPhaseAsync(1, 2));
            Assert.Equal("Only in-progress phases can be completed.", ex.Message);
        }

        [Fact]
        public async Task CompleteExperimentPhaseAsync_WhenValid_ShouldTransitionToCompleted()
        {
            // Arrange
            var phase = new ExperimentPhase
            {
                PhaseId = 1,
                ExperimentId = 5,
                Status = ExperimentPhaseStatus.InProgress.ToString(),
                Experiment = new Experiment
                {
                    ExperimentId = 5,
                    ExperimentName = "Soil Analysis",
                    Status = ExperimentStatus.Running.ToString()
                }
            };

            _phaseRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentPhase, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IOrderedQueryable<ExperimentPhase>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IIncludableQueryable<ExperimentPhase, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(phase);

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act
            var result = await service.CompleteExperimentPhaseAsync(1, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ExperimentPhaseStatus.Completed.ToString(), phase.Status);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelExperimentPhaseAsync_WhenAlreadyCompleted_ShouldThrowException()
        {
            // Arrange
            var phase = new ExperimentPhase
            {
                PhaseId = 1,
                ExperimentId = 5,
                Status = ExperimentPhaseStatus.Completed.ToString(),
                Experiment = new Experiment
                {
                    ExperimentId = 5,
                    Status = ExperimentStatus.Running.ToString()
                }
            };

            _phaseRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentPhase, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IOrderedQueryable<ExperimentPhase>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IIncludableQueryable<ExperimentPhase, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(phase);

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CancelExperimentPhaseAsync(1, 2));
            Assert.Equal("Completed or cancelled phases cannot be cancelled.", ex.Message);
        }

        [Fact]
        public async Task CancelExperimentPhaseAsync_WhenPlannedOrInProgress_ShouldTransitionToCancelled()
        {
            // Arrange
            var phase = new ExperimentPhase
            {
                PhaseId = 1,
                ExperimentId = 5,
                Status = ExperimentPhaseStatus.InProgress.ToString(),
                Experiment = new Experiment
                {
                    ExperimentId = 5,
                    ExperimentName = "Soil Analysis",
                    Status = ExperimentStatus.Running.ToString()
                }
            };

            _phaseRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<ExperimentPhase, bool>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IOrderedQueryable<ExperimentPhase>>>(),
                    It.IsAny<Func<IQueryable<ExperimentPhase>, IIncludableQueryable<ExperimentPhase, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(phase);

            var service = new ExperimentPhaseService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act
            var result = await service.CancelExperimentPhaseAsync(1, 2, "Weather conditions unsuitable");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ExperimentPhaseStatus.Cancelled.ToString(), phase.Status);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}

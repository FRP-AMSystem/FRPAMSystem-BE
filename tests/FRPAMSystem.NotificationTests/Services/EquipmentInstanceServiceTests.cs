using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.EquipmentInstances;
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
    public class EquipmentInstanceServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IClock> _clockMock = new();

        private readonly Mock<IGenericRepository<EquipmentInstance>> _instanceRepoMock = new();
        private readonly Mock<IGenericRepository<EquipmentType>> _typeRepoMock = new();
        private readonly Mock<IGenericRepository<Schedule>> _scheduleRepoMock = new();

        public EquipmentInstanceServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<EquipmentInstance>()).Returns(_instanceRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<EquipmentType>()).Returns(_typeRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<Schedule>()).Returns(_scheduleRepoMock.Object);

            _clockMock.Setup(c => c.Now).Returns(new DateTime(2026, 8, 21, 16, 0, 0));
        }

        // UT148-TC40
        // Normal
        [Fact]
        public async Task CreateEquipmentInstanceAsync_WithValidIndividualType_ShouldCreateInstanceAndIncrementCounters()
        {
            // Arrange
            var request = new EquipmentInstanceRequest
            {
                EquipmentTypeId = 1,
                AssetCode = "EQ-001",
                SerialNumber = "SN-12345",
                TotalUsageHours = 100,
                UsageHoursSinceMaintenance = 20,
                ConditionLevel = EquipmentConditionLevel.Good,
                Status = EquipmentInstanceStatus.Available,
                MaintenanceCount = 1
            };

            var equipmentType = new EquipmentType
            {
                EquipmentTypeId = 1,
                Name = "Chainsaw",
                TrackingType = EquipmentTrackingType.Individual.ToString(),
                BaseMaintenanceIntervalHours = 200,
                TotalQuantity = 5,
                AvailableQuantity = 3
            };

            _typeRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentType, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentType>, IOrderedQueryable<EquipmentType>>>(),
                    It.IsAny<Func<IQueryable<EquipmentType>, IIncludableQueryable<EquipmentType, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(equipmentType);

            _instanceRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<EquipmentInstance, bool>>>())).ReturnsAsync(false);

            _instanceRepoMock.Setup(r => r.InsertAsync(It.IsAny<EquipmentInstance>()))
                .Callback<EquipmentInstance>(e => e.EquipmentInstanceId = 50)
                .Returns(Task.CompletedTask);

            _instanceRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentInstance, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentInstance>, IOrderedQueryable<EquipmentInstance>>>(),
                    It.IsAny<Func<IQueryable<EquipmentInstance>, IIncludableQueryable<EquipmentInstance, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new EquipmentInstance
                {
                    EquipmentInstanceId = 50,
                    EquipmentTypeId = 1,
                    EquipmentType = equipmentType,
                    AssetCode = "EQ-001",
                    SerialNumber = "SN-12345",
                    TotalUsageHour = 100,
                    ConditionLevel = EquipmentConditionLevel.Good.ToString(),
                    Status = EquipmentInstanceStatus.Available.ToString()
                });

            var service = new EquipmentInstanceService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act
            var result = await service.CreateEquipmentInstanceAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EQ-001", result.AssetCode);
            Assert.Equal(6, equipmentType.TotalQuantity); // Incremented
            Assert.Equal(4, equipmentType.AvailableQuantity); // Incremented
            _typeRepoMock.Verify(r => r.Update(equipmentType), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // UT148-TC41
        // Abnormal
        [Fact]
        public async Task CreateEquipmentInstanceAsync_WhenDuplicateAssetCode_ShouldThrowException()
        {
            // Arrange
            var request = new EquipmentInstanceRequest
            {
                EquipmentTypeId = 1,
                AssetCode = "EQ-EXISTING",
                TotalUsageHours = 0,
                UsageHoursSinceMaintenance = 0,
                MaintenanceCount = 0,
                ConditionLevel = EquipmentConditionLevel.Good,
                Status = EquipmentInstanceStatus.Available
            };

            var equipmentType = new EquipmentType
            {
                EquipmentTypeId = 1,
                TrackingType = EquipmentTrackingType.Individual.ToString()
            };

            _typeRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentType, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentType>, IOrderedQueryable<EquipmentType>>>(),
                    It.IsAny<Func<IQueryable<EquipmentType>, IIncludableQueryable<EquipmentType, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(equipmentType);

            _instanceRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<EquipmentInstance, bool>>>())).ReturnsAsync(true);

            var service = new EquipmentInstanceService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateEquipmentInstanceAsync(request));
            Assert.Equal("Asset code already exists.", ex.Message);
        }

        // UT148-TC42
        // Normal
        [Fact]
        public async Task ReportEquipmentAsync_WhenReturned_ShouldAccumulateUsageHoursAndCheckMaintenanceThreshold()
        {
            // Arrange
            var request = new ReportEquipmentRequest
            {
                AllocationPlanId = 1,
                ReportType = EquipmentInstanceStatus.Returned.ToString(),
                EquipmentInstanceIds = new List<int> { 101 }
            };

            var schedules = new List<Schedule>
            {
                new Schedule
                {
                    ScheduleId = 1,
                    AllocationPlanId = 1,
                    Status = "Completed",
                    StartDate = new DateTime(2026, 8, 1, 8, 0, 0),
                    EndDate = new DateTime(2026, 8, 1, 18, 0, 0) // 10 hours
                }
            };

            _scheduleRepoMock.Setup(r => r.GetQueryable()).Returns(schedules.BuildMockQueryable());

            var equipmentInstance = new EquipmentInstance
            {
                EquipmentInstanceId = 101,
                TotalUsageHour = 50,
                UsageHoursSinceLastMaintenance = 95,
                EffectiveIntervalHour = 100, // Threshold 100 hrs. New usage = 95 + 10 = 105 >= 100 -> Maintenance
                Status = EquipmentInstanceStatus.InUse.ToString()
            };

            _instanceRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentInstance, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentInstance>, IOrderedQueryable<EquipmentInstance>>>(),
                    It.IsAny<Func<IQueryable<EquipmentInstance>, IIncludableQueryable<EquipmentInstance, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(equipmentInstance);

            var service = new EquipmentInstanceService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act
            var result = await service.ReportEquipmentAsync(request);

            // Assert
            Assert.True(result);
            Assert.Equal(60, equipmentInstance.TotalUsageHour); // 50 + 10
            Assert.Equal(105, equipmentInstance.UsageHoursSinceLastMaintenance); // 95 + 10
            Assert.Equal(EquipmentInstanceStatus.Maintenance.ToString(), equipmentInstance.Status);
            _instanceRepoMock.Verify(r => r.Update(equipmentInstance), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        // UT148-TC43
        // Normal
        [Fact]
        public async Task ConfirmReportEquipmentAsync_AcceptReturned_ShouldTransitionStatusToAvailable()
        {
            // Arrange
            var request = new ConfirmEquipmentRequest
            {
                ConfirmAction = "AcceptReturned",
                EquipmentInstanceIds = new List<int> { 201 },
                Note = "Verified in good condition"
            };

            var eq = new EquipmentInstance
            {
                EquipmentInstanceId = 201,
                Status = EquipmentInstanceStatus.Returned.ToString(),
                Note = ""
            };

            _instanceRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentInstance, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentInstance>, IOrderedQueryable<EquipmentInstance>>>(),
                    It.IsAny<Func<IQueryable<EquipmentInstance>, IIncludableQueryable<EquipmentInstance, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(eq);

            var service = new EquipmentInstanceService(_unitOfWorkMock.Object, _clockMock.Object);

            // Act
            var result = await service.ConfirmReportEquipmentAsync(request);

            // Assert
            Assert.True(result);
            Assert.Equal(EquipmentInstanceStatus.Available.ToString(), eq.Status);
            Assert.Contains("[Manager Confirm]: Verified in good condition", eq.Note);
            _instanceRepoMock.Verify(r => r.Update(eq), Times.Once);
        }

        // UT148-TC44
        // Boundary
        [Theory]
        [InlineData(100.0, EquipmentConditionLevel.Good, 1, 100.0)]     // 100 * 1.0 * 1.0 = 100
        [InlineData(100.0, EquipmentConditionLevel.Fair, 4, 76.5)]      // 100 * 0.85 * 0.9 = 76.5
        [InlineData(100.0, EquipmentConditionLevel.Poor, 8, 45.0)]      // 100 * 0.6 * 0.75 = 45.0
        [InlineData(100.0, EquipmentConditionLevel.Critical, 12, 18.0)] // 100 * 0.3 * 0.6 = 18.0
        public void CalculateEffectiveIntervalHour_BoundaryConditions_ShouldCalculateAccurateValues(
            double baseInterval,
            EquipmentConditionLevel condition,
            int maintenanceCount,
            double expected)
        {
            // Act
            var actual = EquipmentInstanceService.CalculateEffectiveIntervalHour(baseInterval, condition, maintenanceCount);

            // Assert
            Assert.Equal(expected, actual, precision: 2);
        }
    }
}

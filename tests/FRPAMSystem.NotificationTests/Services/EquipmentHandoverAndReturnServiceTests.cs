using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.EquipmentHandover;
using FRPAMSystem.BusinessTier.Payload.EquipmentReturn;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Implements;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace FRPAMSystem.NotificationTests.Services
{
    public class EquipmentHandoverAndReturnServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IAllocationEquipmentDetailService> _detailServiceMock = new();
        private readonly Mock<IClock> _clockMock = new();

        private readonly Mock<IGenericRepository<EquipmentHandover>> _handoverRepoMock = new();
        private readonly Mock<IGenericRepository<EquipmentReturn>> _returnRepoMock = new();
        private readonly Mock<IGenericRepository<AllocationEquipmentDetail>> _detailRepoMock = new();
        private readonly Mock<IGenericRepository<EquipmentInstance>> _instanceRepoMock = new();
        private readonly Mock<IGenericRepository<User>> _userRepoMock = new();

        private readonly DateTime _testNow = new DateTime(2026, 8, 25, 10, 0, 0);

        public EquipmentHandoverAndReturnServiceTests()
        {
            _unitOfWorkMock.Setup(u => u.GetRepository<EquipmentHandover>()).Returns(_handoverRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<EquipmentReturn>()).Returns(_returnRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<AllocationEquipmentDetail>()).Returns(_detailRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<EquipmentInstance>()).Returns(_instanceRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<User>()).Returns(_userRepoMock.Object);

            _clockMock.Setup(c => c.Now).Returns(_testNow);
            _detailServiceMock.Setup(s => s.UserCanAccessAllocationEquipmentDetailAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handover_SubmitMineAsync_WhenPendingHandoverExists_ShouldConfirmExistingAndSetInUse()
        {
            // Arrange
            int detailId = 10;
            int userId = 3;

            var instance = new EquipmentInstance
            {
                EquipmentInstanceId = 101,
                Status = EquipmentInstanceStatus.Reserved.ToString()
            };

            var detail = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = detailId,
                EquipmentInstanceId = 101,
                EquipmentInstance = instance,
                Status = AllocationDetailStatus.Allocated.ToString()
            };

            var pendingHandover = new EquipmentHandover
            {
                HandoverId = 1,
                AllocationEquipmentDetailId = detailId,
                EquipmentInstanceId = 101,
                HandedOverBy = 1,
                ReceivedBy = userId,
                Status = EquipmentHandoverStatus.Pending.ToString()
            };

            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IOrderedQueryable<AllocationEquipmentDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IIncludableQueryable<AllocationEquipmentDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(detail);

            _handoverRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.Is<Expression<Func<EquipmentHandover, bool>>>(e => true),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IOrderedQueryable<EquipmentHandover>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IIncludableQueryable<EquipmentHandover, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(pendingHandover);

            var service = new EquipmentHandoverService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object);

            // Act
            var result = await service.SubmitMineAsync(detailId, userId, new EquipmentHandoverMineRequest
            {
                ConditionBefore = "Good condition",
                Note = "Received at warehouse"
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(EquipmentHandoverStatus.Confirmed.ToString(), pendingHandover.Status);
            Assert.Equal(userId, pendingHandover.ReceivedBy);
            Assert.Equal(_testNow, pendingHandover.ConfirmedAt);
            Assert.Equal(AllocationDetailStatus.InUse.ToString(), detail.Status);
            Assert.Equal(EquipmentInstanceStatus.InUse.ToString(), instance.Status);

            _handoverRepoMock.Verify(r => r.Update(pendingHandover), Times.Once);
            _detailRepoMock.Verify(r => r.Update(detail), Times.Once);
            _instanceRepoMock.Verify(r => r.Update(instance), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handover_RejectMineAsync_ShouldSetStatusToRejected()
        {
            // Arrange
            int detailId = 10;
            int userId = 3;

            var pendingHandover = new EquipmentHandover
            {
                HandoverId = 1,
                AllocationEquipmentDetailId = detailId,
                Status = EquipmentHandoverStatus.Pending.ToString()
            };

            _handoverRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentHandover, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IOrderedQueryable<EquipmentHandover>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IIncludableQueryable<EquipmentHandover, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(pendingHandover);

            var service = new EquipmentHandoverService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object);

            // Act
            var result = await service.RejectMineAsync(detailId, userId, new RejectHandoverRequest
            {
                Reason = "Screen cracked"
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(EquipmentHandoverStatus.Rejected.ToString(), pendingHandover.Status);
            Assert.Contains("Screen cracked", pendingHandover.Note);
            Assert.Equal(_testNow, pendingHandover.ConfirmedAt);

            _handoverRepoMock.Verify(r => r.Update(pendingHandover), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Return_SubmitMineAsync_ShouldCreatePendingReturnRequest()
        {
            // Arrange
            int detailId = 20;
            int userId = 4;

            var detail = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = detailId,
                EquipmentInstanceId = 102,
                Quantity = 1,
                Status = AllocationDetailStatus.InUse.ToString()
            };

            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IOrderedQueryable<AllocationEquipmentDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IIncludableQueryable<AllocationEquipmentDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(detail);

            _returnRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentReturn, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IOrderedQueryable<EquipmentReturn>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IIncludableQueryable<EquipmentReturn, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((EquipmentReturn?)null);

            var service = new EquipmentReturnService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object);

            // Act
            var result = await service.SubmitMineAsync(detailId, userId, new EquipmentReturnMineRequest
            {
                ConditionAfter = "Clean and working",
                IsDamaged = false,
                Note = "Done with field test"
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(EquipmentReturnStatus.Pending.ToString(), result.Status);
            _returnRepoMock.Verify(r => r.InsertAsync(It.Is<EquipmentReturn>(ret =>
                ret.Status == EquipmentReturnStatus.Pending.ToString() &&
                ret.ReturnedBy == userId)), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Return_ConfirmAsync_ByManager_ShouldSetCompletedAndAvailable()
        {
            // Arrange
            int returnId = 5;
            int managerUserId = 1;

            var instance = new EquipmentInstance
            {
                EquipmentInstanceId = 105,
                Status = EquipmentInstanceStatus.InUse.ToString()
            };

            var detail = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = 30,
                EquipmentInstanceId = 105,
                EquipmentInstance = instance,
                Status = AllocationDetailStatus.InUse.ToString()
            };

            var pendingReturn = new EquipmentReturn
            {
                ReturnId = returnId,
                AllocationEquipmentDetailId = 30,
                AllocationEquipmentDetail = detail,
                IsDamaged = false,
                Status = EquipmentReturnStatus.Pending.ToString()
            };

            _returnRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentReturn, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IOrderedQueryable<EquipmentReturn>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IIncludableQueryable<EquipmentReturn, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(pendingReturn);

            var service = new EquipmentReturnService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object);

            // Act
            var result = await service.ConfirmAsync(returnId, managerUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(EquipmentReturnStatus.Confirmed.ToString(), pendingReturn.Status);
            Assert.Equal(managerUserId, pendingReturn.ReceivedBy);
            Assert.Equal(_testNow, pendingReturn.ConfirmedAt);
            Assert.Equal(AllocationDetailStatus.Completed.ToString(), detail.Status);
            Assert.Equal(EquipmentInstanceStatus.Available.ToString(), instance.Status);

            _returnRepoMock.Verify(r => r.Update(pendingReturn), Times.Once);
            _detailRepoMock.Verify(r => r.Update(detail), Times.Once);
            _instanceRepoMock.Verify(r => r.Update(instance), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Return_RejectAsync_ByManager_ShouldSetRejectedAndKeepDetailInUse()
        {
            // Arrange
            int returnId = 6;
            int managerUserId = 1;

            var pendingReturn = new EquipmentReturn
            {
                ReturnId = returnId,
                Status = EquipmentReturnStatus.Pending.ToString()
            };

            _returnRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentReturn, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IOrderedQueryable<EquipmentReturn>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IIncludableQueryable<EquipmentReturn, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(pendingReturn);

            var service = new EquipmentReturnService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object);

            // Act
            var result = await service.RejectAsync(returnId, managerUserId, new RejectReturnRequest
            {
                Reason = "Missing charger"
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(EquipmentReturnStatus.Rejected.ToString(), pendingReturn.Status);
            Assert.Equal(managerUserId, pendingReturn.ReceivedBy);
            Assert.Contains("Missing charger", pendingReturn.Note);

            _returnRepoMock.Verify(r => r.Update(pendingReturn), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Handover_SubmitMineAsync_WhenNoPendingHandover_ShouldThrowException()
        {
            // Arrange
            int detailId = 15;
            int userId = 3;

            var detail = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = detailId,
                EquipmentInstanceId = 101,
                Status = AllocationDetailStatus.Allocated.ToString()
            };

            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IOrderedQueryable<AllocationEquipmentDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IIncludableQueryable<AllocationEquipmentDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(detail);

            _handoverRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentHandover, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IOrderedQueryable<EquipmentHandover>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IIncludableQueryable<EquipmentHandover, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync((EquipmentHandover?)null);

            var service = new EquipmentHandoverService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.SubmitMineAsync(detailId, userId));

            Assert.Equal("No pending handover found for this allocation equipment detail.", ex.Message);
        }

        [Fact]
        public async Task Handover_SubmitMineAsync_ShouldSendNotificationToManager()
        {
            // Arrange
            int detailId = 16;
            int userId = 3;
            int managerId = 9;

            var detail = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = detailId,
                EquipmentInstanceId = 101,
                Status = AllocationDetailStatus.Allocated.ToString()
            };

            var pendingHandover = new EquipmentHandover
            {
                HandoverId = 42,
                AllocationEquipmentDetailId = detailId,
                HandedOverBy = managerId,
                ReceivedBy = userId,
                Status = EquipmentHandoverStatus.Pending.ToString()
            };

            _detailRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IOrderedQueryable<AllocationEquipmentDetail>>>(),
                    It.IsAny<Func<IQueryable<AllocationEquipmentDetail>, IIncludableQueryable<AllocationEquipmentDetail, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(detail);

            _handoverRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentHandover, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IOrderedQueryable<EquipmentHandover>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IIncludableQueryable<EquipmentHandover, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(pendingHandover);

            var notificationMock = new Mock<INotificationService>();
            var service = new EquipmentHandoverService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object,
                notificationMock.Object);

            // Act
            await service.SubmitMineAsync(detailId, userId);

            // Assert
            notificationMock.Verify(n => n.SendAsync(It.Is<SendNotificationRequest>(req =>
                req.UserId == managerId &&
                req.NotificationType == NotificationTypes.EquipmentHandoverConfirmed &&
                req.ReferenceId == 42)), Times.Once);
        }

        [Fact]
        public async Task Handover_RejectMineAsync_ShouldSendNotificationToManager()
        {
            // Arrange
            int detailId = 17;
            int userId = 3;
            int managerId = 8;

            var pendingHandover = new EquipmentHandover
            {
                HandoverId = 43,
                AllocationEquipmentDetailId = detailId,
                HandedOverBy = managerId,
                ReceivedBy = userId,
                Status = EquipmentHandoverStatus.Pending.ToString()
            };

            _handoverRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentHandover, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IOrderedQueryable<EquipmentHandover>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IIncludableQueryable<EquipmentHandover, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(pendingHandover);

            var notificationMock = new Mock<INotificationService>();
            var service = new EquipmentHandoverService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object,
                notificationMock.Object);

            // Act
            await service.RejectMineAsync(detailId, userId, new RejectHandoverRequest
            {
                Reason = "Wrong model delivered"
            });

            // Assert
            notificationMock.Verify(n => n.SendAsync(It.Is<SendNotificationRequest>(req =>
                req.UserId == managerId &&
                req.NotificationType == NotificationTypes.EquipmentHandoverRejected &&
                req.ReferenceId == 43)), Times.Once);
        }

        [Fact]
        public async Task Handover_UpdateAsync_WhenAlreadyConfirmed_ShouldThrowException()
        {
            // Arrange
            var confirmedHandover = new EquipmentHandover
            {
                HandoverId = 1,
                Status = EquipmentHandoverStatus.Confirmed.ToString()
            };

            _detailRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>()))
                .ReturnsAsync(true);
            _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(true);

            _handoverRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentHandover, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IOrderedQueryable<EquipmentHandover>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IIncludableQueryable<EquipmentHandover, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(confirmedHandover);

            var service = new EquipmentHandoverService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.UpdateAsync(1, new EquipmentHandoverRequest
                {
                    AllocationEquipmentDetailId = 10,
                    HandedOverBy = 1,
                    ReceivedBy = 2,
                    Quantity = 1
                }));

            Assert.Equal("Cannot modify a handover that has already been confirmed or rejected.", ex.Message);
        }

        [Fact]
        public async Task Handover_DeleteAsync_WhenConfirmed_ShouldThrowException()
        {
            // Arrange
            var confirmedHandover = new EquipmentHandover
            {
                HandoverId = 1,
                Status = EquipmentHandoverStatus.Confirmed.ToString()
            };

            _handoverRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentHandover, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IOrderedQueryable<EquipmentHandover>>>(),
                    It.IsAny<Func<IQueryable<EquipmentHandover>, IIncludableQueryable<EquipmentHandover, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(confirmedHandover);

            var service = new EquipmentHandoverService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteAsync(1));
            Assert.Equal("Cannot delete a confirmed equipment handover as it is part of the transaction audit history.", ex.Message);
        }

        [Fact]
        public async Task Return_ConfirmAsync_WhenDamaged_ShouldSetEquipmentInstanceToDamaged()
        {
            // Arrange
            int returnId = 55;
            int managerUserId = 1;

            var instance = new EquipmentInstance
            {
                EquipmentInstanceId = 205,
                Status = EquipmentInstanceStatus.InUse.ToString()
            };

            var detail = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = 35,
                EquipmentInstanceId = 205,
                EquipmentInstance = instance,
                Status = AllocationDetailStatus.InUse.ToString()
            };

            var pendingReturn = new EquipmentReturn
            {
                ReturnId = returnId,
                AllocationEquipmentDetailId = 35,
                AllocationEquipmentDetail = detail,
                IsDamaged = true,
                DamageDescription = "Lens cracked",
                Status = EquipmentReturnStatus.Pending.ToString()
            };

            _returnRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentReturn, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IOrderedQueryable<EquipmentReturn>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IIncludableQueryable<EquipmentReturn, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(pendingReturn);

            var service = new EquipmentReturnService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object);

            // Act
            var result = await service.ConfirmAsync(returnId, managerUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(EquipmentReturnStatus.Confirmed.ToString(), pendingReturn.Status);
            Assert.Equal(AllocationDetailStatus.Completed.ToString(), detail.Status);
            Assert.Equal(EquipmentInstanceStatus.Damaged.ToString(), instance.Status);
        }

        [Fact]
        public async Task Return_ConfirmAsync_ShouldSendNotificationToResearcher()
        {
            // Arrange
            int returnId = 56;
            int managerUserId = 1;
            int researcherUserId = 7;

            var detail = new AllocationEquipmentDetail
            {
                AllocationEquipmentDetailId = 36,
                Status = AllocationDetailStatus.InUse.ToString()
            };

            var pendingReturn = new EquipmentReturn
            {
                ReturnId = returnId,
                AllocationEquipmentDetailId = 36,
                AllocationEquipmentDetail = detail,
                ReturnedBy = researcherUserId,
                Status = EquipmentReturnStatus.Pending.ToString()
            };

            _returnRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentReturn, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IOrderedQueryable<EquipmentReturn>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IIncludableQueryable<EquipmentReturn, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(pendingReturn);

            var notificationMock = new Mock<INotificationService>();
            var service = new EquipmentReturnService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object,
                notificationMock.Object);

            // Act
            await service.ConfirmAsync(returnId, managerUserId);

            // Assert
            notificationMock.Verify(n => n.SendAsync(It.Is<SendNotificationRequest>(req =>
                req.UserId == researcherUserId &&
                req.NotificationType == NotificationTypes.EquipmentReturnConfirmed &&
                req.ReferenceId == returnId)), Times.Once);
        }

        [Fact]
        public async Task Return_RejectAsync_ShouldSendNotificationToResearcher()
        {
            // Arrange
            int returnId = 57;
            int managerUserId = 1;
            int researcherUserId = 7;

            var pendingReturn = new EquipmentReturn
            {
                ReturnId = returnId,
                ReturnedBy = researcherUserId,
                Status = EquipmentReturnStatus.Pending.ToString()
            };

            _returnRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentReturn, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IOrderedQueryable<EquipmentReturn>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IIncludableQueryable<EquipmentReturn, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(pendingReturn);

            var notificationMock = new Mock<INotificationService>();
            var service = new EquipmentReturnService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object,
                notificationMock.Object);

            // Act
            await service.RejectAsync(returnId, managerUserId, new RejectReturnRequest
            {
                Reason = "Accessories missing"
            });

            // Assert
            notificationMock.Verify(n => n.SendAsync(It.Is<SendNotificationRequest>(req =>
                req.UserId == researcherUserId &&
                req.NotificationType == NotificationTypes.EquipmentReturnRejected &&
                req.ReferenceId == returnId)), Times.Once);
        }

        [Fact]
        public async Task Return_UpdateAsync_WhenConfirmed_ShouldThrowException()
        {
            // Arrange
            var confirmedReturn = new EquipmentReturn
            {
                ReturnId = 1,
                Status = EquipmentReturnStatus.Confirmed.ToString()
            };

            _detailRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<AllocationEquipmentDetail, bool>>>()))
                .ReturnsAsync(true);
            _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(true);

            _returnRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentReturn, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IOrderedQueryable<EquipmentReturn>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IIncludableQueryable<EquipmentReturn, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(confirmedReturn);

            var service = new EquipmentReturnService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.UpdateAsync(1, new EquipmentReturnRequest
                {
                    AllocationEquipmentDetailId = 10,
                    ReturnedBy = 1,
                    ReceivedBy = 2,
                    Quantity = 1,
                    ConditionAfter = "Good"
                }));

            Assert.Equal("Cannot modify an equipment return that has already been confirmed or rejected.", ex.Message);
        }

        [Fact]
        public async Task Return_DeleteAsync_WhenConfirmed_ShouldThrowException()
        {
            // Arrange
            var confirmedReturn = new EquipmentReturn
            {
                ReturnId = 1,
                Status = EquipmentReturnStatus.Confirmed.ToString()
            };

            _returnRepoMock.Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<EquipmentReturn, bool>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IOrderedQueryable<EquipmentReturn>>>(),
                    It.IsAny<Func<IQueryable<EquipmentReturn>, IIncludableQueryable<EquipmentReturn, object>>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(confirmedReturn);

            var service = new EquipmentReturnService(
                _unitOfWorkMock.Object,
                _detailServiceMock.Object,
                _clockMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.DeleteAsync(1));
            Assert.Equal("Cannot delete a confirmed equipment return as it is part of the transaction audit history.", ex.Message);
        }
    }
}

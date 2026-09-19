using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.EquipmentHandover;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class EquipmentHandoverService : IEquipmentHandoverService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAllocationEquipmentDetailService _allocationEquipmentDetailService;
        private readonly IClock _clock;
        private readonly INotificationService? _notificationService;

        public EquipmentHandoverService(
            IUnitOfWork unitOfWork,
            IAllocationEquipmentDetailService allocationEquipmentDetailService,
            IClock clock,
            INotificationService? notificationService = null)
        {
            _unitOfWork = unitOfWork;
            _allocationEquipmentDetailService = allocationEquipmentDetailService;
            _clock = clock;
            _notificationService = notificationService;
        }

        public async Task<IPaginate<EquipmentHandoverResponse>> ViewAllAsync(
            EquipmentHandoverFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<EquipmentHandover>()
                .GetQueryable()
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(h => h.CreatedAt);

            return await query
                .Select(h => new EquipmentHandoverResponse
                {
                    HandoverId = h.HandoverId,
                    AllocationEquipmentDetailId = h.AllocationEquipmentDetailId,
                    EquipmentInstanceId = h.EquipmentInstanceId,
                    HandedOverBy = h.HandedOverBy,
                    ReceivedBy = h.ReceivedBy,
                    HandoverDate = h.HandoverDate,
                    Quantity = h.Quantity,
                    ConditionBefore = h.ConditionBefore,
                    Note = h.Note,
                    Status = h.Status,
                    ConfirmedAt = h.ConfirmedAt,
                    CreatedAt = h.CreatedAt,
                    UpdatedAt = h.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<EquipmentHandoverResponse?> GetByIdAsync(int id)
        {
            var handover = await _unitOfWork
                .GetRepository<EquipmentHandover>()
                .FirstOrDefaultAsync(predicate: h => h.HandoverId == id);

            return handover == null ? null : MapToResponse(handover);
        }

        public async Task<EquipmentHandoverResponse?> SubmitMineAsync(
            int allocationEquipmentDetailId,
            int userId,
            EquipmentHandoverMineRequest? request = null)
        {
            if (!await _allocationEquipmentDetailService
                    .UserCanAccessAllocationEquipmentDetailAsync(allocationEquipmentDetailId, userId))
            {
                return null;
            }

            var detail = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationEquipmentDetailId == allocationEquipmentDetailId,
                    include: query => query
                        .Include(d => d.AllocationPlan)
                        .Include(d => d.EquipmentInstance)
                        .Include(d => d.AllocatedEquipmentType),
                    asNoTracking: false
                );

            if (detail == null)
            {
                return null;
            }

            if (!IsHandoverEligible(detail.Status))
            {
                throw new Exception(
                    "Equipment must be in Reserved or Allocated status before handover.");
            }

            if (detail.AllocatedEquipmentType?.TrackingType == EquipmentTrackingType.Individual.ToString() &&
                !detail.EquipmentInstanceId.HasValue)
            {
                throw new Exception(
                    "Individual-tracked equipment must have a specific equipment instance assigned before handover.");
            }

            var pendingHandover = await _unitOfWork
                .GetRepository<EquipmentHandover>()
                .FirstOrDefaultAsync(
                    predicate: h => h.AllocationEquipmentDetailId == allocationEquipmentDetailId &&
                                    h.Status == EquipmentHandoverStatus.Pending.ToString(),
                    asNoTracking: false
                );

            if (pendingHandover == null)
            {
                throw new Exception("No pending handover found for this allocation equipment detail.");
            }

            var now = _clock.Now;
            pendingHandover.Status = EquipmentHandoverStatus.Confirmed.ToString();
            pendingHandover.ReceivedBy = userId;
            pendingHandover.ConfirmedAt = now;

            if (!string.IsNullOrWhiteSpace(request?.ConditionBefore))
            {
                pendingHandover.ConditionBefore = request.ConditionBefore;
            }

            if (!string.IsNullOrWhiteSpace(request?.Note))
            {
                pendingHandover.Note = request.Note;
            }

            _unitOfWork.GetRepository<EquipmentHandover>().Update(pendingHandover);

            detail.Status = AllocationDetailStatus.InUse.ToString();
            if (detail.EquipmentInstanceId.HasValue && detail.EquipmentInstance != null)
            {
                detail.EquipmentInstance.Status = EquipmentInstanceStatus.InUse.ToString();
                _unitOfWork.GetRepository<EquipmentInstance>().Update(detail.EquipmentInstance);
            }
            _unitOfWork.GetRepository<AllocationEquipmentDetail>().Update(detail);

            if (_notificationService != null)
            {
                await _notificationService.SendAsync(new SendNotificationRequest
                {
                    UserId = pendingHandover.HandedOverBy,
                    Title = "Thiết bị đã được tiếp nhận",
                    Message = $"Researcher đã xác nhận nhận thiết bị (Mã bàn giao: #{pendingHandover.HandoverId}).",
                    NotificationType = NotificationTypes.EquipmentHandoverConfirmed,
                    ReferenceType = NotificationReferenceTypes.EquipmentHandover,
                    ReferenceId = pendingHandover.HandoverId
                });
            }

            await _unitOfWork.CommitAsync();

            return MapToResponse(pendingHandover);
        }

        public async Task<EquipmentHandoverResponse?> RejectMineAsync(
            int allocationEquipmentDetailId,
            int userId,
            RejectHandoverRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new Exception("Rejection reason is required.");
            }

            if (!await _allocationEquipmentDetailService
                    .UserCanAccessAllocationEquipmentDetailAsync(allocationEquipmentDetailId, userId))
            {
                return null;
            }

            var pendingHandover = await _unitOfWork
                .GetRepository<EquipmentHandover>()
                .FirstOrDefaultAsync(
                    predicate: h => h.AllocationEquipmentDetailId == allocationEquipmentDetailId &&
                                    h.Status == EquipmentHandoverStatus.Pending.ToString(),
                    asNoTracking: false
                );

            if (pendingHandover == null)
            {
                throw new Exception("No pending handover found to reject.");
            }

            pendingHandover.Status = EquipmentHandoverStatus.Rejected.ToString();
            pendingHandover.ReceivedBy = userId;
            pendingHandover.Note = string.IsNullOrWhiteSpace(pendingHandover.Note)
                ? $"Rejected: {request.Reason}"
                : $"{pendingHandover.Note} | Rejected: {request.Reason}";
            pendingHandover.ConfirmedAt = _clock.Now;

            _unitOfWork.GetRepository<EquipmentHandover>().Update(pendingHandover);

            if (_notificationService != null)
            {
                await _notificationService.SendAsync(new SendNotificationRequest
                {
                    UserId = pendingHandover.HandedOverBy,
                    Title = "Bàn giao thiết bị bị từ chối",
                    Message = $"Researcher đã từ chối nhận thiết bị (Mã bàn giao: #{pendingHandover.HandoverId}). Lý do: {request.Reason}",
                    NotificationType = NotificationTypes.EquipmentHandoverRejected,
                    ReferenceType = NotificationReferenceTypes.EquipmentHandover,
                    ReferenceId = pendingHandover.HandoverId
                });
            }

            await _unitOfWork.CommitAsync();

            return MapToResponse(pendingHandover);
        }

        public async Task<EquipmentHandoverResponse?> RejectAsync(
            int handoverId,
            int managerUserId,
            RejectHandoverRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new Exception("Rejection reason is required.");
            }

            var handover = await _unitOfWork
                .GetRepository<EquipmentHandover>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HandoverId == handoverId,
                    asNoTracking: false
                );

            if (handover == null)
            {
                return null;
            }

            if (handover.Status != EquipmentHandoverStatus.Pending.ToString())
            {
                throw new Exception("Only pending handovers can be rejected.");
            }

            handover.Status = EquipmentHandoverStatus.Rejected.ToString();
            handover.Note = string.IsNullOrWhiteSpace(handover.Note)
                ? $"Rejected: {request.Reason}"
                : $"{handover.Note} | Rejected: {request.Reason}";
            handover.ConfirmedAt = _clock.Now;

            _unitOfWork.GetRepository<EquipmentHandover>().Update(handover);

            if (_notificationService != null)
            {
                await _notificationService.SendAsync(new SendNotificationRequest
                {
                    UserId = handover.ReceivedBy,
                    Title = "Bàn giao thiết bị đã bị hủy/từ chối",
                    Message = $"Yêu cầu bàn giao thiết bị #{handover.HandoverId} đã bị hủy bởi Quản lý. Lý do: {request.Reason}",
                    NotificationType = NotificationTypes.EquipmentHandoverRejected,
                    ReferenceType = NotificationReferenceTypes.EquipmentHandover,
                    ReferenceId = handover.HandoverId
                });
            }

            await _unitOfWork.CommitAsync();

            return MapToResponse(handover);
        }

        public async Task<EquipmentHandoverResponse> CreateAsync(EquipmentHandoverRequest request)
        {
            await ValidateRequestAsync(request);

            var status = string.IsNullOrWhiteSpace(request.Status)
                ? EquipmentHandoverStatus.Pending.ToString()
                : request.Status.Trim();

            var handover = new EquipmentHandover
            {
                AllocationEquipmentDetailId = request.AllocationEquipmentDetailId,
                EquipmentInstanceId = request.EquipmentInstanceId,
                HandedOverBy = request.HandedOverBy,
                ReceivedBy = request.ReceivedBy,
                HandoverDate = request.HandoverDate,
                Quantity = request.Quantity,
                ConditionBefore = request.ConditionBefore,
                Note = request.Note,
                Status = status,
                ConfirmedAt = request.ConfirmedAt
            };

            await _unitOfWork.GetRepository<EquipmentHandover>().InsertAsync(handover);

            if (status == EquipmentHandoverStatus.Confirmed.ToString())
            {
                var detail = await _unitOfWork
                    .GetRepository<AllocationEquipmentDetail>()
                    .FirstOrDefaultAsync(
                        predicate: d => d.AllocationEquipmentDetailId == request.AllocationEquipmentDetailId,
                        include: query => query.Include(d => d.EquipmentInstance),
                        asNoTracking: false
                    );

                if (detail != null)
                {
                    detail.Status = AllocationDetailStatus.InUse.ToString();
                    if (detail.EquipmentInstance != null)
                    {
                        detail.EquipmentInstance.Status = EquipmentInstanceStatus.InUse.ToString();
                        _unitOfWork.GetRepository<EquipmentInstance>().Update(detail.EquipmentInstance);
                    }
                    _unitOfWork.GetRepository<AllocationEquipmentDetail>().Update(detail);
                }
            }
            else if (status == EquipmentHandoverStatus.Pending.ToString() && _notificationService != null)
            {
                await _notificationService.SendAsync(new SendNotificationRequest
                {
                    UserId = request.ReceivedBy,
                    Title = "Thiết bị sẵn sàng bàn giao",
                    Message = "Thiết bị đã sẵn sàng để bàn giao cho bạn. Vui lòng kiểm tra và xác nhận nhận thiết bị.",
                    NotificationType = NotificationTypes.EquipmentHandoverPending,
                    ReferenceType = NotificationReferenceTypes.EquipmentHandover,
                    ReferenceId = handover.HandoverId
                });
            }

            await _unitOfWork.CommitAsync();

            return MapToResponse(handover);
        }

        public async Task<EquipmentHandoverResponse?> UpdateAsync(
            int id,
            EquipmentHandoverRequest request)
        {
            await ValidateRequestAsync(request);

            var handover = await _unitOfWork
                .GetRepository<EquipmentHandover>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HandoverId == id,
                    asNoTracking: false
                );

            if (handover == null)
            {
                return null;
            }

            if (handover.Status == EquipmentHandoverStatus.Confirmed.ToString() ||
                handover.Status == EquipmentHandoverStatus.Rejected.ToString())
            {
                throw new Exception("Cannot modify a handover that has already been confirmed or rejected.");
            }

            var previousStatus = handover.Status;
            var targetStatus = string.IsNullOrWhiteSpace(request.Status)
                ? handover.Status
                : request.Status.Trim();

            handover.AllocationEquipmentDetailId = request.AllocationEquipmentDetailId;
            handover.EquipmentInstanceId = request.EquipmentInstanceId;
            handover.HandedOverBy = request.HandedOverBy;
            handover.ReceivedBy = request.ReceivedBy;
            handover.HandoverDate = request.HandoverDate;
            handover.Quantity = request.Quantity;
            handover.ConditionBefore = request.ConditionBefore;
            handover.Note = request.Note;
            handover.Status = targetStatus;
            handover.ConfirmedAt = request.ConfirmedAt;

            _unitOfWork.GetRepository<EquipmentHandover>().Update(handover);

            if (targetStatus == EquipmentHandoverStatus.Confirmed.ToString() &&
                previousStatus != EquipmentHandoverStatus.Confirmed.ToString())
            {
                var detail = await _unitOfWork
                    .GetRepository<AllocationEquipmentDetail>()
                    .FirstOrDefaultAsync(
                        predicate: d => d.AllocationEquipmentDetailId == request.AllocationEquipmentDetailId,
                        include: query => query.Include(d => d.EquipmentInstance),
                        asNoTracking: false
                    );

                if (detail != null)
                {
                    detail.Status = AllocationDetailStatus.InUse.ToString();
                    if (detail.EquipmentInstance != null)
                    {
                        detail.EquipmentInstance.Status = EquipmentInstanceStatus.InUse.ToString();
                        _unitOfWork.GetRepository<EquipmentInstance>().Update(detail.EquipmentInstance);
                    }
                    _unitOfWork.GetRepository<AllocationEquipmentDetail>().Update(detail);
                }
            }

            await _unitOfWork.CommitAsync();

            return MapToResponse(handover);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var handover = await _unitOfWork
                .GetRepository<EquipmentHandover>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HandoverId == id,
                    asNoTracking: false
                );

            if (handover == null)
            {
                return false;
            }

            if (handover.Status == EquipmentHandoverStatus.Confirmed.ToString())
            {
                throw new Exception("Cannot delete a confirmed equipment handover as it is part of the transaction audit history.");
            }

            _unitOfWork.GetRepository<EquipmentHandover>().Delete(handover);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task ValidateRequestAsync(EquipmentHandoverRequest request)
        {
            if (request.Quantity <= 0)
            {
                throw new Exception("Quantity must be greater than 0.");
            }

            var detailExists = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .AnyAsync(d => d.AllocationEquipmentDetailId == request.AllocationEquipmentDetailId);

            if (!detailExists)
            {
                throw new Exception("Allocation equipment detail does not exist.");
            }

            if (request.EquipmentInstanceId.HasValue)
            {
                var instanceExists = await _unitOfWork
                    .GetRepository<EquipmentInstance>()
                    .AnyAsync(e => e.EquipmentInstanceId == request.EquipmentInstanceId.Value);

                if (!instanceExists)
                {
                    throw new Exception("Equipment instance does not exist.");
                }
            }

            await EnsureUserExistsAsync(request.HandedOverBy, "Handed-over user");
            await EnsureUserExistsAsync(request.ReceivedBy, "Receiving user");
        }

        private async Task EnsureUserExistsAsync(int userId, string label)
        {
            var exists = await _unitOfWork
                .GetRepository<User>()
                .AnyAsync(u => u.UserId == userId);

            if (!exists)
            {
                throw new Exception($"{label} does not exist.");
            }
        }

        internal async Task<EquipmentHandover> CreateConfirmedHandoverInternalAsync(
            AllocationEquipmentDetail detail,
            int handedOverBy,
            int receivedBy,
            string? conditionBefore,
            string? note)
        {
            var now = _clock.Now;

            var handover = new EquipmentHandover
            {
                AllocationEquipmentDetailId = detail.AllocationEquipmentDetailId,
                EquipmentInstanceId = detail.EquipmentInstanceId,
                HandedOverBy = handedOverBy,
                ReceivedBy = receivedBy,
                HandoverDate = now,
                Quantity = detail.Quantity,
                ConditionBefore = conditionBefore,
                Note = note,
                Status = EquipmentHandoverStatus.Confirmed.ToString(),
                ConfirmedAt = now
            };

            detail.Status = AllocationDetailStatus.InUse.ToString();

            if (detail.EquipmentInstanceId.HasValue && detail.EquipmentInstance != null)
            {
                detail.EquipmentInstance.Status = EquipmentInstanceStatus.InUse.ToString();
                _unitOfWork.GetRepository<EquipmentInstance>().Update(detail.EquipmentInstance);
            }

            await _unitOfWork.GetRepository<EquipmentHandover>().InsertAsync(handover);
            _unitOfWork.GetRepository<AllocationEquipmentDetail>().Update(detail);

            return handover;
        }

        private static EquipmentHandoverResponse MapToResponse(EquipmentHandover handover)
        {
            return new EquipmentHandoverResponse
            {
                HandoverId = handover.HandoverId,
                AllocationEquipmentDetailId = handover.AllocationEquipmentDetailId,
                EquipmentInstanceId = handover.EquipmentInstanceId,
                HandedOverBy = handover.HandedOverBy,
                ReceivedBy = handover.ReceivedBy,
                HandoverDate = handover.HandoverDate,
                Quantity = handover.Quantity,
                ConditionBefore = handover.ConditionBefore,
                Note = handover.Note,
                Status = handover.Status,
                ConfirmedAt = handover.ConfirmedAt,
                CreatedAt = handover.CreatedAt,
                UpdatedAt = handover.UpdatedAt
            };
        }

        private static bool IsHandoverEligible(string status)
        {
            return status == AllocationDetailStatus.Reserved.ToString() ||
                status == AllocationDetailStatus.Allocated.ToString();
        }
    }
}

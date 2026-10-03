using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.EquipmentChangeRequest;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class EquipmentChangeRequestService : IEquipmentChangeRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAllocationEquipmentDetailService _allocationEquipmentDetailService;
        private readonly EquipmentReturnService _equipmentReturnService;
        private readonly EquipmentHandoverService _equipmentHandoverService;
        private readonly IClock _clock;

        public EquipmentChangeRequestService(
            IUnitOfWork unitOfWork,
            IAllocationEquipmentDetailService allocationEquipmentDetailService,
            EquipmentReturnService equipmentReturnService,
            EquipmentHandoverService equipmentHandoverService,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _allocationEquipmentDetailService = allocationEquipmentDetailService;
            _equipmentReturnService = equipmentReturnService;
            _equipmentHandoverService = equipmentHandoverService;
            _clock = clock;
        }

        public async Task<IPaginate<EquipmentChangeRequestResponse>> ViewAllAsync(
            EquipmentChangeRequestFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<EquipmentChangeRequest>()
                .GetQueryable()
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt);

            return await query
                .Select(r => new EquipmentChangeRequestResponse
                {
                    ChangeRequestId = r.ChangeRequestId,
                    AllocationEquipmentDetailId = r.AllocationEquipmentDetailId,
                    CurrentEquipmentInstanceId = r.CurrentEquipmentInstanceId,
                    RequestedEquipmentTypeId = r.RequestedEquipmentTypeId,
                    RequestedEquipmentInstanceId = r.RequestedEquipmentInstanceId,
                    RequestedBy = r.RequestedBy,
                    Reason = r.Reason,
                    Status = r.Status,
                    ReviewedBy = r.ReviewedBy,
                    ReviewedAt = r.ReviewedAt,
                    RejectionReason = r.RejectionReason,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<EquipmentChangeRequestResponse?> GetByIdAsync(int id)
        {
            var request = await _unitOfWork
                .GetRepository<EquipmentChangeRequest>()
                .FirstOrDefaultAsync(predicate: r => r.ChangeRequestId == id);

            return request == null ? null : MapToResponse(request);
        }

        public async Task<EquipmentChangeRequestResponse?> CreateAsync(
            int userId,
            EquipmentChangeRequestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new Exception("Reason is required.");
            }

            if (!await _allocationEquipmentDetailService.UserCanAccessAllocationEquipmentDetailAsync(
                    request.AllocationEquipmentDetailId,
                    userId))
            {
                return null;
            }

            var detail = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationEquipmentDetailId == request.AllocationEquipmentDetailId,
                    include: query => query.Include(d => d.EquipmentInstance),
                    asNoTracking: false);

            if (detail == null)
            {
                return null;
            }

            EnsureChangeEligible(detail);
            await ValidateRequestedEquipmentAsync(
                request.RequestedEquipmentTypeId,
                request.RequestedEquipmentInstanceId,
                detail);
            await EnsureNoPendingChangeRequestAsync(detail.AllocationEquipmentDetailId);

            var changeRequest = new EquipmentChangeRequest
            {
                AllocationEquipmentDetailId = detail.AllocationEquipmentDetailId,
                CurrentEquipmentInstanceId = detail.EquipmentInstanceId,
                RequestedEquipmentTypeId = request.RequestedEquipmentTypeId,
                RequestedEquipmentInstanceId = request.RequestedEquipmentInstanceId,
                RequestedBy = userId,
                Reason = request.Reason.Trim(),
                Status = EquipmentChangeRequestStatus.Pending.ToString()
            };

            await _unitOfWork.GetRepository<EquipmentChangeRequest>().InsertAsync(changeRequest);
            await _unitOfWork.CommitAsync();

            return MapToResponse(changeRequest);
        }

        public async Task<EquipmentChangeRequestResponse?> ApproveAsync(int id, int managerUserId)
        {
            var request = await GetTrackedRequestAsync(id);

            if (request == null)
            {
                return null;
            }

            EnsurePending(request.Status);

            var detail = request.AllocationEquipmentDetail;
            EnsureChangeEligible(detail);

            if (detail.EquipmentInstanceId != request.CurrentEquipmentInstanceId)
            {
                throw new Exception("Current equipment instance no longer matches the change request.");
            }

            var requestedInstance = await ValidateRequestedEquipmentAsync(
                request.RequestedEquipmentTypeId,
                request.RequestedEquipmentInstanceId,
                detail);

            var returnCondition = detail.EquipmentInstance?.ConditionLevel
                ?? EquipmentConditionLevel.Good.ToString();

            await _equipmentReturnService.CreateConfirmedReturnInternalAsync(
                detail,
                returnedBy: request.RequestedBy,
                receivedBy: managerUserId,
                conditionAfter: returnCondition,
                isDamaged: false,
                damageDescription: null,
                note: $"Equipment change request #{request.ChangeRequestId}",
                completeAllocation: false);

            detail.AllocatedEquipmentTypeId = request.RequestedEquipmentTypeId;
            detail.EquipmentInstanceId = request.RequestedEquipmentInstanceId;
            detail.EquipmentInstance = requestedInstance;
            detail.Status = AllocationDetailStatus.InUse.ToString();

            await _equipmentHandoverService.CreateConfirmedHandoverInternalAsync(
                detail,
                handedOverBy: managerUserId,
                receivedBy: request.RequestedBy,
                conditionBefore: requestedInstance?.ConditionLevel,
                note: $"Equipment change request #{request.ChangeRequestId}");

            request.Status = EquipmentChangeRequestStatus.Approved.ToString();
            request.ReviewedBy = managerUserId;
            request.ReviewedAt = _clock.Now;
            request.RejectionReason = null;

            _unitOfWork.GetRepository<EquipmentChangeRequest>().Update(request);
            await _unitOfWork.CommitAsync();

            return MapToResponse(request);
        }

        public async Task<EquipmentChangeRequestResponse?> RejectAsync(
            int id,
            int managerUserId,
            EquipmentChangeRequestReviewRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                throw new Exception("Rejection reason is required.");
            }

            var changeRequest = await GetTrackedRequestAsync(id);

            if (changeRequest == null)
            {
                return null;
            }

            EnsurePending(changeRequest.Status);

            changeRequest.Status = EquipmentChangeRequestStatus.Rejected.ToString();
            changeRequest.ReviewedBy = managerUserId;
            changeRequest.ReviewedAt = _clock.Now;
            changeRequest.RejectionReason = request.RejectionReason.Trim();

            _unitOfWork.GetRepository<EquipmentChangeRequest>().Update(changeRequest);
            await _unitOfWork.CommitAsync();

            return MapToResponse(changeRequest);
        }

        private async Task<EquipmentChangeRequest?> GetTrackedRequestAsync(int id)
        {
            return await _unitOfWork
                .GetRepository<EquipmentChangeRequest>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ChangeRequestId == id,
                    include: query => query
                        .Include(r => r.AllocationEquipmentDetail)
                            .ThenInclude(d => d.EquipmentInstance)
                        .Include(r => r.RequestedEquipmentInstance),
                    asNoTracking: false);
        }

        private async Task<EquipmentInstance?> ValidateRequestedEquipmentAsync(
            int requestedEquipmentTypeId,
            int? requestedEquipmentInstanceId,
            AllocationEquipmentDetail detail)
        {
            var equipmentType = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(predicate: t => t.EquipmentTypeId == requestedEquipmentTypeId);

            if (equipmentType == null)
            {
                throw new Exception("Requested equipment type does not exist.");
            }

            var trackingType = EnumHelper.ParseEnum<EquipmentTrackingType>(
                equipmentType.TrackingType);

            if (trackingType == EquipmentTrackingType.Individual &&
                !requestedEquipmentInstanceId.HasValue)
            {
                throw new Exception("Individual equipment changes require a requested equipment instance.");
            }

            if (trackingType == EquipmentTrackingType.QuantityBased &&
                requestedEquipmentInstanceId.HasValue)
            {
                throw new Exception("Quantity-based equipment changes must not include an equipment instance.");
            }

            if (requestedEquipmentInstanceId == detail.EquipmentInstanceId)
            {
                throw new Exception("Requested equipment instance must be different from the current equipment instance.");
            }

            if (!requestedEquipmentInstanceId.HasValue)
            {
                await ValidateQuantityBasedAvailabilityAsync(
                    requestedEquipmentTypeId,
                    detail.Quantity,
                    detail.StartDate,
                    detail.EndDate,
                    detail.AllocationEquipmentDetailId);

                return null;
            }

            var instance = await _unitOfWork
                .GetRepository<EquipmentInstance>()
                .FirstOrDefaultAsync(
                    predicate: i => i.EquipmentInstanceId == requestedEquipmentInstanceId.Value,
                    asNoTracking: false);

            if (instance == null)
            {
                throw new Exception("Requested equipment instance does not exist.");
            }

            if (instance.EquipmentTypeId != requestedEquipmentTypeId)
            {
                throw new Exception("Requested equipment instance does not belong to the requested equipment type.");
            }

            if (instance.Status != EquipmentInstanceStatus.Available.ToString())
            {
                throw new Exception("Requested equipment instance is not available.");
            }

            await ValidateNoEquipmentInstanceTimeConflictAsync(
                instance.EquipmentInstanceId,
                detail.StartDate,
                detail.EndDate,
                detail.AllocationEquipmentDetailId);

            return instance;
        }

        private async Task ValidateNoEquipmentInstanceTimeConflictAsync(
            int equipmentInstanceId,
            DateTime startDate,
            DateTime endDate,
            int exceptAllocationEquipmentDetailId)
        {
            var cancelledDetailStatus = AllocationDetailStatus.Cancelled.ToString();
            var completedDetailStatus = AllocationDetailStatus.Completed.ToString();
            var rejectedPlanStatus = AllocationPlanStatus.Rejected.ToString();

            var hasConflict = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .AnyAsync(d =>
                    d.EquipmentInstanceId == equipmentInstanceId &&
                    d.AllocationEquipmentDetailId != exceptAllocationEquipmentDetailId &&
                    d.Status != cancelledDetailStatus &&
                    d.Status != completedDetailStatus &&
                    d.AllocationPlan.ApproveStatus != rejectedPlanStatus &&
                    d.StartDate < endDate &&
                    startDate < d.EndDate);

            if (hasConflict)
            {
                throw new Exception("Requested equipment instance is already allocated in the selected time range.");
            }
        }

        private async Task ValidateQuantityBasedAvailabilityAsync(
            int requestedEquipmentTypeId,
            int requestedQuantity,
            DateTime startDate,
            DateTime endDate,
            int exceptAllocationEquipmentDetailId)
        {
            var equipmentType = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(predicate: e => e.EquipmentTypeId == requestedEquipmentTypeId);

            if (equipmentType == null)
            {
                throw new Exception("Requested equipment type does not exist.");
            }

            var usableQuantity =
                equipmentType.TotalQuantity -
                equipmentType.DamagedQuantity -
                equipmentType.MissingQuantity;

            if (usableQuantity <= 0)
            {
                throw new Exception("Requested equipment type has no usable quantity.");
            }

            var cancelledDetailStatus = AllocationDetailStatus.Cancelled.ToString();
            var completedDetailStatus = AllocationDetailStatus.Completed.ToString();
            var rejectedPlanStatus = AllocationPlanStatus.Rejected.ToString();

            var overlappedQuantity = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .GetQueryable()
                .Where(d =>
                    d.AllocatedEquipmentTypeId == requestedEquipmentTypeId &&
                    d.EquipmentInstanceId == null &&
                    d.AllocationEquipmentDetailId != exceptAllocationEquipmentDetailId &&
                    d.Status != cancelledDetailStatus &&
                    d.Status != completedDetailStatus &&
                    d.AllocationPlan.ApproveStatus != rejectedPlanStatus &&
                    d.StartDate < endDate &&
                    startDate < d.EndDate)
                .SumAsync(d => d.Quantity);

            if (overlappedQuantity + requestedQuantity > usableQuantity)
            {
                throw new Exception("Not enough quantity-based equipment available in the selected time range.");
            }
        }

        private async Task EnsureNoPendingChangeRequestAsync(int allocationEquipmentDetailId)
        {
            var pendingStatus = EquipmentChangeRequestStatus.Pending.ToString();
            var hasPending = await _unitOfWork
                .GetRepository<EquipmentChangeRequest>()
                .AnyAsync(r =>
                    r.AllocationEquipmentDetailId == allocationEquipmentDetailId &&
                    r.Status == pendingStatus);

            if (hasPending)
            {
                throw new Exception("A pending change request already exists for this allocation equipment detail.");
            }
        }

        private static void EnsureChangeEligible(AllocationEquipmentDetail detail)
        {
            if (detail.Status != AllocationDetailStatus.InUse.ToString())
            {
                throw new Exception("Only in-use allocation equipment details can request equipment changes.");
            }
        }

        private static void EnsurePending(string status)
        {
            if (status != EquipmentChangeRequestStatus.Pending.ToString())
            {
                throw new Exception("Only pending change requests can be reviewed.");
            }
        }

        private static EquipmentChangeRequestResponse MapToResponse(
            EquipmentChangeRequest request)
        {
            return new EquipmentChangeRequestResponse
            {
                ChangeRequestId = request.ChangeRequestId,
                AllocationEquipmentDetailId = request.AllocationEquipmentDetailId,
                CurrentEquipmentInstanceId = request.CurrentEquipmentInstanceId,
                RequestedEquipmentTypeId = request.RequestedEquipmentTypeId,
                RequestedEquipmentInstanceId = request.RequestedEquipmentInstanceId,
                RequestedBy = request.RequestedBy,
                Reason = request.Reason,
                Status = request.Status,
                ReviewedBy = request.ReviewedBy,
                ReviewedAt = request.ReviewedAt,
                RejectionReason = request.RejectionReason,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt
            };
        }
    }
}

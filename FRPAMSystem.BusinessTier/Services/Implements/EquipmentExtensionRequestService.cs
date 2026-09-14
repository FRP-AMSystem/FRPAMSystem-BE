using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.EquipmentExtensionRequest;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class EquipmentExtensionRequestService : IEquipmentExtensionRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAllocationEquipmentDetailService _allocationEquipmentDetailService;
        private readonly IClock _clock;

        public EquipmentExtensionRequestService(
            IUnitOfWork unitOfWork,
            IAllocationEquipmentDetailService allocationEquipmentDetailService,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _allocationEquipmentDetailService = allocationEquipmentDetailService;
            _clock = clock;
        }

        public async Task<IPaginate<EquipmentExtensionRequestResponse>> ViewAllAsync(
            EquipmentExtensionRequestFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<EquipmentExtensionRequest>()
                .GetQueryable()
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt);

            return await query
                .Select(r => new EquipmentExtensionRequestResponse
                {
                    ExtensionRequestId = r.ExtensionRequestId,
                    AllocationEquipmentDetailId = r.AllocationEquipmentDetailId,
                    RequestedBy = r.RequestedBy,
                    OriginalEndDate = r.OriginalEndDate,
                    RequestedEndDate = r.RequestedEndDate,
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

        public async Task<EquipmentExtensionRequestResponse?> GetByIdAsync(int id)
        {
            var request = await _unitOfWork
                .GetRepository<EquipmentExtensionRequest>()
                .FirstOrDefaultAsync(predicate: r => r.ExtensionRequestId == id);

            return request == null ? null : MapToResponse(request);
        }

        public async Task<EquipmentExtensionRequestResponse?> CreateAsync(
            int userId,
            EquipmentExtensionRequestRequest request)
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
                    asNoTracking: false);

            if (detail == null)
            {
                return null;
            }

            EnsureExtensionEligible(detail);

            if (request.RequestedEndDate <= detail.EndDate)
            {
                throw new Exception("Requested end date must be later than the original end date.");
            }

            await EnsureNoPendingExtensionRequestAsync(detail.AllocationEquipmentDetailId);

            var extensionRequest = new EquipmentExtensionRequest
            {
                AllocationEquipmentDetailId = detail.AllocationEquipmentDetailId,
                RequestedBy = userId,
                OriginalEndDate = detail.EndDate,
                RequestedEndDate = request.RequestedEndDate,
                Reason = request.Reason.Trim(),
                Status = EquipmentExtensionRequestStatus.Pending.ToString()
            };

            await _unitOfWork.GetRepository<EquipmentExtensionRequest>().InsertAsync(extensionRequest);
            await _unitOfWork.CommitAsync();

            return MapToResponse(extensionRequest);
        }

        public async Task<EquipmentExtensionRequestResponse?> ApproveAsync(int id, int managerUserId)
        {
            var request = await GetTrackedRequestAsync(id);

            if (request == null)
            {
                return null;
            }

            EnsurePending(request.Status);
            EnsureExtensionEligible(request.AllocationEquipmentDetail);

            request.AllocationEquipmentDetail.EndDate = request.RequestedEndDate;
            request.Status = EquipmentExtensionRequestStatus.Approved.ToString();
            request.ReviewedBy = managerUserId;
            request.ReviewedAt = _clock.Now;
            request.RejectionReason = null;

            _unitOfWork.GetRepository<AllocationEquipmentDetail>().Update(request.AllocationEquipmentDetail);
            _unitOfWork.GetRepository<EquipmentExtensionRequest>().Update(request);
            await _unitOfWork.CommitAsync();

            return MapToResponse(request);
        }

        public async Task<EquipmentExtensionRequestResponse?> RejectAsync(
            int id,
            int managerUserId,
            EquipmentExtensionRequestReviewRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                throw new Exception("Rejection reason is required.");
            }

            var extensionRequest = await GetTrackedRequestAsync(id);

            if (extensionRequest == null)
            {
                return null;
            }

            EnsurePending(extensionRequest.Status);

            extensionRequest.Status = EquipmentExtensionRequestStatus.Rejected.ToString();
            extensionRequest.ReviewedBy = managerUserId;
            extensionRequest.ReviewedAt = _clock.Now;
            extensionRequest.RejectionReason = request.RejectionReason.Trim();

            _unitOfWork.GetRepository<EquipmentExtensionRequest>().Update(extensionRequest);
            await _unitOfWork.CommitAsync();

            return MapToResponse(extensionRequest);
        }

        private async Task<EquipmentExtensionRequest?> GetTrackedRequestAsync(int id)
        {
            return await _unitOfWork
                .GetRepository<EquipmentExtensionRequest>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExtensionRequestId == id,
                    include: query => query.Include(r => r.AllocationEquipmentDetail),
                    asNoTracking: false);
        }

        private async Task EnsureNoPendingExtensionRequestAsync(int allocationEquipmentDetailId)
        {
            var pendingStatus = EquipmentExtensionRequestStatus.Pending.ToString();
            var hasPending = await _unitOfWork
                .GetRepository<EquipmentExtensionRequest>()
                .AnyAsync(r =>
                    r.AllocationEquipmentDetailId == allocationEquipmentDetailId &&
                    r.Status == pendingStatus);

            if (hasPending)
            {
                throw new Exception("A pending extension request already exists for this allocation equipment detail.");
            }
        }

        private static void EnsureExtensionEligible(AllocationEquipmentDetail detail)
        {
            if (detail.Status != AllocationDetailStatus.InUse.ToString())
            {
                throw new Exception("Only in-use allocation equipment details can be extended.");
            }
        }

        private static void EnsurePending(string status)
        {
            if (status != EquipmentExtensionRequestStatus.Pending.ToString())
            {
                throw new Exception("Only pending extension requests can be reviewed.");
            }
        }

        private static EquipmentExtensionRequestResponse MapToResponse(
            EquipmentExtensionRequest request)
        {
            return new EquipmentExtensionRequestResponse
            {
                ExtensionRequestId = request.ExtensionRequestId,
                AllocationEquipmentDetailId = request.AllocationEquipmentDetailId,
                RequestedBy = request.RequestedBy,
                OriginalEndDate = request.OriginalEndDate,
                RequestedEndDate = request.RequestedEndDate,
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

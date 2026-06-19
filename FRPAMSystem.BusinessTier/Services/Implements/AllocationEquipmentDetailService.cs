using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.AllocationEquipmentDetail;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class AllocationEquipmentDetailService : IAllocationEquipmentDetailService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AllocationEquipmentDetailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<AllocationEquipmentDetailResponse>> ViewAllAllocationEquipmentDetailsAsync(
            AllocationEquipmentDetailFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .GetQueryable()
                .Include(d => d.AllocationPlan)
                    .ThenInclude(p => p.Experiment)
                .Include(d => d.ExpEquipmentReq)
                    .ThenInclude(r => r.EquipmentType)
                .Include(d => d.PhaseEquipmentReq)
                    .ThenInclude(r => r.EquipmentType)
                .Include(d => d.PhaseEquipmentReq)
                    .ThenInclude(r => r.Phase)
                .Include(d => d.AllocatedEquipmentType)
                    .ThenInclude(t => t.EquipmentCategory)
                .Include(d => d.EquipmentInstance)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(d => d.CreatedAt);

            return await query
                .Select(d => new AllocationEquipmentDetailResponse
                {
                    AllocationEquipmentDetailId = d.AllocationEquipmentDetailId,
                    AllocationPlanId = d.AllocationPlanId,
                    ExperimentId = d.AllocationPlan.ExperimentId,
                    ExperimentName = d.AllocationPlan.Experiment.ExperimentName,

                    ExpEquipmentReqId = d.ExpEquipmentReqId,
                    PhaseEquipmentReqId = d.PhaseEquipmentReqId,
                    PhaseId = d.PhaseEquipmentReq != null ? d.PhaseEquipmentReq.PhaseId : null,
                    PhaseName = d.PhaseEquipmentReq != null ? d.PhaseEquipmentReq.Phase.PhaseName : null,

                    RequestedEquipmentTypeId = d.ExpEquipmentReq != null
                        ? d.ExpEquipmentReq.EquipmentTypeId
                        : d.PhaseEquipmentReq != null
                            ? d.PhaseEquipmentReq.EquipmentTypeId
                            : null,

                    RequestedEquipmentTypeName = d.ExpEquipmentReq != null
                        ? d.ExpEquipmentReq.EquipmentType.Name
                        : d.PhaseEquipmentReq != null
                            ? d.PhaseEquipmentReq.EquipmentType.Name
                            : null,

                    AllocatedEquipmentTypeId = d.AllocatedEquipmentTypeId,
                    AllocatedEquipmentTypeName = d.AllocatedEquipmentType.Name,
                    TrackingType = d.AllocatedEquipmentType.TrackingType,

                    EquipmentInstanceId = d.EquipmentInstanceId,
                    AssetCode = d.EquipmentInstance != null ? d.EquipmentInstance.AssetCode : null,
                    SerialNumber = d.EquipmentInstance != null ? d.EquipmentInstance.SerialNumber : null,

                    Quantity = d.Quantity,
                    IsSubstitute = d.IsSubstitute,
                    EfficiencyRate = d.EfficiencyRate,
                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<AllocationEquipmentDetailResponse?> GetAllocationEquipmentDetailByIdAsync(int id)
        {
            var detail = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationEquipmentDetailId == id,
                    include: query => query
                        .Include(d => d.AllocationPlan)
                            .ThenInclude(p => p.Experiment)
                        .Include(d => d.ExpEquipmentReq)
                            .ThenInclude(r => r.EquipmentType)
                        .Include(d => d.PhaseEquipmentReq)
                            .ThenInclude(r => r.EquipmentType)
                        .Include(d => d.PhaseEquipmentReq)
                            .ThenInclude(r => r.Phase)
                        .Include(d => d.AllocatedEquipmentType)
                            .ThenInclude(t => t.EquipmentCategory)
                        .Include(d => d.EquipmentInstance)
                );

            if (detail == null)
            {
                return null;
            }

            return MapToResponse(detail);
        }

        public async Task<AllocationEquipmentDetailResponse> CreateAllocationEquipmentDetailAsync(
            AllocationEquipmentDetailRequest request)
        {
            ValidateRequest(request);

            var allocationPlan = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == request.AllocationPlanId,
                    include: query => query.Include(p => p.Experiment)
                );

            if (allocationPlan == null)
            {
                throw new Exception("Allocation plan does not exist.");
            }

            ValidateAllocationPlanCanBeModified(allocationPlan);

            var requirementInfo = await GetAndValidateRequirementInfoAsync(
                allocationPlan,
                request.ExpEquipmentReqId,
                request.PhaseEquipmentReqId);

            var allocatedEquipmentType = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentTypeId == request.AllocatedEquipmentTypeId,
                    include: query => query.Include(e => e.EquipmentCategory)
                );

            if (allocatedEquipmentType == null)
            {
                throw new Exception("Allocated equipment type does not exist.");
            }

            ValidateSubstitution(
                request,
                requirementInfo.RequestedEquipmentTypeId,
                requirementInfo.AllowSubstitute,
                requirementInfo.MinAcceptableEfficiency);

            var trackingType = EnumHelper.ParseEnum<EquipmentTrackingType>(
                allocatedEquipmentType.TrackingType);

            if (trackingType == EquipmentTrackingType.QuantityBased)
            {
                ValidateQuantityBasedRequest(request);

                await ValidateQuantityBasedAvailabilityAsync(
                    request.AllocatedEquipmentTypeId,
                    request.Quantity,
                    request.StartDate,
                    request.EndDate,
                    exceptAllocationEquipmentDetailId: null);
            }
            else
            {
                await ValidateIndividualEquipmentRequestAsync(
                    request,
                    allocatedEquipmentType,
                    exceptAllocationEquipmentDetailId: null);
            }

            var detail = new AllocationEquipmentDetail
            {
                AllocationPlanId = request.AllocationPlanId,
                ExpEquipmentReqId = request.ExpEquipmentReqId,
                PhaseEquipmentReqId = request.PhaseEquipmentReqId,
                AllocatedEquipmentTypeId = request.AllocatedEquipmentTypeId,
                EquipmentInstanceId = request.EquipmentInstanceId,
                Quantity = request.Quantity,
                IsSubstitute = request.IsSubstitute,
                EfficiencyRate = request.EfficiencyRate,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status.ToString(),
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<AllocationEquipmentDetail>()
                .InsertAsync(detail);

            await _unitOfWork.CommitAsync();

            var created = await GetDetailWithIncludesAsync(detail.AllocationEquipmentDetailId);

            return MapToResponse(created!);
        }

        public async Task<AllocationEquipmentDetailResponse?> UpdateAllocationEquipmentDetailAsync(
            int id,
            AllocationEquipmentDetailRequest request)
        {
            ValidateRequest(request);

            var detail = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationEquipmentDetailId == id,
                    include: query => query.Include(d => d.AllocationPlan),
                    asNoTracking: false
                );

            if (detail == null)
            {
                return null;
            }

            ValidateAllocationPlanCanBeModified(detail.AllocationPlan);

            var allocationPlan = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == request.AllocationPlanId,
                    include: query => query.Include(p => p.Experiment)
                );

            if (allocationPlan == null)
            {
                throw new Exception("Allocation plan does not exist.");
            }

            ValidateAllocationPlanCanBeModified(allocationPlan);

            var requirementInfo = await GetAndValidateRequirementInfoAsync(
                allocationPlan,
                request.ExpEquipmentReqId,
                request.PhaseEquipmentReqId);

            var allocatedEquipmentType = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentTypeId == request.AllocatedEquipmentTypeId,
                    include: query => query.Include(e => e.EquipmentCategory)
                );

            if (allocatedEquipmentType == null)
            {
                throw new Exception("Allocated equipment type does not exist.");
            }

            ValidateSubstitution(
                request,
                requirementInfo.RequestedEquipmentTypeId,
                requirementInfo.AllowSubstitute,
                requirementInfo.MinAcceptableEfficiency);

            var trackingType = EnumHelper.ParseEnum<EquipmentTrackingType>(
                allocatedEquipmentType.TrackingType);

            if (trackingType == EquipmentTrackingType.QuantityBased)
            {
                ValidateQuantityBasedRequest(request);

                await ValidateQuantityBasedAvailabilityAsync(
                    request.AllocatedEquipmentTypeId,
                    request.Quantity,
                    request.StartDate,
                    request.EndDate,
                    exceptAllocationEquipmentDetailId: id);
            }
            else
            {
                await ValidateIndividualEquipmentRequestAsync(
                    request,
                    allocatedEquipmentType,
                    exceptAllocationEquipmentDetailId: id);
            }

            detail.AllocationPlanId = request.AllocationPlanId;
            detail.ExpEquipmentReqId = request.ExpEquipmentReqId;
            detail.PhaseEquipmentReqId = request.PhaseEquipmentReqId;
            detail.AllocatedEquipmentTypeId = request.AllocatedEquipmentTypeId;
            detail.EquipmentInstanceId = request.EquipmentInstanceId;
            detail.Quantity = request.Quantity;
            detail.IsSubstitute = request.IsSubstitute;
            detail.EfficiencyRate = request.EfficiencyRate;
            detail.StartDate = request.StartDate;
            detail.EndDate = request.EndDate;
            detail.Status = request.Status.ToString();
            detail.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<AllocationEquipmentDetail>()
                .Update(detail);

            await _unitOfWork.CommitAsync();

            var updated = await GetDetailWithIncludesAsync(id);

            return MapToResponse(updated!);
        }

        public async Task<bool> DeleteAllocationEquipmentDetailAsync(int id)
        {
            var detail = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationEquipmentDetailId == id,
                    include: query => query.Include(d => d.AllocationPlan),
                    asNoTracking: false
                );

            if (detail == null)
            {
                return false;
            }

            ValidateAllocationPlanCanBeModified(detail.AllocationPlan);

            var currentStatus = EnumHelper.ParseEnum<AllocationDetailStatus>(
                detail.Status);

            if (currentStatus == AllocationDetailStatus.InUse ||
                currentStatus == AllocationDetailStatus.Completed)
            {
                throw new Exception("In-use or completed allocation detail cannot be deleted.");
            }

            _unitOfWork.GetRepository<AllocationEquipmentDetail>()
                .Delete(detail);

            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task<AllocationEquipmentDetail?> GetDetailWithIncludesAsync(int id)
        {
            return await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationEquipmentDetailId == id,
                    include: query => query
                        .Include(d => d.AllocationPlan)
                            .ThenInclude(p => p.Experiment)
                        .Include(d => d.ExpEquipmentReq)
                            .ThenInclude(r => r.EquipmentType)
                        .Include(d => d.PhaseEquipmentReq)
                            .ThenInclude(r => r.EquipmentType)
                        .Include(d => d.PhaseEquipmentReq)
                            .ThenInclude(r => r.Phase)
                        .Include(d => d.AllocatedEquipmentType)
                            .ThenInclude(t => t.EquipmentCategory)
                        .Include(d => d.EquipmentInstance)
                );
        }

        private static AllocationEquipmentDetailResponse MapToResponse(
            AllocationEquipmentDetail detail)
        {
            var requestedEquipmentTypeId = detail.ExpEquipmentReq != null
                ? detail.ExpEquipmentReq.EquipmentTypeId
                : detail.PhaseEquipmentReq?.EquipmentTypeId;

            var requestedEquipmentTypeName = detail.ExpEquipmentReq != null
                ? detail.ExpEquipmentReq.EquipmentType?.Name
                : detail.PhaseEquipmentReq?.EquipmentType?.Name;

            return new AllocationEquipmentDetailResponse
            {
                AllocationEquipmentDetailId = detail.AllocationEquipmentDetailId,
                AllocationPlanId = detail.AllocationPlanId,
                ExperimentId = detail.AllocationPlan?.ExperimentId ?? 0,
                ExperimentName = detail.AllocationPlan?.Experiment?.ExperimentName,

                ExpEquipmentReqId = detail.ExpEquipmentReqId,
                PhaseEquipmentReqId = detail.PhaseEquipmentReqId,
                PhaseId = detail.PhaseEquipmentReq?.PhaseId,
                PhaseName = detail.PhaseEquipmentReq?.Phase?.PhaseName,

                RequestedEquipmentTypeId = requestedEquipmentTypeId,
                RequestedEquipmentTypeName = requestedEquipmentTypeName,

                AllocatedEquipmentTypeId = detail.AllocatedEquipmentTypeId,
                AllocatedEquipmentTypeName = detail.AllocatedEquipmentType?.Name,
                TrackingType = detail.AllocatedEquipmentType?.TrackingType,

                EquipmentInstanceId = detail.EquipmentInstanceId,
                AssetCode = detail.EquipmentInstance?.AssetCode,
                SerialNumber = detail.EquipmentInstance?.SerialNumber,

                Quantity = detail.Quantity,
                IsSubstitute = detail.IsSubstitute,
                EfficiencyRate = detail.EfficiencyRate,
                StartDate = detail.StartDate,
                EndDate = detail.EndDate,
                Status = detail.Status,
                CreatedAt = detail.CreatedAt,
                UpdatedAt = detail.UpdatedAt
            };
        }

        private static void ValidateRequest(AllocationEquipmentDetailRequest request)
        {
            if (request.AllocationPlanId <= 0)
            {
                throw new Exception("AllocationPlanId is required.");
            }

            if (!request.ExpEquipmentReqId.HasValue &&
                !request.PhaseEquipmentReqId.HasValue)
            {
                throw new Exception("ExpEquipmentReqId or PhaseEquipmentReqId is required.");
            }

            if (request.ExpEquipmentReqId.HasValue &&
                request.PhaseEquipmentReqId.HasValue)
            {
                throw new Exception("Only one of ExpEquipmentReqId or PhaseEquipmentReqId can be provided.");
            }

            if (request.AllocatedEquipmentTypeId <= 0)
            {
                throw new Exception("AllocatedEquipmentTypeId is required.");
            }

            if (request.Quantity <= 0)
            {
                throw new Exception("Quantity must be greater than 0.");
            }

            if (request.EfficiencyRate <= 0 || request.EfficiencyRate > 1)
            {
                throw new Exception("Efficiency rate must be greater than 0 and less than or equal to 1.");
            }

            if (request.StartDate >= request.EndDate)
            {
                throw new Exception("Start date must be earlier than end date.");
            }
        }

        private static void ValidateAllocationPlanCanBeModified(
            AllocationPlan allocationPlan)
        {
            var status = EnumHelper.ParseEnum<AllocationPlanStatus>(
                allocationPlan.ApproveStatus);

            if (status == AllocationPlanStatus.Approved)
            {
                throw new Exception("Approved allocation plan cannot be modified.");
            }

            if (status == AllocationPlanStatus.Cancelled)
            {
                throw new Exception("Cancelled allocation plan cannot be modified.");
            }
        }

        private async Task<RequirementInfo> GetAndValidateRequirementInfoAsync(
            AllocationPlan allocationPlan,
            int? expEquipmentReqId,
            int? phaseEquipmentReqId)
        {
            if (expEquipmentReqId.HasValue)
            {
                var requirement = await _unitOfWork
                    .GetRepository<ExperimentEquipmentRequirement>()
                    .FirstOrDefaultAsync(
                        predicate: r => r.ExpEquipmentReqId == expEquipmentReqId.Value,
                        include: query => query.Include(r => r.EquipmentType)
                    );

                if (requirement == null)
                {
                    throw new Exception("Experiment equipment requirement does not exist.");
                }

                if (requirement.ExperimentId != allocationPlan.ExperimentId)
                {
                    throw new Exception(
                        "Equipment requirement does not belong to the experiment of this allocation plan.");
                }

                return new RequirementInfo
                {
                    RequestedEquipmentTypeId = requirement.EquipmentTypeId,
                    RequiredQuantity = requirement.Quantity,
                    AllowSubstitute = requirement.AllowSubstitute,
                    MinAcceptableEfficiency = requirement.MinAcceptableEfficiency
                };
            }

            var phaseRequirement = await _unitOfWork
                .GetRepository<PhaseEquipmentRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.PhaseEquipmentReqId == phaseEquipmentReqId!.Value,
                    include: query => query
                        .Include(r => r.EquipmentType)
                        .Include(r => r.Phase)
                );

            if (phaseRequirement == null)
            {
                throw new Exception("Phase equipment requirement does not exist.");
            }

            if (phaseRequirement.Phase.ExperimentId != allocationPlan.ExperimentId)
            {
                throw new Exception(
                    "Phase equipment requirement does not belong to the experiment of this allocation plan.");
            }

            return new RequirementInfo
            {
                RequestedEquipmentTypeId = phaseRequirement.EquipmentTypeId,
                RequiredQuantity = phaseRequirement.Quantity,
                AllowSubstitute = true,
                MinAcceptableEfficiency = null
            };
        }

        private static void ValidateSubstitution(
            AllocationEquipmentDetailRequest request,
            int requestedEquipmentTypeId,
            bool allowSubstitute,
            double? minAcceptableEfficiency)
        {
            var isDifferentType =
                request.AllocatedEquipmentTypeId != requestedEquipmentTypeId;

            if (isDifferentType && !request.IsSubstitute)
            {
                throw new Exception(
                    "Allocated equipment type is different from requested equipment type, so IsSubstitute must be true.");
            }

            if (!isDifferentType && request.IsSubstitute)
            {
                throw new Exception(
                    "IsSubstitute should be false when allocated equipment type is the same as requested equipment type.");
            }

            if (request.IsSubstitute && !allowSubstitute)
            {
                throw new Exception("This requirement does not allow substitute equipment.");
            }

            if (request.IsSubstitute &&
                minAcceptableEfficiency.HasValue &&
                request.EfficiencyRate < minAcceptableEfficiency.Value)
            {
                throw new Exception(
                    "Efficiency rate is lower than the minimum acceptable efficiency.");
            }
        }

        private static void ValidateQuantityBasedRequest(
            AllocationEquipmentDetailRequest request)
        {
            if (request.EquipmentInstanceId.HasValue)
            {
                throw new Exception("Quantity-based equipment must not have EquipmentInstanceId.");
            }
        }

        private async Task ValidateIndividualEquipmentRequestAsync(
            AllocationEquipmentDetailRequest request,
            EquipmentType allocatedEquipmentType,
            int? exceptAllocationEquipmentDetailId)
        {
            if (!request.EquipmentInstanceId.HasValue)
            {
                throw new Exception("Individual equipment must have EquipmentInstanceId.");
            }

            if (request.Quantity != 1)
            {
                throw new Exception("Individual equipment allocation quantity must be 1.");
            }

            var equipmentInstance = await _unitOfWork
                .GetRepository<EquipmentInstance>()
                .FirstOrDefaultAsync(
                    predicate: i => i.EquipmentInstanceId == request.EquipmentInstanceId.Value
                );

            if (equipmentInstance == null)
            {
                throw new Exception("Equipment instance does not exist.");
            }

            if (equipmentInstance.EquipmentTypeId != allocatedEquipmentType.EquipmentTypeId)
            {
                throw new Exception(
                    "Equipment instance does not belong to the allocated equipment type.");
            }

            var instanceStatus = EnumHelper.ParseEnum<EquipmentInstanceStatus>(
                equipmentInstance.Status);

            if (instanceStatus == EquipmentInstanceStatus.Maintenance ||
                instanceStatus == EquipmentInstanceStatus.Damaged ||
                instanceStatus == EquipmentInstanceStatus.Missing)
            {
                throw new Exception(
                    "Equipment instance is not available for allocation.");
            }

            await ValidateNoEquipmentInstanceTimeConflictAsync(
                request.EquipmentInstanceId.Value,
                request.StartDate,
                request.EndDate,
                exceptAllocationEquipmentDetailId);
        }

        private async Task ValidateQuantityBasedAvailabilityAsync(
            int allocatedEquipmentTypeId,
            int requestedQuantity,
            DateTime startDate,
            DateTime endDate,
            int? exceptAllocationEquipmentDetailId)
        {
            var equipmentType = await _unitOfWork
                .GetRepository<EquipmentType>()
                .FirstOrDefaultAsync(
                    predicate: e => e.EquipmentTypeId == allocatedEquipmentTypeId
                );

            if (equipmentType == null)
            {
                throw new Exception("Equipment type does not exist.");
            }

            var usableQuantity =
                equipmentType.TotalQuantity -
                equipmentType.DamagedQuantity -
                equipmentType.MissingQuantity;

            if (usableQuantity <= 0)
            {
                throw new Exception("Equipment type has no usable quantity.");
            }

            var cancelledDetailStatus = AllocationDetailStatus.Cancelled.ToString();
            var completedDetailStatus = AllocationDetailStatus.Completed.ToString();
            var rejectedPlanStatus = AllocationPlanStatus.Rejected.ToString();
            var cancelledPlanStatus = AllocationPlanStatus.Cancelled.ToString();

            var overlappedQuantity = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .GetQueryable()
                .Where(d =>
                    d.AllocatedEquipmentTypeId == allocatedEquipmentTypeId &&
                    d.EquipmentInstanceId == null &&
                    (!exceptAllocationEquipmentDetailId.HasValue ||
                        d.AllocationEquipmentDetailId != exceptAllocationEquipmentDetailId.Value) &&
                    d.Status != cancelledDetailStatus &&
                    d.Status != completedDetailStatus &&
                    d.AllocationPlan.ApproveStatus != rejectedPlanStatus &&
                    d.AllocationPlan.ApproveStatus != cancelledPlanStatus &&
                    d.StartDate < endDate &&
                    startDate < d.EndDate)
                .SumAsync(d => d.Quantity);

            if (overlappedQuantity + requestedQuantity > usableQuantity)
            {
                throw new Exception(
                    "Not enough quantity-based equipment available in the selected time range.");
            }
        }

        private async Task ValidateNoEquipmentInstanceTimeConflictAsync(
            int equipmentInstanceId,
            DateTime startDate,
            DateTime endDate,
            int? exceptAllocationEquipmentDetailId)
        {
            var cancelledDetailStatus = AllocationDetailStatus.Cancelled.ToString();
            var completedDetailStatus = AllocationDetailStatus.Completed.ToString();
            var rejectedPlanStatus = AllocationPlanStatus.Rejected.ToString();
            var cancelledPlanStatus = AllocationPlanStatus.Cancelled.ToString();

            var hasConflict = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .AnyAsync(d =>
                    d.EquipmentInstanceId == equipmentInstanceId &&
                    (!exceptAllocationEquipmentDetailId.HasValue ||
                        d.AllocationEquipmentDetailId != exceptAllocationEquipmentDetailId.Value) &&
                    d.Status != cancelledDetailStatus &&
                    d.Status != completedDetailStatus &&
                    d.AllocationPlan.ApproveStatus != rejectedPlanStatus &&
                    d.AllocationPlan.ApproveStatus != cancelledPlanStatus &&
                    d.StartDate < endDate &&
                    startDate < d.EndDate);

            if (hasConflict)
            {
                throw new Exception(
                    "Equipment instance is already allocated in the selected time range.");
            }
        }

        private class RequirementInfo
        {
            public int RequestedEquipmentTypeId { get; set; }

            public int RequiredQuantity { get; set; }

            public bool AllowSubstitute { get; set; }

            public double? MinAcceptableEfficiency { get; set; }
        }
    }
}

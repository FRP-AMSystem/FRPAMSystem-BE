using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.AllocationLandDetail;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class AllocationLandDetailService : IAllocationLandDetailService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AllocationLandDetailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<AllocationLandDetailResponse>> ViewAllAllocationLandDetailsAsync(
            AllocationLandDetailFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<AllocationLandDetail>()
                .GetQueryable()
                .Include(d => d.AllocationPlan)
                    .ThenInclude(p => p.Experiment)
                .Include(d => d.Land)
                    .ThenInclude(l => l.Area)
                .Include(d => d.ExpLandReq)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(d => d.CreatedAt);

            return await query
                .Select(d => new AllocationLandDetailResponse
                {
                    AllocationLandDetailId = d.AllocationLandDetailId,
                    AllocationPlanId = d.AllocationPlanId,
                    ExperimentId = d.AllocationPlan.ExperimentId,
                    ExperimentName = d.AllocationPlan.Experiment.ExperimentName,

                    LandId = d.LandId,
                    LandCode = d.Land.LandCode,
                    AreaId = d.Land.AreaId,
                    AreaName = d.Land.Area.AreaName,
                    AreaSize = d.Land.AreaSize,
                    Location = d.Land.Location,
                    SoilType = d.Land.SoilType,

                    ExpLandReqId = d.ExpLandReqId,
                    RequiredArea = d.ExpLandReq.RequiredArea,
                    RequiredSoilType = d.ExpLandReq.RequiredSoilType,
                    RequirementNote = d.ExpLandReq.Note,

                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<AllocationLandDetailResponse?> GetAllocationLandDetailByIdAsync(int id)
        {
            var detail = await _unitOfWork
                .GetRepository<AllocationLandDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationLandDetailId == id,
                    include: query => query
                        .Include(d => d.AllocationPlan)
                            .ThenInclude(p => p.Experiment)
                        .Include(d => d.Land)
                            .ThenInclude(l => l.Area)
                        .Include(d => d.ExpLandReq)
                );

            if (detail == null)
            {
                return null;
            }

            return MapToResponse(detail);
        }

        public async Task<AllocationLandDetailResponse> CreateAllocationLandDetailAsync(
            AllocationLandDetailRequest request)
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

            var land = await _unitOfWork
                .GetRepository<LandResource>()
                .FirstOrDefaultAsync(
                    predicate: l => l.LandId == request.LandId,
                    include: query => query.Include(l => l.Area)
                );

            if (land == null)
            {
                throw new Exception("Land resource does not exist.");
            }

            var landRequirement = await _unitOfWork
                .GetRepository<ExperimentLandRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExpLandReqId == request.ExpLandReqId
                );

            if (landRequirement == null)
            {
                throw new Exception("Experiment land requirement does not exist.");
            }

            ValidateRequirementBelongsToPlanExperiment(allocationPlan, landRequirement);
            ValidateLandMatchesRequirement(land, landRequirement);

            await ValidateNoLandTimeConflictAsync(
                request.LandId,
                request.StartDate,
                request.EndDate,
                exceptAllocationLandDetailId: null);

            var detail = new AllocationLandDetail
            {
                AllocationPlanId = request.AllocationPlanId,
                LandId = request.LandId,
                ExpLandReqId = request.ExpLandReqId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status.ToString(),
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<AllocationLandDetail>()
                .InsertAsync(detail);

            await _unitOfWork.CommitAsync();

            var created = await _unitOfWork
                .GetRepository<AllocationLandDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationLandDetailId == detail.AllocationLandDetailId,
                    include: query => query
                        .Include(d => d.AllocationPlan)
                            .ThenInclude(p => p.Experiment)
                        .Include(d => d.Land)
                            .ThenInclude(l => l.Area)
                        .Include(d => d.ExpLandReq)
                );

            return MapToResponse(created!);
        }

        public async Task<AllocationLandDetailResponse?> UpdateAllocationLandDetailAsync(
            int id,
            AllocationLandDetailRequest request)
        {
            ValidateRequest(request);

            var detail = await _unitOfWork
                .GetRepository<AllocationLandDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationLandDetailId == id,
                    asNoTracking: false
                );

            if (detail == null)
            {
                return null;
            }

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

            var land = await _unitOfWork
                .GetRepository<LandResource>()
                .FirstOrDefaultAsync(
                    predicate: l => l.LandId == request.LandId,
                    include: query => query.Include(l => l.Area)
                );

            if (land == null)
            {
                throw new Exception("Land resource does not exist.");
            }

            var landRequirement = await _unitOfWork
                .GetRepository<ExperimentLandRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExpLandReqId == request.ExpLandReqId
                );

            if (landRequirement == null)
            {
                throw new Exception("Experiment land requirement does not exist.");
            }

            ValidateRequirementBelongsToPlanExperiment(allocationPlan, landRequirement);
            ValidateLandMatchesRequirement(land, landRequirement);

            await ValidateNoLandTimeConflictAsync(
                request.LandId,
                request.StartDate,
                request.EndDate,
                exceptAllocationLandDetailId: id);

            detail.AllocationPlanId = request.AllocationPlanId;
            detail.LandId = request.LandId;
            detail.ExpLandReqId = request.ExpLandReqId;
            detail.StartDate = request.StartDate;
            detail.EndDate = request.EndDate;
            detail.Status = request.Status.ToString();
            detail.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<AllocationLandDetail>()
                .Update(detail);

            await _unitOfWork.CommitAsync();

            var updated = await _unitOfWork
                .GetRepository<AllocationLandDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationLandDetailId == id,
                    include: query => query
                        .Include(d => d.AllocationPlan)
                            .ThenInclude(p => p.Experiment)
                        .Include(d => d.Land)
                            .ThenInclude(l => l.Area)
                        .Include(d => d.ExpLandReq)
                );

            return MapToResponse(updated!);
        }

        public async Task<bool> DeleteAllocationLandDetailAsync(int id)
        {
            var detail = await _unitOfWork
                .GetRepository<AllocationLandDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationLandDetailId == id,
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

            _unitOfWork.GetRepository<AllocationLandDetail>()
                .Delete(detail);

            await _unitOfWork.CommitAsync();

            return true;
        }

        private static AllocationLandDetailResponse MapToResponse(
            AllocationLandDetail detail)
        {
            return new AllocationLandDetailResponse
            {
                AllocationLandDetailId = detail.AllocationLandDetailId,
                AllocationPlanId = detail.AllocationPlanId,
                ExperimentId = detail.AllocationPlan?.ExperimentId ?? 0,
                ExperimentName = detail.AllocationPlan?.Experiment?.ExperimentName,

                LandId = detail.LandId,
                LandCode = detail.Land?.LandCode,
                AreaId = detail.Land?.AreaId,
                AreaName = detail.Land?.Area?.AreaName,
                AreaSize = detail.Land?.AreaSize,
                Location = detail.Land?.Location,
                SoilType = detail.Land?.SoilType,

                ExpLandReqId = detail.ExpLandReqId,
                RequiredArea = detail.ExpLandReq?.RequiredArea,
                RequiredSoilType = detail.ExpLandReq?.RequiredSoilType,
                RequirementNote = detail.ExpLandReq?.Note,

                StartDate = detail.StartDate,
                EndDate = detail.EndDate,
                Status = detail.Status,
                CreatedAt = detail.CreatedAt,
                UpdatedAt = detail.UpdatedAt
            };
        }

        private static void ValidateRequest(AllocationLandDetailRequest request)
        {
            if (request.AllocationPlanId <= 0)
            {
                throw new Exception("AllocationPlanId is required.");
            }

            if (request.LandId <= 0)
            {
                throw new Exception("LandId is required.");
            }

            if (request.ExpLandReqId <= 0)
            {
                throw new Exception("ExpLandReqId is required.");
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

        }

        private static void ValidateRequirementBelongsToPlanExperiment(
            AllocationPlan allocationPlan,
            ExperimentLandRequirement landRequirement)
        {
            if (landRequirement.ExperimentId != allocationPlan.ExperimentId)
            {
                throw new Exception(
                    "Land requirement does not belong to the experiment of this allocation plan.");
            }
        }

        private static void ValidateLandMatchesRequirement(
            LandResource land,
            ExperimentLandRequirement landRequirement)
        {
            if (land.AreaSize < landRequirement.RequiredArea)
            {
                throw new Exception(
                    "Selected land area size is smaller than required area.");
            }

            var requiredSoilType = landRequirement.RequiredSoilType?.Trim();
            var landSoilType = land.SoilType?.Trim();

            if (!string.IsNullOrWhiteSpace(requiredSoilType) &&
                !requiredSoilType.Equals("Any", StringComparison.OrdinalIgnoreCase) &&
                !requiredSoilType.Equals(landSoilType, StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception(
                    "Selected land soil type does not match required soil type.");
            }
        }

        private async Task ValidateNoLandTimeConflictAsync(
            int landId,
            DateTime startDate,
            DateTime endDate,
            int? exceptAllocationLandDetailId)
        {
            var cancelledDetailStatus = AllocationDetailStatus.Cancelled.ToString();
            var completedDetailStatus = AllocationDetailStatus.Completed.ToString();
            var rejectedPlanStatus = AllocationPlanStatus.Rejected.ToString();

            var hasConflict = await _unitOfWork
                .GetRepository<AllocationLandDetail>()
                .AnyAsync(d =>
                    d.LandId == landId &&
                    (!exceptAllocationLandDetailId.HasValue ||
                        d.AllocationLandDetailId != exceptAllocationLandDetailId.Value) &&
                    d.Status != cancelledDetailStatus &&
                    d.Status != completedDetailStatus &&
                    d.AllocationPlan.ApproveStatus != rejectedPlanStatus &&
                    d.StartDate < endDate &&
                    startDate < d.EndDate);

            if (hasConflict)
            {
                throw new Exception(
                    "Selected land is already allocated in the selected time range.");
            }
        }
    }
}

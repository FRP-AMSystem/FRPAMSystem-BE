using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.AllocationHumanDetail;
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
    public class AllocationHumanDetailService : IAllocationHumanDetailService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AllocationHumanDetailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<AllocationHumanDetailResponse>> ViewAllAllocationHumanDetailsAsync(
            AllocationHumanDetailFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<AllocationHumanDetail>()
                .GetQueryable()
                .Include(d => d.AllocationPlan)
                    .ThenInclude(p => p.Experiment)
                .Include(d => d.HumanResource)
                    .ThenInclude(h => h.User)
                        .ThenInclude(u => u.Role)
                .Include(d => d.ExpHumanReq)
                    .ThenInclude(r => r.Role)
                .Include(d => d.ExpHumanReq)
                    .ThenInclude(r => r.RequiredSkill)
                .Include(d => d.PhaseHumanReq)
                    .ThenInclude(r => r.Role)
                .Include(d => d.PhaseHumanReq)
                    .ThenInclude(r => r.RequiredSkill)
                .Include(d => d.PhaseHumanReq)
                    .ThenInclude(r => r.Phase)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(d => d.CreatedAt);

            return await query
                .Select(d => new AllocationHumanDetailResponse
                {
                    AllocationHumanDetailId = d.AllocationHumanDetailId,
                    AllocationPlanId = d.AllocationPlanId,
                    ExperimentId = d.AllocationPlan.ExperimentId,
                    ExperimentName = d.AllocationPlan.Experiment.ExperimentName,

                    ExpHumanReqId = d.ExpHumanReqId,
                    PhaseHumanReqId = d.PhaseHumanReqId,
                    PhaseId = d.PhaseHumanReq != null ? d.PhaseHumanReq.PhaseId : null,
                    PhaseName = d.PhaseHumanReq != null ? d.PhaseHumanReq.Phase.PhaseName : null,

                    HumanResourceId = d.HumanResourceId,
                    UserId = d.HumanResource.UserId,
                    FullName = d.HumanResource.User.FullName,
                    Username = d.HumanResource.User.Username,
                    Email = d.HumanResource.User.Email,

                    HumanResourceRoleId = d.HumanResource.User.RoleId,
                    HumanResourceRoleName = d.HumanResource.User.Role.RoleName,

                    RequiredRoleId = d.ExpHumanReq != null
                        ? d.ExpHumanReq.RoleId
                        : d.PhaseHumanReq != null
                            ? d.PhaseHumanReq.RoleId
                            : null,

                    RequiredRoleName = d.ExpHumanReq != null
                        ? d.ExpHumanReq.Role.RoleName
                        : d.PhaseHumanReq != null
                            ? d.PhaseHumanReq.Role.RoleName
                            : null,

                    RequiredSkillId = d.ExpHumanReq != null
                        ? d.ExpHumanReq.RequiredSkillId
                        : d.PhaseHumanReq != null
                            ? d.PhaseHumanReq.RequiredSkillId
                            : null,

                    RequiredSkillName = d.ExpHumanReq != null && d.ExpHumanReq.RequiredSkill != null
                        ? d.ExpHumanReq.RequiredSkill.SkillName
                        : d.PhaseHumanReq != null && d.PhaseHumanReq.RequiredSkill != null
                            ? d.PhaseHumanReq.RequiredSkill.SkillName
                            : null,

                    RequiredQuantity = d.ExpHumanReq != null
                        ? d.ExpHumanReq.Quantity
                        : d.PhaseHumanReq != null
                            ? d.PhaseHumanReq.Quantity
                            : 0,

                    RequiredWorkingHoursPerDay = d.ExpHumanReq != null
                        ? d.ExpHumanReq.WorkingHoursPerDay
                        : null,

                    WorkingHours = d.WorkingHours,
                    MaxWorkingHoursPerDay = d.HumanResource.MaxWorkingHoursPerDay,
                    CurrentWorkload = d.HumanResource.CurrentWorkload,
                    HumanResourceStatus = d.HumanResource.Status,

                    RequirementNote = d.ExpHumanReq != null
                        ? d.ExpHumanReq.Note
                        : d.PhaseHumanReq != null
                            ? d.PhaseHumanReq.Note
                            : null,

                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<AllocationHumanDetailResponse?> GetAllocationHumanDetailByIdAsync(int id)
        {
            var detail = await GetDetailWithIncludesAsync(id);

            if (detail == null)
            {
                return null;
            }

            return MapToResponse(detail);
        }

        public async Task<AllocationHumanDetailResponse> CreateAllocationHumanDetailAsync(
            AllocationHumanDetailRequest request)
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
                request.ExpHumanReqId,
                request.PhaseHumanReqId);

            var humanResource = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceId == request.HumanResourceId,
                    include: query => query
                        .Include(h => h.User)
                            .ThenInclude(u => u.Role)
                        .Include(h => h.HumanResourceSkills)
                );

            if (humanResource == null)
            {
                throw new Exception("Human resource profile does not exist.");
            }

            ValidateHumanResourceMatchesRequirement(humanResource, requirementInfo);

            await ValidateHumanResourceAvailabilityAsync(
                humanResource,
                request.WorkingHours,
                request.StartDate,
                request.EndDate,
                exceptAllocationHumanDetailId: null);

            var detail = new AllocationHumanDetail
            {
                AllocationPlanId = request.AllocationPlanId,
                ExpHumanReqId = request.ExpHumanReqId,
                PhaseHumanReqId = request.PhaseHumanReqId,
                HumanResourceId = request.HumanResourceId,
                WorkingHours = request.WorkingHours,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status.ToString(),
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<AllocationHumanDetail>()
                .InsertAsync(detail);

            await _unitOfWork.CommitAsync();

            var created = await GetDetailWithIncludesAsync(detail.AllocationHumanDetailId);

            return MapToResponse(created!);
        }

        public async Task<AllocationHumanDetailResponse?> UpdateAllocationHumanDetailAsync(
            int id,
            AllocationHumanDetailRequest request)
        {
            ValidateRequest(request);

            var detail = await _unitOfWork
                .GetRepository<AllocationHumanDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationHumanDetailId == id,
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
                request.ExpHumanReqId,
                request.PhaseHumanReqId);

            var humanResource = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceId == request.HumanResourceId,
                    include: query => query
                        .Include(h => h.User)
                            .ThenInclude(u => u.Role)
                        .Include(h => h.HumanResourceSkills)
                );

            if (humanResource == null)
            {
                throw new Exception("Human resource profile does not exist.");
            }

            ValidateHumanResourceMatchesRequirement(humanResource, requirementInfo);

            await ValidateHumanResourceAvailabilityAsync(
                humanResource,
                request.WorkingHours,
                request.StartDate,
                request.EndDate,
                exceptAllocationHumanDetailId: id);

            detail.AllocationPlanId = request.AllocationPlanId;
            detail.ExpHumanReqId = request.ExpHumanReqId;
            detail.PhaseHumanReqId = request.PhaseHumanReqId;
            detail.HumanResourceId = request.HumanResourceId;
            detail.WorkingHours = request.WorkingHours;
            detail.StartDate = request.StartDate;
            detail.EndDate = request.EndDate;
            detail.Status = request.Status.ToString();
            detail.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<AllocationHumanDetail>()
                .Update(detail);

            await _unitOfWork.CommitAsync();

            var updated = await GetDetailWithIncludesAsync(id);

            return MapToResponse(updated!);
        }

        public async Task<bool> DeleteAllocationHumanDetailAsync(int id)
        {
            var detail = await _unitOfWork
                .GetRepository<AllocationHumanDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationHumanDetailId == id,
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

            _unitOfWork.GetRepository<AllocationHumanDetail>()
                .Delete(detail);

            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task<AllocationHumanDetail?> GetDetailWithIncludesAsync(int id)
        {
            return await _unitOfWork
                .GetRepository<AllocationHumanDetail>()
                .FirstOrDefaultAsync(
                    predicate: d => d.AllocationHumanDetailId == id,
                    include: query => query
                        .Include(d => d.AllocationPlan)
                            .ThenInclude(p => p.Experiment)
                        .Include(d => d.HumanResource)
                            .ThenInclude(h => h.User)
                                .ThenInclude(u => u.Role)
                        .Include(d => d.ExpHumanReq)
                            .ThenInclude(r => r.Role)
                        .Include(d => d.ExpHumanReq)
                            .ThenInclude(r => r.RequiredSkill)
                        .Include(d => d.PhaseHumanReq)
                            .ThenInclude(r => r.Role)
                        .Include(d => d.PhaseHumanReq)
                            .ThenInclude(r => r.RequiredSkill)
                        .Include(d => d.PhaseHumanReq)
                            .ThenInclude(r => r.Phase)
                );
        }

        private static AllocationHumanDetailResponse MapToResponse(
            AllocationHumanDetail detail)
        {
            var requiredRoleId = detail.ExpHumanReq != null
                ? detail.ExpHumanReq.RoleId
                : detail.PhaseHumanReq?.RoleId;

            var requiredRoleName = detail.ExpHumanReq != null
                ? detail.ExpHumanReq.Role?.RoleName
                : detail.PhaseHumanReq?.Role?.RoleName;

            var requiredSkillId = detail.ExpHumanReq != null
                ? detail.ExpHumanReq.RequiredSkillId
                : detail.PhaseHumanReq?.RequiredSkillId;

            var requiredSkillName = detail.ExpHumanReq != null
                ? detail.ExpHumanReq.RequiredSkill?.SkillName
                : detail.PhaseHumanReq?.RequiredSkill?.SkillName;

            var requiredQuantity = detail.ExpHumanReq != null
                ? detail.ExpHumanReq.Quantity
                : detail.PhaseHumanReq?.Quantity ?? 0;

            var requiredWorkingHoursPerDay = detail.ExpHumanReq?.WorkingHoursPerDay;

            var requirementNote = detail.ExpHumanReq != null
                ? detail.ExpHumanReq.Note
                : detail.PhaseHumanReq?.Note;

            return new AllocationHumanDetailResponse
            {
                AllocationHumanDetailId = detail.AllocationHumanDetailId,
                AllocationPlanId = detail.AllocationPlanId,
                ExperimentId = detail.AllocationPlan?.ExperimentId ?? 0,
                ExperimentName = detail.AllocationPlan?.Experiment?.ExperimentName,

                ExpHumanReqId = detail.ExpHumanReqId,
                PhaseHumanReqId = detail.PhaseHumanReqId,
                PhaseId = detail.PhaseHumanReq?.PhaseId,
                PhaseName = detail.PhaseHumanReq?.Phase?.PhaseName,

                HumanResourceId = detail.HumanResourceId,
                UserId = detail.HumanResource?.UserId ?? 0,
                FullName = detail.HumanResource?.User?.FullName,
                Username = detail.HumanResource?.User?.Username,
                Email = detail.HumanResource?.User?.Email,

                HumanResourceRoleId = detail.HumanResource?.User?.RoleId,
                HumanResourceRoleName = detail.HumanResource?.User?.Role?.RoleName,

                RequiredRoleId = requiredRoleId,
                RequiredRoleName = requiredRoleName,
                RequiredSkillId = requiredSkillId,
                RequiredSkillName = requiredSkillName,
                RequiredQuantity = requiredQuantity,
                RequiredWorkingHoursPerDay = requiredWorkingHoursPerDay,

                WorkingHours = detail.WorkingHours,
                MaxWorkingHoursPerDay = detail.HumanResource?.MaxWorkingHoursPerDay ?? 0,
                CurrentWorkload = detail.HumanResource?.CurrentWorkload ?? 0,
                HumanResourceStatus = detail.HumanResource?.Status,
                RequirementNote = requirementNote,

                StartDate = detail.StartDate,
                EndDate = detail.EndDate,
                Status = detail.Status,
                CreatedAt = detail.CreatedAt,
                UpdatedAt = detail.UpdatedAt
            };
        }

        private static void ValidateRequest(AllocationHumanDetailRequest request)
        {
            if (request.AllocationPlanId <= 0)
            {
                throw new Exception("AllocationPlanId is required.");
            }

            if (!request.ExpHumanReqId.HasValue &&
                !request.PhaseHumanReqId.HasValue)
            {
                throw new Exception("ExpHumanReqId or PhaseHumanReqId is required.");
            }

            if (request.ExpHumanReqId.HasValue &&
                request.PhaseHumanReqId.HasValue)
            {
                throw new Exception("Only one of ExpHumanReqId or PhaseHumanReqId can be provided.");
            }

            if (request.HumanResourceId <= 0)
            {
                throw new Exception("HumanResourceId is required.");
            }

            if (request.WorkingHours <= 0)
            {
                throw new Exception("Working hours must be greater than 0.");
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

        private async Task<RequirementInfo> GetAndValidateRequirementInfoAsync(
            AllocationPlan allocationPlan,
            int? expHumanReqId,
            int? phaseHumanReqId)
        {
            if (expHumanReqId.HasValue)
            {
                var requirement = await _unitOfWork
                    .GetRepository<ExperimentHumanRequirement>()
                    .FirstOrDefaultAsync(
                        predicate: r => r.ExpHumanReqId == expHumanReqId.Value,
                        include: query => query
                            .Include(r => r.Role)
                            .Include(r => r.RequiredSkill)
                    );

                if (requirement == null)
                {
                    throw new Exception("Experiment human requirement does not exist.");
                }

                if (requirement.ExperimentId != allocationPlan.ExperimentId)
                {
                    throw new Exception(
                        "Human requirement does not belong to the experiment of this allocation plan.");
                }

                return new RequirementInfo
                {
                    RequiredRoleId = requirement.RoleId,
                    RequiredSkillId = requirement.RequiredSkillId,
                    RequiredQuantity = requirement.Quantity,
                    RequiredWorkingHoursPerDay = requirement.WorkingHoursPerDay
                };
            }

            var phaseRequirement = await _unitOfWork
                .GetRepository<PhaseHumanRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.PhaseHumanReqId == phaseHumanReqId!.Value,
                    include: query => query
                        .Include(r => r.Role)
                        .Include(r => r.RequiredSkill)
                        .Include(r => r.Phase)
                );

            if (phaseRequirement == null)
            {
                throw new Exception("Phase human requirement does not exist.");
            }

            if (phaseRequirement.Phase.ExperimentId != allocationPlan.ExperimentId)
            {
                throw new Exception(
                    "Phase human requirement does not belong to the experiment of this allocation plan.");
            }

            return new RequirementInfo
            {
                RequiredRoleId = phaseRequirement.RoleId,
                RequiredSkillId = phaseRequirement.RequiredSkillId,
                RequiredQuantity = phaseRequirement.Quantity,
                RequiredWorkingHoursPerDay = null
            };
        }

        private static void ValidateHumanResourceMatchesRequirement(
            HumanResourceProfile humanResource,
            RequirementInfo requirementInfo)
        {
            if (humanResource.User.RoleId != requirementInfo.RequiredRoleId)
            {
                throw new Exception(
                    "Human resource role does not match the required role.");
            }

            if (requirementInfo.RequiredSkillId.HasValue)
            {
                var hasRequiredSkill = humanResource.HumanResourceSkills.Any(s =>
                    s.SkillId == requirementInfo.RequiredSkillId.Value);

                if (!hasRequiredSkill)
                {
                    throw new Exception(
                        "Human resource does not have the required skill.");
                }
            }

            var status = EnumHelper.ParseEnum<HumanResourceStatus>(
                humanResource.Status);

            if (status != HumanResourceStatus.Available &&
                status != HumanResourceStatus.Busy)
            {
                throw new Exception("Human resource is not available for allocation.");
            }
        }

        private async Task ValidateHumanResourceAvailabilityAsync(
            HumanResourceProfile humanResource,
            double requestedWorkingHours,
            DateTime startDate,
            DateTime endDate,
            int? exceptAllocationHumanDetailId)
        {
            if (requestedWorkingHours > humanResource.MaxWorkingHoursPerDay)
            {
                throw new Exception(
                    "Requested working hours exceed max working hours per day.");
            }

            var cancelledDetailStatus = AllocationDetailStatus.Cancelled.ToString();
            var completedDetailStatus = AllocationDetailStatus.Completed.ToString();
            var rejectedPlanStatus = AllocationPlanStatus.Rejected.ToString();

            var overlappedWorkingHours = await _unitOfWork
                .GetRepository<AllocationHumanDetail>()
                .GetQueryable()
                .Where(d =>
                    d.HumanResourceId == humanResource.HumanResourceId &&
                    (!exceptAllocationHumanDetailId.HasValue ||
                        d.AllocationHumanDetailId != exceptAllocationHumanDetailId.Value) &&
                    d.Status != cancelledDetailStatus &&
                    d.Status != completedDetailStatus &&
                    d.AllocationPlan.ApproveStatus != rejectedPlanStatus &&
                    d.StartDate < endDate &&
                    startDate < d.EndDate)
                .SumAsync(d => d.WorkingHours);

            if (overlappedWorkingHours + requestedWorkingHours >
                humanResource.MaxWorkingHoursPerDay)
            {
                throw new Exception(
                    "Human resource workload exceeds max working hours per day in the selected time range.");
            }

            var scheduleConflict = await _unitOfWork
                .GetRepository<Schedule>()
                .AnyAsync(s =>
                    s.AssignedHumanResourceId == humanResource.HumanResourceId &&
                    s.StartDate < endDate &&
                    startDate < s.EndDate);

            if (scheduleConflict)
            {
                throw new Exception(
                    "Human resource already has a schedule in the selected time range.");
            }
        }

        private class RequirementInfo
        {
            public int RequiredRoleId { get; set; }

            public int? RequiredSkillId { get; set; }

            public int RequiredQuantity { get; set; }

            public double? RequiredWorkingHoursPerDay { get; set; }
        }
    }
}

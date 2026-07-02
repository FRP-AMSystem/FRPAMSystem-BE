using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.AllocationPlan;
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
    public class AllocationPlanService : IAllocationPlanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AllocationPlanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<AllocationPlanResponse>> ViewAllAllocationPlansAsync(
            AllocationPlanFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<AllocationPlan>()
                .GetQueryable()
                .Include(p => p.Experiment)
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.ApproveByNavigation)
                .Include(p => p.AllocationLandDetails)
                .Include(p => p.AllocationEquipmentDetails)
                .Include(p => p.AllocationHumanDetails)
                .Include(p => p.Schedules)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt);

            return await query
                .Select(p => new AllocationPlanResponse
                {
                    AllocationPlanId = p.AllocationPlanId,
                    ExperimentId = p.ExperimentId,
                    ExperimentName = p.Experiment.ExperimentName,
                    FitnessScore = p.FitnessScore,
                    CreatedBy = p.CreatedBy,
                    CreatedByName = p.CreatedByNavigation != null
                        ? p.CreatedByNavigation.FullName
                        : null,
                    ApproveBy = p.ApproveBy,
                    ApproveByName = p.ApproveByNavigation != null
                        ? p.ApproveByNavigation.FullName
                        : null,
                    ApproveStatus = p.ApproveStatus,
                    ApprovedAt = p.ApprovedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    LandDetailCount = p.AllocationLandDetails.Count,
                    EquipmentDetailCount = p.AllocationEquipmentDetails.Count,
                    HumanDetailCount = p.AllocationHumanDetails.Count,
                    ScheduleCount = p.Schedules.Count
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<AllocationPlanResponse?> GetAllocationPlanByIdAsync(int id)
        {
            var allocationPlan = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    include: query => query
                        .Include(p => p.Experiment)
                        .Include(p => p.CreatedByNavigation)
                        .Include(p => p.ApproveByNavigation)
                        .Include(p => p.AllocationLandDetails)
                        .Include(p => p.AllocationEquipmentDetails)
                        .Include(p => p.AllocationHumanDetails)
                        .Include(p => p.Schedules)
                );

            if (allocationPlan == null)
            {
                return null;
            }

            return MapToResponse(allocationPlan);
        }

        public async Task<AllocationPlanResponse> CreateAllocationPlanAsync(
            AllocationPlanRequest request,
            int? currentUserId)
        {
            ValidateAllocationPlanRequest(request);

            var experimentExists = await _unitOfWork
                .GetRepository<Experiment>()
                .AnyAsync(e => e.ExperimentId == request.ExperimentId);

            if (!experimentExists)
            {
                throw new Exception("Experiment does not exist.");
            }

            if (currentUserId.HasValue)
            {
                var creatorExists = await _unitOfWork
                    .GetRepository<User>()
                    .AnyAsync(u => u.UserId == currentUserId.Value);

                if (!creatorExists)
                {
                    throw new Exception("Current user does not exist.");
                }
            }

            var allocationPlan = new AllocationPlan
            {
                ExperimentId = request.ExperimentId,
                FitnessScore = request.FitnessScore,
                CreatedBy = currentUserId,
                ApproveBy = null,
                ApproveStatus = request.ApproveStatus.ToString(),
                ApprovedAt = null,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<AllocationPlan>()
                .InsertAsync(allocationPlan);

            await _unitOfWork.CommitAsync();

            var created = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == allocationPlan.AllocationPlanId,
                    include: query => query
                        .Include(p => p.Experiment)
                        .Include(p => p.CreatedByNavigation)
                        .Include(p => p.ApproveByNavigation)
                        .Include(p => p.AllocationLandDetails)
                        .Include(p => p.AllocationEquipmentDetails)
                        .Include(p => p.AllocationHumanDetails)
                        .Include(p => p.Schedules)
                );

            return MapToResponse(created!);
        }

        public async Task<AllocationPlanResponse?> UpdateAllocationPlanAsync(
            int id,
            AllocationPlanRequest request)
        {
            ValidateAllocationPlanRequest(request);

            var allocationPlan = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    asNoTracking: false
                );

            if (allocationPlan == null)
            {
                return null;
            }

            var currentStatus = EnumHelper.ParseEnum<AllocationPlanStatus>(
                allocationPlan.ApproveStatus);

            if (currentStatus == AllocationPlanStatus.Approved)
            {
                throw new Exception("Approved allocation plan cannot be updated.");
            }

            var experimentExists = await _unitOfWork
                .GetRepository<Experiment>()
                .AnyAsync(e => e.ExperimentId == request.ExperimentId);

            if (!experimentExists)
            {
                throw new Exception("Experiment does not exist.");
            }

            allocationPlan.ExperimentId = request.ExperimentId;
            allocationPlan.FitnessScore = request.FitnessScore;
            allocationPlan.ApproveStatus = request.ApproveStatus.ToString();

            if (request.ApproveStatus != AllocationPlanStatus.Approved)
            {
                allocationPlan.ApproveBy = null;
                allocationPlan.ApprovedAt = null;
            }

            allocationPlan.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<AllocationPlan>().Update(allocationPlan);

            await _unitOfWork.CommitAsync();

            var updated = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    include: query => query
                        .Include(p => p.Experiment)
                        .Include(p => p.CreatedByNavigation)
                        .Include(p => p.ApproveByNavigation)
                        .Include(p => p.AllocationLandDetails)
                        .Include(p => p.AllocationEquipmentDetails)
                        .Include(p => p.AllocationHumanDetails)
                        .Include(p => p.Schedules)
                );

            return MapToResponse(updated!);
        }

        public async Task<bool> DeleteAllocationPlanAsync(int id)
        {
            var allocationPlan = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    asNoTracking: false
                );

            if (allocationPlan == null)
            {
                return false;
            }

            var status = EnumHelper.ParseEnum<AllocationPlanStatus>(
                allocationPlan.ApproveStatus);

            if (status == AllocationPlanStatus.Approved)
            {
                throw new Exception("Approved allocation plan cannot be deleted.");
            }

            var hasLandDetail = await _unitOfWork
                .GetRepository<AllocationLandDetail>()
                .AnyAsync(d => d.AllocationPlanId == id);

            if (hasLandDetail)
            {
                throw new Exception(
                    "Cannot delete allocation plan because it has land allocation details.");
            }

            var hasEquipmentDetail = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .AnyAsync(d => d.AllocationPlanId == id);

            if (hasEquipmentDetail)
            {
                throw new Exception(
                    "Cannot delete allocation plan because it has equipment allocation details.");
            }

            var hasHumanDetail = await _unitOfWork
                .GetRepository<AllocationHumanDetail>()
                .AnyAsync(d => d.AllocationPlanId == id);

            if (hasHumanDetail)
            {
                throw new Exception(
                    "Cannot delete allocation plan because it has human allocation details.");
            }

            var hasSchedule = await _unitOfWork
                .GetRepository<Schedule>()
                .AnyAsync(s => s.AllocationPlanId == id);

            if (hasSchedule)
            {
                throw new Exception(
                    "Cannot delete allocation plan because it has schedules.");
            }

            _unitOfWork.GetRepository<AllocationPlan>().Delete(allocationPlan);

            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<AllocationPlanResponse?> ApproveAllocationPlanAsync(
            int id,
            int? currentUserId)
        {
            var allocationPlan = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    asNoTracking: false
                );

            if (allocationPlan == null)
            {
                return null;
            }

            if (!currentUserId.HasValue)
            {
                throw new Exception("Current user is required to approve allocation plan.");
            }

            var approverExists = await _unitOfWork
                .GetRepository<User>()
                .AnyAsync(u => u.UserId == currentUserId.Value);

            if (!approverExists)
            {
                throw new Exception("Approver does not exist.");
            }

            var currentStatus = EnumHelper.ParseEnum<AllocationPlanStatus>(
                allocationPlan.ApproveStatus);

            if (currentStatus == AllocationPlanStatus.Approved)
            {
                throw new Exception("Allocation plan is already approved.");
            }

            allocationPlan.ApproveStatus = AllocationPlanStatus.Approved.ToString();
            allocationPlan.ApproveBy = currentUserId.Value;
            allocationPlan.ApprovedAt = DateTime.Now;
            allocationPlan.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<AllocationPlan>().Update(allocationPlan);

            await _unitOfWork.CommitAsync();

            var approved = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    include: query => query
                        .Include(p => p.Experiment)
                        .Include(p => p.CreatedByNavigation)
                        .Include(p => p.ApproveByNavigation)
                        .Include(p => p.AllocationLandDetails)
                        .Include(p => p.AllocationEquipmentDetails)
                        .Include(p => p.AllocationHumanDetails)
                        .Include(p => p.Schedules)
                );

            return MapToResponse(approved!);
        }

        public async Task<AllocationPlanResponse?> RejectAllocationPlanAsync(
            int id,
            int? currentUserId)
        {
            var allocationPlan = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    asNoTracking: false
                );

            if (allocationPlan == null)
            {
                return null;
            }

            if (!currentUserId.HasValue)
            {
                throw new Exception("Current user is required to reject allocation plan.");
            }

            var currentStatus = EnumHelper.ParseEnum<AllocationPlanStatus>(
                allocationPlan.ApproveStatus);

            if (currentStatus == AllocationPlanStatus.Approved)
            {
                throw new Exception("Approved allocation plan cannot be rejected.");
            }

            allocationPlan.ApproveStatus = AllocationPlanStatus.Rejected.ToString();
            allocationPlan.ApproveBy = currentUserId.Value;
            allocationPlan.ApprovedAt = null;
            allocationPlan.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<AllocationPlan>().Update(allocationPlan);

            await _unitOfWork.CommitAsync();

            var rejected = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    include: query => query
                        .Include(p => p.Experiment)
                        .Include(p => p.CreatedByNavigation)
                        .Include(p => p.ApproveByNavigation)
                        .Include(p => p.AllocationLandDetails)
                        .Include(p => p.AllocationEquipmentDetails)
                        .Include(p => p.AllocationHumanDetails)
                        .Include(p => p.Schedules)
                );

            return MapToResponse(rejected!);
        }

        public async Task<AllocationPlanResponse?> CancelAllocationPlanAsync(int id)
        {
            var allocationPlan = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    asNoTracking: false
                );

            if (allocationPlan == null)
            {
                return null;
            }

            var currentStatus = EnumHelper.ParseEnum<AllocationPlanStatus>(
                allocationPlan.ApproveStatus);

            if (currentStatus == AllocationPlanStatus.Approved)
            {
                throw new Exception("Approved allocation plan cannot be cancelled.");
            }

            allocationPlan.ApproveStatus = AllocationPlanStatus.Rejected.ToString();
            allocationPlan.ApprovedAt = null;
            allocationPlan.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<AllocationPlan>().Update(allocationPlan);

            await _unitOfWork.CommitAsync();

            var cancelled = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    include: query => query
                        .Include(p => p.Experiment)
                        .Include(p => p.CreatedByNavigation)
                        .Include(p => p.ApproveByNavigation)
                        .Include(p => p.AllocationLandDetails)
                        .Include(p => p.AllocationEquipmentDetails)
                        .Include(p => p.AllocationHumanDetails)
                        .Include(p => p.Schedules)
                );

            return MapToResponse(cancelled!);
        }

        private static AllocationPlanResponse MapToResponse(
            AllocationPlan allocationPlan)
        {
            return new AllocationPlanResponse
            {
                AllocationPlanId = allocationPlan.AllocationPlanId,
                ExperimentId = allocationPlan.ExperimentId,
                ExperimentName = allocationPlan.Experiment?.ExperimentName,
                FitnessScore = allocationPlan.FitnessScore,
                CreatedBy = allocationPlan.CreatedBy,
                CreatedByName = allocationPlan.CreatedByNavigation?.FullName,
                ApproveBy = allocationPlan.ApproveBy,
                ApproveByName = allocationPlan.ApproveByNavigation?.FullName,
                ApproveStatus = allocationPlan.ApproveStatus,
                ApprovedAt = allocationPlan.ApprovedAt,
                CreatedAt = allocationPlan.CreatedAt,
                UpdatedAt = allocationPlan.UpdatedAt,
                LandDetailCount = allocationPlan.AllocationLandDetails?.Count ?? 0,
                EquipmentDetailCount = allocationPlan.AllocationEquipmentDetails?.Count ?? 0,
                HumanDetailCount = allocationPlan.AllocationHumanDetails?.Count ?? 0,
                ScheduleCount = allocationPlan.Schedules?.Count ?? 0
            };
        }

        private static void ValidateAllocationPlanRequest(
            AllocationPlanRequest request)
        {
            if (request.ExperimentId <= 0)
            {
                throw new Exception("ExperimentId is required.");
            }

            if (request.FitnessScore.HasValue &&
                request.FitnessScore.Value < 0)
            {
                throw new Exception("Fitness score cannot be negative.");
            }
        }
    }
}

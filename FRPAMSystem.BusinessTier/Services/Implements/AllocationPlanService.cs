using FRPAMSystem.BusinessTier.AI.Fitness;
using FRPAMSystem.BusinessTier.AI.Mappers;
using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.AllocationPlan;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Abstractions;
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
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly IFitnessCalculator _fitnessCalculator;
        private readonly IAllocationPlanChromosomeMapper _chromosomeMapper;
        private readonly IClock _clock;

        public AllocationPlanService(
            IUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher,
            IFitnessCalculator fitnessCalculator,
            IAllocationPlanChromosomeMapper chromosomeMapper,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _domainEventDispatcher = domainEventDispatcher;
            _fitnessCalculator = fitnessCalculator;
            _chromosomeMapper = chromosomeMapper;
            _clock = clock;
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
                ApprovedAt = null
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


            _unitOfWork.GetRepository<AllocationPlan>().Update(allocationPlan);

            await _unitOfWork.CommitAsync();

            var updated = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    include: query => query
                        .Include(p => p.Experiment)
                            .ThenInclude(e => e.ExperimentPhases)
                        .Include(p => p.CreatedByNavigation)
                        .Include(p => p.ApproveByNavigation)
                        .Include(p => p.AllocationLandDetails)
                            .ThenInclude(l => l.Land)
                        .Include(p => p.AllocationLandDetails)
                            .ThenInclude(l => l.ExpLandReq)
                        .Include(p => p.AllocationEquipmentDetails)
                            .ThenInclude(e => e.AllocatedEquipmentType)
                        .Include(p => p.AllocationEquipmentDetails)
                            .ThenInclude(e => e.EquipmentInstance)
                        .Include(p => p.AllocationEquipmentDetails)
                            .ThenInclude(e => e.PhaseEquipmentReq)
                        .Include(p => p.AllocationEquipmentDetails)
                            .ThenInclude(e => e.ExpEquipmentReq)
                        .Include(p => p.AllocationHumanDetails)
                            .ThenInclude(h => h.HumanResource)
                        .Include(p => p.AllocationHumanDetails)
                            .ThenInclude(h => h.PhaseHumanReq)
                        .Include(p => p.AllocationHumanDetails)
                            .ThenInclude(h => h.ExpHumanReq)
                        .Include(p => p.Schedules),
                    asNoTracking: false
                );

            FitnessResult? fitnessResult = null;
            if (updated != null &&
                (updated.AllocationLandDetails.Count > 0 ||
                 updated.AllocationEquipmentDetails.Count > 0 ||
                 updated.AllocationHumanDetails.Count > 0 ||
                 updated.Schedules.Count > 0))
            {
                var input = await BuildOptimizationInputForPlanAsync(
                    updated.ExperimentId,
                    updated.AllocationPlanId,
                    null);
                var chromosome = _chromosomeMapper.MapToChromosome(updated, input);
                fitnessResult = _fitnessCalculator.Evaluate(chromosome, input);
                updated.FitnessScore = fitnessResult.FitnessScore;
                _unitOfWork.GetRepository<AllocationPlan>().Update(updated);
                await _unitOfWork.CommitAsync();
            }

            return MapToResponse(updated!, fitnessResult);
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

        public async Task<AllocationPlanResponse?> SubmitAllocationPlanAsync(int id)
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
                throw new Exception("Approved allocation plan cannot be submitted.");
            }

            allocationPlan.ApproveStatus = AllocationPlanStatus.Pending.ToString();
            allocationPlan.ApproveBy = null;
            allocationPlan.ApprovedAt = null;

            _unitOfWork.GetRepository<AllocationPlan>().Update(allocationPlan);
            await _unitOfWork.CommitAsync();

            var submitted = await _unitOfWork
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

            await _domainEventDispatcher.DispatchAsync(new AllocationPlanSubmittedEvent(
                submitted!.AllocationPlanId,
                submitted.ExperimentId,
                submitted.Experiment?.ExperimentName,
                submitted.CreatedBy,
                _clock.Now));

            return MapToResponse(submitted!);
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
            allocationPlan.ApprovedAt = _clock.Now;

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

            await _domainEventDispatcher.DispatchAsync(new AllocationPlanApprovedEvent(
                approved!.AllocationPlanId,
                approved.ExperimentId,
                approved.Experiment?.ExperimentName,
                approved.CreatedBy,
                currentUserId.Value,
                _clock.Now));

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

            await _domainEventDispatcher.DispatchAsync(new AllocationPlanRejectedEvent(
                rejected!.AllocationPlanId,
                rejected.ExperimentId,
                rejected.Experiment?.ExperimentName,
                rejected.CreatedBy,
                currentUserId.Value,
                _clock.Now));

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

        public async Task<AllocationPlanResponse?> EvaluatePlanFitnessAsync(
            int id,
            OptimizationSettings? settings = null)
        {
            var allocationPlan = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .FirstOrDefaultAsync(
                    predicate: p => p.AllocationPlanId == id,
                    include: query => query
                        .Include(p => p.Experiment)
                            .ThenInclude(e => e.ExperimentPhases)
                        .Include(p => p.CreatedByNavigation)
                        .Include(p => p.ApproveByNavigation)
                        .Include(p => p.AllocationLandDetails)
                            .ThenInclude(l => l.Land)
                        .Include(p => p.AllocationLandDetails)
                            .ThenInclude(l => l.ExpLandReq)
                        .Include(p => p.AllocationEquipmentDetails)
                            .ThenInclude(e => e.AllocatedEquipmentType)
                        .Include(p => p.AllocationEquipmentDetails)
                            .ThenInclude(e => e.EquipmentInstance)
                        .Include(p => p.AllocationEquipmentDetails)
                            .ThenInclude(e => e.PhaseEquipmentReq)
                        .Include(p => p.AllocationEquipmentDetails)
                            .ThenInclude(e => e.ExpEquipmentReq)
                        .Include(p => p.AllocationHumanDetails)
                            .ThenInclude(h => h.HumanResource)
                        .Include(p => p.AllocationHumanDetails)
                            .ThenInclude(h => h.PhaseHumanReq)
                        .Include(p => p.AllocationHumanDetails)
                            .ThenInclude(h => h.ExpHumanReq)
                        .Include(p => p.Schedules),
                    asNoTracking: false
                );

            if (allocationPlan == null)
            {
                return null;
            }

            var input = await BuildOptimizationInputForPlanAsync(
                allocationPlan.ExperimentId,
                allocationPlan.AllocationPlanId,
                settings);

            var chromosome = _chromosomeMapper.MapToChromosome(allocationPlan, input);
            var fitnessResult = _fitnessCalculator.Evaluate(chromosome, input);

            allocationPlan.FitnessScore = fitnessResult.FitnessScore;

            _unitOfWork.GetRepository<AllocationPlan>().Update(allocationPlan);
            await _unitOfWork.CommitAsync();

            return MapToResponse(allocationPlan, fitnessResult);
        }

        private async Task<OptimizationInput> BuildOptimizationInputForPlanAsync(
            int experimentId,
            int? currentPlanId,
            OptimizationSettings? settings)
        {
            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(
                    predicate: e => e.ExperimentId == experimentId,
                    include: query => query
                        .Include(e => e.ExperimentPhases)
                        .Include(e => e.ExperimentLandRequirements)
                        .Include(e => e.ExperimentHumanRequirements)
                            .ThenInclude(r => r.RequiredSkill)
                        .Include(e => e.ExperimentEquipmentRequirements)
                            .ThenInclude(r => r.EquipmentType));

            if (experiment is null)
            {
                throw new Exception("Experiment does not exist.");
            }

            var phaseIds = experiment.ExperimentPhases.Select(p => p.PhaseId).ToList();

            var phaseHumanRequirements = await _unitOfWork
                .GetRepository<PhaseHumanRequirement>()
                .GetQueryable()
                .Include(r => r.RequiredSkill)
                .Where(r => phaseIds.Contains(r.PhaseId))
                .AsNoTracking()
                .ToListAsync();

            var phaseEquipmentRequirements = await _unitOfWork
                .GetRepository<PhaseEquipmentRequirement>()
                .GetQueryable()
                .Include(r => r.EquipmentType)
                .Where(r => phaseIds.Contains(r.PhaseId))
                .AsNoTracking()
                .ToListAsync();

            var lands = await _unitOfWork
                .GetRepository<LandResource>()
                .GetQueryable()
                .Include(l => l.Area)
                .AsNoTracking()
                .ToListAsync();

            var humans = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .GetQueryable()
                .Include(h => h.User)
                .Include(h => h.HumanResourceSkills)
                    .ThenInclude(s => s.Skill)
                .AsNoTracking()
                .ToListAsync();

            var equipment = await _unitOfWork
                .GetRepository<EquipmentInstance>()
                .GetQueryable()
                .Include(e => e.EquipmentType)
                .AsNoTracking()
                .ToListAsync();

            var skills = await _unitOfWork
                .GetRepository<Skill>()
                .GetQueryable()
                .AsNoTracking()
                .ToListAsync();

            var schedules = await _unitOfWork
                .GetRepository<Schedule>()
                .GetQueryable()
                .Where(s => currentPlanId == null || s.AllocationPlanId != currentPlanId.Value)
                .AsNoTracking()
                .ToListAsync();

            var landAllocations = await _unitOfWork
                .GetRepository<AllocationLandDetail>()
                .GetQueryable()
                .Where(a => currentPlanId == null || a.AllocationPlanId != currentPlanId.Value)
                .AsNoTracking()
                .ToListAsync();

            var humanAllocations = await _unitOfWork
                .GetRepository<AllocationHumanDetail>()
                .GetQueryable()
                .Where(a => currentPlanId == null || a.AllocationPlanId != currentPlanId.Value)
                .AsNoTracking()
                .ToListAsync();

            var equipmentAllocations = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .GetQueryable()
                .Where(a => currentPlanId == null || a.AllocationPlanId != currentPlanId.Value)
                .AsNoTracking()
                .ToListAsync();

            var substitutions = await _unitOfWork
                .GetRepository<EquipmentSubstitution>()
                .GetQueryable()
                .AsNoTracking()
                .ToListAsync();

            return new OptimizationInput
            {
                Experiment = experiment,
                ExperimentPhases = experiment.ExperimentPhases.OrderBy(p => p.PhaseOrder).ToList(),
                LandResources = lands,
                HumanResources = humans,
                EquipmentInstances = equipment,
                Skills = skills,
                ExperimentLandRequirements = experiment.ExperimentLandRequirements.ToList(),
                ExperimentHumanRequirements = experiment.ExperimentHumanRequirements.ToList(),
                ExperimentEquipmentRequirements = experiment.ExperimentEquipmentRequirements.ToList(),
                PhaseHumanRequirements = phaseHumanRequirements,
                PhaseEquipmentRequirements = phaseEquipmentRequirements,
                ExistingSchedules = schedules,
                ExistingLandAllocations = landAllocations,
                ExistingHumanAllocations = humanAllocations,
                ExistingEquipmentAllocations = equipmentAllocations,
                EquipmentSubstitutions = substitutions,
                Settings = settings?.Clone() ?? new OptimizationSettings()
            };
        }

        private static AllocationPlanResponse MapToResponse(
            AllocationPlan allocationPlan,
            FitnessResult? fitnessResult = null)
        {
            var response = new AllocationPlanResponse
            {
                AllocationPlanId = allocationPlan.AllocationPlanId,
                ExperimentId = allocationPlan.ExperimentId,
                ExperimentName = allocationPlan.Experiment?.ExperimentName,
                FitnessScore = fitnessResult?.FitnessScore ?? allocationPlan.FitnessScore,
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

            if (fitnessResult != null)
            {
                response.PenaltyScore = fitnessResult.PenaltyScore;
                response.BonusScore = fitnessResult.BonusScore;
                response.ConflictCount = fitnessResult.ConflictCount;
                response.FitnessBreakdown = fitnessResult.Breakdown;
                response.ConstraintReport = fitnessResult.ConstraintReport;
                response.Advantages = fitnessResult.Advantages;
                response.Disadvantages = fitnessResult.Disadvantages;
            }

            return response;
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

using FRPAMSystem.BusinessTier.AI.DTO;
using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.AI.Services
{
    public class AllocationOptimizationService : IAllocationOptimizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeneticAlgorithmService _geneticAlgorithmService;

        public AllocationOptimizationService(
            IUnitOfWork unitOfWork,
            IGeneticAlgorithmService geneticAlgorithmService)
        {
            _unitOfWork = unitOfWork;
            _geneticAlgorithmService = geneticAlgorithmService;
        }

        public async Task<IReadOnlyList<AllocationSuggestionDTO>> GenerateTopSuggestionsAsync(
            int experimentId,
            OptimizationSettings? settings = null)
        {
            var input = await BuildOptimizationInputAsync(experimentId, settings);
            return _geneticAlgorithmService.GenerateSuggestions(input);
        }

        private async Task<OptimizationInput> BuildOptimizationInputAsync(
            int experimentId,
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

            var cancelledDetailStatus = AllocationDetailStatus.Cancelled.ToString();
            var completedDetailStatus = AllocationDetailStatus.Completed.ToString();
            var rejectedPlanStatus = AllocationPlanStatus.Rejected.ToString();
            var cancelledPlanStatus = AllocationPlanStatus.Cancelled.ToString();
            var cancelledScheduleStatus = ScheduleStatus.Cancelled.ToString();
            var completedScheduleStatus = ScheduleStatus.Completed.ToString();

            var landAllocations = await _unitOfWork
                .GetRepository<AllocationLandDetail>()
                .GetQueryable()
                .Include(a => a.AllocationPlan)
                .Where(a => a.Status != cancelledDetailStatus &&
                            a.Status != completedDetailStatus &&
                            (a.AllocationPlan == null || (a.AllocationPlan.ApproveStatus != rejectedPlanStatus &&
                                                          a.AllocationPlan.ApproveStatus != cancelledPlanStatus)))
                .AsNoTracking()
                .ToListAsync();

            var humanAllocations = await _unitOfWork
                .GetRepository<AllocationHumanDetail>()
                .GetQueryable()
                .Include(a => a.AllocationPlan)
                .Where(a => a.Status != cancelledDetailStatus &&
                            a.Status != completedDetailStatus &&
                            (a.AllocationPlan == null || (a.AllocationPlan.ApproveStatus != rejectedPlanStatus &&
                                                          a.AllocationPlan.ApproveStatus != cancelledPlanStatus)))
                .AsNoTracking()
                .ToListAsync();

            var equipmentAllocations = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .GetQueryable()
                .Include(a => a.AllocationPlan)
                .Where(a => a.Status != cancelledDetailStatus &&
                            a.Status != completedDetailStatus &&
                            (a.AllocationPlan == null || (a.AllocationPlan.ApproveStatus != rejectedPlanStatus &&
                                                          a.AllocationPlan.ApproveStatus != cancelledPlanStatus)))
                .AsNoTracking()
                .ToListAsync();

            var schedules = await _unitOfWork
                .GetRepository<Schedule>()
                .GetQueryable()
                .Where(s => s.Status != cancelledScheduleStatus &&
                            s.Status != completedScheduleStatus)
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
                ExistingLandAllocations = landAllocations,
                ExistingHumanAllocations = humanAllocations,
                ExistingEquipmentAllocations = equipmentAllocations,
                ExistingSchedules = schedules,
                EquipmentSubstitutions = substitutions,
                Settings = settings?.Clone() ?? new OptimizationSettings()
            };
        }
    }
}

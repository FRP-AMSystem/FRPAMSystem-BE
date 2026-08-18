using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Payload.Schedule;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly IClock _clock;

        public ScheduleService(
            IUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _domainEventDispatcher = domainEventDispatcher;
            _clock = clock;
        }

        public async Task<IPaginate<ScheduleResponse>> ViewAllSchedulesAsync(
            ScheduleFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<Schedule>()
                .GetQueryable()
                .Include(s => s.AllocationPlan)
                    .ThenInclude(p => p.Experiment)
                .Include(s => s.Phase)
                .Include(s => s.CreatedByNavigation)
                .Include(s => s.AssignedHumanResource)
                    .ThenInclude(h => h!.User)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(s => s.StartDate)
                .ThenByDescending(s => s.CreatedAt);

            return await query
                .Select(s => new ScheduleResponse
                {
                    ScheduleId = s.ScheduleId,
                    AllocationPlanId = s.AllocationPlanId,
                    ExperimentId = s.AllocationPlan.ExperimentId,
                    ExperimentName = s.AllocationPlan.Experiment.ExperimentName,
                    PhaseId = s.PhaseId,
                    PhaseName = s.Phase != null ? s.Phase.PhaseName : null,
                    Title = s.Title,
                    Description = s.Description,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status,
                    CreatedBy = s.CreatedBy,
                    CreatedByName = s.CreatedByNavigation != null
                        ? s.CreatedByNavigation.FullName
                        : null,
                    AssignedHumanResourceId = s.AssignedHumanResourceId,
                    AssignedHumanResourceName = s.AssignedHumanResource != null
                        ? s.AssignedHumanResource.User.FullName
                        : null,
                    Notes = s.Notes,
                    Priority = s.Priority,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<IPaginate<ScheduleResponse>> ViewMineAsync(
            int userId,
            ScheduleFilter filter,
            PagingModel pagingModel)
        {
            var humanResourceId = await GetHumanResourceIdByUserIdAsync(userId);

            if (!humanResourceId.HasValue)
            {
                throw new Exception("Human resource profile not found for current user.");
            }

            filter.AssignedHumanResourceId = humanResourceId.Value;

            return await ViewAllSchedulesAsync(filter, pagingModel);
        }

        public async Task<ScheduleResponse?> GetScheduleByIdAsync(int id)
        {
            var schedule = await _unitOfWork
                .GetRepository<Schedule>()
                .FirstOrDefaultAsync(
                    predicate: s => s.ScheduleId == id,
                    include: query => query
                        .Include(s => s.AllocationPlan)
                            .ThenInclude(p => p.Experiment)
                        .Include(s => s.Phase)
                        .Include(s => s.CreatedByNavigation)
                        .Include(s => s.AssignedHumanResource)
                            .ThenInclude(h => h!.User));

            return schedule == null ? null : MapToResponse(schedule);
        }

        public async Task<ScheduleResponse?> GetScheduleByIdForUserAsync(int id, int userId)
        {
            var humanResourceId = await GetHumanResourceIdByUserIdAsync(userId);

            if (!humanResourceId.HasValue)
            {
                return null;
            }

            var schedule = await GetScheduleByIdAsync(id);

            if (schedule == null || schedule.AssignedHumanResourceId != humanResourceId.Value)
            {
                return null;
            }

            return schedule;
        }

        public async Task<ScheduleResponse> CreateScheduleAsync(ScheduleRequest request)
        {
            await ValidateRequestAsync(request);

            var schedule = new Schedule
            {
                AllocationPlanId = request.AllocationPlanId,
                PhaseId = request.PhaseId,
                Title = request.Title.Trim(),
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status.ToString(),
                CreatedBy = request.CreatedBy,
                AssignedHumanResourceId = request.AssignedHumanResourceId,
                Notes = request.Notes,
                Priority = request.Priority
            };

            await _unitOfWork.GetRepository<Schedule>().InsertAsync(schedule);
            await _unitOfWork.CommitAsync();

            
            if (schedule.AssignedHumanResourceId.HasValue)
            {
                var created = await LoadScheduleWithContextAsync(schedule.ScheduleId);

                await _domainEventDispatcher.DispatchAsync(new ScheduleAssignedEvent(
                    scheduleId: schedule.ScheduleId,
                    allocationPlanId: schedule.AllocationPlanId,
                    experimentId: created?.AllocationPlan?.ExperimentId,
                    experimentName: created?.AllocationPlan?.Experiment?.ExperimentName,
                    scheduleTitle: schedule.Title,
                    assignedHumanResourceId: schedule.AssignedHumanResourceId.Value,
                    isNewAssignment: true,
                    occurredAt: _clock.Now));
            }

            return (await GetScheduleByIdAsync(schedule.ScheduleId))!;
        }

        public async Task<ScheduleResponse?> UpdateScheduleAsync(int id, ScheduleRequest request)
        {
            await ValidateRequestAsync(request);

            var schedule = await _unitOfWork
                .GetRepository<Schedule>()
                .FirstOrDefaultAsync(
                    predicate: s => s.ScheduleId == id,
                    asNoTracking: false);

            if (schedule == null)
            {
                return null;
            }

           
            var previousAssignedHumanResourceId = schedule.AssignedHumanResourceId;

            schedule.AllocationPlanId = request.AllocationPlanId;
            schedule.PhaseId = request.PhaseId;
            schedule.Title = request.Title.Trim();
            schedule.Description = request.Description;
            schedule.StartDate = request.StartDate;
            schedule.EndDate = request.EndDate;
            schedule.Status = request.Status.ToString();
            schedule.CreatedBy = request.CreatedBy;
            schedule.AssignedHumanResourceId = request.AssignedHumanResourceId;
            schedule.Notes = request.Notes;
            schedule.Priority = request.Priority;

            _unitOfWork.GetRepository<Schedule>().Update(schedule);
            await _unitOfWork.CommitAsync();

            
            bool assignmentChanged = request.AssignedHumanResourceId.HasValue
                && request.AssignedHumanResourceId != previousAssignedHumanResourceId;

            if (assignmentChanged)
            {
                var updated = await LoadScheduleWithContextAsync(schedule.ScheduleId);

                await _domainEventDispatcher.DispatchAsync(new ScheduleAssignedEvent(
                    scheduleId: schedule.ScheduleId,
                    allocationPlanId: schedule.AllocationPlanId,
                    experimentId: updated?.AllocationPlan?.ExperimentId,
                    experimentName: updated?.AllocationPlan?.Experiment?.ExperimentName,
                    scheduleTitle: schedule.Title,
                    assignedHumanResourceId: request.AssignedHumanResourceId!.Value,
                    isNewAssignment: true,
                    occurredAt: _clock.Now));
            }

            return await GetScheduleByIdAsync(id);
        }

        public async Task<bool> DeleteScheduleAsync(int id)
        {
            var schedule = await _unitOfWork
                .GetRepository<Schedule>()
                .FirstOrDefaultAsync(
                    predicate: s => s.ScheduleId == id,
                    asNoTracking: false);

            if (schedule == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<Schedule>().Delete(schedule);
            await _unitOfWork.CommitAsync();

            return true;
        }

        
        private async Task<Schedule?> LoadScheduleWithContextAsync(int scheduleId)
        {
            return await _unitOfWork
                .GetRepository<Schedule>()
                .FirstOrDefaultAsync(
                    predicate: s => s.ScheduleId == scheduleId,
                    include: query => query
                        .Include(s => s.AllocationPlan)
                            .ThenInclude(p => p.Experiment));
        }

        private async Task ValidateRequestAsync(ScheduleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new Exception("Schedule title is required.");
            }

            if (request.EndDate < request.StartDate)
            {
                throw new Exception("End date must be greater than or equal to start date.");
            }

            if (request.Priority < 1 || request.Priority > 4)
            {
                throw new Exception("Priority must be between 1 and 4.");
            }

            var allocationPlanExists = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .AnyAsync(p => p.AllocationPlanId == request.AllocationPlanId);

            if (!allocationPlanExists)
            {
                throw new Exception("Allocation plan does not exist.");
            }

            if (request.PhaseId.HasValue)
            {
                var phaseExists = await _unitOfWork
                    .GetRepository<ExperimentPhase>()
                    .AnyAsync(p => p.PhaseId == request.PhaseId.Value);

                if (!phaseExists)
                {
                    throw new Exception("Experiment phase does not exist.");
                }
            }

            if (request.CreatedBy.HasValue)
            {
                var userExists = await _unitOfWork
                    .GetRepository<User>()
                    .AnyAsync(u => u.UserId == request.CreatedBy.Value);

                if (!userExists)
                {
                    throw new Exception("Created-by user does not exist.");
                }
            }

            if (request.AssignedHumanResourceId.HasValue)
            {
                var humanResourceExists = await _unitOfWork
                    .GetRepository<HumanResourceProfile>()
                    .AnyAsync(h => h.HumanResourceId == request.AssignedHumanResourceId.Value);

                if (!humanResourceExists)
                {
                    throw new Exception("Assigned human resource does not exist.");
                }
            }
        }

        private async Task<int?> GetHumanResourceIdByUserIdAsync(int userId)
        {
            var profile = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .FirstOrDefaultAsync(predicate: h => h.UserId == userId);

            return profile?.HumanResourceId;
        }

        private static ScheduleResponse MapToResponse(Schedule schedule)
        {
            return new ScheduleResponse
            {
                ScheduleId = schedule.ScheduleId,
                AllocationPlanId = schedule.AllocationPlanId,
                ExperimentId = schedule.AllocationPlan.ExperimentId,
                ExperimentName = schedule.AllocationPlan.Experiment.ExperimentName,
                PhaseId = schedule.PhaseId,
                PhaseName = schedule.Phase?.PhaseName,
                Title = schedule.Title,
                Description = schedule.Description,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                Status = schedule.Status,
                CreatedBy = schedule.CreatedBy,
                CreatedByName = schedule.CreatedByNavigation?.FullName,
                AssignedHumanResourceId = schedule.AssignedHumanResourceId,
                AssignedHumanResourceName = schedule.AssignedHumanResource?.User?.FullName,
                Notes = schedule.Notes,
                Priority = schedule.Priority,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt
            };
        }
    }
}

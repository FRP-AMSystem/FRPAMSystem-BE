using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.DomainEvents;
using FRPAMSystem.BusinessTier.DomainEvents.Events;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.Experiment;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class ExperimentService : IExperimentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly IClock _clock;

        public ExperimentService(
            IUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher,
            IClock clock)
        {
            _unitOfWork = unitOfWork;
            _domainEventDispatcher = domainEventDispatcher;
            _clock = clock;
        }

        public async Task<IPaginate<ExperimentResponse>> ViewAllExperimentsAsync(
            ExperimentFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<Experiment>()
                .GetQueryable()
                .Include(e => e.Researcher)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt);

            return await query
                .Select(e => new ExperimentResponse
                {
                    ExperimentId = e.ExperimentId,
                    ExperimentName = e.ExperimentName,
                    Description = e.Description,
                    ResearcherId = e.ResearcherId,
                    ResearcherName = e.Researcher.FullName,
                    ExpectStartDate = e.ExpectStartDate,
                    ExpectEndDate = e.ExpectEndDate,
                    Deadline = e.Deadline,
                    Priority = e.Priority,
                    Status = e.Status,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<ExperimentResponse?> GetExperimentByIdAsync(int id)
        {
            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(
                    predicate: e => e.ExperimentId == id,
                    include: query => query.Include(e => e.Researcher)
                );

            if (experiment == null)
            {
                return null;
            }

            return MapToResponse(experiment);
        }

        private static readonly Dictionary<ExperimentStatus, HashSet<ExperimentStatus>> AllowedTransitions = new()
        {
            [ExperimentStatus.Draft] = new() { ExperimentStatus.Submitted, ExperimentStatus.Cancelled },
            [ExperimentStatus.Submitted] = new() { ExperimentStatus.Planning, ExperimentStatus.Draft, ExperimentStatus.Cancelled },
            [ExperimentStatus.Planning] = new() { ExperimentStatus.Ready, ExperimentStatus.Cancelled },
            [ExperimentStatus.Ready] = new() { ExperimentStatus.Running, ExperimentStatus.Cancelled },
            [ExperimentStatus.Running] = new() { ExperimentStatus.Completed, ExperimentStatus.Cancelled },
            [ExperimentStatus.Completed] = new(),
            [ExperimentStatus.Cancelled] = new()
        };

        public async Task<ExperimentResponse> CreateExperimentAsync(ExperimentRequest request)
        {
            await ValidateRequestAsync(request);

            var experiment = new Experiment
            {
                ExperimentName = request.ExperimentName.Trim(),
                Description = request.Description,
                ResearcherId = request.ResearcherId,
                ExpectStartDate = request.ExpectStartDate,
                ExpectEndDate = request.ExpectEndDate,
                Deadline = request.Deadline,
                Priority = request.Priority,
                Status = ExperimentStatus.Draft.ToString()
            };

            await _unitOfWork.GetRepository<Experiment>().InsertAsync(experiment);
            await _unitOfWork.CommitAsync();

            await _domainEventDispatcher.DispatchAsync(new ExperimentCreatedEvent(
                experiment.ExperimentId,
                experiment.ExperimentName,
                experiment.ResearcherId,
                _clock.Now));

            return (await GetExperimentByIdAsync(experiment.ExperimentId))!;
        }

        public async Task<ExperimentResponse?> UpdateExperimentAsync(int id, ExperimentRequest request)
        {
            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(
                    predicate: e => e.ExperimentId == id,
                    asNoTracking: false
                );

            if (experiment == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(experiment.Status) && experiment.Status != ExperimentStatus.Draft.ToString())
            {
                throw new Exception("Only draft experiments can be edited.");
            }

            await ValidateRequestAsync(request, id);

            experiment.ExperimentName = request.ExperimentName.Trim();
            experiment.Description = request.Description;
            experiment.ResearcherId = request.ResearcherId;
            experiment.ExpectStartDate = request.ExpectStartDate;
            experiment.ExpectEndDate = request.ExpectEndDate;
            experiment.Deadline = request.Deadline;
            experiment.Priority = request.Priority;
            experiment.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<Experiment>().Update(experiment);
            await _unitOfWork.CommitAsync();

            return await GetExperimentByIdAsync(id);
        }

        public async Task<ExperimentResponse?> SubmitExperimentAsync(int id)
        {
            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(
                    predicate: e => e.ExperimentId == id,
                    asNoTracking: false
                );

            if (experiment == null)
            {
                return null;
            }

            if (experiment.Status != ExperimentStatus.Draft.ToString())
            {
                throw new Exception("Only draft experiments can be submitted.");
            }

            experiment.Status = ExperimentStatus.Submitted.ToString();
            experiment.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<Experiment>().Update(experiment);
            await _unitOfWork.CommitAsync();

            await _domainEventDispatcher.DispatchAsync(new ExperimentSubmittedEvent(
                experiment.ExperimentId,
                experiment.ExperimentName,
                experiment.ResearcherId,
                _clock.Now));

            return await GetExperimentByIdAsync(id);
        }

        public async Task<ExperimentResponse?> ApproveExperimentAsync(int id, int? currentUserId)
        {
            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(
                    predicate: e => e.ExperimentId == id,
                    asNoTracking: false
                );

            if (experiment == null) return null;

            if (experiment.Status != ExperimentStatus.Submitted.ToString())
            {
                throw new Exception("Only submitted experiments can be approved.");
            }

            experiment.Status = ExperimentStatus.Planning.ToString();
            experiment.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<Experiment>().Update(experiment);
            await _unitOfWork.CommitAsync();

            await _domainEventDispatcher.DispatchAsync(new ExperimentApprovedEvent(
                experiment.ExperimentId,
                experiment.ExperimentName,
                experiment.ResearcherId,
                currentUserId,
                _clock.Now));

            return await GetExperimentByIdAsync(id);
        }

        public async Task<ExperimentResponse?> RejectExperimentAsync(int id, int? currentUserId, string? reason)
        {
            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(
                    predicate: e => e.ExperimentId == id,
                    asNoTracking: false
                );

            if (experiment == null) return null;

            if (experiment.Status != ExperimentStatus.Submitted.ToString())
            {
                throw new Exception("Only submitted experiments can be rejected.");
            }

            experiment.Status = ExperimentStatus.Draft.ToString();
            experiment.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<Experiment>().Update(experiment);
            await _unitOfWork.CommitAsync();

            await _domainEventDispatcher.DispatchAsync(new ExperimentRejectedEvent(
                experiment.ExperimentId,
                experiment.ExperimentName,
                experiment.ResearcherId,
                currentUserId,
                reason,
                _clock.Now));

            return await GetExperimentByIdAsync(id);
        }

        public async Task<ExperimentResponse?> StartExperimentAsync(int id, int? currentUserId)
        {
            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(
                    predicate: e => e.ExperimentId == id,
                    asNoTracking: false
                );

            if (experiment == null) return null;

            if (experiment.Status != ExperimentStatus.Ready.ToString())
            {
                throw new Exception("Only ready experiments can be started.");
            }

            experiment.Status = ExperimentStatus.Running.ToString();
            experiment.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<Experiment>().Update(experiment);
            await _unitOfWork.CommitAsync();

            return await GetExperimentByIdAsync(id);
        }

        public async Task<ExperimentResponse?> CompleteExperimentAsync(int id, int? currentUserId)
        {
            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(
                    predicate: e => e.ExperimentId == id,
                    asNoTracking: false
                );

            if (experiment == null) return null;

            if (experiment.Status != ExperimentStatus.Running.ToString())
            {
                throw new Exception("Only running experiments can be completed.");
            }

            var hasInUseEquipment = await _unitOfWork
                .GetRepository<AllocationEquipmentDetail>()
                .AnyAsync(d => d.AllocationPlan!.ExperimentId == id &&
                               d.Status == AllocationDetailStatus.InUse.ToString());

            if (hasInUseEquipment)
            {
                throw new Exception("Cannot complete experiment while equipment is still in use. Please ensure all equipment is returned.");
            }

            var hasUnfinishedPhases = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .AnyAsync(p => p.ExperimentId == id &&
                               (p.Status == ExperimentPhaseStatus.Planned.ToString() ||
                                p.Status == ExperimentPhaseStatus.InProgress.ToString()));

            if (hasUnfinishedPhases)
            {
                throw new Exception("Cannot complete experiment while phases are still planned or in progress.");
            }

            experiment.Status = ExperimentStatus.Completed.ToString();
            experiment.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<Experiment>().Update(experiment);
            await _unitOfWork.CommitAsync();

            return await GetExperimentByIdAsync(id);
        }

        public async Task<ExperimentResponse?> CancelExperimentAsync(int id, int? currentUserId, string? reason = null)
        {
            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(
                    predicate: e => e.ExperimentId == id,
                    asNoTracking: false
                );

            if (experiment == null) return null;

            if (experiment.Status == ExperimentStatus.Completed.ToString() ||
                experiment.Status == ExperimentStatus.Cancelled.ToString())
            {
                throw new Exception("Completed or cancelled experiments cannot be cancelled.");
            }

            var unfinishedPhases = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .GetListAsync(
                    predicate: p => p.ExperimentId == id &&
                                    (p.Status == ExperimentPhaseStatus.Planned.ToString() ||
                                     p.Status == ExperimentPhaseStatus.InProgress.ToString()),
                    asNoTracking: false
                );

            foreach (var phase in unfinishedPhases)
            {
                phase.Status = ExperimentPhaseStatus.Cancelled.ToString();
                phase.UpdatedAt = _clock.Now;
                _unitOfWork.GetRepository<ExperimentPhase>().Update(phase);
            }

            experiment.Status = ExperimentStatus.Cancelled.ToString();
            experiment.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<Experiment>().Update(experiment);
            await _unitOfWork.CommitAsync();

            return await GetExperimentByIdAsync(id);
        }

        public async Task<bool> DeleteExperimentAsync(int id)
        {
            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(
                    predicate: e => e.ExperimentId == id,
                    asNoTracking: false
                );

            if (experiment == null)
            {
                return false;
            }

            if (experiment.Status != ExperimentStatus.Draft.ToString() &&
                experiment.Status != ExperimentStatus.Cancelled.ToString())
            {
                throw new Exception("Only draft or cancelled experiments can be deleted.");
            }

            var hasPlan = await _unitOfWork
                .GetRepository<AllocationPlan>()
                .AnyAsync(p => p.ExperimentId == id);

            if (hasPlan)
            {
                throw new Exception("Cannot delete an experiment that has associated allocation plans.");
            }

            _unitOfWork.GetRepository<Experiment>().Delete(experiment);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<ExperimentResponse?> UpdateExperimentStatusAsync(int id, UpdateExperimentStatusRequest request)
        {
            if (!Enum.TryParse<ExperimentStatus>(request.Status, true, out var targetStatus))
            {
                throw new Exception($"Invalid experiment status '{request.Status}'.");
            }

            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(
                    predicate: e => e.ExperimentId == id,
                    asNoTracking: false
                );

            if (experiment == null) return null;

            if (!Enum.TryParse<ExperimentStatus>(experiment.Status, true, out var currentStatus))
            {
                currentStatus = ExperimentStatus.Draft;
            }

            if (currentStatus == targetStatus)
            {
                return await GetExperimentByIdAsync(id);
            }

            if (!AllowedTransitions.TryGetValue(currentStatus, out var allowed) || !allowed.Contains(targetStatus))
            {
                throw new Exception($"Invalid status transition from {currentStatus} to {targetStatus}.");
            }

            if (targetStatus == ExperimentStatus.Completed)
            {
                var hasInUseEquipment = await _unitOfWork
                    .GetRepository<AllocationEquipmentDetail>()
                    .AnyAsync(d => d.AllocationPlan!.ExperimentId == id &&
                                   d.Status == AllocationDetailStatus.InUse.ToString());

                if (hasInUseEquipment)
                {
                    throw new Exception("Cannot complete experiment while equipment is still in use. Please ensure all equipment is returned.");
                }
            }

            experiment.Status = targetStatus.ToString();
            experiment.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<Experiment>().Update(experiment);
            await _unitOfWork.CommitAsync();

            return await GetExperimentByIdAsync(id);
        }

        private async Task ValidateRequestAsync(ExperimentRequest request, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(request.ExperimentName))
            {
                throw new Exception("Experiment name is required.");
            }

            if (request.ExpectEndDate < request.ExpectStartDate)
            {
                throw new Exception("Expect end date must be greater than or equal to expect start date.");
            }

            if (request.Priority < 1 || request.Priority > 4)
            {
                throw new Exception("Priority must be between 1 and 4.");
            }

            if (request.Deadline.HasValue && request.Deadline.Value < request.ExpectEndDate)
            {
                throw new Exception("Deadline must be greater than or equal to expect end date.");
            }

            var researcherExists = await _unitOfWork
                .GetRepository<User>()
                .AnyAsync(u => u.UserId == request.ResearcherId);

            if (!researcherExists)
            {
                throw new Exception("Researcher does not exist.");
            }

            var duplicateNameQuery = _unitOfWork
                .GetRepository<Experiment>()
                .GetQueryable()
                .Where(e => e.ExperimentName == request.ExperimentName.Trim());

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(e => e.ExperimentId != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                throw new Exception("Experiment name already exists.");
            }
        }

        private static ExperimentResponse MapToResponse(Experiment experiment)
        {
            return new ExperimentResponse
            {
                ExperimentId = experiment.ExperimentId,
                ExperimentName = experiment.ExperimentName,
                Description = experiment.Description,
                ResearcherId = experiment.ResearcherId,
                ResearcherName = experiment.Researcher?.FullName,
                ExpectStartDate = experiment.ExpectStartDate,
                ExpectEndDate = experiment.ExpectEndDate,
                Deadline = experiment.Deadline,
                Priority = experiment.Priority,
                Status = experiment.Status,
                CreatedAt = experiment.CreatedAt,
                UpdatedAt = experiment.UpdatedAt
            };
        }
    }
}

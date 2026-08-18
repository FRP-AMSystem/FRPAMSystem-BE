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
                Status = request.Status.ToString()
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
            await ValidateRequestAsync(request, id);

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

            experiment.ExperimentName = request.ExperimentName.Trim();
            experiment.Description = request.Description;
            experiment.ResearcherId = request.ResearcherId;
            experiment.ExpectStartDate = request.ExpectStartDate;
            experiment.ExpectEndDate = request.ExpectEndDate;
            experiment.Deadline = request.Deadline;
            experiment.Priority = request.Priority;
            experiment.Status = request.Status.ToString();

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

            experiment.Status = ExperimentStatus.Submitted.ToString();

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

            experiment.Status = ExperimentStatus.Planning.ToString(); // Or Ready

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

            experiment.Status = ExperimentStatus.Draft.ToString();

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

            _unitOfWork.GetRepository<Experiment>().Delete(experiment);
            await _unitOfWork.CommitAsync();

            return true;
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
                ResearcherName = experiment.Researcher.FullName,
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

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Enums;
using FRPAMSystem.BusinessTier.Payload.ExperimentPhase;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class ExperimentPhaseService : IExperimentPhaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public ExperimentPhaseService(IUnitOfWork unitOfWork, IClock clock)
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<IPaginate<ExperimentPhaseResponse>> ViewAllExperimentPhasesAsync(
            ExperimentPhaseFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<ExperimentPhase>()
                .GetQueryable()
                .Include(p => p.Experiment)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderBy(p => p.ExperimentId)
                .ThenBy(p => p.PhaseOrder);

            return await query
                .Select(p => new ExperimentPhaseResponse
                {
                    PhaseId = p.PhaseId,
                    ExperimentId = p.ExperimentId,
                    ExperimentName = p.Experiment != null ? p.Experiment.ExperimentName : null!,
                    PhaseName = p.PhaseName,
                    PhaseDescription = p.PhaseDescription,
                    PhaseOrder = p.PhaseOrder,
                    ExpectedStartDate = p.ExpectedStartDate,
                    ExpectedEndDate = p.ExpectedEndDate,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<ExperimentPhaseResponse?> GetExperimentPhaseByIdAsync(int id)
        {
            var phase = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .FirstOrDefaultAsync(
                    predicate: p => p.PhaseId == id,
                    include: query => query.Include(p => p.Experiment)
                );

            if (phase == null)
            {
                return null;
            }

            return MapToResponse(phase);
        }

        public async Task<ExperimentPhaseResponse> CreateExperimentPhaseAsync(ExperimentPhaseRequest request)
        {
            await ValidateRequestAsync(request);

            var phase = new ExperimentPhase
            {
                ExperimentId = request.ExperimentId,
                PhaseName = request.PhaseName.Trim(),
                PhaseDescription = request.PhaseDescription,
                PhaseOrder = request.PhaseOrder,
                ExpectedStartDate = request.ExpectedStartDate,
                ExpectedEndDate = request.ExpectedEndDate,
                Status = ExperimentPhaseStatus.Planned.ToString(),
                CreatedAt = _clock.Now,
                UpdatedAt = _clock.Now
            };

            await _unitOfWork.GetRepository<ExperimentPhase>().InsertAsync(phase);
            await _unitOfWork.CommitAsync();

            return (await GetExperimentPhaseByIdAsync(phase.PhaseId))!;
        }

        public async Task<ExperimentPhaseResponse?> UpdateExperimentPhaseAsync(
            int id,
            ExperimentPhaseRequest request)
        {
            var phase = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .FirstOrDefaultAsync(
                    predicate: p => p.PhaseId == id,
                    include: query => query.Include(p => p.Experiment),
                    asNoTracking: false
                );

            if (phase == null)
            {
                return null;
            }

            if (phase.Experiment != null && phase.Experiment.Status != ExperimentStatus.Draft.ToString())
            {
                throw new Exception("Phases can only be edited when the experiment is in draft status.");
            }

            await ValidateRequestAsync(request, id);

            phase.ExperimentId = request.ExperimentId;
            phase.PhaseName = request.PhaseName.Trim();
            phase.PhaseDescription = request.PhaseDescription;
            phase.PhaseOrder = request.PhaseOrder;
            phase.ExpectedStartDate = request.ExpectedStartDate;
            phase.ExpectedEndDate = request.ExpectedEndDate;
            phase.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<ExperimentPhase>().Update(phase);
            await _unitOfWork.CommitAsync();

            return await GetExperimentPhaseByIdAsync(id);
        }

        public async Task<bool> DeleteExperimentPhaseAsync(int id)
        {
            var phase = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .FirstOrDefaultAsync(
                    predicate: p => p.PhaseId == id,
                    include: query => query.Include(p => p.Experiment),
                    asNoTracking: false
                );

            if (phase == null)
            {
                return false;
            }

            if (phase.Experiment != null && phase.Experiment.Status != ExperimentStatus.Draft.ToString())
            {
                throw new Exception("Phases can only be deleted when the experiment is in draft status.");
            }

            if (phase.Status != ExperimentPhaseStatus.Planned.ToString())
            {
                throw new Exception("Only planned phases can be deleted.");
            }

            var hasEquipmentReqs = await _unitOfWork
                .GetRepository<PhaseEquipmentRequirement>()
                .AnyAsync(r => r.PhaseId == id);

            var hasHumanReqs = await _unitOfWork
                .GetRepository<PhaseHumanRequirement>()
                .AnyAsync(r => r.PhaseId == id);

            if (hasEquipmentReqs || hasHumanReqs)
            {
                throw new Exception("Cannot delete phase that has associated equipment or human requirements.");
            }

            _unitOfWork.GetRepository<ExperimentPhase>().Delete(phase);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<ExperimentPhaseResponse?> StartExperimentPhaseAsync(int id, int? currentUserId)
        {
            var phase = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .FirstOrDefaultAsync(
                    predicate: p => p.PhaseId == id,
                    include: query => query.Include(p => p.Experiment),
                    asNoTracking: false
                );

            if (phase == null) return null;

            if (phase.Experiment == null || phase.Experiment.Status != ExperimentStatus.Running.ToString())
            {
                throw new Exception("Cannot start phase when experiment is not running.");
            }

            if (phase.Status != ExperimentPhaseStatus.Planned.ToString())
            {
                throw new Exception("Only planned phases can be started.");
            }

            var hasUnfinishedPreviousPhase = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .AnyAsync(p => p.ExperimentId == phase.ExperimentId &&
                               p.PhaseOrder < phase.PhaseOrder &&
                               p.Status != ExperimentPhaseStatus.Completed.ToString() &&
                               p.Status != ExperimentPhaseStatus.Cancelled.ToString());

            if (hasUnfinishedPreviousPhase)
            {
                throw new Exception("Previous phases must be completed or cancelled before starting this phase.");
            }

            phase.Status = ExperimentPhaseStatus.InProgress.ToString();
            phase.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<ExperimentPhase>().Update(phase);
            await _unitOfWork.CommitAsync();

            return await GetExperimentPhaseByIdAsync(id);
        }

        public async Task<ExperimentPhaseResponse?> CompleteExperimentPhaseAsync(int id, int? currentUserId)
        {
            var phase = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .FirstOrDefaultAsync(
                    predicate: p => p.PhaseId == id,
                    include: query => query.Include(p => p.Experiment),
                    asNoTracking: false
                );

            if (phase == null) return null;

            if (phase.Experiment == null || phase.Experiment.Status != ExperimentStatus.Running.ToString())
            {
                throw new Exception("Cannot complete phase when experiment is not running.");
            }

            if (phase.Status != ExperimentPhaseStatus.InProgress.ToString())
            {
                throw new Exception("Only in-progress phases can be completed.");
            }

            phase.Status = ExperimentPhaseStatus.Completed.ToString();
            phase.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<ExperimentPhase>().Update(phase);
            await _unitOfWork.CommitAsync();

            return await GetExperimentPhaseByIdAsync(id);
        }

        public async Task<ExperimentPhaseResponse?> CancelExperimentPhaseAsync(int id, int? currentUserId, string? reason = null)
        {
            var phase = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .FirstOrDefaultAsync(
                    predicate: p => p.PhaseId == id,
                    include: query => query.Include(p => p.Experiment),
                    asNoTracking: false
                );

            if (phase == null) return null;

            if (phase.Status == ExperimentPhaseStatus.Completed.ToString() ||
                phase.Status == ExperimentPhaseStatus.Cancelled.ToString())
            {
                throw new Exception("Completed or cancelled phases cannot be cancelled.");
            }

            phase.Status = ExperimentPhaseStatus.Cancelled.ToString();
            phase.UpdatedAt = _clock.Now;

            _unitOfWork.GetRepository<ExperimentPhase>().Update(phase);
            await _unitOfWork.CommitAsync();

            return await GetExperimentPhaseByIdAsync(id);
        }

        private async Task ValidateRequestAsync(ExperimentPhaseRequest request, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(request.PhaseName))
            {
                throw new Exception("Phase name is required.");
            }

            if (request.PhaseOrder <= 0)
            {
                throw new Exception("Phase order must be greater than 0.");
            }

            if (request.ExpectedEndDate < request.ExpectedStartDate)
            {
                throw new Exception("Expected end date must be greater than or equal to expected start date.");
            }

            var experiment = await _unitOfWork
                .GetRepository<Experiment>()
                .FirstOrDefaultAsync(predicate: e => e.ExperimentId == request.ExperimentId);

            if (experiment == null)
            {
                throw new Exception("Experiment does not exist.");
            }

            if (experiment.Status != ExperimentStatus.Draft.ToString())
            {
                throw new Exception("Phases can only be configured when the experiment is in draft status.");
            }

            var duplicateOrderQuery = _unitOfWork
                .GetRepository<ExperimentPhase>()
                .GetQueryable()
                .Where(p =>
                    p.ExperimentId == request.ExperimentId &&
                    p.PhaseOrder == request.PhaseOrder);

            if (excludeId.HasValue)
            {
                duplicateOrderQuery = duplicateOrderQuery.Where(p => p.PhaseId != excludeId.Value);
            }

            if (await duplicateOrderQuery.AnyAsync())
            {
                throw new Exception("Phase order already exists for this experiment.");
            }
        }

        private static ExperimentPhaseResponse MapToResponse(ExperimentPhase phase)
        {
            return new ExperimentPhaseResponse
            {
                PhaseId = phase.PhaseId,
                ExperimentId = phase.ExperimentId,
                ExperimentName = phase.Experiment != null ? phase.Experiment.ExperimentName : null!,
                PhaseName = phase.PhaseName,
                PhaseDescription = phase.PhaseDescription,
                PhaseOrder = phase.PhaseOrder,
                ExpectedStartDate = phase.ExpectedStartDate,
                ExpectedEndDate = phase.ExpectedEndDate,
                Status = phase.Status,
                CreatedAt = phase.CreatedAt,
                UpdatedAt = phase.UpdatedAt
            };
        }
    }
}

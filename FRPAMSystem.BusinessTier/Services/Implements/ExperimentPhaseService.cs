using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.ExperimentPhase;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class ExperimentPhaseService : IExperimentPhaseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExperimentPhaseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                    ExperimentName = p.Experiment.ExperimentName,
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
                Status = request.Status.ToString(),
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<ExperimentPhase>().InsertAsync(phase);
            await _unitOfWork.CommitAsync();

            return (await GetExperimentPhaseByIdAsync(phase.PhaseId))!;
        }

        public async Task<ExperimentPhaseResponse?> UpdateExperimentPhaseAsync(
            int id,
            ExperimentPhaseRequest request)
        {
            await ValidateRequestAsync(request, id);

            var phase = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .FirstOrDefaultAsync(
                    predicate: p => p.PhaseId == id,
                    asNoTracking: false
                );

            if (phase == null)
            {
                return null;
            }

            phase.ExperimentId = request.ExperimentId;
            phase.PhaseName = request.PhaseName.Trim();
            phase.PhaseDescription = request.PhaseDescription;
            phase.PhaseOrder = request.PhaseOrder;
            phase.ExpectedStartDate = request.ExpectedStartDate;
            phase.ExpectedEndDate = request.ExpectedEndDate;
            phase.Status = request.Status.ToString();
            phase.UpdatedAt = DateTime.Now;

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
                    asNoTracking: false
                );

            if (phase == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<ExperimentPhase>().Delete(phase);
            await _unitOfWork.CommitAsync();

            return true;
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

            var experimentExists = await _unitOfWork
                .GetRepository<Experiment>()
                .AnyAsync(e => e.ExperimentId == request.ExperimentId);

            if (!experimentExists)
            {
                throw new Exception("Experiment does not exist.");
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
                ExperimentName = phase.Experiment.ExperimentName,
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

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.PhaseEquipmentRequirement;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.BusinessTier.Validators;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class PhaseEquipmentRequirementService : IPhaseEquipmentRequirementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PhaseEquipmentRequirementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<PhaseEquipmentRequirementResponse>> ViewAllPhaseEquipmentRequirementsAsync(
            PhaseEquipmentRequirementFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<PhaseEquipmentRequirement>()
                .GetQueryable()
                .Include(r => r.Phase)
                    .ThenInclude(p => p.Experiment)
                .Include(r => r.EquipmentType)
                    .ThenInclude(t => t.EquipmentCategory)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt);

            return await query
                .Select(r => new PhaseEquipmentRequirementResponse
                {
                    PhaseEquipmentReqId = r.PhaseEquipmentReqId,
                    PhaseId = r.PhaseId,
                    PhaseName = r.Phase.PhaseName,
                    ExperimentId = r.Phase.ExperimentId,
                    ExperimentName = r.Phase.Experiment.ExperimentName,
                    EquipmentTypeId = r.EquipmentTypeId,
                    EquipmentTypeName = r.EquipmentType.Name,
                    EquipmentCategoryId = r.EquipmentType.EquipmentCategoryId,
                    EquipmentCategoryName = r.EquipmentType.EquipmentCategory.CategoryName,
                    Quantity = r.Quantity,
                    Note = r.Note,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<PhaseEquipmentRequirementResponse?> GetPhaseEquipmentRequirementByIdAsync(int id)
        {
            var requirement = await _unitOfWork
                .GetRepository<PhaseEquipmentRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.PhaseEquipmentReqId == id,
                    include: query => query
                        .Include(r => r.Phase)
                            .ThenInclude(p => p.Experiment)
                        .Include(r => r.EquipmentType)
                            .ThenInclude(t => t.EquipmentCategory)
                );

            return requirement == null ? null : MapToResponse(requirement);
        }

        public async Task<PhaseEquipmentRequirementResponse> CreatePhaseEquipmentRequirementAsync(
            PhaseEquipmentRequirementRequest request)
        {
            await ValidateRequestAsync(request);

            var requirement = new PhaseEquipmentRequirement
            {
                PhaseId = request.PhaseId,
                EquipmentTypeId = request.EquipmentTypeId,
                Quantity = request.Quantity,
                Note = request.Note,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<PhaseEquipmentRequirement>()
                .InsertAsync(requirement);

            await _unitOfWork.CommitAsync();

            return (await GetPhaseEquipmentRequirementByIdAsync(requirement.PhaseEquipmentReqId))!;
        }

        public async Task<PhaseEquipmentRequirementResponse?> UpdatePhaseEquipmentRequirementAsync(
            int id,
            PhaseEquipmentRequirementRequest request)
        {
            await ValidateRequestAsync(request);

            var requirement = await _unitOfWork
                .GetRepository<PhaseEquipmentRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.PhaseEquipmentReqId == id,
                    asNoTracking: false
                );

            if (requirement == null)
            {
                return null;
            }

            requirement.PhaseId = request.PhaseId;
            requirement.EquipmentTypeId = request.EquipmentTypeId;
            requirement.Quantity = request.Quantity;
            requirement.Note = request.Note;
            requirement.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<PhaseEquipmentRequirement>().Update(requirement);
            await _unitOfWork.CommitAsync();

            return await GetPhaseEquipmentRequirementByIdAsync(id);
        }

        public async Task<bool> DeletePhaseEquipmentRequirementAsync(int id)
        {
            var requirement = await _unitOfWork
                .GetRepository<PhaseEquipmentRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.PhaseEquipmentReqId == id,
                    asNoTracking: false
                );

            if (requirement == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<PhaseEquipmentRequirement>().Delete(requirement);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task ValidateRequestAsync(PhaseEquipmentRequirementRequest request)
        {
            PhaseEquipmentRequirementValidator.Validate(request);

            var phaseExists = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .AnyAsync(p => p.PhaseId == request.PhaseId);

            if (!phaseExists)
            {
                throw new Exception("Experiment phase does not exist.");
            }

            var equipmentTypeExists = await _unitOfWork
                .GetRepository<EquipmentType>()
                .AnyAsync(e => e.EquipmentTypeId == request.EquipmentTypeId);

            if (!equipmentTypeExists)
            {
                throw new Exception("Equipment type does not exist.");
            }
        }

        private static PhaseEquipmentRequirementResponse MapToResponse(
            PhaseEquipmentRequirement requirement)
        {
            return new PhaseEquipmentRequirementResponse
            {
                PhaseEquipmentReqId = requirement.PhaseEquipmentReqId,
                PhaseId = requirement.PhaseId,
                PhaseName = requirement.Phase.PhaseName,
                ExperimentId = requirement.Phase.ExperimentId,
                ExperimentName = requirement.Phase.Experiment.ExperimentName,
                EquipmentTypeId = requirement.EquipmentTypeId,
                EquipmentTypeName = requirement.EquipmentType.Name,
                EquipmentCategoryId = requirement.EquipmentType.EquipmentCategoryId,
                EquipmentCategoryName = requirement.EquipmentType.EquipmentCategory.CategoryName,
                Quantity = requirement.Quantity,
                Note = requirement.Note,
                CreatedAt = requirement.CreatedAt,
                UpdatedAt = requirement.UpdatedAt
            };
        }
    }
}

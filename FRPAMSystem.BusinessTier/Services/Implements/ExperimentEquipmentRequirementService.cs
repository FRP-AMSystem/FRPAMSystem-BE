using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.ExperimentEquipmentRequirement;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class ExperimentEquipmentRequirementService : IExperimentEquipmentRequirementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExperimentEquipmentRequirementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<ExperimentEquipmentRequirementResponse>> ViewAllAsync(
            ExperimentEquipmentRequirementFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<ExperimentEquipmentRequirement>()
                .GetQueryable()
                .Include(r => r.Experiment)
                .Include(r => r.EquipmentType)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt);

            return await query
                .Select(r => new ExperimentEquipmentRequirementResponse
                {
                    ExpEquipmentReqId = r.ExpEquipmentReqId,
                    ExperimentId = r.ExperimentId,
                    ExperimentName = r.Experiment.ExperimentName,
                    EquipmentTypeId = r.EquipmentTypeId,
                    EquipmentTypeName = r.EquipmentType.Name,
                    Quantity = r.Quantity,
                    AllowSubstitute = r.AllowSubstitute,
                    MinAcceptableEfficiency = r.MinAcceptableEfficiency,
                    Note = r.Note,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<ExperimentEquipmentRequirementResponse?> GetByIdAsync(int id)
        {
            var requirement = await _unitOfWork
                .GetRepository<ExperimentEquipmentRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExpEquipmentReqId == id,
                    include: query => query
                        .Include(r => r.Experiment)
                        .Include(r => r.EquipmentType)
                );

            return requirement == null ? null : MapToResponse(requirement);
        }

        public async Task<ExperimentEquipmentRequirementResponse> CreateAsync(
            ExperimentEquipmentRequirementRequest request)
        {
            await ValidateRequestAsync(request);

            var requirement = new ExperimentEquipmentRequirement
            {
                ExperimentId = request.ExperimentId,
                EquipmentTypeId = request.EquipmentTypeId,
                Quantity = request.Quantity,
                AllowSubstitute = request.AllowSubstitute,
                MinAcceptableEfficiency = request.MinAcceptableEfficiency,
                Note = request.Note,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<ExperimentEquipmentRequirement>().InsertAsync(requirement);
            await _unitOfWork.CommitAsync();

            return (await GetByIdAsync(requirement.ExpEquipmentReqId))!;
        }

        public async Task<ExperimentEquipmentRequirementResponse?> UpdateAsync(
            int id,
            ExperimentEquipmentRequirementRequest request)
        {
            await ValidateRequestAsync(request);

            var requirement = await _unitOfWork
                .GetRepository<ExperimentEquipmentRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExpEquipmentReqId == id,
                    asNoTracking: false
                );

            if (requirement == null)
            {
                return null;
            }

            requirement.ExperimentId = request.ExperimentId;
            requirement.EquipmentTypeId = request.EquipmentTypeId;
            requirement.Quantity = request.Quantity;
            requirement.AllowSubstitute = request.AllowSubstitute;
            requirement.MinAcceptableEfficiency = request.MinAcceptableEfficiency;
            requirement.Note = request.Note;
            requirement.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<ExperimentEquipmentRequirement>().Update(requirement);
            await _unitOfWork.CommitAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var requirement = await _unitOfWork
                .GetRepository<ExperimentEquipmentRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExpEquipmentReqId == id,
                    asNoTracking: false
                );

            if (requirement == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<ExperimentEquipmentRequirement>().Delete(requirement);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task ValidateRequestAsync(ExperimentEquipmentRequirementRequest request)
        {
            if (request.Quantity <= 0)
            {
                throw new Exception("Quantity must be greater than 0.");
            }

            if (request.MinAcceptableEfficiency.HasValue &&
                (request.MinAcceptableEfficiency.Value <= 0 || request.MinAcceptableEfficiency.Value > 1))
            {
                throw new Exception("Min acceptable efficiency must be between 0 and 1.");
            }

            var experimentExists = await _unitOfWork
                .GetRepository<Experiment>()
                .AnyAsync(e => e.ExperimentId == request.ExperimentId);

            if (!experimentExists)
            {
                throw new Exception("Experiment does not exist.");
            }

            var equipmentTypeExists = await _unitOfWork
                .GetRepository<EquipmentType>()
                .AnyAsync(e => e.EquipmentTypeId == request.EquipmentTypeId);

            if (!equipmentTypeExists)
            {
                throw new Exception("Equipment type does not exist.");
            }
        }

        private static ExperimentEquipmentRequirementResponse MapToResponse(
            ExperimentEquipmentRequirement requirement)
        {
            return new ExperimentEquipmentRequirementResponse
            {
                ExpEquipmentReqId = requirement.ExpEquipmentReqId,
                ExperimentId = requirement.ExperimentId,
                ExperimentName = requirement.Experiment.ExperimentName,
                EquipmentTypeId = requirement.EquipmentTypeId,
                EquipmentTypeName = requirement.EquipmentType.Name,
                Quantity = requirement.Quantity,
                AllowSubstitute = requirement.AllowSubstitute,
                MinAcceptableEfficiency = requirement.MinAcceptableEfficiency,
                Note = requirement.Note,
                CreatedAt = requirement.CreatedAt,
                UpdatedAt = requirement.UpdatedAt
            };
        }
    }
}

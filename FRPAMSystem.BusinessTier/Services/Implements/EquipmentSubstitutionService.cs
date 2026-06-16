using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentSubstitution;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class EquipmentSubstitutionService : IEquipmentSubstitutionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EquipmentSubstitutionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<EquipmentSubstitutionResponse>> ViewAllAsync(
            EquipmentSubstitutionFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<EquipmentSubstitution>()
                .GetQueryable()
                .Include(s => s.PrimaryEquipmentType)
                .Include(s => s.SubEquipmentType)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt);

            return await query
                .Select(s => new EquipmentSubstitutionResponse
                {
                    EquipmentSubId = s.EquipmentSubId,
                    PrimaryEquipmentTypeId = s.PrimaryEquipmentTypeId,
                    PrimaryEquipmentTypeName = s.PrimaryEquipmentType.Name,
                    SubEquipmentTypeId = s.SubEquipmentTypeId,
                    SubEquipmentTypeName = s.SubEquipmentType.Name,
                    EfficiencyRate = s.EfficiencyRate,
                    TimeMultiplier = s.TimeMultiplier,
                    Note = s.Note,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<EquipmentSubstitutionResponse?> GetByIdAsync(int id)
        {
            var substitution = await _unitOfWork
                .GetRepository<EquipmentSubstitution>()
                .FirstOrDefaultAsync(
                    predicate: s => s.EquipmentSubId == id,
                    include: query => query
                        .Include(s => s.PrimaryEquipmentType)
                        .Include(s => s.SubEquipmentType)
                );

            return substitution == null ? null : MapToResponse(substitution);
        }

        public async Task<EquipmentSubstitutionResponse> CreateAsync(EquipmentSubstitutionRequest request)
        {
            await ValidateRequestAsync(request, null);

            var substitution = new EquipmentSubstitution
            {
                PrimaryEquipmentTypeId = request.PrimaryEquipmentTypeId,
                SubEquipmentTypeId = request.SubEquipmentTypeId,
                EfficiencyRate = request.EfficiencyRate,
                TimeMultiplier = request.TimeMultiplier,
                Note = request.Note,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<EquipmentSubstitution>().InsertAsync(substitution);
            await _unitOfWork.CommitAsync();

            return (await GetByIdAsync(substitution.EquipmentSubId))!;
        }

        public async Task<EquipmentSubstitutionResponse?> UpdateAsync(
            int id,
            EquipmentSubstitutionRequest request)
        {
            await ValidateRequestAsync(request, id);

            var substitution = await _unitOfWork
                .GetRepository<EquipmentSubstitution>()
                .FirstOrDefaultAsync(
                    predicate: s => s.EquipmentSubId == id,
                    asNoTracking: false
                );

            if (substitution == null)
            {
                return null;
            }

            substitution.PrimaryEquipmentTypeId = request.PrimaryEquipmentTypeId;
            substitution.SubEquipmentTypeId = request.SubEquipmentTypeId;
            substitution.EfficiencyRate = request.EfficiencyRate;
            substitution.TimeMultiplier = request.TimeMultiplier;
            substitution.Note = request.Note;
            substitution.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<EquipmentSubstitution>().Update(substitution);
            await _unitOfWork.CommitAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var substitution = await _unitOfWork
                .GetRepository<EquipmentSubstitution>()
                .FirstOrDefaultAsync(
                    predicate: s => s.EquipmentSubId == id,
                    asNoTracking: false
                );

            if (substitution == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<EquipmentSubstitution>().Delete(substitution);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task ValidateRequestAsync(EquipmentSubstitutionRequest request, int? excludeId)
        {
            if (request.PrimaryEquipmentTypeId == request.SubEquipmentTypeId)
            {
                throw new Exception("Primary equipment type and substitute equipment type must be different.");
            }

            if (request.EfficiencyRate <= 0 || request.EfficiencyRate > 1)
            {
                throw new Exception("Efficiency rate must be between 0 and 1.");
            }

            if (request.TimeMultiplier < 1)
            {
                throw new Exception("Time multiplier must be greater than or equal to 1.");
            }

            var primaryExists = await _unitOfWork
                .GetRepository<EquipmentType>()
                .AnyAsync(t => t.EquipmentTypeId == request.PrimaryEquipmentTypeId);

            if (!primaryExists)
            {
                throw new Exception("Primary equipment type does not exist.");
            }

            var subExists = await _unitOfWork
                .GetRepository<EquipmentType>()
                .AnyAsync(t => t.EquipmentTypeId == request.SubEquipmentTypeId);

            if (!subExists)
            {
                throw new Exception("Substitute equipment type does not exist.");
            }

            var duplicate = await _unitOfWork
                .GetRepository<EquipmentSubstitution>()
                .AnyAsync(s =>
                    s.PrimaryEquipmentTypeId == request.PrimaryEquipmentTypeId &&
                    s.SubEquipmentTypeId == request.SubEquipmentTypeId &&
                    (!excludeId.HasValue || s.EquipmentSubId != excludeId.Value));

            if (duplicate)
            {
                throw new Exception("Equipment substitution for this pair already exists.");
            }
        }

        private static EquipmentSubstitutionResponse MapToResponse(EquipmentSubstitution substitution)
        {
            return new EquipmentSubstitutionResponse
            {
                EquipmentSubId = substitution.EquipmentSubId,
                PrimaryEquipmentTypeId = substitution.PrimaryEquipmentTypeId,
                PrimaryEquipmentTypeName = substitution.PrimaryEquipmentType.Name,
                SubEquipmentTypeId = substitution.SubEquipmentTypeId,
                SubEquipmentTypeName = substitution.SubEquipmentType.Name,
                EfficiencyRate = substitution.EfficiencyRate,
                TimeMultiplier = substitution.TimeMultiplier,
                Note = substitution.Note,
                CreatedAt = substitution.CreatedAt,
                UpdatedAt = substitution.UpdatedAt
            };
        }
    }
}

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.ExperimentLandRequirement;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class ExperimentLandRequirementService : IExperimentLandRequirementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExperimentLandRequirementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<ExperimentLandRequirementResponse>> ViewAllAsync(
            ExperimentLandRequirementFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<ExperimentLandRequirement>()
                .GetQueryable()
                .Include(r => r.Experiment)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt);

            return await query
                .Select(r => new ExperimentLandRequirementResponse
                {
                    ExpLandReqId = r.ExpLandReqId,
                    ExperimentId = r.ExperimentId,
                    ExperimentName = r.Experiment.ExperimentName,
                    RequiredArea = r.RequiredArea,
                    RequiredSoilType = r.RequiredSoilType,
                    Note = r.Note,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<ExperimentLandRequirementResponse?> GetByIdAsync(int id)
        {
            var requirement = await _unitOfWork
                .GetRepository<ExperimentLandRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExpLandReqId == id,
                    include: query => query.Include(r => r.Experiment)
                );

            return requirement == null ? null : MapToResponse(requirement);
        }

        public async Task<ExperimentLandRequirementResponse> CreateAsync(ExperimentLandRequirementRequest request)
        {
            await ValidateRequestAsync(request);

            var requirement = new ExperimentLandRequirement
            {
                ExperimentId = request.ExperimentId,
                RequiredArea = request.RequiredArea,
                RequiredSoilType = request.RequiredSoilType.Trim(),
                Note = request.Note,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<ExperimentLandRequirement>().InsertAsync(requirement);
            await _unitOfWork.CommitAsync();

            return (await GetByIdAsync(requirement.ExpLandReqId))!;
        }

        public async Task<ExperimentLandRequirementResponse?> UpdateAsync(
            int id,
            ExperimentLandRequirementRequest request)
        {
            await ValidateRequestAsync(request);

            var requirement = await _unitOfWork
                .GetRepository<ExperimentLandRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExpLandReqId == id,
                    asNoTracking: false
                );

            if (requirement == null)
            {
                return null;
            }

            requirement.ExperimentId = request.ExperimentId;
            requirement.RequiredArea = request.RequiredArea;
            requirement.RequiredSoilType = request.RequiredSoilType.Trim();
            requirement.Note = request.Note;
            requirement.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<ExperimentLandRequirement>().Update(requirement);
            await _unitOfWork.CommitAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var requirement = await _unitOfWork
                .GetRepository<ExperimentLandRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExpLandReqId == id,
                    asNoTracking: false
                );

            if (requirement == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<ExperimentLandRequirement>().Delete(requirement);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task ValidateRequestAsync(ExperimentLandRequirementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RequiredSoilType))
            {
                throw new Exception("Required soil type is required.");
            }

            if (request.RequiredArea <= 0)
            {
                throw new Exception("Required area must be greater than 0.");
            }

            var experimentExists = await _unitOfWork
                .GetRepository<Experiment>()
                .AnyAsync(e => e.ExperimentId == request.ExperimentId);

            if (!experimentExists)
            {
                throw new Exception("Experiment does not exist.");
            }
        }

        private static ExperimentLandRequirementResponse MapToResponse(ExperimentLandRequirement requirement)
        {
            return new ExperimentLandRequirementResponse
            {
                ExpLandReqId = requirement.ExpLandReqId,
                ExperimentId = requirement.ExperimentId,
                ExperimentName = requirement.Experiment.ExperimentName,
                RequiredArea = requirement.RequiredArea,
                RequiredSoilType = requirement.RequiredSoilType,
                Note = requirement.Note,
                CreatedAt = requirement.CreatedAt,
                UpdatedAt = requirement.UpdatedAt
            };
        }
    }
}

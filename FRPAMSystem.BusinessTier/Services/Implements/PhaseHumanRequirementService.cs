using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.PhaseHumanRequirement;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.BusinessTier.Validators;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class PhaseHumanRequirementService : IPhaseHumanRequirementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PhaseHumanRequirementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<PhaseHumanRequirementResponse>> ViewAllPhaseHumanRequirementsAsync(
            PhaseHumanRequirementFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<PhaseHumanRequirement>()
                .GetQueryable()
                .Include(r => r.Phase)
                    .ThenInclude(p => p.Experiment)
                .Include(r => r.Role)
                .Include(r => r.RequiredSkill)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt);

            return await query
                .Select(r => new PhaseHumanRequirementResponse
                {
                    PhaseHumanReqId = r.PhaseHumanReqId,
                    PhaseId = r.PhaseId,
                    PhaseName = r.Phase.PhaseName,
                    ExperimentId = r.Phase.ExperimentId,
                    ExperimentName = r.Phase.Experiment.ExperimentName,
                    RoleId = r.RoleId,
                    RoleName = r.Role.RoleName,
                    Quantity = r.Quantity,
                    RequiredSkillId = r.RequiredSkillId,
                    RequiredSkillName = r.RequiredSkill != null
                        ? r.RequiredSkill.SkillName
                        : null,
                    Note = r.Note,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<PhaseHumanRequirementResponse?> GetPhaseHumanRequirementByIdAsync(int id)
        {
            var requirement = await _unitOfWork
                .GetRepository<PhaseHumanRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.PhaseHumanReqId == id,
                    include: query => query
                        .Include(r => r.Phase)
                            .ThenInclude(p => p.Experiment)
                        .Include(r => r.Role)
                        .Include(r => r.RequiredSkill)
                );

            return requirement == null ? null : MapToResponse(requirement);
        }

        public async Task<PhaseHumanRequirementResponse> CreatePhaseHumanRequirementAsync(
            PhaseHumanRequirementRequest request)
        {
            await ValidateRequestAsync(request);

            var requirement = new PhaseHumanRequirement
            {
                PhaseId = request.PhaseId,
                RoleId = request.RoleId,
                Quantity = request.Quantity,
                RequiredSkillId = request.RequiredSkillId,
                Note = request.Note,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<PhaseHumanRequirement>()
                .InsertAsync(requirement);

            await _unitOfWork.CommitAsync();

            return (await GetPhaseHumanRequirementByIdAsync(requirement.PhaseHumanReqId))!;
        }

        public async Task<PhaseHumanRequirementResponse?> UpdatePhaseHumanRequirementAsync(
            int id,
            PhaseHumanRequirementRequest request)
        {
            await ValidateRequestAsync(request);

            var requirement = await _unitOfWork
                .GetRepository<PhaseHumanRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.PhaseHumanReqId == id,
                    asNoTracking: false
                );

            if (requirement == null)
            {
                return null;
            }

            requirement.PhaseId = request.PhaseId;
            requirement.RoleId = request.RoleId;
            requirement.Quantity = request.Quantity;
            requirement.RequiredSkillId = request.RequiredSkillId;
            requirement.Note = request.Note;
            requirement.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<PhaseHumanRequirement>().Update(requirement);
            await _unitOfWork.CommitAsync();

            return await GetPhaseHumanRequirementByIdAsync(id);
        }

        public async Task<bool> DeletePhaseHumanRequirementAsync(int id)
        {
            var requirement = await _unitOfWork
                .GetRepository<PhaseHumanRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.PhaseHumanReqId == id,
                    asNoTracking: false
                );

            if (requirement == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<PhaseHumanRequirement>().Delete(requirement);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task ValidateRequestAsync(PhaseHumanRequirementRequest request)
        {
            PhaseHumanRequirementValidator.Validate(request);

            var phaseExists = await _unitOfWork
                .GetRepository<ExperimentPhase>()
                .AnyAsync(p => p.PhaseId == request.PhaseId);

            if (!phaseExists)
            {
                throw new Exception("Experiment phase does not exist.");
            }

            var roleExists = await _unitOfWork
                .GetRepository<Role>()
                .AnyAsync(r => r.RoleId == request.RoleId);

            if (!roleExists)
            {
                throw new Exception("Role does not exist.");
            }

            if (request.RequiredSkillId.HasValue)
            {
                var skillExists = await _unitOfWork
                    .GetRepository<Skill>()
                    .AnyAsync(s => s.SkillId == request.RequiredSkillId.Value);

                if (!skillExists)
                {
                    throw new Exception("Required skill does not exist.");
                }
            }
        }

        private static PhaseHumanRequirementResponse MapToResponse(
            PhaseHumanRequirement requirement)
        {
            return new PhaseHumanRequirementResponse
            {
                PhaseHumanReqId = requirement.PhaseHumanReqId,
                PhaseId = requirement.PhaseId,
                PhaseName = requirement.Phase.PhaseName,
                ExperimentId = requirement.Phase.ExperimentId,
                ExperimentName = requirement.Phase.Experiment.ExperimentName,
                RoleId = requirement.RoleId,
                RoleName = requirement.Role.RoleName,
                Quantity = requirement.Quantity,
                RequiredSkillId = requirement.RequiredSkillId,
                RequiredSkillName = requirement.RequiredSkill?.SkillName,
                Note = requirement.Note,
                CreatedAt = requirement.CreatedAt,
                UpdatedAt = requirement.UpdatedAt
            };
        }
    }
}

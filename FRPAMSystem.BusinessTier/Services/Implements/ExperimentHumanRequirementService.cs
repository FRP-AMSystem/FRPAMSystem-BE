using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.ExperimentHumanRequirement;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class ExperimentHumanRequirementService : IExperimentHumanRequirementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExperimentHumanRequirementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<ExperimentHumanRequirementResponse>> ViewAllAsync(
            ExperimentHumanRequirementFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<ExperimentHumanRequirement>()
                .GetQueryable()
                .Include(r => r.Experiment)
                .Include(r => r.Role)
                .Include(r => r.RequiredSkill)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt);

            return await query
                .Select(r => new ExperimentHumanRequirementResponse
                {
                    ExpHumanReqId = r.ExpHumanReqId,
                    ExperimentId = r.ExperimentId,
                    ExperimentName = r.Experiment.ExperimentName,
                    RoleId = r.RoleId,
                    RoleName = r.Role.RoleName,
                    Quantity = r.Quantity,
                    RequiredSkillId = r.RequiredSkillId,
                    RequiredSkillName = r.RequiredSkill != null ? r.RequiredSkill.SkillName : null,
                    WorkingHoursPerDay = r.WorkingHoursPerDay,
                    Note = r.Note,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<ExperimentHumanRequirementResponse?> GetByIdAsync(int id)
        {
            var requirement = await _unitOfWork
                .GetRepository<ExperimentHumanRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExpHumanReqId == id,
                    include: query => query
                        .Include(r => r.Experiment)
                        .Include(r => r.Role)
                        .Include(r => r.RequiredSkill)
                );

            return requirement == null ? null : MapToResponse(requirement);
        }

        public async Task<ExperimentHumanRequirementResponse> CreateAsync(
            ExperimentHumanRequirementRequest request)
        {
            await ValidateRequestAsync(request);

            var requirement = new ExperimentHumanRequirement
            {
                ExperimentId = request.ExperimentId,
                RoleId = request.RoleId,
                Quantity = request.Quantity,
                RequiredSkillId = request.RequiredSkillId,
                WorkingHoursPerDay = request.WorkingHoursPerDay,
                Note = request.Note,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<ExperimentHumanRequirement>().InsertAsync(requirement);
            await _unitOfWork.CommitAsync();

            return (await GetByIdAsync(requirement.ExpHumanReqId))!;
        }

        public async Task<ExperimentHumanRequirementResponse?> UpdateAsync(
            int id,
            ExperimentHumanRequirementRequest request)
        {
            await ValidateRequestAsync(request);

            var requirement = await _unitOfWork
                .GetRepository<ExperimentHumanRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExpHumanReqId == id,
                    asNoTracking: false
                );

            if (requirement == null)
            {
                return null;
            }

            requirement.ExperimentId = request.ExperimentId;
            requirement.RoleId = request.RoleId;
            requirement.Quantity = request.Quantity;
            requirement.RequiredSkillId = request.RequiredSkillId;
            requirement.WorkingHoursPerDay = request.WorkingHoursPerDay;
            requirement.Note = request.Note;
            requirement.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<ExperimentHumanRequirement>().Update(requirement);
            await _unitOfWork.CommitAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var requirement = await _unitOfWork
                .GetRepository<ExperimentHumanRequirement>()
                .FirstOrDefaultAsync(
                    predicate: r => r.ExpHumanReqId == id,
                    asNoTracking: false
                );

            if (requirement == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<ExperimentHumanRequirement>().Delete(requirement);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task ValidateRequestAsync(ExperimentHumanRequirementRequest request)
        {
            if (request.Quantity <= 0)
            {
                throw new Exception("Quantity must be greater than 0.");
            }

            if (request.WorkingHoursPerDay.HasValue && request.WorkingHoursPerDay.Value <= 0)
            {
                throw new Exception("Working hours per day must be greater than 0.");
            }

            var experimentExists = await _unitOfWork
                .GetRepository<Experiment>()
                .AnyAsync(e => e.ExperimentId == request.ExperimentId);

            if (!experimentExists)
            {
                throw new Exception("Experiment does not exist.");
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

        private static ExperimentHumanRequirementResponse MapToResponse(
            ExperimentHumanRequirement requirement)
        {
            return new ExperimentHumanRequirementResponse
            {
                ExpHumanReqId = requirement.ExpHumanReqId,
                ExperimentId = requirement.ExperimentId,
                ExperimentName = requirement.Experiment.ExperimentName,
                RoleId = requirement.RoleId,
                RoleName = requirement.Role.RoleName,
                Quantity = requirement.Quantity,
                RequiredSkillId = requirement.RequiredSkillId,
                RequiredSkillName = requirement.RequiredSkill?.SkillName,
                WorkingHoursPerDay = requirement.WorkingHoursPerDay,
                Note = requirement.Note,
                CreatedAt = requirement.CreatedAt,
                UpdatedAt = requirement.UpdatedAt
            };
        }
    }
}

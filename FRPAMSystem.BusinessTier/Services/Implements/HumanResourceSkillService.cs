using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.HumanResourceSkill;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class HumanResourceSkillService : IHumanResourceSkillService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HumanResourceSkillService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<HumanResourceSkillResponse>> ViewAllHumanResourceSkillsAsync(
            HumanResourceSkillFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<HumanResourceSkill>()
                .GetQueryable()
                .Include(h => h.HumanResource)
                    .ThenInclude(hr => hr.User)
                .Include(h => h.Skill)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderBy(h => h.HumanResource.User.FullName)
                .ThenBy(h => h.Skill.SkillName);

            return await query
                .Select(h => new HumanResourceSkillResponse
                {
                    HumanResourceSkillId = h.HumanResourceSkillId,
                    HumanResourceId = h.HumanResourceId,
                    UserId = h.HumanResource.UserId,
                    FullName = h.HumanResource.User.FullName,
                    Username = h.HumanResource.User.Username,
                    Email = h.HumanResource.User.Email,
                    SkillId = h.SkillId,
                    SkillName = h.Skill.SkillName,
                    SkillDescription = h.Skill.Description,
                    SkillLevel = h.SkillLevel
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<HumanResourceSkillResponse?> GetHumanResourceSkillByIdAsync(int id)
        {
            var humanResourceSkill = await _unitOfWork
                .GetRepository<HumanResourceSkill>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceSkillId == id,
                    include: query => query
                        .Include(h => h.HumanResource)
                            .ThenInclude(hr => hr.User)
                        .Include(h => h.Skill)
                );

            if (humanResourceSkill == null)
            {
                return null;
            }

            return MapToResponse(humanResourceSkill);
        }

        public async Task<HumanResourceSkillResponse> CreateHumanResourceSkillAsync(
            HumanResourceSkillRequest request)
        {
            ValidateHumanResourceSkillRequest(request);

            var humanResourceExists = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .AnyAsync(h => h.HumanResourceId == request.HumanResourceId);

            if (!humanResourceExists)
            {
                throw new Exception("Human resource profile does not exist.");
            }

            var skillExists = await _unitOfWork
                .GetRepository<Skill>()
                .AnyAsync(s => s.SkillId == request.SkillId);

            if (!skillExists)
            {
                throw new Exception("Skill does not exist.");
            }

            var duplicate = await _unitOfWork
                .GetRepository<HumanResourceSkill>()
                .AnyAsync(h =>
                    h.HumanResourceId == request.HumanResourceId &&
                    h.SkillId == request.SkillId);

            if (duplicate)
            {
                throw new Exception("This human resource already has this skill.");
            }

            var humanResourceSkill = new HumanResourceSkill
            {
                HumanResourceId = request.HumanResourceId,
                SkillId = request.SkillId,
                SkillLevel = request.SkillLevel.ToString()
            };

            await _unitOfWork.GetRepository<HumanResourceSkill>()
                .InsertAsync(humanResourceSkill);

            await _unitOfWork.CommitAsync();

            var created = await _unitOfWork
                .GetRepository<HumanResourceSkill>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceSkillId == humanResourceSkill.HumanResourceSkillId,
                    include: query => query
                        .Include(h => h.HumanResource)
                            .ThenInclude(hr => hr.User)
                        .Include(h => h.Skill)
                );

            return MapToResponse(created!);
        }

        public async Task<HumanResourceSkillResponse?> UpdateHumanResourceSkillAsync(
            int id,
            HumanResourceSkillRequest request)
        {
            ValidateHumanResourceSkillRequest(request);

            var humanResourceSkill = await _unitOfWork
                .GetRepository<HumanResourceSkill>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceSkillId == id,
                    asNoTracking: false
                );

            if (humanResourceSkill == null)
            {
                return null;
            }

            var humanResourceExists = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .AnyAsync(h => h.HumanResourceId == request.HumanResourceId);

            if (!humanResourceExists)
            {
                throw new Exception("Human resource profile does not exist.");
            }

            var skillExists = await _unitOfWork
                .GetRepository<Skill>()
                .AnyAsync(s => s.SkillId == request.SkillId);

            if (!skillExists)
            {
                throw new Exception("Skill does not exist.");
            }

            var duplicate = await _unitOfWork
                .GetRepository<HumanResourceSkill>()
                .AnyAsync(h =>
                    h.HumanResourceId == request.HumanResourceId &&
                    h.SkillId == request.SkillId &&
                    h.HumanResourceSkillId != id);

            if (duplicate)
            {
                throw new Exception("This human resource already has this skill.");
            }

            humanResourceSkill.HumanResourceId = request.HumanResourceId;
            humanResourceSkill.SkillId = request.SkillId;
            humanResourceSkill.SkillLevel = request.SkillLevel.ToString();

            _unitOfWork.GetRepository<HumanResourceSkill>()
                .Update(humanResourceSkill);

            await _unitOfWork.CommitAsync();

            var updated = await _unitOfWork
                .GetRepository<HumanResourceSkill>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceSkillId == id,
                    include: query => query
                        .Include(h => h.HumanResource)
                            .ThenInclude(hr => hr.User)
                        .Include(h => h.Skill)
                );

            return MapToResponse(updated!);
        }

        public async Task<bool> DeleteHumanResourceSkillAsync(int id)
        {
            var humanResourceSkill = await _unitOfWork
                .GetRepository<HumanResourceSkill>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceSkillId == id,
                    asNoTracking: false
                );

            if (humanResourceSkill == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<HumanResourceSkill>()
                .Delete(humanResourceSkill);

            await _unitOfWork.CommitAsync();

            return true;
        }

        private static HumanResourceSkillResponse MapToResponse(
            HumanResourceSkill humanResourceSkill)
        {
            return new HumanResourceSkillResponse
            {
                HumanResourceSkillId = humanResourceSkill.HumanResourceSkillId,
                HumanResourceId = humanResourceSkill.HumanResourceId,
                UserId = humanResourceSkill.HumanResource?.UserId ?? 0,
                FullName = humanResourceSkill.HumanResource?.User?.FullName,
                Username = humanResourceSkill.HumanResource?.User?.Username,
                Email = humanResourceSkill.HumanResource?.User?.Email,
                SkillId = humanResourceSkill.SkillId,
                SkillName = humanResourceSkill.Skill?.SkillName,
                SkillDescription = humanResourceSkill.Skill?.Description,
                SkillLevel = humanResourceSkill.SkillLevel
            };
        }

        private static void ValidateHumanResourceSkillRequest(
            HumanResourceSkillRequest request)
        {
            if (request.HumanResourceId <= 0)
            {
                throw new Exception("HumanResourceId is required.");
            }

            if (request.SkillId <= 0)
            {
                throw new Exception("SkillId is required.");
            }
        }
    }
}

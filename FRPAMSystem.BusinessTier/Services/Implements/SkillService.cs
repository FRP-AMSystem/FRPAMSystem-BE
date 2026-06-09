using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Skill;
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
    public class SkillService : ISkillService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SkillService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<SkillResponse>> ViewAllSkillsAsync(
           SkillFilter filter,
           PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<Skill>()
                .GetQueryable()
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderBy(s => s.SkillName);

            return await query
                .Select(s => new SkillResponse
                {
                    SkillId = s.SkillId,
                    SkillName = s.SkillName,
                    Description = s.Description
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<SkillResponse?> GetSkillByIdAsync(int id)
        {
            var skill = await _unitOfWork
                .GetRepository<Skill>()
                .FirstOrDefaultAsync(predicate: s => s.SkillId == id);

            if (skill == null)
            {
                return null;
            }

            return new SkillResponse
            {
                SkillId = skill.SkillId,
                SkillName = skill.SkillName,
                Description = skill.Description
            };
        }

        public async Task<SkillResponse> CreateSkillAsync(SkillRequest request)
        {
            var exists = await _unitOfWork
                .GetRepository<Skill>()
                .AnyAsync(s => s.SkillName == request.SkillName);

            if (exists)
            {
                throw new Exception("Skill name already exists.");
            }

            var skill = new Skill
            {
                SkillName = request.SkillName,
                Description = request.Description
            };

            await _unitOfWork.GetRepository<Skill>().InsertAsync(skill);
            await _unitOfWork.CommitAsync();

            return new SkillResponse
            {
                SkillId = skill.SkillId,
                SkillName = skill.SkillName,
                Description = skill.Description
            };
        }

        public async Task<SkillResponse?> UpdateSkillAsync(int id, SkillRequest request)
        {
            var skill = await _unitOfWork
                .GetRepository<Skill>()
                .FirstOrDefaultAsync(
                    predicate: s => s.SkillId == id,
                    asNoTracking: false
                );

            if (skill == null)
            {
                return null;
            }

            var duplicateName = await _unitOfWork
                .GetRepository<Skill>()
                .AnyAsync(s => s.SkillName == request.SkillName && s.SkillId != id);

            if (duplicateName)
            {
                throw new Exception("Skill name already exists.");
            }

            skill.SkillName = request.SkillName;
            skill.Description = request.Description;

            _unitOfWork.GetRepository<Skill>().Update(skill);
            await _unitOfWork.CommitAsync();

            return new SkillResponse
            {
                SkillId = skill.SkillId,
                SkillName = skill.SkillName,
                Description = skill.Description
            };
        }

        public async Task<bool> DeleteSkillAsync(int id)
        {
            var skill = await _unitOfWork
                .GetRepository<Skill>()
                .FirstOrDefaultAsync(
                    predicate: s => s.SkillId == id,
                    asNoTracking: false
                );

            if (skill == null)
            {
                return false;
            }

            _unitOfWork.GetRepository<Skill>().Delete(skill);
            await _unitOfWork.CommitAsync();

            return true;
        }
    }
}

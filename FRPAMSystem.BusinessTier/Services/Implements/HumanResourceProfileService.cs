using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.HumanResourceProfile;
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
    public class HumanResourceProfileService : IHumanResourceProfileService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HumanResourceProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<HumanResourceProfileResponse>> ViewAllHumanResourceProfilesAsync(
            HumanResourceProfileFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .GetQueryable()
                .Include(h => h.User)
                    .ThenInclude(u => u.Role)
                .Include(h => h.HumanResourceSkills)
                    .ThenInclude(hs => hs.Skill)
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderBy(h => h.User.FullName);

            return await query
                .Select(h => new HumanResourceProfileResponse
                {
                    HumanResourceId = h.HumanResourceId,
                    UserId = h.UserId,
                    FullName = h.User.FullName,
                    Username = h.User.Username,
                    Email = h.User.Email,
                    RoleId = h.User.RoleId,
                    RoleName = h.User.Role.RoleName,
                    MaxWorkingHoursPerDay = h.MaxWorkingHoursPerDay,
                    CurrentWorkload = h.CurrentWorkload,
                    Status = h.Status,
                    CreatedAt = h.CreatedAt,
                    UpdatedAt = h.UpdatedAt
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<HumanResourceProfileResponse?> GetHumanResourceProfileByIdAsync(int id)
        {
            var profile = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceId == id,
                    include: query => query
                        .Include(h => h.User)
                        .ThenInclude(u => u.Role)
                        .Include(h => h.HumanResourceSkills)
                        .ThenInclude(hs => hs.Skill)
                );

            if (profile == null)
            {
                return null;
            }

            return MapToResponse(profile);
        }

        public async Task<HumanResourceProfileResponse> CreateHumanResourceProfileAsync(
            HumanResourceProfileRequest request)
        {
            ValidateHumanResourceProfileRequest(request);

            var user = await _unitOfWork
                .GetRepository<User>()
                .FirstOrDefaultAsync(
                    predicate: u => u.UserId == request.UserId,
                    include: query => query.Include(u => u.Role)
                );

            if (user == null)
            {
                throw new Exception("User does not exist.");
            }

            var profileExists = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .AnyAsync(h => h.UserId == request.UserId);

            if (profileExists)
            {
                throw new Exception("This user already has a human resource profile.");
            }

            var profile = new HumanResourceProfile
            {
                UserId = request.UserId,
                MaxWorkingHoursPerDay = request.MaxWorkingHoursPerDay,
                CurrentWorkload = request.CurrentWorkload,
                Status = request.Status.ToString()
            };

            await _unitOfWork.GetRepository<HumanResourceProfile>()
                .InsertAsync(profile);

            await _unitOfWork.CommitAsync();

            var created = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceId == profile.HumanResourceId,
                    include: query => query
                        .Include(h => h.User)
                        .ThenInclude(u => u.Role)
                );

            return MapToResponse(created!);
        }

        public async Task<HumanResourceProfileResponse?> UpdateHumanResourceProfileAsync(
            int id,
            HumanResourceProfileRequest request)
        {
            ValidateHumanResourceProfileRequest(request);

            var profile = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceId == id,
                    asNoTracking: false
                );

            if (profile == null)
            {
                return null;
            }

            var user = await _unitOfWork
                .GetRepository<User>()
                .FirstOrDefaultAsync(
                    predicate: u => u.UserId == request.UserId,
                    include: query => query.Include(u => u.Role)
                );

            if (user == null)
            {
                throw new Exception("User does not exist.");
            }

            var duplicateUserProfile = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .AnyAsync(h =>
                    h.UserId == request.UserId &&
                    h.HumanResourceId != id);

            if (duplicateUserProfile)
            {
                throw new Exception("This user already has another human resource profile.");
            }

            profile.UserId = request.UserId;
            profile.MaxWorkingHoursPerDay = request.MaxWorkingHoursPerDay;
            profile.CurrentWorkload = request.CurrentWorkload;
            profile.Status = request.Status.ToString();

            _unitOfWork.GetRepository<HumanResourceProfile>().Update(profile);

            await _unitOfWork.CommitAsync();

            var updated = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceId == id,
                    include: query => query
                        .Include(h => h.User)
                        .ThenInclude(u => u.Role)
                );

            return MapToResponse(updated!);
        }

        public async Task<bool> DeleteHumanResourceProfileAsync(int id)
        {
            var profile = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceId == id,
                    asNoTracking: false
                );

            if (profile == null)
            {
                return false;
            }

            var hasAllocation = await _unitOfWork
                .GetRepository<AllocationHumanDetail>()
                .AnyAsync(a => a.HumanResourceId == id);

            if (hasAllocation)
            {
                throw new Exception(
                    "Cannot delete human resource profile because it has allocation records.");
            }

            var hasSchedule = await _unitOfWork
                .GetRepository<Schedule>()
                .AnyAsync(s => s.AssignedHumanResourceId == id);

            if (hasSchedule)
            {
                throw new Exception(
                    "Cannot delete human resource profile because it has schedules.");
            }

            var hasSkill = await _unitOfWork
                .GetRepository<HumanResourceSkill>()
                .AnyAsync(s => s.HumanResourceId == id);

            if (hasSkill)
            {
                throw new Exception(
                    "Cannot delete human resource profile because it has assigned skills.");
            }

            _unitOfWork.GetRepository<HumanResourceProfile>().Delete(profile);

            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<HumanResourceProfileResponse?> SyncSkillsAsync(int id, SyncHumanResourceSkillsRequest request)
        {
            var profile = await _unitOfWork
                .GetRepository<HumanResourceProfile>()
                .FirstOrDefaultAsync(
                    predicate: h => h.HumanResourceId == id,
                    include: query => query
                        .Include(h => h.User)
                            .ThenInclude(u => u.Role)
                        .Include(h => h.HumanResourceSkills)
                            .ThenInclude(hs => hs.Skill),
                    asNoTracking: false
                );

            if (profile == null)
            {
                return null;
            }

            // Remove existing skills
            var existingSkills = profile.HumanResourceSkills.ToList();
            foreach (var existingSkill in existingSkills)
            {
                _unitOfWork.GetRepository<HumanResourceSkill>().Delete(existingSkill);
            }

            // Validate and add new skills
            foreach (var item in request.Skills)
            {
                var skillExists = await _unitOfWork
                    .GetRepository<Skill>()
                    .AnyAsync(s => s.SkillId == item.SkillId);

                if (!skillExists)
                {
                    throw new Exception($"Skill with ID {item.SkillId} does not exist.");
                }

                var newSkill = new HumanResourceSkill
                {
                    HumanResourceId = profile.HumanResourceId,
                    SkillId = item.SkillId,
                    SkillLevel = item.SkillLevel.ToString()
                };
                
                await _unitOfWork.GetRepository<HumanResourceSkill>().InsertAsync(newSkill);
                profile.HumanResourceSkills.Add(newSkill);
            }

            await _unitOfWork.CommitAsync();

            // Refresh to get full skill objects for response mapping
            var updatedProfile = await GetHumanResourceProfileByIdAsync(id);
            return updatedProfile;
        }

        private static HumanResourceProfileResponse MapToResponse(
            HumanResourceProfile profile)
        {
            var response = new HumanResourceProfileResponse
            {
                HumanResourceId = profile.HumanResourceId,
                UserId = profile.UserId,
                FullName = profile.User?.FullName,
                Username = profile.User?.Username,
                Email = profile.User?.Email,
                RoleId = profile.User?.RoleId,
                RoleName = profile.User?.Role?.RoleName,
                MaxWorkingHoursPerDay = profile.MaxWorkingHoursPerDay,
                CurrentWorkload = profile.CurrentWorkload,
                Status = profile.Status,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };

            if (profile.HumanResourceSkills != null)
            {
                response.Skills = profile.HumanResourceSkills.Select(hs => new FRPAMSystem.BusinessTier.Payload.HumanResourceSkill.HumanResourceSkillResponse
                {
                    HumanResourceSkillId = hs.HumanResourceSkillId,
                    HumanResourceId = hs.HumanResourceId,
                    UserId = profile.UserId,
                    FullName = profile.User?.FullName,
                    Username = profile.User?.Username,
                    Email = profile.User?.Email,
                    SkillId = hs.SkillId,
                    SkillName = hs.Skill?.SkillName,
                    SkillDescription = hs.Skill?.Description,
                    SkillLevel = hs.SkillLevel.ToString()
                }).ToList();
            }

            return response;
        }

        private static void ValidateHumanResourceProfileRequest(
            HumanResourceProfileRequest request)
        {
            if (request.UserId <= 0)
            {
                throw new Exception("UserId is required.");
            }

            if (request.MaxWorkingHoursPerDay <= 0)
            {
                throw new Exception("Max working hours per day must be greater than 0.");
            }

            if (request.MaxWorkingHoursPerDay > 24)
            {
                throw new Exception("Max working hours per day cannot exceed 24.");
            }

            if (request.CurrentWorkload < 0)
            {
                throw new Exception("Current workload cannot be negative.");
            }

            if (request.CurrentWorkload > request.MaxWorkingHoursPerDay)
            {
                throw new Exception(
                    "Current workload cannot exceed max working hours per day.");
            }
        }
    }
}

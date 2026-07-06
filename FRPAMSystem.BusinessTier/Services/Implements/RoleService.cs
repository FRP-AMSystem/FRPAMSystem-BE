using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Role;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IPaginate<RoleResponse>> ViewAllRolesAsync(
            RoleFilter filter,
            PagingModel pagingModel)
        {
            PagingModelHelper.NormalizePaging(pagingModel);

            var query = _unitOfWork
                .GetRepository<Role>()
                .GetQueryable()
                .ApplyFilter(filter)
                .AsNoTracking()
                .OrderBy(r => r.RoleName);

            return await query
                .Select(r => new RoleResponse
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToPaginateAsync(pagingModel.Page, pagingModel.Size, 1);
        }

        public async Task<RoleResponse?> GetRoleByIdAsync(int id)
        {
            var role = await _unitOfWork
                .GetRepository<Role>()
                .FirstOrDefaultAsync(predicate: r => r.RoleId == id);

            if (role == null)
            {
                return null;
            }

            return new RoleResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            };
        }

        public async Task<RoleResponse> CreateRoleAsync(RoleRequest request)
        {
            var roleName = request.RoleName.Trim();

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new Exception("Role name is required.");
            }

            var exists = await _unitOfWork
                .GetRepository<Role>()
                .AnyAsync(r => r.RoleName == roleName);

            if (exists)
            {
                throw new Exception("Role name already exists.");
            }

            var role = new Role
            {
                RoleName = roleName
            };

            await _unitOfWork.GetRepository<Role>().InsertAsync(role);
            await _unitOfWork.CommitAsync();

            return new RoleResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            };
        }

        public async Task<RoleResponse?> UpdateRoleAsync(int id, RoleRequest request)
        {
            var roleName = request.RoleName.Trim();

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new Exception("Role name is required.");
            }

            var role = await _unitOfWork
                .GetRepository<Role>()
                .FirstOrDefaultAsync(
                    predicate: r => r.RoleId == id,
                    asNoTracking: false
                );

            if (role == null)
            {
                return null;
            }

            var duplicateName = await _unitOfWork
                .GetRepository<Role>()
                .AnyAsync(r => r.RoleName == roleName && r.RoleId != id);

            if (duplicateName)
            {
                throw new Exception("Role name already exists.");
            }

            role.RoleName = roleName;

            _unitOfWork.GetRepository<Role>().Update(role);
            await _unitOfWork.CommitAsync();

            return new RoleResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            };
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _unitOfWork
                .GetRepository<Role>()
                .FirstOrDefaultAsync(
                    predicate: r => r.RoleId == id,
                    asNoTracking: false
                );

            if (role == null)
            {
                return false;
            }

            var hasUsers = await _unitOfWork
                .GetRepository<User>()
                .AnyAsync(u => u.RoleId == id);

            if (hasUsers)
            {
                throw new Exception("Cannot delete role because it is assigned to users.");
            }

            var hasExperimentRequirements = await _unitOfWork
                .GetRepository<ExperimentHumanRequirement>()
                .AnyAsync(r => r.RoleId == id);

            if (hasExperimentRequirements)
            {
                throw new Exception("Cannot delete role because it is used in experiment human requirements.");
            }

            var hasPhaseRequirements = await _unitOfWork
                .GetRepository<PhaseHumanRequirement>()
                .AnyAsync(r => r.RoleId == id);

            if (hasPhaseRequirements)
            {
                throw new Exception("Cannot delete role because it is used in phase human requirements.");
            }

            _unitOfWork.GetRepository<Role>().Delete(role);
            await _unitOfWork.CommitAsync();

            return true;
        }
    }
}

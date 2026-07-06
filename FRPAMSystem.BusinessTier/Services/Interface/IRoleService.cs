using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Role;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IRoleService
    {
        Task<IPaginate<RoleResponse>> ViewAllRolesAsync(
            RoleFilter filter,
            PagingModel pagingModel);

        Task<RoleResponse?> GetRoleByIdAsync(int id);

        Task<RoleResponse> CreateRoleAsync(RoleRequest request);

        Task<RoleResponse?> UpdateRoleAsync(int id, RoleRequest request);

        Task<bool> DeleteRoleAsync(int id);
    }
}

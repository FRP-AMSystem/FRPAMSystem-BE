using FRPAMSystem.BusinessTier.Payload.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IRoleService
    {
        Task<ICollection<RoleResponse>> GetAllRolesAsync();
    }
}

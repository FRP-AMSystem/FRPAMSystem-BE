using FRPAMSystem.BusinessTier.Payload.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IUserService
    {
        Task<ICollection<UserResponse>> GetAllUsersAsync();

        Task<UserResponse?> GetUserByIdAsync(int id);

        Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    }
}

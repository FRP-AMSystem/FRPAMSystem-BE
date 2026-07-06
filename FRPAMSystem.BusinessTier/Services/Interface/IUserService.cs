using FRPAMSystem.BusinessTier.Payload.Users;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IUserService
    {
        Task<ICollection<UserResponse>> GetAllUsersAsync();

        Task<UserResponse?> GetUserByIdAsync(int id);

        Task<UserResponse> CreateUserAsync(CreateUserRequest request);

        Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request);

        Task<bool> DeleteUserAsync(int id);
    }
}

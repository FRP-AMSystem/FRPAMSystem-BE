using FRPAMSystem.BusinessTier.Payload.Users;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ICollection<UserResponse>> GetAllUsersAsync()
        {
            var users = await _unitOfWork
                .GetRepository<User>()
                .GetListAsync(
                    include: x => x.Include(u => u.Role),
                    orderBy: x => Queryable.OrderBy<User, int>(x, u => u.UserId)
                );

            return users.Select(MapToResponse).ToList();
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork
                .GetRepository<User>()
                .FirstOrDefaultAsync(
                    predicate: x => x.UserId == id,
                    include: x => x.Include(u => u.Role)
                );

            return user == null ? null : MapToResponse(user);
        }

        public async Task<UserProfileResponse?> GetCurrentUserProfileAsync(int userId)
        {
            var user = await _unitOfWork
                .GetRepository<User>()
                .FirstOrDefaultAsync(
                    predicate: x => x.UserId == userId,
                    include: x => x.Include(u => u.Role)
                );

            if (user == null)
            {
                return null;
            }

            return new UserProfileResponse
            {
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                RoleName = user.Role?.RoleName
            };
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            await ValidateUserRequestAsync(
                request.FullName,
                request.Username,
                request.Email,
                request.RoleId,
                request.Password,
                isPasswordRequired: true);

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Username = request.Username.Trim(),
                Email = request.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = request.RoleId,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<User>().InsertAsync(user);
            await _unitOfWork.CommitAsync();

            return (await GetUserByIdAsync(user.UserId))!;
        }

        public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            await ValidateUserRequestAsync(
                request.FullName,
                request.Username,
                request.Email,
                request.RoleId,
                request.Password,
                isPasswordRequired: false,
                excludeUserId: id);

            var user = await _unitOfWork
                .GetRepository<User>()
                .FirstOrDefaultAsync(
                    predicate: u => u.UserId == id,
                    asNoTracking: false
                );

            if (user == null)
            {
                return null;
            }

            user.FullName = request.FullName.Trim();
            user.Username = request.Username.Trim();
            user.Email = request.Email.Trim();
            user.RoleId = request.RoleId;
            user.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            _unitOfWork.GetRepository<User>().Update(user);
            await _unitOfWork.CommitAsync();

            return await GetUserByIdAsync(user.UserId);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _unitOfWork
                .GetRepository<User>()
                .FirstOrDefaultAsync(
                    predicate: u => u.UserId == id,
                    asNoTracking: false
                );

            if (user == null)
            {
                return false;
            }

            if (await _unitOfWork.GetRepository<AllocationPlan>().AnyAsync(p => p.CreatedBy == id))
            {
                throw new Exception("Cannot delete user because they created allocation plans.");
            }

            if (await _unitOfWork.GetRepository<AllocationPlan>().AnyAsync(p => p.ApproveBy == id))
            {
                throw new Exception("Cannot delete user because they approved allocation plans.");
            }

            if (await _unitOfWork.GetRepository<Experiment>().AnyAsync(e => e.ResearcherId == id))
            {
                throw new Exception("Cannot delete user because they are assigned as a researcher.");
            }

            if (await _unitOfWork.GetRepository<HumanResourceProfile>().AnyAsync(h => h.UserId == id))
            {
                throw new Exception("Cannot delete user because they have a human resource profile.");
            }

            if (await _unitOfWork.GetRepository<Notification>().AnyAsync(n => n.UserId == id))
            {
                throw new Exception("Cannot delete user because they have notifications.");
            }

            if (await _unitOfWork.GetRepository<Schedule>().AnyAsync(s => s.CreatedBy == id))
            {
                throw new Exception("Cannot delete user because they created schedules.");
            }

            _unitOfWork.GetRepository<User>().Delete(user);
            await _unitOfWork.CommitAsync();

            return true;
        }

        private async Task ValidateUserRequestAsync(
            string fullName,
            string username,
            string email,
            int roleId,
            string? password,
            bool isPasswordRequired,
            int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new Exception("Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("Email is required.");
            }

            if (isPasswordRequired && string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Password is required.");
            }

            var roleExists = await _unitOfWork
                .GetRepository<Role>()
                .AnyAsync(r => r.RoleId == roleId);

            if (!roleExists)
            {
                throw new Exception("Role does not exist.");
            }

            var usernameExists = await _unitOfWork
                .GetRepository<User>()
                .AnyAsync(u => u.Username == username.Trim()
                    && (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));

            if (usernameExists)
            {
                throw new Exception("Username already exists.");
            }

            var emailExists = await _unitOfWork
                .GetRepository<User>()
                .AnyAsync(u => u.Email == email.Trim()
                    && (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));

            if (emailExists)
            {
                throw new Exception("Email already exists.");
            }
        }

        private static UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName
            };
        }
    }
}

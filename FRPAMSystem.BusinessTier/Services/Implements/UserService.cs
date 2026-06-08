using FRPAMSystem.BusinessTier.Payload.Users;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            return users.Select(u => new UserResponse
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Username = u.Username,
                Email = u.Email,
                RoleId = u.RoleId,
                RoleName = u.Role.RoleName
            }).ToList();
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork
                .GetRepository<User>()
                .FirstOrDefaultAsync(
                    predicate: x => x.UserId == id,
                    include: x => x.Include(u => u.Role)
                );

            if (user == null)
            {
                return null;
            }

            return new UserResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role.RoleName
            };
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            var roleExists = await _unitOfWork
                .GetRepository<Role>()
                .AnyAsync(r => r.RoleId == request.RoleId);

            if (!roleExists)
            {
                throw new Exception("Role does not exist.");
            }

            var usernameExists = await _unitOfWork
                .GetRepository<User>()
                .AnyAsync(u => u.Username == request.Username);

            if (usernameExists)
            {
                throw new Exception("Username already exists.");
            }

            var emailExists = await _unitOfWork
                .GetRepository<User>()
                .AnyAsync(u => u.Email == request.Email);

            if (emailExists)
            {
                throw new Exception("Email already exists.");
            }

            var user = new User
            {
                FullName = request.FullName,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = request.RoleId,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<User>().InsertAsync(user);
            await _unitOfWork.CommitAsync();

            var role = await _unitOfWork
                .GetRepository<Role>()
                .FirstOrDefaultAsync(predicate: r => r.RoleId == user.RoleId);

            return new UserResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = role?.RoleName
            };
        }
    }
}

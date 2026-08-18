using FRPAMSystem.BusinessTier.Payload.Users;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAllUsersAsync();

            return Ok(new
            {
                success = true,
                message = "Get users successfully",
                data = result
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _userService.GetCurrentUserProfileAsync(userId);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get current user successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get user successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var result = await _userService.CreateUserAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create user successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var result = await _userService.UpdateUserAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update user successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete user successfully"
            });
        }
    }
}

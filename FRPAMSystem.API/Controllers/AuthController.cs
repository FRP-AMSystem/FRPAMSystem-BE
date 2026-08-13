using FRPAMSystem.BusinessTier.Payload.Auth;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.BusinessTier.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            return Ok(new
            {
                success = true,
                message = "Login successfully",
                data = result
            });
        }

        [HttpPost("hash-password")]
        [AllowAnonymous]
        public IActionResult HashPassword([FromBody] PasswordHashRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Password is required." });
            }

            var hash = PasswordUtil.HashPassword(request.Password);

            return Ok(new
            {
                success = true,
                message = "Password hashed successfully",
                data = new
                {
                    password = request.Password,
                    hash,
                    algorithm = "BCrypt"
                }
            });
        }

        [HttpPost("verify-password")]
        [AllowAnonymous]
        public IActionResult VerifyPassword([FromBody] PasswordVerifyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Password is required." });
            }

            if (string.IsNullOrWhiteSpace(request.Hash))
            {
                return BadRequest(new { success = false, message = "Hash is required." });
            }

            try
            {
                var isMatch = PasswordUtil.VerifyPassword(request.Password, request.Hash);

                return Ok(new
                {
                    success = true,
                    message = isMatch
                        ? "Password matches the hash."
                        : "Password does not match the hash.",
                    data = new
                    {
                        request.Password,
                        request.Hash,
                        isMatch,
                        algorithm = "BCrypt",
                        note = "BCrypt is one-way. This endpoint verifies a password against a hash; it cannot decode hash back to plain text."
                    }
                });
            }
            catch (Exception)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid BCrypt hash format."
                });
            }
        }
    }
}

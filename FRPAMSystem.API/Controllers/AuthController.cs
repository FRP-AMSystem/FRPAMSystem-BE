using FRPAMSystem.BusinessTier.Payload.Auth;
using FRPAMSystem.BusinessTier.Services.Interface;
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
    }
}

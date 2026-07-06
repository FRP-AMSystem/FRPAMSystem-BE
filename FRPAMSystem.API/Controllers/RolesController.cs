using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Role;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllRoles(
            [FromQuery] RoleFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _roleService.ViewAllRolesAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get roles successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var result = await _roleService.GetRoleByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Role not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get role successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateRole([FromBody] RoleRequest request)
        {
            var result = await _roleService.CreateRoleAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create role successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleRequest request)
        {
            var result = await _roleService.UpdateRoleAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Role not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update role successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var result = await _roleService.DeleteRoleAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Role not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete role successfully"
            });
        }
    }
}

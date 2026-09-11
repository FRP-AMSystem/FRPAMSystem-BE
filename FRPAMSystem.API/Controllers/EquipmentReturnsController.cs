using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentReturn;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EquipmentReturnsController : ControllerBase
    {
        private readonly IEquipmentReturnService _service;

        public EquipmentReturnsController(IEquipmentReturnService service)
        {
            _service = service;
        }

        /// <summary>
        /// Mobile: submit equipment return for an allocation the user can access.
        /// </summary>
        [HttpPatch("mine/{allocationEquipmentDetailId:int}/return")]
        [Authorize(Roles = "Researcher,Student,Technician")]
        public async Task<IActionResult> SubmitMineReturn(
            int allocationEquipmentDetailId,
            [FromBody] EquipmentReturnMineRequest request)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _service.SubmitMineAsync(
                allocationEquipmentDetailId,
                userId.Value,
                request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation equipment detail not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Equipment returned successfully",
                data = result
            });
        }

        [HttpGet]
        public async Task<IActionResult> ViewAll(
            [FromQuery] EquipmentReturnFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _service.ViewAllAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get equipment returns successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment return not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get equipment return successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] EquipmentReturnRequest request)
        {
            var result = await _service.CreateAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create equipment return successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] EquipmentReturnRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment return not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update equipment return successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment return not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete equipment return successfully"
            });
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdValue, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}

using System.Security.Claims;
using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentExtensionRequest;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EquipmentExtensionRequestsController : ControllerBase
    {
        private readonly IEquipmentExtensionRequestService _service;

        public EquipmentExtensionRequestsController(
            IEquipmentExtensionRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAll(
            [FromQuery] EquipmentExtensionRequestFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _service.ViewAllAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get equipment extension requests successfully",
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
                    message = "Equipment extension request not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get equipment extension request successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Researcher,Student,Technician")]
        public async Task<IActionResult> Create(
            [FromBody] EquipmentExtensionRequestRequest request)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _service.CreateAsync(userId.Value, request);

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
                message = "Create equipment extension request successfully",
                data = result
            });
        }

        [HttpPatch("{id:int}/approve")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _service.ApproveAsync(id, userId.Value);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment extension request not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Equipment extension request approved successfully",
                data = result
            });
        }

        [HttpPatch("{id:int}/reject")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Reject(
            int id,
            [FromBody] EquipmentExtensionRequestReviewRequest request)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _service.RejectAsync(id, userId.Value, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment extension request not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Equipment extension request rejected successfully",
                data = result
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

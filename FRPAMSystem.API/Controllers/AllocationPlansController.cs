using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.AllocationPlan;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AllocationPlansController : ControllerBase
    {
        private readonly IAllocationPlanService _allocationPlanService;

        public AllocationPlansController(
            IAllocationPlanService allocationPlanService)
        {
            _allocationPlanService = allocationPlanService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllAllocationPlans(
            [FromQuery] AllocationPlanFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _allocationPlanService
                .ViewAllAllocationPlansAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get allocation plans successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAllocationPlanById(int id)
        {
            var result = await _allocationPlanService
                .GetAllocationPlanByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation plan not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get allocation plan successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateAllocationPlan(
            [FromBody] AllocationPlanRequest request)
        {
            var result = await _allocationPlanService
                .CreateAllocationPlanAsync(request, GetCurrentUserId());

            return Ok(new
            {
                success = true,
                message = "Create allocation plan successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateAllocationPlan(
            int id,
            [FromBody] AllocationPlanRequest request)
        {
            var result = await _allocationPlanService
                .UpdateAllocationPlanAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation plan not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update allocation plan successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteAllocationPlan(int id)
        {
            var result = await _allocationPlanService
                .DeleteAllocationPlanAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation plan not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete allocation plan successfully"
            });
        }

        [HttpPost("{id:int}/approve")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ApproveAllocationPlan(int id)
        {
            var result = await _allocationPlanService
                .ApproveAllocationPlanAsync(id, GetCurrentUserId());

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation plan not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Approve allocation plan successfully",
                data = result
            });
        }

        [HttpPost("{id:int}/reject")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RejectAllocationPlan(int id)
        {
            var result = await _allocationPlanService
                .RejectAllocationPlanAsync(id, GetCurrentUserId());

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation plan not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Reject allocation plan successfully",
                data = result
            });
        }

        [HttpPost("{id:int}/cancel")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CancelAllocationPlan(int id)
        {
            var result = await _allocationPlanService
                .CancelAllocationPlanAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation plan not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Cancel allocation plan successfully",
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

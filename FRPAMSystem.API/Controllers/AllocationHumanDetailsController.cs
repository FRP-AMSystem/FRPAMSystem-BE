using System.Security.Claims;
using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.AllocationHumanDetail;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AllocationHumanDetailsController : ControllerBase
    {
        private readonly IAllocationHumanDetailService _allocationHumanDetailService;

        public AllocationHumanDetailsController(
            IAllocationHumanDetailService allocationHumanDetailService)
        {
            _allocationHumanDetailService = allocationHumanDetailService;
        }

        /// <summary>
        /// Mobile: experiments/assignments for the logged-in user (Assigned Experiment).
        /// </summary>
        [HttpGet("mine")]
        [Authorize(Roles = "Researcher,Student,Technician")]
        public async Task<IActionResult> ViewMine(
            [FromQuery] AllocationHumanDetailFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _allocationHumanDetailService
                .ViewMineAsync(userId.Value, filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get my allocation human details successfully",
                data = result
            });
        }

        [HttpGet("mine/{id:int}")]
        [Authorize(Roles = "Researcher,Student,Technician")]
        public async Task<IActionResult> GetMineById(int id)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _allocationHumanDetailService
                .GetAllocationHumanDetailByIdForUserAsync(id, userId.Value);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation human detail not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get allocation human detail successfully",
                data = result
            });
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllAllocationHumanDetails(
            [FromQuery] AllocationHumanDetailFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _allocationHumanDetailService
                .ViewAllAllocationHumanDetailsAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get allocation human details successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAllocationHumanDetailById(int id)
        {
            var result = await _allocationHumanDetailService
                .GetAllocationHumanDetailByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation human detail not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get allocation human detail successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> CreateAllocationHumanDetail(
            [FromBody] AllocationHumanDetailRequest request)
        {
            var result = await _allocationHumanDetailService
                .CreateAllocationHumanDetailAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create allocation human detail successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> UpdateAllocationHumanDetail(
            int id,
            [FromBody] AllocationHumanDetailRequest request)
        {
            var result = await _allocationHumanDetailService
                .UpdateAllocationHumanDetailAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation human detail not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update allocation human detail successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteAllocationHumanDetail(int id)
        {
            var result = await _allocationHumanDetailService
                .DeleteAllocationHumanDetailAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation human detail not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete allocation human detail successfully"
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

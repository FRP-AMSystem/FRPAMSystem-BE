using System.Security.Claims;
using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.AllocationEquipmentDetail;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AllocationEquipmentDetailsController : ControllerBase
    {
        private readonly IAllocationEquipmentDetailService _allocationEquipmentDetailService;

        public AllocationEquipmentDetailsController(
            IAllocationEquipmentDetailService allocationEquipmentDetailService)
        {
            _allocationEquipmentDetailService = allocationEquipmentDetailService;
        }

        /// <summary>
        /// Mobile: equipment allocated to experiments the user can access (Request Equipment).
        /// </summary>
        [HttpGet("mine")]
        [Authorize(Roles = "Researcher,Student,Technician")]
        public async Task<IActionResult> ViewMine(
            [FromQuery] AllocationEquipmentDetailFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _allocationEquipmentDetailService
                .ViewMineAsync(userId.Value, filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get my allocation equipment details successfully",
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

            var result = await _allocationEquipmentDetailService
                .GetAllocationEquipmentDetailByIdForUserAsync(id, userId.Value);

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
                message = "Get allocation equipment detail successfully",
                data = result
            });
        }

        [HttpPatch("mine/{id:int}/handover")]
        [Authorize(Roles = "Researcher,Student,Technician")]
        public async Task<IActionResult> HandoverMine(int id)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            try
            {
                var result = await _allocationEquipmentDetailService
                    .HandoverMineAsync(id, userId.Value);

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
                    message = "Equipment handover confirmed successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("mine/{id:int}/return")]
        [Authorize(Roles = "Researcher,Student,Technician")]
        public async Task<IActionResult> ReturnMine(int id)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            try
            {
                var result = await _allocationEquipmentDetailService
                    .ReturnMineAsync(id, userId.Value);

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
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllAllocationEquipmentDetails(
            [FromQuery] AllocationEquipmentDetailFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _allocationEquipmentDetailService
                .ViewAllAllocationEquipmentDetailsAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get allocation equipment details successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAllocationEquipmentDetailById(int id)
        {
            var result = await _allocationEquipmentDetailService
                .GetAllocationEquipmentDetailByIdAsync(id);

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
                message = "Get allocation equipment detail successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> CreateAllocationEquipmentDetail(
            [FromBody] AllocationEquipmentDetailRequest request)
        {
            var result = await _allocationEquipmentDetailService
                .CreateAllocationEquipmentDetailAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create allocation equipment detail successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> UpdateAllocationEquipmentDetail(
            int id,
            [FromBody] AllocationEquipmentDetailRequest request)
        {
            var result = await _allocationEquipmentDetailService
                .UpdateAllocationEquipmentDetailAsync(id, request);

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
                message = "Update allocation equipment detail successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteAllocationEquipmentDetail(int id)
        {
            var result = await _allocationEquipmentDetailService
                .DeleteAllocationEquipmentDetailAsync(id);

            if (!result)
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
                message = "Delete allocation equipment detail successfully"
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

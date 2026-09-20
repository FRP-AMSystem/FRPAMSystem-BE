using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentHandover;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EquipmentHandoversController : ControllerBase
    {
        private readonly IEquipmentHandoverService _service;

        public EquipmentHandoversController(IEquipmentHandoverService service)
        {
            _service = service;
        }

        /// <summary>
        /// Mobile: confirm equipment handover for an allocation the user can access.
        /// </summary>
        [HttpPatch("mine/{allocationEquipmentDetailId:int}/handover")]
        [Authorize(Roles = "Researcher,Student,Technician")]
        public async Task<IActionResult> SubmitMineHandover(
            int allocationEquipmentDetailId,
            [FromBody] EquipmentHandoverMineRequest? request = null)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            try
            {
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
                    message = "Equipment handover confirmed successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("mine/{allocationEquipmentDetailId:int}/reject")]
        [Authorize(Roles = "Researcher,Student,Technician")]
        public async Task<IActionResult> RejectMineHandover(
            int allocationEquipmentDetailId,
            [FromBody] RejectHandoverRequest request)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            try
            {
                var result = await _service.RejectMineAsync(
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
                    message = "Equipment handover rejected successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
        [HttpPatch("{id:int}/reject")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectHandoverRequest request)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            try
            {
                var result = await _service.RejectAsync(id, userId.Value, request);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Equipment handover not found"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Equipment handover rejected successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewAll(
            [FromQuery] EquipmentHandoverFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _service.ViewAllAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get equipment handovers successfully",
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
                    message = "Equipment handover not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get equipment handover successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] EquipmentHandoverRequest request)
        {
            var result = await _service.CreateAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create equipment handover successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] EquipmentHandoverRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment handover not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update equipment handover successfully",
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
                    message = "Equipment handover not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete equipment handover successfully"
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

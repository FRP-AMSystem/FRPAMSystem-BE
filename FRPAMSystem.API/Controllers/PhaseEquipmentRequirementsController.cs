using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.PhaseEquipmentRequirement;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PhaseEquipmentRequirementsController : ControllerBase
    {
        private readonly IPhaseEquipmentRequirementService _phaseEquipmentRequirementService;

        public PhaseEquipmentRequirementsController(
            IPhaseEquipmentRequirementService phaseEquipmentRequirementService)
        {
            _phaseEquipmentRequirementService = phaseEquipmentRequirementService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllPhaseEquipmentRequirements(
            [FromQuery] PhaseEquipmentRequirementFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _phaseEquipmentRequirementService
                .ViewAllPhaseEquipmentRequirementsAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get phase equipment requirements successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPhaseEquipmentRequirementById(int id)
        {
            var result = await _phaseEquipmentRequirementService
                .GetPhaseEquipmentRequirementByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Phase equipment requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get phase equipment requirement successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> CreatePhaseEquipmentRequirement(
            [FromBody] PhaseEquipmentRequirementRequest request)
        {
            var result = await _phaseEquipmentRequirementService
                .CreatePhaseEquipmentRequirementAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create phase equipment requirement successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> UpdatePhaseEquipmentRequirement(
            int id,
            [FromBody] PhaseEquipmentRequirementRequest request)
        {
            var result = await _phaseEquipmentRequirementService
                .UpdatePhaseEquipmentRequirementAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Phase equipment requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update phase equipment requirement successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeletePhaseEquipmentRequirement(int id)
        {
            var result = await _phaseEquipmentRequirementService
                .DeletePhaseEquipmentRequirementAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Phase equipment requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete phase equipment requirement successfully"
            });
        }
    }
}

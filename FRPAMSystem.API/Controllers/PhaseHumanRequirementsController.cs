using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.PhaseHumanRequirement;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PhaseHumanRequirementsController : ControllerBase
    {
        private readonly IPhaseHumanRequirementService _phaseHumanRequirementService;

        public PhaseHumanRequirementsController(
            IPhaseHumanRequirementService phaseHumanRequirementService)
        {
            _phaseHumanRequirementService = phaseHumanRequirementService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllPhaseHumanRequirements(
            [FromQuery] PhaseHumanRequirementFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _phaseHumanRequirementService
                .ViewAllPhaseHumanRequirementsAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get phase human requirements successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPhaseHumanRequirementById(int id)
        {
            var result = await _phaseHumanRequirementService
                .GetPhaseHumanRequirementByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Phase human requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get phase human requirement successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> CreatePhaseHumanRequirement(
            [FromBody] PhaseHumanRequirementRequest request)
        {
            var result = await _phaseHumanRequirementService
                .CreatePhaseHumanRequirementAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create phase human requirement successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> UpdatePhaseHumanRequirement(
            int id,
            [FromBody] PhaseHumanRequirementRequest request)
        {
            var result = await _phaseHumanRequirementService
                .UpdatePhaseHumanRequirementAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Phase human requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update phase human requirement successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeletePhaseHumanRequirement(int id)
        {
            var result = await _phaseHumanRequirementService
                .DeletePhaseHumanRequirementAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Phase human requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete phase human requirement successfully"
            });
        }
    }
}

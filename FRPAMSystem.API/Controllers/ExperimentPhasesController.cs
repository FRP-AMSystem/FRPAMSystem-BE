using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.ExperimentPhase;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExperimentPhasesController : ControllerBase
    {
        private readonly IExperimentPhaseService _experimentPhaseService;

        public ExperimentPhasesController(IExperimentPhaseService experimentPhaseService)
        {
            _experimentPhaseService = experimentPhaseService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllExperimentPhases(
            [FromQuery] ExperimentPhaseFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _experimentPhaseService.ViewAllExperimentPhasesAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get experiment phases successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetExperimentPhaseById(int id)
        {
            var result = await _experimentPhaseService.GetExperimentPhaseByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Experiment phase not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get experiment phase successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> CreateExperimentPhase([FromBody] ExperimentPhaseRequest request)
        {
            var result = await _experimentPhaseService.CreateExperimentPhaseAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create experiment phase successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> UpdateExperimentPhase(int id, [FromBody] ExperimentPhaseRequest request)
        {
            var result = await _experimentPhaseService.UpdateExperimentPhaseAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Experiment phase not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update experiment phase successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteExperimentPhase(int id)
        {
            var result = await _experimentPhaseService.DeleteExperimentPhaseAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Experiment phase not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete experiment phase successfully"
            });
        }
    }
}

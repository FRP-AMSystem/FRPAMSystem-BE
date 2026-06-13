using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Experiment;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExperimentsController : ControllerBase
    {
        private readonly IExperimentService _experimentService;

        public ExperimentsController(IExperimentService experimentService)
        {
            _experimentService = experimentService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllExperiments(
            [FromQuery] ExperimentFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _experimentService.ViewAllExperimentsAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get experiments successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetExperimentById(int id)
        {
            var result = await _experimentService.GetExperimentByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Experiment not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get experiment successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> CreateExperiment([FromBody] ExperimentRequest request)
        {
            var result = await _experimentService.CreateExperimentAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create experiment successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> UpdateExperiment(int id, [FromBody] ExperimentRequest request)
        {
            var result = await _experimentService.UpdateExperimentAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Experiment not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update experiment successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteExperiment(int id)
        {
            var result = await _experimentService.DeleteExperimentAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Experiment not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete experiment successfully"
            });
        }
    }
}

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.ExperimentHumanRequirement;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExperimentHumanRequirementsController : ControllerBase
    {
        private readonly IExperimentHumanRequirementService _service;

        public ExperimentHumanRequirementsController(IExperimentHumanRequirementService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAll(
            [FromQuery] ExperimentHumanRequirementFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _service.ViewAllAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get experiment human requirements successfully",
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
                    message = "Experiment human requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get experiment human requirement successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> Create([FromBody] ExperimentHumanRequirementRequest request)
        {
            var result = await _service.CreateAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create experiment human requirement successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> Update(int id, [FromBody] ExperimentHumanRequirementRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Experiment human requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update experiment human requirement successfully",
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
                    message = "Experiment human requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete experiment human requirement successfully"
            });
        }
    }
}

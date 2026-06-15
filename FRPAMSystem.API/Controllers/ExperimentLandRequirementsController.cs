using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.ExperimentLandRequirement;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExperimentLandRequirementsController : ControllerBase
    {
        private readonly IExperimentLandRequirementService _service;

        public ExperimentLandRequirementsController(IExperimentLandRequirementService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAll(
            [FromQuery] ExperimentLandRequirementFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _service.ViewAllAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get experiment land requirements successfully",
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
                    message = "Experiment land requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get experiment land requirement successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> Create([FromBody] ExperimentLandRequirementRequest request)
        {
            var result = await _service.CreateAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create experiment land requirement successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> Update(int id, [FromBody] ExperimentLandRequirementRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Experiment land requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update experiment land requirement successfully",
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
                    message = "Experiment land requirement not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete experiment land requirement successfully"
            });
        }
    }
}

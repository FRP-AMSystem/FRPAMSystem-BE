using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.LandResource;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LandResourcesController : ControllerBase
    {
        private readonly ILandResourceService _landResourceService;

        public LandResourcesController(ILandResourceService landResourceService)
        {
            _landResourceService = landResourceService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllLandResources(
            [FromQuery] LandResourceFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _landResourceService.ViewAllLandResourcesAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get land resources successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetLandResourceById(int id)
        {
            var result = await _landResourceService.GetLandResourceByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Land resource not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get land resource successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateLandResource([FromBody] LandResourceRequest request)
        {
            var result = await _landResourceService.CreateLandResourceAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create land resource successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateLandResource(int id, [FromBody] LandResourceRequest request)
        {
            var result = await _landResourceService.UpdateLandResourceAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Land resource not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update land resource successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteLandResource(int id)
        {
            var result = await _landResourceService.DeleteLandResourceAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Land resource not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete land resource successfully"
            });
        }
    }
}

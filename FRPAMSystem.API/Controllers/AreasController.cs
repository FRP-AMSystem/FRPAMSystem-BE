using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Area;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AreasController : ControllerBase
    {
        private readonly IAreaService _areaService;

        public AreasController(IAreaService areaService)
        {
            _areaService = areaService;
        }
        [HttpGet]
        public async Task<IActionResult> ViewAllAreas(
            [FromQuery] AreaFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _areaService.ViewAllAreasAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get areas successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAreaById(int id)
        {
            var result = await _areaService.GetAreaByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Area not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get area successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateArea([FromBody] AreaRequest request)
        {
            var result = await _areaService.CreateAreaAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create area successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateArea(int id, [FromBody] AreaRequest request)
        {
            var result = await _areaService.UpdateAreaAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Area not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update area successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteArea(int id)
        {
            var result = await _areaService.DeleteAreaAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Area not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete area successfully"
            });
        }
    }
}

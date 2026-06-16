using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentShortageLog;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EquipmentShortageLogsController : ControllerBase
    {
        private readonly IEquipmentShortageLogService _service;

        public EquipmentShortageLogsController(IEquipmentShortageLogService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAll(
            [FromQuery] EquipmentShortageLogFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _service.ViewAllAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get equipment shortage logs successfully",
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
                    message = "Equipment shortage log not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get equipment shortage log successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] EquipmentShortageLogRequest request)
        {
            var result = await _service.CreateAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create equipment shortage log successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] EquipmentShortageLogRequest request)
        {
            var result = await _service.UpdateAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment shortage log not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update equipment shortage log successfully",
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
                    message = "Equipment shortage log not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete equipment shortage log successfully"
            });
        }
    }
}

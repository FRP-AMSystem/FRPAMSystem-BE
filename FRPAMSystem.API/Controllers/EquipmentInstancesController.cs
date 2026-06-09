using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentInstances;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EquipmentInstancesController : ControllerBase
    {
        private readonly IEquipmentInstanceService _equipmentInstanceService;

        public EquipmentInstancesController(
            IEquipmentInstanceService equipmentInstanceService)
        {
            _equipmentInstanceService = equipmentInstanceService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllEquipmentInstances(
            [FromQuery] EquipmentInstanceFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _equipmentInstanceService
                .ViewAllEquipmentInstancesAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get equipment instances successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEquipmentInstanceById(int id)
        {
            var result = await _equipmentInstanceService
                .GetEquipmentInstanceByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment instance not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get equipment instance successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateEquipmentInstance(
            [FromBody] EquipmentInstanceRequest request)
        {
            var result = await _equipmentInstanceService
                .CreateEquipmentInstanceAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create equipment instance successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateEquipmentInstance(
            int id,
            [FromBody] EquipmentInstanceRequest request)
        {
            var result = await _equipmentInstanceService
                .UpdateEquipmentInstanceAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment instance not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update equipment instance successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteEquipmentInstance(int id)
        {
            var result = await _equipmentInstanceService
                .DeleteEquipmentInstanceAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment instance not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete equipment instance successfully"
            });
        }
    }
}

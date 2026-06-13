using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentType;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EquipmentTypesController : ControllerBase
    {
        private readonly IEquipmentTypeService _equipmentTypeService;

        public EquipmentTypesController(IEquipmentTypeService equipmentTypeService)
        {
            _equipmentTypeService = equipmentTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllEquipmentTypes(
            [FromQuery] EquipmentTypeFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _equipmentTypeService
                .ViewAllEquipmentTypesAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get equipment types successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEquipmentTypeById(int id)
        {
            var result = await _equipmentTypeService
                .GetEquipmentTypeByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment type not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get equipment type successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateEquipmentType(
            [FromBody] EquipmentTypeRequest request)
        {
            var result = await _equipmentTypeService
                .CreateEquipmentTypeAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create equipment type successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateEquipmentType(
            int id,
            [FromBody] EquipmentTypeRequest request)
        {
            var result = await _equipmentTypeService
                .UpdateEquipmentTypeAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment type not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update equipment type successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteEquipmentType(int id)
        {
            var result = await _equipmentTypeService
                .DeleteEquipmentTypeAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment type not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete equipment type successfully"
            });
        }
    }
}

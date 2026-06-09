using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentCategory;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EquipmentCategoriesController : ControllerBase
    {
        private readonly IEquipmentCategoryService _equipmentCategoryService;

        public EquipmentCategoriesController(IEquipmentCategoryService equipmentCategoryService)
        {
            _equipmentCategoryService = equipmentCategoryService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllEquipmentCategories(
            [FromQuery] EquipmentCategoryFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _equipmentCategoryService
                .ViewAllEquipmentCategoriesAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get equipment categories successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEquipmentCategoryById(int id)
        {
            var result = await _equipmentCategoryService
                .GetEquipmentCategoryByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment category not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get equipment category successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateEquipmentCategory(
            [FromBody] EquipmentCategoryRequest request)
        {
            var result = await _equipmentCategoryService
                .CreateEquipmentCategoryAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create equipment category successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateEquipmentCategory(
            int id,
            [FromBody] EquipmentCategoryRequest request)
        {
            var result = await _equipmentCategoryService
                .UpdateEquipmentCategoryAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment category not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update equipment category successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteEquipmentCategory(int id)
        {
            var result = await _equipmentCategoryService
                .DeleteEquipmentCategoryAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Equipment category not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete equipment category successfully"
            });
        }
    }
}

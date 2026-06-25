using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.AllocationLandDetail;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AllocationLandDetailsController : ControllerBase
    {
        private readonly IAllocationLandDetailService _allocationLandDetailService;

        public AllocationLandDetailsController(
            IAllocationLandDetailService allocationLandDetailService)
        {
            _allocationLandDetailService = allocationLandDetailService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllAllocationLandDetails(
            [FromQuery] AllocationLandDetailFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _allocationLandDetailService
                .ViewAllAllocationLandDetailsAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get allocation land details successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAllocationLandDetailById(int id)
        {
            var result = await _allocationLandDetailService
                .GetAllocationLandDetailByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation land detail not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get allocation land detail successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateAllocationLandDetail(
            [FromBody] AllocationLandDetailRequest request)
        {
            var result = await _allocationLandDetailService
                .CreateAllocationLandDetailAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create allocation land detail successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateAllocationLandDetail(
            int id,
            [FromBody] AllocationLandDetailRequest request)
        {
            var result = await _allocationLandDetailService
                .UpdateAllocationLandDetailAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation land detail not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update allocation land detail successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteAllocationLandDetail(int id)
        {
            var result = await _allocationLandDetailService
                .DeleteAllocationLandDetailAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Allocation land detail not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete allocation land detail successfully"
            });
        }
    }
}

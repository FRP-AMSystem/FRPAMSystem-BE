using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.HumanResourceProfile;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HumanResourceProfilesController : ControllerBase
    {
        private readonly IHumanResourceProfileService _humanResourceProfileService;

        public HumanResourceProfilesController(
            IHumanResourceProfileService humanResourceProfileService)
        {
            _humanResourceProfileService = humanResourceProfileService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllHumanResourceProfiles(
            [FromQuery] HumanResourceProfileFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _humanResourceProfileService
                .ViewAllHumanResourceProfilesAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get human resource profiles successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetHumanResourceProfileById(int id)
        {
            var result = await _humanResourceProfileService
                .GetHumanResourceProfileByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Human resource profile not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get human resource profile successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateHumanResourceProfile(
            [FromBody] HumanResourceProfileRequest request)
        {
            var result = await _humanResourceProfileService
                .CreateHumanResourceProfileAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create human resource profile successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateHumanResourceProfile(
            int id,
            [FromBody] HumanResourceProfileRequest request)
        {
            var result = await _humanResourceProfileService
                .UpdateHumanResourceProfileAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Human resource profile not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update human resource profile successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteHumanResourceProfile(int id)
        {
            var result = await _humanResourceProfileService
                .DeleteHumanResourceProfileAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Human resource profile not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete human resource profile successfully"
            });
        }
    }
}

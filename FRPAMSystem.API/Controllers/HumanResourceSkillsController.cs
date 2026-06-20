using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.HumanResourceSkill;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HumanResourceSkillsController : ControllerBase
    {
        private readonly IHumanResourceSkillService _humanResourceSkillService;

        public HumanResourceSkillsController(
            IHumanResourceSkillService humanResourceSkillService)
        {
            _humanResourceSkillService = humanResourceSkillService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllHumanResourceSkills(
            [FromQuery] HumanResourceSkillFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _humanResourceSkillService
                .ViewAllHumanResourceSkillsAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get human resource skills successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetHumanResourceSkillById(int id)
        {
            var result = await _humanResourceSkillService
                .GetHumanResourceSkillByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Human resource skill not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get human resource skill successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateHumanResourceSkill(
            [FromBody] HumanResourceSkillRequest request)
        {
            var result = await _humanResourceSkillService
                .CreateHumanResourceSkillAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create human resource skill successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateHumanResourceSkill(
            int id,
            [FromBody] HumanResourceSkillRequest request)
        {
            var result = await _humanResourceSkillService
                .UpdateHumanResourceSkillAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Human resource skill not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update human resource skill successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteHumanResourceSkill(int id)
        {
            var result = await _humanResourceSkillService
                .DeleteHumanResourceSkillAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Human resource skill not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete human resource skill successfully"
            });
        }
    }
}

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Skill;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SkillsController : ControllerBase
    {
        private readonly ISkillService _skillService;

        public SkillsController(ISkillService skillService)
        {
            _skillService = skillService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllSkills(
            [FromQuery] SkillFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _skillService.ViewAllSkillsAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get skills successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSkillById(int id)
        {
            var result = await _skillService.GetSkillByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Skill not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get skill successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateSkill([FromBody] SkillRequest request)
        {
            var result = await _skillService.CreateSkillAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create skill successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateSkill(int id, [FromBody] SkillRequest request)
        {
            var result = await _skillService.UpdateSkillAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Skill not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update skill successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var result = await _skillService.DeleteSkillAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Skill not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete skill successfully"
            });
        }
    }
}

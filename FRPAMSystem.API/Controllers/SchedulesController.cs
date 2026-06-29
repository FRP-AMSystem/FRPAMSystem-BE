using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Schedule;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public SchedulesController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllSchedules(
            [FromQuery] ScheduleFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var result = await _scheduleService.ViewAllSchedulesAsync(filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get schedules successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            var result = await _scheduleService.GetScheduleByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Schedule not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get schedule successfully",
                data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> CreateSchedule([FromBody] ScheduleRequest request)
        {
            var result = await _scheduleService.CreateScheduleAsync(request);

            return Ok(new
            {
                success = true,
                message = "Create schedule successfully",
                data = result
            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Manager,Researcher")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] ScheduleRequest request)
        {
            var result = await _scheduleService.UpdateScheduleAsync(id, request);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Schedule not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Update schedule successfully",
                data = result
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var result = await _scheduleService.DeleteScheduleAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Schedule not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete schedule successfully"
            });
        }
    }
}

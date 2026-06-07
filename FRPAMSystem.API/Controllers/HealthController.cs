using FRPAMSystem.DataTier.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ForestryResourcePlanningDbContext _context;

        public HealthController(ForestryResourcePlanningDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Check()
        {
            return Ok(new
            {
                success = true,
                message = "FRPAMSystem API is running"
            });
        }

        [HttpGet("database")]
        public async Task<IActionResult> CheckDatabase()
        {
            var canConnect = await _context.Database.CanConnectAsync();

            return Ok(new
            {
                success = canConnect,
                message = canConnect
                    ? "Database connection is working"
                    : "Database connection failed"
            });
        }
    }
}
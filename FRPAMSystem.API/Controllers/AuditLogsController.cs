using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterRequest filter)
        {
            var result = await _auditLogService.GetAuditLogsAsync(filter);

            return Ok(new
            {
                success = true,
                message = "Get audit logs successfully",
                data = result
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAuditLogById(int id)
        {
            var result = await _auditLogService.GetAuditLogByIdAsync(id);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Audit log not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get audit log details successfully",
                data = result
            });
        }
    }
}

using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Text.Json;

namespace FRPAMSystem_BE.Filters
{
    public class AuditLogActionFilter : IAsyncActionFilter
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogActionFilter> _logger;

        public AuditLogActionFilter(
            IAuditLogService auditLogService,
            ILogger<AuditLogActionFilter> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpMethod = context.HttpContext.Request.Method.ToUpper();

            //audit mutating operations: POST, PUT, DELETE, PATCH
            bool isMutatingMethod = httpMethod == "POST" || httpMethod == "PUT" || httpMethod == "DELETE" || httpMethod == "PATCH";

            // Execute the action first
            var executedContext = await next();

            if (!isMutatingMethod)
            {
                return;
            }

            // Exclude audit logs controller itself from creating audit logs
            var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
            if (controllerName.Equals("AuditLogs", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var actionName = context.RouteData.Values["action"]?.ToString() ?? "Unknown";

            // Extract ActorUserId
            int? actorUserId = null;
            var userIdClaim = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? context.HttpContext.User?.FindFirst("sub")?.Value;
            if (int.TryParse(userIdClaim, out int parsedUserId))
            {
                actorUserId = parsedUserId;
            }

            var statusCode = context.HttpContext.Response.StatusCode;
            string severity = (executedContext.Exception != null || statusCode >= 400) ? "WARNING" : "INFO";
            if (statusCode >= 500) severity = "ERROR";

            var metadataObj = new
            {
                Path = context.HttpContext.Request.Path.Value,
                Method = httpMethod,
                StatusCode = statusCode,
                QueryString = context.HttpContext.Request.QueryString.Value,
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.HttpContext.Request.Headers["User-Agent"].ToString()
            };

            string metadataJson = JsonSerializer.Serialize(metadataObj);

            string description = $"{httpMethod} {context.HttpContext.Request.Path} - Status {statusCode}";

            try
            {
                await _auditLogService.RecordLogAsync(new CreateAuditLogRequest
                {
                    ActorUserId = actorUserId,
                    Module = controllerName,
                    Action = actionName,
                    Severity = severity,
                    Description = description,
                    Metadata = metadataJson
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to write audit log for {Method} {Path}.",
                    httpMethod,
                    context.HttpContext.Request.Path);
            }
        }
    }
}

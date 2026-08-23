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

            // Skip HTTP audit log if a rich business audit log was already recorded by a Domain Event handler
            if (context.HttpContext.Items.TryGetValue("AuditLogRecordedByDomainEvent", out var recorded) && recorded is true)
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
            string severity = (executedContext.Exception != null || statusCode >= 400) ? "Warning" : "Information";
            if (statusCode >= 500) severity = "Error";

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

            int? referenceId = ExtractReferenceId(context);
            string referenceType = ExtractReferenceType(controllerName);

            try
            {
                await _auditLogService.RecordLogAsync(new CreateAuditLogRequest
                {
                    ActorUserId = actorUserId,
                    Module = controllerName,
                    Action = actionName,
                    ReferenceType = referenceType,
                    ReferenceId = referenceId,
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

        private static int? ExtractReferenceId(ActionExecutingContext context)
        {
            foreach (var kvp in context.RouteData.Values)
            {
                if (kvp.Key.Equals("id", StringComparison.OrdinalIgnoreCase) || kvp.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(kvp.Value?.ToString(), out int routeId))
                    {
                        return routeId;
                    }
                }
            }

            foreach (var arg in context.ActionArguments)
            {
                if (arg.Key.Equals("id", StringComparison.OrdinalIgnoreCase) || arg.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    if (arg.Value is int intVal)
                    {
                        return intVal;
                    }
                    if (int.TryParse(arg.Value?.ToString(), out int parsedArgId))
                    {
                        return parsedArgId;
                    }
                }
            }

            return null;
        }

        private static string ExtractReferenceType(string controllerName)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
            {
                return "Unknown";
            }

            var controllerMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "AllocationEquipmentDetails", "AllocationEquipmentDetail" },
                { "AllocationHumanDetails", "AllocationHumanDetail" },
                { "AllocationLandDetails", "AllocationLandDetail" },
                { "AllocationOptimizations", "AllocationOptimization" },
                { "AllocationPlans", "AllocationPlan" },
                { "Areas", "Area" },
                { "EquipmentCategories", "EquipmentCategory" },
                { "EquipmentInstances", "EquipmentInstance" },
                { "EquipmentShortageLogs", "EquipmentShortageLog" },
                { "EquipmentSubstitutions", "EquipmentSubstitution" },
                { "EquipmentTypes", "EquipmentType" },
                { "ExperimentEquipmentRequirements", "ExperimentEquipmentRequirement" },
                { "ExperimentHumanRequirements", "ExperimentHumanRequirement" },
                { "ExperimentLandRequirements", "ExperimentLandRequirement" },
                { "ExperimentPhases", "ExperimentPhase" },
                { "Experiments", "Experiment" },
                { "HumanResourceProfiles", "HumanResourceProfile" },
                { "HumanResourceSkills", "HumanResourceSkill" },
                { "LandResources", "LandResource" },
                { "Notifications", "Notification" },
                { "PhaseEquipmentRequirements", "PhaseEquipmentRequirement" },
                { "PhaseHumanRequirements", "PhaseHumanRequirement" },
                { "Roles", "Role" },
                { "Schedules", "Schedule" },
                { "Skills", "Skill" },
                { "Users", "User" }
            };

            if (controllerMapping.TryGetValue(controllerName, out var mappedType))
            {
                return mappedType;
            }

            if (controllerName.EndsWith("ies", StringComparison.OrdinalIgnoreCase) && controllerName.Length > 3)
            {
                return controllerName[..^3] + "y";
            }

            if (controllerName.EndsWith("s", StringComparison.OrdinalIgnoreCase) && controllerName.Length > 1)
            {
                return controllerName[..^1];
            }

            return controllerName;
        }
    }
}

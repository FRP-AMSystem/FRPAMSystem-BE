using FRPAMSystem.BusinessTier.Configuration;
using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<AuditLogOptions>? _options;
        private readonly ILogger<AuditLogService>? _logger;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AuditLogService(
            IUnitOfWork unitOfWork,
            IOptions<AuditLogOptions>? options = null,
            ILogger<AuditLogService>? logger = null,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _unitOfWork = unitOfWork;
            _options = options;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuditLogResponse> RecordLogAsync(CreateAuditLogRequest request)
        {
            int? validActorUserId = null;
            if (request.ActorUserId.HasValue)
            {
                bool userExists = await _unitOfWork.GetRepository<User>().AnyAsync(u => u.UserId == request.ActorUserId.Value);
                if (userExists)
                {
                    validActorUserId = request.ActorUserId.Value;
                }
                else
                {
                    _logger?.LogWarning("Audit log requested with invalid ActorUserId {ActorUserId}. Storing NULL ActorUserId to prevent database foreign key constraint failure.", request.ActorUserId.Value);
                }
            }

            var auditLog = new AuditLog
            {
                ActorUserId = validActorUserId,
                Module = request.Module,
                Action = request.Action,
                ReferenceType = request.ReferenceType,
                ReferenceId = request.ReferenceId,
                Severity = NormalizeSeverity(request.Severity),
                Description = request.Description,
                Metadata = request.Metadata,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<AuditLog>().InsertAsync(auditLog);
            await _unitOfWork.CommitAsync();

            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items["AuditLogRecordedByDomainEvent"] = true;
            }

            await TrimAuditLogsAsync();

            return await GetAuditLogByIdAsync(auditLog.AuditLogId) 
                   ?? MapToResponse(auditLog);
        }

        private static string NormalizeSeverity(string? severity)
        {
            if (string.IsNullOrWhiteSpace(severity)) return "Information";

            var s = severity.Trim().ToUpperInvariant();
            return s switch
            {
                "INFO" or "INFORMATION" => "Information",
                "WARNING" or "WARN" => "Warning",
                "ERROR" or "FAIL" or "CRITICAL" => "Error",
                _ => "Information"
            };
        }

        private async Task TrimAuditLogsAsync()
        {
            try
            {
                int maxRecords = _options?.Value?.MaxRecords ?? 100;
                if (maxRecords <= 0) return;

                var repo = _unitOfWork.GetRepository<AuditLog>();
                int totalCount = await repo.GetQueryable().CountAsync();

                if (totalCount > maxRecords)
                {
                    int deleteCount = totalCount - maxRecords;
                    var expiredIds = await repo.GetQueryable()
                        .OrderBy(x => x.CreatedAt)
                        .ThenBy(x => x.AuditLogId)
                        .Take(deleteCount)
                        .Select(x => x.AuditLogId)
                        .ToListAsync();

                    if (expiredIds.Count > 0)
                    {
                        await repo.ExecuteDeleteAsync(x => expiredIds.Contains(x.AuditLogId));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to trim expired audit log records.");
            }
        }

        public async Task<IPaginate<AuditLogResponse>> GetAuditLogsAsync(AuditLogFilterRequest filter)
        {
            int page = filter.Page <= 0 ? 1 : filter.Page;
            int size = filter.PageSize <= 0 ? 10 : filter.PageSize;

            Expression<Func<AuditLog, bool>> predicate = x =>
                (string.IsNullOrWhiteSpace(filter.Module) || x.Module.ToLower() == filter.Module.ToLower()) &&
                (string.IsNullOrWhiteSpace(filter.Action) || x.Action.ToLower() == filter.Action.ToLower()) &&
                (!filter.ActorUserId.HasValue || x.ActorUserId == filter.ActorUserId.Value) &&
                (string.IsNullOrWhiteSpace(filter.Severity) || x.Severity.ToLower() == filter.Severity.ToLower()) &&
                (!filter.FromDate.HasValue || x.CreatedAt >= filter.FromDate.Value) &&
                (!filter.ToDate.HasValue || x.CreatedAt <= filter.ToDate.Value) &&
                (string.IsNullOrWhiteSpace(filter.Search) ||
                 x.Description.Contains(filter.Search) ||
                 x.Module.Contains(filter.Search) ||
                 x.Action.Contains(filter.Search));

            var result = await _unitOfWork.GetRepository<AuditLog>().GetPagingListAsync(
                selector: x => MapToResponse(x),
                predicate: predicate,
                orderBy: q => q.OrderByDescending(x => x.CreatedAt),
                include: q => q.Include(x => x.ActorUser!).ThenInclude(u => u.Role!),
                page: page,
                size: size
            );

            return result;
        }

        public async Task<AuditLogResponse?> GetAuditLogByIdAsync(int id)
        {
            var auditLog = await _unitOfWork.GetRepository<AuditLog>().FirstOrDefaultAsync(
                predicate: x => x.AuditLogId == id,
                include: q => q.Include(x => x.ActorUser!).ThenInclude(u => u.Role!)
            );

            return auditLog == null ? null : MapToResponse(auditLog);
        }

        private static AuditLogResponse MapToResponse(AuditLog log)
        {
            return new AuditLogResponse
            {
                AuditLogId = log.AuditLogId,
                ActorUserId = log.ActorUserId,
                ActorFullName = log.ActorUser?.FullName,
                ActorUsername = log.ActorUser?.Username,
                ActorRoleName = log.ActorUser?.Role?.RoleName,
                Module = log.Module,
                Action = log.Action,
                ReferenceType = log.ReferenceType,
                ReferenceId = log.ReferenceId,
                Severity = log.Severity,
                Description = log.Description,
                Metadata = log.Metadata,
                CreatedAt = log.CreatedAt
            };
        }
    }
}

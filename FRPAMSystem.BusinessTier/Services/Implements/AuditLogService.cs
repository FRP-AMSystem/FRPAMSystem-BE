using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.BusinessTier.Services.Interface;
using FRPAMSystem.DataTier.Models;
using FRPAMSystem.DataTier.Paginate;
using FRPAMSystem.DataTier.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Implements
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AuditLogResponse> RecordLogAsync(CreateAuditLogRequest request)
        {
            var auditLog = new AuditLog
            {
                ActorUserId = request.ActorUserId,
                Module = request.Module,
                Action = request.Action,
                ReferenceType = request.ReferenceType,
                ReferenceId = request.ReferenceId,
                Severity = string.IsNullOrWhiteSpace(request.Severity) ? "INFO" : request.Severity.ToUpper(),
                Description = request.Description,
                Metadata = request.Metadata
            };

            await _unitOfWork.GetRepository<AuditLog>().InsertAsync(auditLog);
            await _unitOfWork.CommitAsync();

            return await GetAuditLogByIdAsync(auditLog.AuditLogId) 
                   ?? MapToResponse(auditLog);
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

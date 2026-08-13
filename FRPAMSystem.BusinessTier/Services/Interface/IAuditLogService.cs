using FRPAMSystem.BusinessTier.Payload.AuditLog;
using FRPAMSystem.DataTier.Paginate;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IAuditLogService
    {
        Task<AuditLogResponse> RecordLogAsync(CreateAuditLogRequest request);
        Task<IPaginate<AuditLogResponse>> GetAuditLogsAsync(AuditLogFilterRequest filter);
        Task<AuditLogResponse?> GetAuditLogByIdAsync(int id);
    }
}

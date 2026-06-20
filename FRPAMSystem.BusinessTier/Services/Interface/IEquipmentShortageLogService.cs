using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentShortageLog;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IEquipmentShortageLogService
    {
        Task<IPaginate<EquipmentShortageLogResponse>> ViewAllAsync(
            EquipmentShortageLogFilter filter,
            PagingModel pagingModel);

        Task<EquipmentShortageLogResponse?> GetByIdAsync(int id);

        Task<EquipmentShortageLogResponse> CreateAsync(EquipmentShortageLogRequest request);

        Task<EquipmentShortageLogResponse?> UpdateAsync(int id, EquipmentShortageLogRequest request);

        Task<bool> DeleteAsync(int id);
    }
}

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentHandover;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IEquipmentHandoverService
    {
        Task<IPaginate<EquipmentHandoverResponse>> ViewAllAsync(
            EquipmentHandoverFilter filter,
            PagingModel pagingModel);

        Task<EquipmentHandoverResponse?> GetByIdAsync(int id);

        Task<EquipmentHandoverResponse?> SubmitMineAsync(
            int allocationEquipmentDetailId,
            int userId,
            EquipmentHandoverMineRequest? request = null);

        Task<EquipmentHandoverResponse?> RejectMineAsync(
            int allocationEquipmentDetailId,
            int userId,
            RejectHandoverRequest request);

        Task<EquipmentHandoverResponse?> RejectAsync(
            int handoverId,
            int managerUserId,
            RejectHandoverRequest request);

        Task<EquipmentHandoverResponse> CreateAsync(EquipmentHandoverRequest request);

        Task<EquipmentHandoverResponse?> UpdateAsync(int id, EquipmentHandoverRequest request);

        Task<bool> DeleteAsync(int id);
    }
}

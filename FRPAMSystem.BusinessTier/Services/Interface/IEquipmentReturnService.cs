using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentReturn;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IEquipmentReturnService
    {
        Task<IPaginate<EquipmentReturnResponse>> ViewAllAsync(
            EquipmentReturnFilter filter,
            PagingModel pagingModel);

        Task<EquipmentReturnResponse?> GetByIdAsync(int id);

        Task<EquipmentReturnResponse?> SubmitMineAsync(
            int allocationEquipmentDetailId,
            int userId,
            EquipmentReturnMineRequest request);

        Task<EquipmentReturnResponse?> ConfirmAsync(
            int returnId,
            int managerUserId);

        Task<EquipmentReturnResponse?> RejectAsync(
            int returnId,
            int managerUserId,
            RejectReturnRequest request);

        Task<EquipmentReturnResponse> CreateAsync(EquipmentReturnRequest request);

        Task<EquipmentReturnResponse?> UpdateAsync(int id, EquipmentReturnRequest request);

        Task<bool> DeleteAsync(int id);
    }
}

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentChangeRequest;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IEquipmentChangeRequestService
    {
        Task<IPaginate<EquipmentChangeRequestResponse>> ViewAllAsync(
            EquipmentChangeRequestFilter filter,
            PagingModel pagingModel);

        Task<EquipmentChangeRequestResponse?> GetByIdAsync(int id);

        Task<EquipmentChangeRequestResponse?> CreateAsync(
            int userId,
            EquipmentChangeRequestRequest request);

        Task<EquipmentChangeRequestResponse?> ApproveAsync(int id, int managerUserId);

        Task<EquipmentChangeRequestResponse?> RejectAsync(
            int id,
            int managerUserId,
            EquipmentChangeRequestReviewRequest request);
    }
}

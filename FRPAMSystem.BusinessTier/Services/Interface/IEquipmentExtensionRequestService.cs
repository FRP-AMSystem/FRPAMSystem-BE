using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentExtensionRequest;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IEquipmentExtensionRequestService
    {
        Task<IPaginate<EquipmentExtensionRequestResponse>> ViewAllAsync(
            EquipmentExtensionRequestFilter filter,
            PagingModel pagingModel);

        Task<EquipmentExtensionRequestResponse?> GetByIdAsync(int id);

        Task<EquipmentExtensionRequestResponse?> CreateAsync(
            int userId,
            EquipmentExtensionRequestRequest request);

        Task<EquipmentExtensionRequestResponse?> ApproveAsync(int id, int managerUserId);

        Task<EquipmentExtensionRequestResponse?> RejectAsync(
            int id,
            int managerUserId,
            EquipmentExtensionRequestReviewRequest request);
    }
}

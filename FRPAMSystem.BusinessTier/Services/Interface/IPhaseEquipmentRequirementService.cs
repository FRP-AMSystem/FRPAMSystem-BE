using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.PhaseEquipmentRequirement;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IPhaseEquipmentRequirementService
    {
        Task<IPaginate<PhaseEquipmentRequirementResponse>> ViewAllPhaseEquipmentRequirementsAsync(
            PhaseEquipmentRequirementFilter filter,
            PagingModel pagingModel);

        Task<PhaseEquipmentRequirementResponse?> GetPhaseEquipmentRequirementByIdAsync(int id);

        Task<PhaseEquipmentRequirementResponse> CreatePhaseEquipmentRequirementAsync(
            PhaseEquipmentRequirementRequest request);

        Task<PhaseEquipmentRequirementResponse?> UpdatePhaseEquipmentRequirementAsync(
            int id,
            PhaseEquipmentRequirementRequest request);

        Task<bool> DeletePhaseEquipmentRequirementAsync(int id);
    }
}

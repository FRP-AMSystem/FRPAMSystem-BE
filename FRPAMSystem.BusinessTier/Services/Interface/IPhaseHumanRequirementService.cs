using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.PhaseHumanRequirement;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IPhaseHumanRequirementService
    {
        Task<IPaginate<PhaseHumanRequirementResponse>> ViewAllPhaseHumanRequirementsAsync(
            PhaseHumanRequirementFilter filter,
            PagingModel pagingModel);

        Task<PhaseHumanRequirementResponse?> GetPhaseHumanRequirementByIdAsync(int id);

        Task<PhaseHumanRequirementResponse> CreatePhaseHumanRequirementAsync(
            PhaseHumanRequirementRequest request);

        Task<PhaseHumanRequirementResponse?> UpdatePhaseHumanRequirementAsync(
            int id,
            PhaseHumanRequirementRequest request);

        Task<bool> DeletePhaseHumanRequirementAsync(int id);
    }
}

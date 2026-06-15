using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.ExperimentHumanRequirement;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IExperimentHumanRequirementService
    {
        Task<IPaginate<ExperimentHumanRequirementResponse>> ViewAllAsync(
            ExperimentHumanRequirementFilter filter,
            PagingModel pagingModel);

        Task<ExperimentHumanRequirementResponse?> GetByIdAsync(int id);

        Task<ExperimentHumanRequirementResponse> CreateAsync(ExperimentHumanRequirementRequest request);

        Task<ExperimentHumanRequirementResponse?> UpdateAsync(int id, ExperimentHumanRequirementRequest request);

        Task<bool> DeleteAsync(int id);
    }
}

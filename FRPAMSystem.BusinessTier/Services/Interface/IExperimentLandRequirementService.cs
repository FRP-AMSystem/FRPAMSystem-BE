using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.ExperimentLandRequirement;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IExperimentLandRequirementService
    {
        Task<IPaginate<ExperimentLandRequirementResponse>> ViewAllAsync(
            ExperimentLandRequirementFilter filter,
            PagingModel pagingModel);

        Task<ExperimentLandRequirementResponse?> GetByIdAsync(int id);

        Task<ExperimentLandRequirementResponse> CreateAsync(ExperimentLandRequirementRequest request);

        Task<ExperimentLandRequirementResponse?> UpdateAsync(int id, ExperimentLandRequirementRequest request);

        Task<bool> DeleteAsync(int id);
    }
}

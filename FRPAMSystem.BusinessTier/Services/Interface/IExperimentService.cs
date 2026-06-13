using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Experiment;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IExperimentService
    {
        Task<IPaginate<ExperimentResponse>> ViewAllExperimentsAsync(
            ExperimentFilter filter,
            PagingModel pagingModel);

        Task<ExperimentResponse?> GetExperimentByIdAsync(int id);

        Task<ExperimentResponse> CreateExperimentAsync(ExperimentRequest request);

        Task<ExperimentResponse?> UpdateExperimentAsync(int id, ExperimentRequest request);

        Task<bool> DeleteExperimentAsync(int id);
    }
}

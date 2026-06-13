using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.ExperimentPhase;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IExperimentPhaseService
    {
        Task<IPaginate<ExperimentPhaseResponse>> ViewAllExperimentPhasesAsync(
            ExperimentPhaseFilter filter,
            PagingModel pagingModel);

        Task<ExperimentPhaseResponse?> GetExperimentPhaseByIdAsync(int id);

        Task<ExperimentPhaseResponse> CreateExperimentPhaseAsync(ExperimentPhaseRequest request);

        Task<ExperimentPhaseResponse?> UpdateExperimentPhaseAsync(int id, ExperimentPhaseRequest request);

        Task<bool> DeleteExperimentPhaseAsync(int id);
    }
}

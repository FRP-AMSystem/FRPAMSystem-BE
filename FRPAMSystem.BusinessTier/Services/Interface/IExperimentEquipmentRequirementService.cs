using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.ExperimentEquipmentRequirement;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IExperimentEquipmentRequirementService
    {
        Task<IPaginate<ExperimentEquipmentRequirementResponse>> ViewAllAsync(
            ExperimentEquipmentRequirementFilter filter,
            PagingModel pagingModel);

        Task<ExperimentEquipmentRequirementResponse?> GetByIdAsync(int id);

        Task<ExperimentEquipmentRequirementResponse> CreateAsync(ExperimentEquipmentRequirementRequest request);

        Task<ExperimentEquipmentRequirementResponse?> UpdateAsync(int id, ExperimentEquipmentRequirementRequest request);

        Task<bool> DeleteAsync(int id);
    }
}

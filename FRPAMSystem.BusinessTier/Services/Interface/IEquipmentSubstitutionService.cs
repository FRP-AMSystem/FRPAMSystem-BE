using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.EquipmentSubstitution;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IEquipmentSubstitutionService
    {
        Task<IPaginate<EquipmentSubstitutionResponse>> ViewAllAsync(
            EquipmentSubstitutionFilter filter,
            PagingModel pagingModel);

        Task<EquipmentSubstitutionResponse?> GetByIdAsync(int id);

        Task<EquipmentSubstitutionResponse> CreateAsync(EquipmentSubstitutionRequest request);

        Task<EquipmentSubstitutionResponse?> UpdateAsync(int id, EquipmentSubstitutionRequest request);

        Task<bool> DeleteAsync(int id);
    }
}

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.LandResource;
using FRPAMSystem.DataTier.Paginate;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface ILandResourceService
    {
        Task<IPaginate<LandResourceResponse>> ViewAllLandResourcesAsync(
            LandResourceFilter filter,
            PagingModel pagingModel);

        Task<LandResourceResponse?> GetLandResourceByIdAsync(int id);

        Task<LandResourceResponse> CreateLandResourceAsync(LandResourceRequest request);

        Task<LandResourceResponse?> UpdateLandResourceAsync(int id, LandResourceRequest request);

        Task<bool> DeleteLandResourceAsync(int id);
    }
}

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.HumanResourceProfile;
using FRPAMSystem.DataTier.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IHumanResourceProfileService
    {
        Task<IPaginate<HumanResourceProfileResponse>> ViewAllHumanResourceProfilesAsync(
            HumanResourceProfileFilter filter,
            PagingModel pagingModel);

        Task<HumanResourceProfileResponse?> GetHumanResourceProfileByIdAsync(int id);

        Task<HumanResourceProfileResponse> CreateHumanResourceProfileAsync(
            HumanResourceProfileRequest request);

        Task<HumanResourceProfileResponse?> UpdateHumanResourceProfileAsync(
            int id,
            HumanResourceProfileRequest request);

        Task<bool> DeleteHumanResourceProfileAsync(int id);
    }
}

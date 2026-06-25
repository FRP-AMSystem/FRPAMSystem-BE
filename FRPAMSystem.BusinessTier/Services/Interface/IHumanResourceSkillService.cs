using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.HumanResourceSkill;
using FRPAMSystem.DataTier.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface IHumanResourceSkillService
    {
        Task<IPaginate<HumanResourceSkillResponse>> ViewAllHumanResourceSkillsAsync(
            HumanResourceSkillFilter filter,
            PagingModel pagingModel);

        Task<HumanResourceSkillResponse?> GetHumanResourceSkillByIdAsync(int id);

        Task<HumanResourceSkillResponse> CreateHumanResourceSkillAsync(
            HumanResourceSkillRequest request);

        Task<HumanResourceSkillResponse?> UpdateHumanResourceSkillAsync(
            int id,
            HumanResourceSkillRequest request);

        Task<bool> DeleteHumanResourceSkillAsync(int id);
    }
}

using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Skill;
using FRPAMSystem.DataTier.Paginate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Services.Interface
{
    public interface ISkillService
    {
        Task<IPaginate<SkillResponse>> ViewAllSkillsAsync(
            SkillFilter filter,
            PagingModel pagingModel);

        Task<SkillResponse?> GetSkillByIdAsync(int id);

        Task<SkillResponse> CreateSkillAsync(SkillRequest request);

        Task<SkillResponse?> UpdateSkillAsync(int id, SkillRequest request);

        Task<bool> DeleteSkillAsync(int id);
    }
}

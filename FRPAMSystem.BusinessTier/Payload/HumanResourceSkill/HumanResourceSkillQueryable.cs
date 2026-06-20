using FRPAMSystem.BusinessTier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.HumanResourceSkill
{
    public static class HumanResourceSkillQueryable
    {
        public static IQueryable<DataTier.Models.HumanResourceSkill> ApplyFilter(
            this IQueryable<DataTier.Models.HumanResourceSkill> query,
            HumanResourceSkillFilter filter)
        {
            var skillLevel = filter.SkillLevel?.ToString();

            return query
                .SearchIf(
                    filter.Keyword,
                    h => h.HumanResource.User.FullName,
                    h => h.HumanResource.User.Username,
                    h => h.HumanResource.User.Email,
                    h => h.Skill.SkillName,
                    h => h.Skill.Description
                )
                .WhereEqualsIf(
                    filter.HumanResourceId,
                    h => h.HumanResourceId
                )
                .WhereEqualsIf(
                    filter.UserId,
                    h => h.HumanResource.UserId
                )
                .WhereEqualsIf(
                    filter.SkillId,
                    h => h.SkillId
                )
                .WhereStringEqualsIf(
                    skillLevel,
                    h => h.SkillLevel
                );
        }
    }
}

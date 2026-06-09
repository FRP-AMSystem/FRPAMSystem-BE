using FRPAMSystem.BusinessTier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.Skill
{
    public static class SkillQueryable    {
        public static IQueryable<DataTier.Models.Skill> ApplyFilter(
            this IQueryable<DataTier.Models.Skill> query,
            SkillFilter filter)
        {
            return query.SearchIf(
                filter.Keyword,
                s => s.SkillName,
                s => s.Description
            );
        }
    }
}

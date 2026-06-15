using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.ExperimentHumanRequirement
{
    public static class ExperimentHumanRequirementQueryable
    {
        public static IQueryable<DataTier.Models.ExperimentHumanRequirement> ApplyFilter(
            this IQueryable<DataTier.Models.ExperimentHumanRequirement> query,
            ExperimentHumanRequirementFilter filter)
        {
            return query
                .SearchIf(filter.Keyword, r => r.Note)
                .WhereEqualsIf(filter.ExperimentId, r => r.ExperimentId)
                .WhereEqualsIf(filter.RoleId, r => r.RoleId)
                .WhereIf(
                    filter.RequiredSkillId.HasValue,
                    r => r.RequiredSkillId == filter.RequiredSkillId!.Value);
        }
    }
}

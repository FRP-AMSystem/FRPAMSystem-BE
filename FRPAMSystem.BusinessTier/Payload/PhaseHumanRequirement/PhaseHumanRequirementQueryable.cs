using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.PhaseHumanRequirement
{
    public static class PhaseHumanRequirementQueryable
    {
        public static IQueryable<DataTier.Models.PhaseHumanRequirement> ApplyFilter(
            this IQueryable<DataTier.Models.PhaseHumanRequirement> query,
            PhaseHumanRequirementFilter filter)
        {
            return query
                .SearchIf(
                    filter.Keyword,
                    r => r.Note,
                    r => r.Phase.PhaseName,
                    r => r.Phase.Experiment.ExperimentName,
                    r => r.Role.RoleName,
                    r => r.RequiredSkill != null ? r.RequiredSkill.SkillName : null
                )
                .WhereEqualsIf(
                    filter.PhaseId,
                    r => r.PhaseId
                )
                .WhereEqualsIf(
                    filter.ExperimentId,
                    r => r.Phase.ExperimentId
                )
                .WhereEqualsIf(
                    filter.RoleId,
                    r => r.RoleId
                )
                .WhereIf(
                    filter.RequiredSkillId.HasValue,
                    r => r.RequiredSkillId == filter.RequiredSkillId!.Value
                );
        }
    }
}

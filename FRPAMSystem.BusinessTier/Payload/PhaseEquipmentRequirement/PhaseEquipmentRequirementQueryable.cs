using FRPAMSystem.BusinessTier.Utils;
using FRPAMSystem.DataTier.Models;

namespace FRPAMSystem.BusinessTier.Payload.PhaseEquipmentRequirement
{
    public static class PhaseEquipmentRequirementQueryable
    {
        public static IQueryable<DataTier.Models.PhaseEquipmentRequirement> ApplyFilter(
            this IQueryable<DataTier.Models.PhaseEquipmentRequirement> query,
            PhaseEquipmentRequirementFilter filter)
        {
            return query
                .SearchIf(
                    filter.Keyword,
                    r => r.Note,
                    r => r.Phase.PhaseName,
                    r => r.Phase.Experiment.ExperimentName,
                    r => r.EquipmentType.Name,
                    r => r.EquipmentType.EquipmentCategory.CategoryName
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
                    filter.EquipmentTypeId,
                    r => r.EquipmentTypeId
                )
                .WhereEqualsIf(
                    filter.EquipmentCategoryId,
                    r => r.EquipmentType.EquipmentCategoryId
                );
        }
    }
}

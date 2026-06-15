using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.ExperimentEquipmentRequirement
{
    public static class ExperimentEquipmentRequirementQueryable
    {
        public static IQueryable<DataTier.Models.ExperimentEquipmentRequirement> ApplyFilter(
            this IQueryable<DataTier.Models.ExperimentEquipmentRequirement> query,
            ExperimentEquipmentRequirementFilter filter)
        {
            return query
                .SearchIf(filter.Keyword, r => r.Note)
                .WhereEqualsIf(filter.ExperimentId, r => r.ExperimentId)
                .WhereEqualsIf(filter.EquipmentTypeId, r => r.EquipmentTypeId)
                .WhereIf(
                    filter.AllowSubstitute.HasValue,
                    r => r.AllowSubstitute == filter.AllowSubstitute!.Value);
        }
    }
}

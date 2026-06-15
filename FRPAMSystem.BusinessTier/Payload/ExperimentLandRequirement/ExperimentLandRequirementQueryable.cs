using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.ExperimentLandRequirement
{
    public static class ExperimentLandRequirementQueryable
    {
        public static IQueryable<DataTier.Models.ExperimentLandRequirement> ApplyFilter(
            this IQueryable<DataTier.Models.ExperimentLandRequirement> query,
            ExperimentLandRequirementFilter filter)
        {
            return query
                .SearchIf(
                    filter.Keyword,
                    r => r.RequiredSoilType,
                    r => r.Note
                )
                .WhereEqualsIf(filter.ExperimentId, r => r.ExperimentId)
                .WhereStringEqualsIf(filter.RequiredSoilType, r => r.RequiredSoilType);
        }
    }
}

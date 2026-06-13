using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.LandResource
{
    public static class LandResourceQueryable
    {
        public static IQueryable<DataTier.Models.LandResource> ApplyFilter(
            this IQueryable<DataTier.Models.LandResource> query,
            LandResourceFilter filter)
        {
            return query
                .SearchIf(
                    filter.Keyword,
                    l => l.LandCode,
                    l => l.Location,
                    l => l.SoilType,
                    l => l.Status
                )
                .WhereEqualsIf(filter.AreaId, l => l.AreaId)
                .WhereStringEqualsIf(filter.SoilType, l => l.SoilType)
                .WhereStringEqualsIf(
                    filter.Status?.ToString(),
                    l => l.Status);
        }
    }
}

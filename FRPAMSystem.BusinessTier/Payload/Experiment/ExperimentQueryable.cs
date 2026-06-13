using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.Experiment
{
    public static class ExperimentQueryable
    {
        public static IQueryable<DataTier.Models.Experiment> ApplyFilter(
            this IQueryable<DataTier.Models.Experiment> query,
            ExperimentFilter filter)
        {
            return query
                .SearchIf(
                    filter.Keyword,
                    e => e.ExperimentName,
                    e => e.Description,
                    e => e.Status
                )
                .WhereEqualsIf(filter.ResearcherId, e => e.ResearcherId)
                .WhereEqualsIf(filter.Priority, e => e.Priority)
                .WhereStringEqualsIf(
                    filter.Status?.ToString(),
                    e => e.Status)
                .WhereDateFromIf(filter.ExpectStartDateFrom, e => e.ExpectStartDate)
                .WhereDateToIf(filter.ExpectStartDateTo, e => e.ExpectStartDate);
        }
    }
}

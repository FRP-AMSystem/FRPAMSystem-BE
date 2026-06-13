using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.ExperimentPhase
{
    public static class ExperimentPhaseQueryable
    {
        public static IQueryable<DataTier.Models.ExperimentPhase> ApplyFilter(
            this IQueryable<DataTier.Models.ExperimentPhase> query,
            ExperimentPhaseFilter filter)
        {
            return query
                .SearchIf(
                    filter.Keyword,
                    p => p.PhaseName,
                    p => p.PhaseDescription,
                    p => p.Status
                )
                .WhereEqualsIf(filter.ExperimentId, p => p.ExperimentId)
                .WhereStringEqualsIf(
                    filter.Status?.ToString(),
                    p => p.Status)
                .WhereDateFromIf(filter.ExpectedStartDateFrom, p => p.ExpectedStartDate)
                .WhereDateToIf(filter.ExpectedStartDateTo, p => p.ExpectedStartDate);
        }
    }
}

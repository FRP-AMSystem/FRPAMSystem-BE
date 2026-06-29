using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.Schedule
{
    public static class ScheduleQueryable
    {
        public static IQueryable<DataTier.Models.Schedule> ApplyFilter(
            this IQueryable<DataTier.Models.Schedule> query,
            ScheduleFilter filter)
        {
            return query
                .SearchIf(
                    filter.Keyword,
                    s => s.Title,
                    s => s.Description,
                    s => s.Status,
                    s => s.Notes)
                .WhereEqualsIf(filter.AllocationPlanId, s => s.AllocationPlanId)
                .WhereNullableEqualsIf(filter.PhaseId, s => s.PhaseId)
                .WhereNullableEqualsIf(filter.AssignedHumanResourceId, s => s.AssignedHumanResourceId)
                .WhereNullableEqualsIf(filter.CreatedBy, s => s.CreatedBy)
                .WhereStringEqualsIf(
                    filter.Status?.ToString(),
                    s => s.Status)
                .WhereDateFromIf(filter.StartDateFrom, s => s.StartDate)
                .WhereDateToIf(filter.StartDateTo, s => s.StartDate);
        }
    }
}

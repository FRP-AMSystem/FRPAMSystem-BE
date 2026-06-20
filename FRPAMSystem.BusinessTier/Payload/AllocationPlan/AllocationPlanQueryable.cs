using FRPAMSystem.BusinessTier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationPlan
{
    public static class AllocationPlanQueryable
    {
        public static IQueryable<DataTier.Models.AllocationPlan> ApplyFilter(
            this IQueryable<DataTier.Models.AllocationPlan> query,
            AllocationPlanFilter filter)
        {
            var approveStatus = filter.ApproveStatus?.ToString();

            query = query
                .SearchIf(
                    filter.Keyword,
                    p => p.Experiment.ExperimentName,
                    p => p.Experiment.Description
                )
                .WhereEqualsIf(
                    filter.ExperimentId,
                    p => p.ExperimentId
                )
                .WhereNullableEqualsIf(
                    filter.CreatedBy,
                    p => p.CreatedBy
                )
                .WhereNullableEqualsIf(
                    filter.ApproveBy,
                    p => p.ApproveBy
                )
                .WhereStringEqualsIf(
                    approveStatus,
                    p => p.ApproveStatus
                )
                .WhereDateFromIf(
                    filter.CreatedFrom,
                    p => p.CreatedAt
                )
                .WhereDateToIf(
                    filter.CreatedTo,
                    p => p.CreatedAt
                )
                .WhereNullableDateFromIf(
                    filter.ApprovedFrom,
                    p => p.ApprovedAt
                )
                .WhereNullableDateToIf(
                    filter.ApprovedTo,
                    p => p.ApprovedAt
                );

            if (filter.MinFitnessScore.HasValue)
            {
                query = query.Where(p =>
                    p.FitnessScore.HasValue &&
                    p.FitnessScore.Value >= filter.MinFitnessScore.Value);
            }

            if (filter.MaxFitnessScore.HasValue)
            {
                query = query.Where(p =>
                    p.FitnessScore.HasValue &&
                    p.FitnessScore.Value <= filter.MaxFitnessScore.Value);
            }

            return query;
        }
    }
}

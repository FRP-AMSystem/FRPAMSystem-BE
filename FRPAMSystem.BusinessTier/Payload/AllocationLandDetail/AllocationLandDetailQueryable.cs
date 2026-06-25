using FRPAMSystem.BusinessTier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationLandDetail
{
    public static class AllocationLandDetailQueryable
    {
        public static IQueryable<DataTier.Models.AllocationLandDetail> ApplyFilter(
            this IQueryable<DataTier.Models.AllocationLandDetail> query,
            AllocationLandDetailFilter filter)
        {
            var status = filter.Status?.ToString();

            return query
                .SearchIf(
                    filter.Keyword,
                    d => d.Land.LandCode,
                    d => d.Land.Location,
                    d => d.Land.SoilType,
                    d => d.AllocationPlan.Experiment.ExperimentName,
                    d => d.AllocationPlan.Experiment.Description,
                    d => d.ExpLandReq.Note
                )
                .WhereEqualsIf(
                    filter.AllocationPlanId,
                    d => d.AllocationPlanId
                )
                .WhereEqualsIf(
                    filter.ExperimentId,
                    d => d.AllocationPlan.ExperimentId
                )
                .WhereEqualsIf(
                    filter.LandId,
                    d => d.LandId
                )
                .WhereEqualsIf(
                    filter.AreaId,
                    d => d.Land.AreaId
                )
                .WhereEqualsIf(
                    filter.ExpLandReqId,
                    d => d.ExpLandReqId
                )
                .WhereStringEqualsIf(
                    status,
                    d => d.Status
                )
                .WhereDateFromIf(
                    filter.StartFrom,
                    d => d.StartDate
                )
                .WhereDateToIf(
                    filter.StartTo,
                    d => d.StartDate
                )
                .WhereDateFromIf(
                    filter.EndFrom,
                    d => d.EndDate
                )
                .WhereDateToIf(
                    filter.EndTo,
                    d => d.EndDate
                );
        }
    }
}

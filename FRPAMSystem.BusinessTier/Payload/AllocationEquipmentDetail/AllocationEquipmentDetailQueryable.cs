using FRPAMSystem.BusinessTier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationEquipmentDetail
{
    public static class AllocationEquipmentDetailQueryable
    {
        public static IQueryable<DataTier.Models.AllocationEquipmentDetail> ApplyFilter(
            this IQueryable<DataTier.Models.AllocationEquipmentDetail> query,
            AllocationEquipmentDetailFilter filter)
        {
            var status = filter.Status?.ToString();

            query = query
                .SearchIf(
                    filter.Keyword,
                    d => d.AllocationPlan.Experiment.ExperimentName,
                    d => d.AllocationPlan.Experiment.Description,
                    d => d.AllocatedEquipmentType.Name,
                    d => d.EquipmentInstance != null ? d.EquipmentInstance.AssetCode : null,
                    d => d.EquipmentInstance != null ? d.EquipmentInstance.SerialNumber : null,
                    d => d.ExpEquipmentReq != null ? d.ExpEquipmentReq.Note : null,
                    d => d.PhaseEquipmentReq != null ? d.PhaseEquipmentReq.Note : null
                )
                .WhereEqualsIf(
                    filter.AllocationPlanId,
                    d => d.AllocationPlanId
                )
                .WhereEqualsIf(
                    filter.ExperimentId,
                    d => d.AllocationPlan.ExperimentId
                )
                .WhereNullableEqualsIf(
                    filter.ExpEquipmentReqId,
                    d => d.ExpEquipmentReqId
                )
                .WhereNullableEqualsIf(
                    filter.PhaseEquipmentReqId,
                    d => d.PhaseEquipmentReqId
                )
                .WhereEqualsIf(
                    filter.AllocatedEquipmentTypeId,
                    d => d.AllocatedEquipmentTypeId
                )
                .WhereNullableEqualsIf(
                    filter.EquipmentInstanceId,
                    d => d.EquipmentInstanceId
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

            if (filter.IsSubstitute.HasValue)
            {
                query = query.Where(d => d.IsSubstitute == filter.IsSubstitute.Value);
            }

            return query;
        }
    }
}

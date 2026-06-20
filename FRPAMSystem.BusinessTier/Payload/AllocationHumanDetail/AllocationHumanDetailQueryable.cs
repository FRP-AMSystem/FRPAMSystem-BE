using FRPAMSystem.BusinessTier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationHumanDetail
{
    public static class AllocationHumanDetailQueryable
    {
        public static IQueryable<DataTier.Models.AllocationHumanDetail> ApplyFilter(
            this IQueryable<DataTier.Models.AllocationHumanDetail> query,
            AllocationHumanDetailFilter filter)
        {
            var status = filter.Status?.ToString();

            query = query
                .SearchIf(
                    filter.Keyword,
                    d => d.AllocationPlan.Experiment.ExperimentName,
                    d => d.AllocationPlan.Experiment.Description,
                    d => d.HumanResource.User.FullName,
                    d => d.HumanResource.User.Username,
                    d => d.HumanResource.User.Email,
                    d => d.ExpHumanReq != null ? d.ExpHumanReq.Note : null,
                    d => d.PhaseHumanReq != null ? d.PhaseHumanReq.Note : null
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
                    filter.ExpHumanReqId,
                    d => d.ExpHumanReqId
                )
                .WhereNullableEqualsIf(
                    filter.PhaseHumanReqId,
                    d => d.PhaseHumanReqId
                )
                .WhereEqualsIf(
                    filter.HumanResourceId,
                    d => d.HumanResourceId
                )
                .WhereEqualsIf(
                    filter.UserId,
                    d => d.HumanResource.UserId
                )
                .WhereEqualsIf(
                    filter.RoleId,
                    d => d.HumanResource.User.RoleId
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

            if (filter.RequiredSkillId.HasValue)
            {
                query = query.Where(d =>
                    (d.ExpHumanReq != null &&
                        d.ExpHumanReq.RequiredSkillId == filter.RequiredSkillId.Value) ||
                    (d.PhaseHumanReq != null &&
                        d.PhaseHumanReq.RequiredSkillId == filter.RequiredSkillId.Value));
            }

            if (filter.MinWorkingHours.HasValue)
            {
                query = query.Where(d =>
                    d.WorkingHours >= filter.MinWorkingHours.Value);
            }

            if (filter.MaxWorkingHours.HasValue)
            {
                query = query.Where(d =>
                    d.WorkingHours <= filter.MaxWorkingHours.Value);
            }

            return query;
        }
    }
}

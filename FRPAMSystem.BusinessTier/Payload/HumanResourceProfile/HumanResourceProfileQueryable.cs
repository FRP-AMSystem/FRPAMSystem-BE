using FRPAMSystem.BusinessTier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.HumanResourceProfile
{
    public static class HumanResourceProfileQueryable
    {
        public static IQueryable<DataTier.Models.HumanResourceProfile> ApplyFilter(
            this IQueryable<DataTier.Models.HumanResourceProfile> query,
            HumanResourceProfileFilter filter)
        {
            var status = filter.Status?.ToString();

            query = query
                .SearchIf(
                    filter.Keyword,
                    h => h.User.FullName,
                    h => h.User.Username,
                    h => h.User.Email
                )
                .WhereEqualsIf(
                    filter.UserId,
                    h => h.UserId
                )
                .WhereEqualsIf(
                    filter.RoleId,
                    h => h.User.RoleId
                )
                .WhereStringEqualsIf(
                    status,
                    h => h.Status
                );

            if (filter.MinMaxWorkingHoursPerDay.HasValue)
            {
                query = query.Where(h =>
                    h.MaxWorkingHoursPerDay >= filter.MinMaxWorkingHoursPerDay.Value);
            }

            if (filter.MaxMaxWorkingHoursPerDay.HasValue)
            {
                query = query.Where(h =>
                    h.MaxWorkingHoursPerDay <= filter.MaxMaxWorkingHoursPerDay.Value);
            }

            if (filter.MinCurrentWorkload.HasValue)
            {
                query = query.Where(h =>
                    h.CurrentWorkload >= filter.MinCurrentWorkload.Value);
            }

            if (filter.MaxCurrentWorkload.HasValue)
            {
                query = query.Where(h =>
                    h.CurrentWorkload <= filter.MaxCurrentWorkload.Value);
            }

            return query;
        }
    }
}

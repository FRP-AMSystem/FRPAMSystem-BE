using FRPAMSystem.BusinessTier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.Area
{
    public static class AreaQueryable
    {
        public static IQueryable<DataTier.Models.Area> ApplyFilter(
            this IQueryable<DataTier.Models.Area> query,
            AreaFilter filter)
        {
            return query.SearchIf(
                filter.Keyword,
                a => a.AreaName,
                a => a.Description
            );
        }
    }
}

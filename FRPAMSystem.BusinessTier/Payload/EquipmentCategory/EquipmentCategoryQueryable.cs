using FRPAMSystem.BusinessTier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentCategory
{
    public static class EquipmentCategoryQueryable
    {
        public static IQueryable<DataTier.Models.EquipmentCategory> ApplyFilter(
            this IQueryable<DataTier.Models.EquipmentCategory> query,
            EquipmentCategoryFilter filter)
        {
            return query.SearchIf(
                filter.Keyword,
                c => c.CategoryName,
                c => c.Description
            );
        }
    }
}

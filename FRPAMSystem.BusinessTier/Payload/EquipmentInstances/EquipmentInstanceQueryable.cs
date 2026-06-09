using FRPAMSystem.BusinessTier.Utils;
using FRPAMSystem.DataTier.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentInstances
{
    public static class EquipmentInstanceQueryable
    {
        public static IQueryable<EquipmentInstance> ApplyFilter(
            this IQueryable<EquipmentInstance> query,
            EquipmentInstanceFilter filter)
        {
            var conditionLevel = filter.ConditionLevel?.ToString();
            var status = filter.Status?.ToString();

            return query
                .SearchIf(
                    filter.Keyword,
                    e => e.AssetCode,
                    e => e.SerialNumber,
                    e => e.Note
                )
                .WhereEqualsIf(
                    filter.EquipmentTypeId,
                    e => e.EquipmentTypeId
                )
                .WhereEqualsIf(
                    filter.EquipmentCategoryId,
                    e => e.EquipmentType.EquipmentCategoryId
                )
                .WhereStringEqualsIf(
                    conditionLevel,
                    e => e.ConditionLevel
                )
                .WhereStringEqualsIf(
                    status,
                    e => e.Status
                );
        }
    }
}

using FRPAMSystem.BusinessTier.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentType
{
    public static class EquipmentTypeQueryable
    {
            public static IQueryable<DataTier.Models.EquipmentType> ApplyFilter(
                this IQueryable<DataTier.Models.EquipmentType> query,
                EquipmentTypeFilter filter)
            {
                var trackingType = filter.TrackingType?.ToString();

                return query
                    .SearchIf(
                        filter.Keyword,
                        e => e.Name,
                        e => e.Description
                    )
                    .WhereEqualsIf(
                        filter.EquipmentCategoryId,
                        e => e.EquipmentCategoryId
                    )
                    .WhereStringEqualsIf(
                        trackingType,
                        e => e.TrackingType
                    );
            }
        }
 }

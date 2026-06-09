using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentType
{
    public class EquipmentTypeFilter
    {
        public string? Keyword { get; set; }

        public int? EquipmentCategoryId { get; set; }

        public EquipmentTrackingType? TrackingType { get; set; }
    }
}

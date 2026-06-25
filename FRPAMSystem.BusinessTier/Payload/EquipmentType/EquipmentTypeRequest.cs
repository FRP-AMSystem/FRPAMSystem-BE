using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentType
{
    public class EquipmentTypeRequest
    {
        public int EquipmentCategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public EquipmentTrackingType TrackingType { get; set; }

        public double? BaseMaintenanceIntervalHours { get; set; }

        public int TotalQuantity { get; set; }

        public string? Description { get; set; }
    }
}

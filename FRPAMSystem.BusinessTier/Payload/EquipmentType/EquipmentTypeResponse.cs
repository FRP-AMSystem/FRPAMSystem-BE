using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentType
{
    public class EquipmentTypeResponse
    {
        public int EquipmentTypeId { get; set; }

        public int EquipmentCategoryId { get; set; }

        public string? EquipmentCategoryName { get; set; }

        public string Name { get; set; } = string.Empty;

        public string TrackingType { get; set; } = string.Empty;

        public double? BaseMaintenanceIntervalHours { get; set; }

        public int TotalQuantity { get; set; }

        public int DamagedQuantity { get; set; }

        public int AvailableQuantity { get; set; }

        public int ReservedQuantity { get; set; }

        public int InUseQuantity { get; set; }

        public int MissingQuantity { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentInstances
{
    public class EquipmentInstanceResponse
    {
        public int EquipmentInstanceId { get; set; }

        public int EquipmentTypeId { get; set; }

        public string? EquipmentTypeName { get; set; }

        public int? EquipmentCategoryId { get; set; }

        public string? EquipmentCategoryName { get; set; }

        public string AssetCode { get; set; } = string.Empty;

        public string? SerialNumber { get; set; }

        public double TotalUsageHours { get; set; }

        public DateTime? LastMaintenanceDate { get; set; }

        public double UsageHoursSinceMaintenance { get; set; }


        public string ConditionLevel { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public double? EffectiveMaintenanceIntervalHours { get; set; }

        public int MaintenanceCount { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

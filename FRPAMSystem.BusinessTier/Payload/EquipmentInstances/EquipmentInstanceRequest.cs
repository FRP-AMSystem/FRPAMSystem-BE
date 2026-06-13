using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentInstances
{
        public class EquipmentInstanceRequest
        {
            public int EquipmentTypeId { get; set; }

            public string AssetCode { get; set; } = string.Empty;

            public string? SerialNumber { get; set; }

            public double TotalUsageHours { get; set; }

            public DateTime? LastMaintenanceDate { get; set; }

            public double UsageHoursSinceMaintenance { get; set; }

            public DateTime? NextMaintenanceDate { get; set; }

            public EquipmentConditionLevel ConditionLevel { get; set; }

            public EquipmentInstanceStatus Status { get; set; }

            public double? EffectiveMaintenanceIntervalHours { get; set; }

            public int MaintenanceCount { get; set; }

            public string? Note { get; set; }
        }
    }


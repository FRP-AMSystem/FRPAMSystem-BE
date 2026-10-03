using System;

namespace FRPAMSystem.BusinessTier.Payload.AllocationEquipmentDetail
{
    public class AvailableEquipmentSubstituteResponse
    {
        public int EquipmentInstanceId { get; set; }

        public string AssetCode { get; set; } = null!;

        public int EquipmentTypeId { get; set; }

        public string EquipmentTypeName { get; set; } = null!;

        public bool IsSubstitute { get; set; }

        public double EfficiencyRate { get; set; }

        public double TimeMultiplier { get; set; }

        public string? ConditionLevel { get; set; }

        public string Status { get; set; } = null!;

        public double? RemainingMaintenanceHours { get; set; }

        public string Reason { get; set; } = null!;
    }
}

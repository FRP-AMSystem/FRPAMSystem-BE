using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class EquipmentInstance
{
    public int EquipmentInstanceId { get; set; }

    public int EquipmentTypeId { get; set; }

    public string AssetCode { get; set; } = null!;

    public string? SerialNumber { get; set; }

    public double TotalUsageHour { get; set; }

    public DateTime? LastMaintenanceDate { get; set; }

    public double UsageHoursSinceLastMaintenance { get; set; }

    public string ConditionLevel { get; set; } = null!;

    public string Status { get; set; } = null!;

    public double? EffectiveIntervalHour { get; set; }

    public int MaintenanceCount { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllocationEquipmentDetail> AllocationEquipmentDetails { get; set; } = new List<AllocationEquipmentDetail>();

    public virtual EquipmentType EquipmentType { get; set; } = null!;
}

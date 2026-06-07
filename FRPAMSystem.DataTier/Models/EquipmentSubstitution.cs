using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class EquipmentSubstitution
{
    public int EquipmentSubId { get; set; }

    public int PrimaryEquipmentTypeId { get; set; }

    public int SubEquipmentTypeId { get; set; }

    public double EfficiencyRate { get; set; }

    public double TimeMultiplier { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual EquipmentType PrimaryEquipmentType { get; set; } = null!;

    public virtual EquipmentType SubEquipmentType { get; set; } = null!;
}

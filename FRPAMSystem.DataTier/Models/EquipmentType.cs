using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class EquipmentType
{
    public int EquipmentTypeId { get; set; }

    public int EquipmentCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string TrackingType { get; set; } = null!;

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

    public virtual ICollection<AllocationEquipmentDetail> AllocationEquipmentDetails { get; set; } = new List<AllocationEquipmentDetail>();

    public virtual EquipmentCategory EquipmentCategory { get; set; } = null!;

    public virtual ICollection<EquipmentInstance> EquipmentInstances { get; set; } = new List<EquipmentInstance>();

    public virtual ICollection<EquipmentSubstitution> EquipmentSubstitutionPrimaryEquipmentTypes { get; set; } = new List<EquipmentSubstitution>();

    public virtual ICollection<EquipmentSubstitution> EquipmentSubstitutionSubEquipmentTypes { get; set; } = new List<EquipmentSubstitution>();

    public virtual ICollection<ExperimentEquipmentRequirement> ExperimentEquipmentRequirements { get; set; } = new List<ExperimentEquipmentRequirement>();

    public virtual ICollection<PhaseEquipmentRequirement> PhaseEquipmentRequirements { get; set; } = new List<PhaseEquipmentRequirement>();
}

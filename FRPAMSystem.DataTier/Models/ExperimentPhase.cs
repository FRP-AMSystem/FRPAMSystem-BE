using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class ExperimentPhase
{
    public int PhaseId { get; set; }

    public int ExperimentId { get; set; }

    public string PhaseName { get; set; } = null!;

    public string? PhaseDescription { get; set; }

    public int PhaseOrder { get; set; }

    public DateTime ExpectedStartDate { get; set; }

    public DateTime ExpectedEndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Experiment Experiment { get; set; } = null!;

    public virtual ICollection<PhaseEquipmentRequirement> PhaseEquipmentRequirements { get; set; } = new List<PhaseEquipmentRequirement>();

    public virtual ICollection<PhaseHumanRequirement> PhaseHumanRequirements { get; set; } = new List<PhaseHumanRequirement>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}

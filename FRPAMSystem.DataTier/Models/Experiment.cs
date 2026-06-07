using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class Experiment
{
    public int ExperimentId { get; set; }

    public string ExperimentName { get; set; } = null!;

    public string? Description { get; set; }

    public int ResearcherId { get; set; }

    public DateTime ExpectStartDate { get; set; }

    public DateTime ExpectEndDate { get; set; }

    public DateTime? Deadline { get; set; }

    public int Priority { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllocationPlan> AllocationPlans { get; set; } = new List<AllocationPlan>();

    public virtual ICollection<ExperimentEquipmentRequirement> ExperimentEquipmentRequirements { get; set; } = new List<ExperimentEquipmentRequirement>();

    public virtual ICollection<ExperimentHumanRequirement> ExperimentHumanRequirements { get; set; } = new List<ExperimentHumanRequirement>();

    public virtual ICollection<ExperimentLandRequirement> ExperimentLandRequirements { get; set; } = new List<ExperimentLandRequirement>();

    public virtual ICollection<ExperimentPhase> ExperimentPhases { get; set; } = new List<ExperimentPhase>();

    public virtual User Researcher { get; set; } = null!;
}

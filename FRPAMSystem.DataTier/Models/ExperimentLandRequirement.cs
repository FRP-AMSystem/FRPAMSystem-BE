using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class ExperimentLandRequirement
{
    public int ExpLandReqId { get; set; }

    public int ExperimentId { get; set; }

    public decimal RequiredArea { get; set; }

    public string RequiredSoilType { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllocationLandDetail> AllocationLandDetails { get; set; } = new List<AllocationLandDetail>();

    public virtual Experiment Experiment { get; set; } = null!;
}

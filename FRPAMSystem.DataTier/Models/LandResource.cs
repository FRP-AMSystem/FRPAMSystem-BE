using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class LandResource
{
    public int LandId { get; set; }

    public int AreaId { get; set; }

    public string LandCode { get; set; } = null!;

    public decimal AreaSize { get; set; }

    public string? Location { get; set; }

    public string SoilType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllocationLandDetail> AllocationLandDetails { get; set; } = new List<AllocationLandDetail>();

    public virtual Area Area { get; set; } = null!;
}

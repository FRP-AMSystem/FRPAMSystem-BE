using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class Area
{
    public int AreaId { get; set; }

    public string AreaName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<LandResource> LandResources { get; set; } = new List<LandResource>();
}

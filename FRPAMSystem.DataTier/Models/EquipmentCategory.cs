using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class EquipmentCategory
{
    public int EquipmentCategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<EquipmentType> EquipmentTypes { get; set; } = new List<EquipmentType>();
}

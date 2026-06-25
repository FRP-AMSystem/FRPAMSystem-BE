using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentCategory
{
    public class EquipmentCategoryRequest
    {
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}

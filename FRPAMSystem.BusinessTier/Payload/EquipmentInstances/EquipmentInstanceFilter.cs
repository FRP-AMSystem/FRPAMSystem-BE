using FRPAMSystem.BusinessTier.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentInstances
{
    public class EquipmentInstanceFilter
    {
        public string? Keyword { get; set; }

        public int? EquipmentTypeId { get; set; }

        public int? EquipmentCategoryId { get; set; }

        public EquipmentConditionLevel? ConditionLevel { get; set; }

        public EquipmentInstanceStatus? Status { get; set; }
    }
}

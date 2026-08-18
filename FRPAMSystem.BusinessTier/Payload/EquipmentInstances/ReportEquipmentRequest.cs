using System;
using System.Collections.Generic;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentInstances
{
    public class ReportEquipmentRequest
    {
        public int AllocationPlanId { get; set; }
        public List<int> EquipmentInstanceIds { get; set; } = new();
        public string ReportType { get; set; } = string.Empty; // "Returned", "Damaged", "Missing"
        public string? Note { get; set; }
    }
}

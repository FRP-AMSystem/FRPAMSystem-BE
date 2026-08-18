using System.Collections.Generic;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentInstances
{
    public class ConfirmEquipmentRequest
    {
        public List<int> EquipmentInstanceIds { get; set; } = new();
        public string ConfirmAction { get; set; } = string.Empty; // "AcceptReturned", "SendToMaintenance"
        public string? Note { get; set; }
    }
}

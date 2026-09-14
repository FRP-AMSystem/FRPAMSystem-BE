namespace FRPAMSystem.BusinessTier.Payload.EquipmentExtensionRequest
{
    public class EquipmentExtensionRequestRequest
    {
        public int AllocationEquipmentDetailId { get; set; }

        public DateTime RequestedEndDate { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}

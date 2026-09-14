namespace FRPAMSystem.BusinessTier.Payload.EquipmentExtensionRequest
{
    public class EquipmentExtensionRequestFilter
    {
        public int? AllocationEquipmentDetailId { get; set; }

        public int? RequestedBy { get; set; }

        public string? Status { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedTo { get; set; }
    }
}

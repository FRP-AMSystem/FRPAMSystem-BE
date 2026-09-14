namespace FRPAMSystem.BusinessTier.Payload.EquipmentExtensionRequest
{
    public class EquipmentExtensionRequestResponse
    {
        public int ExtensionRequestId { get; set; }

        public int AllocationEquipmentDetailId { get; set; }

        public int RequestedBy { get; set; }

        public DateTime OriginalEndDate { get; set; }

        public DateTime RequestedEndDate { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

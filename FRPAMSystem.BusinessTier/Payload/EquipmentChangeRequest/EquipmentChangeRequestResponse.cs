namespace FRPAMSystem.BusinessTier.Payload.EquipmentChangeRequest
{
    public class EquipmentChangeRequestResponse
    {
        public int ChangeRequestId { get; set; }

        public int AllocationEquipmentDetailId { get; set; }

        public int? CurrentEquipmentInstanceId { get; set; }

        public int RequestedEquipmentTypeId { get; set; }

        public int? RequestedEquipmentInstanceId { get; set; }

        public int RequestedBy { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

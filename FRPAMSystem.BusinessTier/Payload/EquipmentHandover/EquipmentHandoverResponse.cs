namespace FRPAMSystem.BusinessTier.Payload.EquipmentHandover
{
    public class EquipmentHandoverResponse
    {
        public int HandoverId { get; set; }

        public int AllocationEquipmentDetailId { get; set; }

        public int? EquipmentInstanceId { get; set; }

        public int HandedOverBy { get; set; }

        public int ReceivedBy { get; set; }

        public DateTime HandoverDate { get; set; }

        public int Quantity { get; set; }

        public string? ConditionBefore { get; set; }

        public string? Note { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? ConfirmedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

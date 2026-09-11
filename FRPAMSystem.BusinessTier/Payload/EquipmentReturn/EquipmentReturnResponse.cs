namespace FRPAMSystem.BusinessTier.Payload.EquipmentReturn
{
    public class EquipmentReturnResponse
    {
        public int ReturnId { get; set; }

        public int AllocationEquipmentDetailId { get; set; }

        public int? EquipmentInstanceId { get; set; }

        public int ReturnedBy { get; set; }

        public int ReceivedBy { get; set; }

        public DateTime ReturnDate { get; set; }

        public int Quantity { get; set; }

        public string ConditionAfter { get; set; } = string.Empty;

        public bool IsDamaged { get; set; }

        public string? DamageDescription { get; set; }

        public string? Note { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? ConfirmedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

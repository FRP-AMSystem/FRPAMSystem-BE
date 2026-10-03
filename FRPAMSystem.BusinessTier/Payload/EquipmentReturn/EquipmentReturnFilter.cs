namespace FRPAMSystem.BusinessTier.Payload.EquipmentReturn
{
    public class EquipmentReturnFilter
    {
        public int? AllocationEquipmentDetailId { get; set; }

        public int? EquipmentInstanceId { get; set; }

        public int? ReturnedBy { get; set; }

        public int? ReceivedBy { get; set; }

        public string? Status { get; set; }

        public bool? IsDamaged { get; set; }

        public DateTime? ReturnDateFrom { get; set; }

        public DateTime? ReturnDateTo { get; set; }
    }
}

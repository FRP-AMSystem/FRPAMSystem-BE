namespace FRPAMSystem.BusinessTier.Payload.EquipmentHandover
{
    public class EquipmentHandoverFilter
    {
        public int? AllocationEquipmentDetailId { get; set; }

        public int? EquipmentInstanceId { get; set; }

        public int? HandedOverBy { get; set; }

        public int? ReceivedBy { get; set; }

        public string? Status { get; set; }

        public DateTime? HandoverDateFrom { get; set; }

        public DateTime? HandoverDateTo { get; set; }
    }
}

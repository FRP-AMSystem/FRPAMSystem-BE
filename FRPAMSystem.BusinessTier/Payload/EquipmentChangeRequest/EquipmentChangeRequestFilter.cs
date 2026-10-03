namespace FRPAMSystem.BusinessTier.Payload.EquipmentChangeRequest
{
    public class EquipmentChangeRequestFilter
    {
        public int? AllocationEquipmentDetailId { get; set; }

        public int? RequestedBy { get; set; }

        public int? RequestedEquipmentTypeId { get; set; }

        public int? RequestedEquipmentInstanceId { get; set; }

        public string? Status { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedTo { get; set; }
    }
}

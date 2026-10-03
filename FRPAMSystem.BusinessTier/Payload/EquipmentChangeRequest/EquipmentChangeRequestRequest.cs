namespace FRPAMSystem.BusinessTier.Payload.EquipmentChangeRequest
{
    public class EquipmentChangeRequestRequest
    {
        public int AllocationEquipmentDetailId { get; set; }

        public int RequestedEquipmentTypeId { get; set; }

        public int? RequestedEquipmentInstanceId { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}

namespace FRPAMSystem.BusinessTier.Payload.EquipmentShortageLog
{
    public class EquipmentShortageLogRequest
    {
        public int AllocationPlanId { get; set; }

        public int? ExpEquipmentReqId { get; set; }

        public int? PhaseEquipmentReqId { get; set; }

        public int ShortageQuantity { get; set; }
    }
}

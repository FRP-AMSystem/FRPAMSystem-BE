namespace FRPAMSystem.BusinessTier.Payload.EquipmentShortageLog
{
    public class EquipmentShortageLogResponse
    {
        public int ShortageLogId { get; set; }

        public int AllocationPlanId { get; set; }

        public int? ExpEquipmentReqId { get; set; }

        public int? PhaseEquipmentReqId { get; set; }

        public int ShortageQuantity { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

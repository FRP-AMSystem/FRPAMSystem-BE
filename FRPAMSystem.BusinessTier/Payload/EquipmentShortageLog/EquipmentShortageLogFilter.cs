namespace FRPAMSystem.BusinessTier.Payload.EquipmentShortageLog
{
    public class EquipmentShortageLogFilter
    {
        public int? AllocationPlanId { get; set; }

        public int? ExpEquipmentReqId { get; set; }

        public int? PhaseEquipmentReqId { get; set; }
    }
}

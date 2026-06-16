using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentShortageLog
{
    public static class EquipmentShortageLogQueryable
    {
        public static IQueryable<DataTier.Models.EquipmentShortageLog> ApplyFilter(
            this IQueryable<DataTier.Models.EquipmentShortageLog> query,
            EquipmentShortageLogFilter filter)
        {
            return query
                .WhereEqualsIf(filter.AllocationPlanId, r => r.AllocationPlanId)
                .WhereIf(
                    filter.ExpEquipmentReqId.HasValue,
                    r => r.ExpEquipmentReqId == filter.ExpEquipmentReqId!.Value)
                .WhereIf(
                    filter.PhaseEquipmentReqId.HasValue,
                    r => r.PhaseEquipmentReqId == filter.PhaseEquipmentReqId!.Value);
        }
    }
}

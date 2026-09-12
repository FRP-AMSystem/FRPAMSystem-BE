using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentHandover
{
    public static class EquipmentHandoverQueryable
    {
        public static IQueryable<DataTier.Models.EquipmentHandover> ApplyFilter(
            this IQueryable<DataTier.Models.EquipmentHandover> query,
            EquipmentHandoverFilter filter)
        {
            return query
                .WhereEqualsIf(filter.AllocationEquipmentDetailId, h => h.AllocationEquipmentDetailId)
                .WhereIf(
                    filter.EquipmentInstanceId.HasValue,
                    h => h.EquipmentInstanceId == filter.EquipmentInstanceId!.Value)
                .WhereEqualsIf(filter.HandedOverBy, h => h.HandedOverBy)
                .WhereEqualsIf(filter.ReceivedBy, h => h.ReceivedBy)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(filter.Status),
                    h => h.Status == filter.Status)
                .WhereIf(
                    filter.HandoverDateFrom.HasValue,
                    h => h.HandoverDate >= filter.HandoverDateFrom!.Value)
                .WhereIf(
                    filter.HandoverDateTo.HasValue,
                    h => h.HandoverDate <= filter.HandoverDateTo!.Value);
        }
    }
}

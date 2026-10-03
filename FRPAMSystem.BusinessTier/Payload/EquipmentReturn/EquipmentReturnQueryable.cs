using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentReturn
{
    public static class EquipmentReturnQueryable
    {
        public static IQueryable<DataTier.Models.EquipmentReturn> ApplyFilter(
            this IQueryable<DataTier.Models.EquipmentReturn> query,
            EquipmentReturnFilter filter)
        {
            return query
                .WhereEqualsIf(filter.AllocationEquipmentDetailId, r => r.AllocationEquipmentDetailId)
                .WhereIf(
                    filter.EquipmentInstanceId.HasValue,
                    r => r.EquipmentInstanceId == filter.EquipmentInstanceId!.Value)
                .WhereEqualsIf(filter.ReturnedBy, r => r.ReturnedBy)
                .WhereEqualsIf(filter.ReceivedBy, r => r.ReceivedBy)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(filter.Status),
                    r => r.Status == filter.Status)
                .WhereIf(
                    filter.IsDamaged.HasValue,
                    r => r.IsDamaged == filter.IsDamaged!.Value)
                .WhereIf(
                    filter.ReturnDateFrom.HasValue,
                    r => r.ReturnDate >= filter.ReturnDateFrom!.Value)
                .WhereIf(
                    filter.ReturnDateTo.HasValue,
                    r => r.ReturnDate <= filter.ReturnDateTo!.Value);
        }
    }
}

using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentChangeRequest
{
    public static class EquipmentChangeRequestQueryable
    {
        public static IQueryable<DataTier.Models.EquipmentChangeRequest> ApplyFilter(
            this IQueryable<DataTier.Models.EquipmentChangeRequest> query,
            EquipmentChangeRequestFilter filter)
        {
            return query
                .WhereEqualsIf(filter.AllocationEquipmentDetailId, r => r.AllocationEquipmentDetailId)
                .WhereEqualsIf(filter.RequestedBy, r => r.RequestedBy)
                .WhereEqualsIf(filter.RequestedEquipmentTypeId, r => r.RequestedEquipmentTypeId)
                .WhereIf(
                    filter.RequestedEquipmentInstanceId.HasValue,
                    r => r.RequestedEquipmentInstanceId == filter.RequestedEquipmentInstanceId!.Value)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(filter.Status),
                    r => r.Status == filter.Status)
                .WhereIf(
                    filter.CreatedFrom.HasValue,
                    r => r.CreatedAt >= filter.CreatedFrom!.Value)
                .WhereIf(
                    filter.CreatedTo.HasValue,
                    r => r.CreatedAt <= filter.CreatedTo!.Value);
        }
    }
}

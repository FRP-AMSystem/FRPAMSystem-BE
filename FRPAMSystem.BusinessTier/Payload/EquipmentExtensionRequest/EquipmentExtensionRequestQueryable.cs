using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentExtensionRequest
{
    public static class EquipmentExtensionRequestQueryable
    {
        public static IQueryable<DataTier.Models.EquipmentExtensionRequest> ApplyFilter(
            this IQueryable<DataTier.Models.EquipmentExtensionRequest> query,
            EquipmentExtensionRequestFilter filter)
        {
            return query
                .WhereEqualsIf(filter.AllocationEquipmentDetailId, r => r.AllocationEquipmentDetailId)
                .WhereEqualsIf(filter.RequestedBy, r => r.RequestedBy)
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

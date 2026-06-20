using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.EquipmentSubstitution
{
    public static class EquipmentSubstitutionQueryable
    {
        public static IQueryable<DataTier.Models.EquipmentSubstitution> ApplyFilter(
            this IQueryable<DataTier.Models.EquipmentSubstitution> query,
            EquipmentSubstitutionFilter filter)
        {
            return query
                .SearchIf(filter.Keyword, s => s.Note)
                .WhereEqualsIf(filter.PrimaryEquipmentTypeId, s => s.PrimaryEquipmentTypeId)
                .WhereEqualsIf(filter.SubEquipmentTypeId, s => s.SubEquipmentTypeId);
        }
    }
}

using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.Role
{
    public static class RoleQueryable
    {
        public static IQueryable<DataTier.Models.Role> ApplyFilter(
            this IQueryable<DataTier.Models.Role> query,
            RoleFilter filter)
        {
            return query.SearchIf(
                filter.Keyword,
                r => r.RoleName
            );
        }
    }
}

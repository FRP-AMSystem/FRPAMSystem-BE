using FRPAMSystem.BusinessTier.Utils;

namespace FRPAMSystem.BusinessTier.Payload.Notification
{
    public static class NotificationQueryable
    {
        public static IQueryable<DataTier.Models.Notification> ApplyFilter(
            this IQueryable<DataTier.Models.Notification> query,
            NotificationFilter filter)
        {
            if (!filter.IncludeDeleted)
            {
                query = query.Where(n => !n.IsDeleted);
            }

            return query
                .WhereIf(filter.IsRead.HasValue, n => n.IsRead == filter.IsRead!.Value)
                .WhereIf(
                    !string.IsNullOrWhiteSpace(filter.NotificationType),
                    n => n.NotificationType == filter.NotificationType);
        }
    }
}

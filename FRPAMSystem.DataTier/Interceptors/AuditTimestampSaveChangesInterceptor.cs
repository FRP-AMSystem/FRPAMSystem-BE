using FRPAMSystem.DataTier.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FRPAMSystem.DataTier.Interceptors
{
    public class AuditTimestampSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IClock _clock;

        public AuditTimestampSaveChangesInterceptor(IClock clock)
        {
            _clock = clock;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ApplyTimestamps(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplyTimestamps(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyTimestamps(DbContext? context)
        {
            if (context == null)
            {
                return;
            }

            var now = _clock.Now;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    SetDateTimeProperty(entry, "CreatedAt", now);
                }

                if (entry.State == EntityState.Modified)
                {
                    SetDateTimeProperty(entry, "UpdatedAt", now);
                }
            }
        }

        private static void SetDateTimeProperty(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
            string propertyName,
            DateTime value)
        {
            var property = entry.Metadata.FindProperty(propertyName);

            if (property == null ||
                (property.ClrType != typeof(DateTime) && property.ClrType != typeof(DateTime?)))
            {
                return;
            }

            entry.Property(propertyName).CurrentValue = value;
        }
    }
}

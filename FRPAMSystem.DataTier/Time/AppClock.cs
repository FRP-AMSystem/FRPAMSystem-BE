using FRPAMSystem.DataTier.Abstractions;
using FRPAMSystem.DataTier.Configuration;
using Microsoft.Extensions.Options;

namespace FRPAMSystem.DataTier.Time
{
    public class AppClock : IClock
    {
        private readonly TimeZoneInfo _timeZone;

        public AppClock(IOptions<AppClockOptions> options)
        {
            var timeZoneId = options.Value.TimeZoneId;

            if (string.IsNullOrWhiteSpace(timeZoneId))
            {
                timeZoneId = OperatingSystem.IsWindows()
                    ? "SE Asia Standard Time"
                    : "Asia/Ho_Chi_Minh";
            }

            try
            {
                _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                var fallbackId = OperatingSystem.IsWindows()
                    ? "SE Asia Standard Time"
                    : "Asia/Ho_Chi_Minh";

                _timeZone = TimeZoneInfo.FindSystemTimeZoneById(fallbackId);
            }
        }

        public DateTime Now =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);

        public DateTime UtcNow => DateTime.UtcNow;
    }
}

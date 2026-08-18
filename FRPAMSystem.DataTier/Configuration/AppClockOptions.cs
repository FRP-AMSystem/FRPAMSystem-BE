namespace FRPAMSystem.DataTier.Configuration
{
    public class AppClockOptions
    {
        public const string SectionName = "AppClock";

        /// <summary>
        /// Windows: "SE Asia Standard Time". Linux: "Asia/Ho_Chi_Minh".
        /// </summary>
        public string TimeZoneId { get; set; } = "SE Asia Standard Time";
    }
}

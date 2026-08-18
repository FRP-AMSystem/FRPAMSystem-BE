namespace FRPAMSystem.DataTier.Abstractions
{
    public interface IClock
    {
        /// <summary>Current time in the configured application timezone (default: Vietnam UTC+7).</summary>
        DateTime Now { get; }

        DateTime UtcNow { get; }
    }
}

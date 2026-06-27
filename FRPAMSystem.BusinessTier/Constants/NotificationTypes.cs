namespace FRPAMSystem.BusinessTier.Constants
{
    /// <summary>
    /// Known notification_type values. Callers may use other strings when needed.
    /// </summary>
    public static class NotificationTypes
    {
        public const string ExperimentApproved = "ExperimentApproved";
        public const string ExperimentPending = "ExperimentPending";
        public const string ConflictDetected = "ConflictDetected";
        public const string ScheduleAssigned = "ScheduleAssigned";
    }

    public static class NotificationReferenceTypes
    {
        public const string AllocationPlan = "AllocationPlan";
        public const string Experiment = "Experiment";
        public const string Schedule = "Schedule";
    }
}

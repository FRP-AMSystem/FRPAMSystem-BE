namespace FRPAMSystem.BusinessTier.Constants
{
    /// <summary>
    /// Known notification_type values. Callers may use other strings when needed.
    /// </summary>
    public static class NotificationTypes
    {
        public const string ExperimentCreated = "ExperimentCreated";
        public const string ExperimentSubmitted = "ExperimentSubmitted";
        public const string ExperimentApproved = "ExperimentApproved";
        public const string ExperimentPending = "ExperimentPending";
        public const string AllocationPlanGenerated = "AllocationPlanGenerated";
        public const string AllocationPlanSubmitted = "AllocationPlanSubmitted";
        public const string AllocationPlanApproved = "AllocationPlanApproved";
        public const string AllocationPlanRejected = "AllocationPlanRejected";
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

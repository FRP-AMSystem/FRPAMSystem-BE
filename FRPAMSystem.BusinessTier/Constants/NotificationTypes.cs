namespace FRPAMSystem.BusinessTier.Constants
{
    public static class NotificationTypes
    {
        public const string ExperimentCreated = "ExperimentCreated";
        public const string ExperimentSubmitted = "ExperimentSubmitted";
        public const string ExperimentApproved = "ExperimentApproved";
        public const string ExperimentRejected = "ExperimentRejected";
        public const string ExperimentPending = "ExperimentPending";
        public const string AllocationPlanGenerated = "AllocationPlanGenerated";
        public const string AllocationPlanSubmitted = "AllocationPlanSubmitted";
        public const string AllocationPlanApproved = "AllocationPlanApproved";
        public const string AllocationPlanRejected = "AllocationPlanRejected";
        public const string ConflictDetected = "ConflictDetected";
        public const string ScheduleAssigned = "ScheduleAssigned";

        public const string EquipmentHandoverPending = "EquipmentHandoverPending";
        public const string EquipmentHandoverConfirmed = "EquipmentHandoverConfirmed";
        public const string EquipmentHandoverRejected = "EquipmentHandoverRejected";

        public const string EquipmentReturnPending = "EquipmentReturnPending";
        public const string EquipmentReturnConfirmed = "EquipmentReturnConfirmed";
        public const string EquipmentReturnRejected = "EquipmentReturnRejected";
    }

    public static class NotificationReferenceTypes
    {
        public const string AllocationPlan = "AllocationPlan";
        public const string Experiment = "Experiment";
        public const string Schedule = "Schedule";
        public const string EquipmentHandover = "EquipmentHandover";
        public const string EquipmentReturn = "EquipmentReturn";
    }
}

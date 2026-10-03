using FRPAMSystem.BusinessTier.Constants;

namespace FRPAMSystem.BusinessTier.Emails
{
    public static class EmailNotificationTemplateCatalog
    {
        public sealed record TypeCopy(string Badge, string Accent, string Intro);

        private static readonly TypeCopy DefaultCopy = new(
            "Notification",
            "#2E7D32",
            "You have a new notification from FRPAM System.");

        private static readonly Dictionary<string, TypeCopy> Copies = new(StringComparer.OrdinalIgnoreCase)
        {
            [NotificationTypes.ExperimentCreated] = new(
                "Experiment",
                "#2E7D32",
                "A new experiment has been created and is available in the system."),
            [NotificationTypes.ExperimentSubmitted] = new(
                "Experiment",
                "#1565C0",
                "An experiment has been submitted and is waiting for manager review."),
            [NotificationTypes.ExperimentApproved] = new(
                "Experiment",
                "#2E7D32",
                "An experiment request has been approved. Planning can continue."),
            [NotificationTypes.ExperimentRejected] = new(
                "Experiment",
                "#C62828",
                "An experiment request has been rejected. Please review the reason and update if needed."),
            [NotificationTypes.ExperimentPending] = new(
                "Experiment",
                "#EF6C00",
                "An experiment is pending further action."),
            [NotificationTypes.AllocationPlanGenerated] = new(
                "Allocation",
                "#2E7D32",
                "An allocation plan has been generated for an experiment."),
            [NotificationTypes.AllocationPlanSubmitted] = new(
                "Allocation",
                "#1565C0",
                "An allocation plan has been submitted and is waiting for approval."),
            [NotificationTypes.AllocationPlanApproved] = new(
                "Allocation",
                "#2E7D32",
                "An allocation plan has been approved. Reserved resources and schedules may now apply."),
            [NotificationTypes.AllocationPlanRejected] = new(
                "Allocation",
                "#C62828",
                "An allocation plan has been rejected. Another plan can be prepared."),
            [NotificationTypes.ConflictDetected] = new(
                "Shortage",
                "#C62828",
                "An equipment shortage or allocation conflict was detected. Please review the plan."),
            [NotificationTypes.ScheduleAssigned] = new(
                "Schedule",
                "#1565C0",
                "A schedule has been assigned. Please check the date and assigned task."),
            [NotificationTypes.EquipmentHandoverPending] = new(
                "Handover",
                "#EF6C00",
                "Equipment is ready for handover. Please confirm receiving the equipment in the app."),
            [NotificationTypes.EquipmentHandoverConfirmed] = new(
                "Handover",
                "#2E7D32",
                "Equipment handover has been confirmed. The equipment is now in use."),
            [NotificationTypes.EquipmentHandoverRejected] = new(
                "Handover",
                "#C62828",
                "An equipment handover was rejected or cancelled. Please review the reason."),
            [NotificationTypes.EquipmentReturnPending] = new(
                "Return",
                "#EF6C00",
                "A new equipment return request is waiting for confirmation."),
            [NotificationTypes.EquipmentReturnConfirmed] = new(
                "Return",
                "#2E7D32",
                "An equipment return has been confirmed by the manager."),
            [NotificationTypes.EquipmentReturnRejected] = new(
                "Return",
                "#C62828",
                "An equipment return request has been rejected. Please review the reason.")
        };

        public static TypeCopy Get(string? notificationType)
        {
            if (string.IsNullOrWhiteSpace(notificationType))
            {
                return DefaultCopy;
            }

            return Copies.TryGetValue(notificationType, out var copy)
                ? copy
                : DefaultCopy;
        }
    }
}

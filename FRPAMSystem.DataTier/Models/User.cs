using System;
using System.Collections.Generic;

namespace FRPAMSystem.DataTier.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AllocationPlan> AllocationPlanApproveByNavigations { get; set; } = new List<AllocationPlan>();

    public virtual ICollection<AllocationPlan> AllocationPlanCreatedByNavigations { get; set; } = new List<AllocationPlan>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Experiment> Experiments { get; set; } = new List<Experiment>();

    public virtual HumanResourceProfile? HumanResourceProfile { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<EquipmentChangeRequest> EquipmentChangeRequestRequestedByNavigations { get; set; } = new List<EquipmentChangeRequest>();

    public virtual ICollection<EquipmentChangeRequest> EquipmentChangeRequestReviewedByNavigations { get; set; } = new List<EquipmentChangeRequest>();

    public virtual ICollection<EquipmentExtensionRequest> EquipmentExtensionRequestRequestedByNavigations { get; set; } = new List<EquipmentExtensionRequest>();

    public virtual ICollection<EquipmentExtensionRequest> EquipmentExtensionRequestReviewedByNavigations { get; set; } = new List<EquipmentExtensionRequest>();

    public virtual ICollection<EquipmentHandover> EquipmentHandoverHandedOverByNavigations { get; set; } = new List<EquipmentHandover>();

    public virtual ICollection<EquipmentHandover> EquipmentHandoverReceivedByNavigations { get; set; } = new List<EquipmentHandover>();

    public virtual ICollection<EquipmentReturn> EquipmentReturnReturnedByNavigations { get; set; } = new List<EquipmentReturn>();

    public virtual ICollection<EquipmentReturn> EquipmentReturnReceivedByNavigations { get; set; } = new List<EquipmentReturn>();
}

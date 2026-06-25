namespace FRPAMSystem.BusinessTier.Payload.ExperimentHumanRequirement
{
    public class ExperimentHumanRequirementResponse
    {
        public int ExpHumanReqId { get; set; }

        public int ExperimentId { get; set; }

        public string ExperimentName { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int? RequiredSkillId { get; set; }

        public string? RequiredSkillName { get; set; }

        public double? WorkingHoursPerDay { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

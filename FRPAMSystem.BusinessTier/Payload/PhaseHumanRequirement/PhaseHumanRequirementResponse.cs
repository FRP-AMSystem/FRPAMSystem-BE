namespace FRPAMSystem.BusinessTier.Payload.PhaseHumanRequirement
{
    public class PhaseHumanRequirementResponse
    {
        public int PhaseHumanReqId { get; set; }

        public int PhaseId { get; set; }

        public string PhaseName { get; set; } = string.Empty;

        public int ExperimentId { get; set; }

        public string ExperimentName { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int? RequiredSkillId { get; set; }

        public string? RequiredSkillName { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

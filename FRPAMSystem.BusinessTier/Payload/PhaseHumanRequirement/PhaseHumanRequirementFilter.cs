namespace FRPAMSystem.BusinessTier.Payload.PhaseHumanRequirement
{
    public class PhaseHumanRequirementFilter
    {
        public string? Keyword { get; set; }

        public int? PhaseId { get; set; }

        public int? ExperimentId { get; set; }

        public int? RoleId { get; set; }

        public int? RequiredSkillId { get; set; }
    }
}

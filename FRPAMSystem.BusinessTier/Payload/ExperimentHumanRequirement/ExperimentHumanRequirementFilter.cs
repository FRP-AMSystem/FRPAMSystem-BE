namespace FRPAMSystem.BusinessTier.Payload.ExperimentHumanRequirement
{
    public class ExperimentHumanRequirementFilter
    {
        public string? Keyword { get; set; }

        public int? ExperimentId { get; set; }

        public int? RoleId { get; set; }

        public int? RequiredSkillId { get; set; }
    }
}

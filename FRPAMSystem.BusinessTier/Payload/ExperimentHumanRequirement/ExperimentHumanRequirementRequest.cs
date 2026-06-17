namespace FRPAMSystem.BusinessTier.Payload.ExperimentHumanRequirement
{
    public class ExperimentHumanRequirementRequest
    {
        public int ExperimentId { get; set; }

        public int RoleId { get; set; }

        public int Quantity { get; set; }

        public int? RequiredSkillId { get; set; }

        public double? WorkingHoursPerDay { get; set; }

        public string? Note { get; set; }
    }
}

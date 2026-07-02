namespace FRPAMSystem.BusinessTier.Payload.PhaseHumanRequirement
{
    public class PhaseHumanRequirementRequest
    {
        public int PhaseId { get; set; }

        public int RoleId { get; set; }

        public int Quantity { get; set; }

        public int? RequiredSkillId { get; set; }

        public string? Note { get; set; }
    }
}

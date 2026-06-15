namespace FRPAMSystem.BusinessTier.Payload.ExperimentLandRequirement
{
    public class ExperimentLandRequirementRequest
    {
        public int ExperimentId { get; set; }

        public decimal RequiredArea { get; set; }

        public string RequiredSoilType { get; set; } = string.Empty;

        public string? Note { get; set; }
    }
}

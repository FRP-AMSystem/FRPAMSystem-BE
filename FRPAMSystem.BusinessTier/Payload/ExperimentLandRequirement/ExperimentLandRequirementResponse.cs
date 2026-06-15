namespace FRPAMSystem.BusinessTier.Payload.ExperimentLandRequirement
{
    public class ExperimentLandRequirementResponse
    {
        public int ExpLandReqId { get; set; }

        public int ExperimentId { get; set; }

        public string ExperimentName { get; set; } = string.Empty;

        public decimal RequiredArea { get; set; }

        public string RequiredSoilType { get; set; } = string.Empty;

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

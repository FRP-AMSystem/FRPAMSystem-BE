namespace FRPAMSystem.BusinessTier.Payload.Experiment
{
    public class ExperimentResponse
    {
        public int ExperimentId { get; set; }

        public string ExperimentName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int ResearcherId { get; set; }

        public string ResearcherName { get; set; } = string.Empty;

        public DateTime ExpectStartDate { get; set; }

        public DateTime ExpectEndDate { get; set; }

        public DateTime? Deadline { get; set; }

        public int Priority { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

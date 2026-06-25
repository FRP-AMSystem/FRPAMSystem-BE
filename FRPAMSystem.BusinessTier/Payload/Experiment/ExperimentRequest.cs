using FRPAMSystem.BusinessTier.Enums;

namespace FRPAMSystem.BusinessTier.Payload.Experiment
{
    public class ExperimentRequest
    {
        public string ExperimentName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int ResearcherId { get; set; }

        public DateTime ExpectStartDate { get; set; }

        public DateTime ExpectEndDate { get; set; }

        public DateTime? Deadline { get; set; }

        public int Priority { get; set; } = 2;

        public ExperimentStatus Status { get; set; } = ExperimentStatus.Draft;
    }
}

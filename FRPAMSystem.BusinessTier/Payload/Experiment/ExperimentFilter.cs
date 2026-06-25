using FRPAMSystem.BusinessTier.Enums;

namespace FRPAMSystem.BusinessTier.Payload.Experiment
{
    public class ExperimentFilter
    {
        public string? Keyword { get; set; }

        public int? ResearcherId { get; set; }

        public ExperimentStatus? Status { get; set; }

        public int? Priority { get; set; }

        public DateTime? ExpectStartDateFrom { get; set; }

        public DateTime? ExpectStartDateTo { get; set; }
    }
}

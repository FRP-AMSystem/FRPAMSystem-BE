using FRPAMSystem.BusinessTier.Enums;

namespace FRPAMSystem.BusinessTier.Payload.ExperimentPhase
{
    public class ExperimentPhaseFilter
    {
        public string? Keyword { get; set; }

        public int? ExperimentId { get; set; }

        public ExperimentPhaseStatus? Status { get; set; }

        public DateTime? ExpectedStartDateFrom { get; set; }

        public DateTime? ExpectedStartDateTo { get; set; }
    }
}

using FRPAMSystem.BusinessTier.Enums;

namespace FRPAMSystem.BusinessTier.Payload.ExperimentPhase
{
    public class ExperimentPhaseRequest
    {
        public int ExperimentId { get; set; }

        public string PhaseName { get; set; } = string.Empty;

        public string? PhaseDescription { get; set; }

        public int PhaseOrder { get; set; }

        public DateTime ExpectedStartDate { get; set; }

        public DateTime ExpectedEndDate { get; set; }

        public ExperimentPhaseStatus Status { get; set; } = ExperimentPhaseStatus.Planned;
    }
}

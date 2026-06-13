namespace FRPAMSystem.BusinessTier.Payload.ExperimentPhase
{
    public class ExperimentPhaseResponse
    {
        public int PhaseId { get; set; }

        public int ExperimentId { get; set; }

        public string ExperimentName { get; set; } = string.Empty;

        public string PhaseName { get; set; } = string.Empty;

        public string? PhaseDescription { get; set; }

        public int PhaseOrder { get; set; }

        public DateTime ExpectedStartDate { get; set; }

        public DateTime ExpectedEndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

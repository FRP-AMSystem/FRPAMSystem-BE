namespace FRPAMSystem.BusinessTier.Payload.Experiment
{
    public class UpdateExperimentStatusRequest
    {
        public string Status { get; set; } = string.Empty; // "Completed", "Canceled"
    }
}

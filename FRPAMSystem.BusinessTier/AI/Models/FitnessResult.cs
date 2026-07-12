namespace FRPAMSystem.BusinessTier.AI.Models
{
    public class FitnessResult
    {
        public double FitnessScore { get; set; }

        public double PenaltyScore { get; set; }

        public int ConflictCount { get; set; }

        public List<string> Advantages { get; set; } = new();

        public List<string> Disadvantages { get; set; } = new();
    }
}

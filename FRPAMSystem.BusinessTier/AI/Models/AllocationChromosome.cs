namespace FRPAMSystem.BusinessTier.AI.Models
{
    public class AllocationChromosome
    {
        public List<AllocationGene> Genes { get; set; } = new();

        public double FitnessScore { get; set; }

        public double PenaltyScore { get; set; }

        public int ConflictCount { get; set; }

        public List<string> Advantages { get; set; } = new();

        public List<string> Disadvantages { get; set; } = new();

        public AllocationChromosome Clone()
        {
            return new AllocationChromosome
            {
                Genes = Genes.Select(g => g.Clone()).ToList(),
                FitnessScore = FitnessScore,
                PenaltyScore = PenaltyScore,
                ConflictCount = ConflictCount,
                Advantages = Advantages.ToList(),
                Disadvantages = Disadvantages.ToList()
            };
        }
    }
}

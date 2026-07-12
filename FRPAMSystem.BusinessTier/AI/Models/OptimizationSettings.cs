namespace FRPAMSystem.BusinessTier.AI.Models
{
    public class OptimizationSettings
    {
        public int PopulationSize { get; set; } = 100;

        public int GenerationCount { get; set; } = 150;

        public double MutationRate { get; set; } = 0.08d;

        public double CrossoverRate { get; set; } = 0.75d;

        public int EliteCount { get; set; } = 5;

        public int TournamentSize { get; set; } = 4;

        public int TopSuggestionCount { get; set; } = 5;

        public int MaxScheduleShiftDays { get; set; } = 14;

        public void Normalize()
        {
            PopulationSize = Math.Clamp(PopulationSize, 20, 1000);
            GenerationCount = Math.Clamp(GenerationCount, 1, 5000);
            MutationRate = Math.Clamp(MutationRate, 0.001d, 0.8d);
            CrossoverRate = Math.Clamp(CrossoverRate, 0d, 1d);
            EliteCount = Math.Clamp(EliteCount, 1, Math.Max(1, PopulationSize / 4));
            TournamentSize = Math.Clamp(TournamentSize, 2, Math.Max(2, PopulationSize));
            TopSuggestionCount = Math.Clamp(TopSuggestionCount, 1, 5);
            MaxScheduleShiftDays = Math.Clamp(MaxScheduleShiftDays, 0, 90);
        }

        public OptimizationSettings Clone()
        {
            return new OptimizationSettings
            {
                PopulationSize = PopulationSize,
                GenerationCount = GenerationCount,
                MutationRate = MutationRate,
                CrossoverRate = CrossoverRate,
                EliteCount = EliteCount,
                TournamentSize = TournamentSize,
                TopSuggestionCount = TopSuggestionCount,
                MaxScheduleShiftDays = MaxScheduleShiftDays
            };
        }
    }
}

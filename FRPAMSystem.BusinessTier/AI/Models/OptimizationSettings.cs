namespace FRPAMSystem.BusinessTier.AI.Models
{
    public class OptimizationSettings
    {
        public int PopulationSize { get; set; } = 100;

        public int GenerationCount { get; set; } = 150;

        public double MutationRate { get; set; } = 0.08d;

        public double InitialMutationRate { get; set; } = 0.30d;

        public double FinalMutationRate { get; set; } = 0.05d;

        public double CrossoverRate { get; set; } = 0.75d;

        public int EliteCount { get; set; } = 5;

        public int TournamentSize { get; set; } = 4;

        public int TopSuggestionCount { get; set; } = 5;

        public int MaxScheduleShiftDays { get; set; } = 14;

        public double LandWeight { get; set; } = 25d;

        public double HumanWeight { get; set; } = 25d;

        public double EquipmentWeight { get; set; } = 25d;

        public double ScheduleWeight { get; set; } = 25d;

        public double PenaltyWeight { get; set; } = 1d;

        public double BonusWeight { get; set; } = 1d;

        public double HardConstraintPenalty { get; set; } = 25d;

        public double SoftConstraintPenalty { get; set; } = 5d;

        public void Normalize()
        {
            PopulationSize = Math.Clamp(PopulationSize, 20, 1000);
            GenerationCount = Math.Clamp(GenerationCount, 1, 5000);
            MutationRate = Math.Clamp(MutationRate, 0.001d, 0.8d);
            InitialMutationRate = Math.Clamp(InitialMutationRate, MutationRate, 0.8d);
            FinalMutationRate = Math.Clamp(FinalMutationRate, 0.001d, InitialMutationRate);
            CrossoverRate = Math.Clamp(CrossoverRate, 0d, 1d);
            EliteCount = Math.Clamp(EliteCount, 1, Math.Max(1, PopulationSize / 4));
            TournamentSize = Math.Clamp(TournamentSize, 2, Math.Max(2, PopulationSize));
            TopSuggestionCount = Math.Clamp(TopSuggestionCount, 1, 5);
            MaxScheduleShiftDays = Math.Clamp(MaxScheduleShiftDays, 0, 90);
            LandWeight = Math.Clamp(LandWeight, 0d, 100d);
            HumanWeight = Math.Clamp(HumanWeight, 0d, 100d);
            EquipmentWeight = Math.Clamp(EquipmentWeight, 0d, 100d);
            ScheduleWeight = Math.Clamp(ScheduleWeight, 0d, 100d);
            PenaltyWeight = Math.Clamp(PenaltyWeight, 0d, 10d);
            BonusWeight = Math.Clamp(BonusWeight, 0d, 10d);
            HardConstraintPenalty = Math.Clamp(HardConstraintPenalty, 1d, 100d);
            SoftConstraintPenalty = Math.Clamp(SoftConstraintPenalty, 0.1d, 50d);
        }

        public OptimizationSettings Clone()
        {
            return new OptimizationSettings
            {
                PopulationSize = PopulationSize,
                GenerationCount = GenerationCount,
                MutationRate = MutationRate,
                InitialMutationRate = InitialMutationRate,
                FinalMutationRate = FinalMutationRate,
                CrossoverRate = CrossoverRate,
                EliteCount = EliteCount,
                TournamentSize = TournamentSize,
                TopSuggestionCount = TopSuggestionCount,
                MaxScheduleShiftDays = MaxScheduleShiftDays,
                LandWeight = LandWeight,
                HumanWeight = HumanWeight,
                EquipmentWeight = EquipmentWeight,
                ScheduleWeight = ScheduleWeight,
                PenaltyWeight = PenaltyWeight,
                BonusWeight = BonusWeight,
                HardConstraintPenalty = HardConstraintPenalty,
                SoftConstraintPenalty = SoftConstraintPenalty
            };
        }
    }
}

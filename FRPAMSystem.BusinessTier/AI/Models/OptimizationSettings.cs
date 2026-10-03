namespace FRPAMSystem.BusinessTier.AI.Models
{
    public class OptimizationSettings
    {
        public int PopulationSize { get; set; } = 100;

        public int GenerationCount { get; set; } = 80;

        public double MutationRate { get; set; } = 0.15d;

        public double InitialMutationRate { get; set; } = 0.30d;

        public double FinalMutationRate { get; set; } = 0.05d;

        public double CrossoverRate { get; set; } = 0.8d;

        public int EliteCount { get; set; } = 10;

        public int TournamentSize { get; set; } = 5;

        public int TopSuggestionCount { get; set; } = 5;


        public double LandWeight { get; set; } = 0.20d;

        public double HumanWeight { get; set; } = 0.25d;

        public double EquipmentWeight { get; set; } = 0.40d;

        public double MaintenanceWeight { get; set; } = 0.15d;

        public double SoftConstraintPenalty { get; set; } = 5d;

        public double MaximumBonus { get; set; } = 5d;

        [Obsolete("Use SoftConstraintPenalty and MaximumBonus.")]
        public double HardConstraintPenalty { get; set; } = 25d;

        [Obsolete("Use SoftConstraintPenalty.")]
        public double PenaltyWeight { get; set; } = 1d;

        [Obsolete("Bonuses are capped by MaximumBonus.")]
        public double BonusWeight { get; set; } = 1d;

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
            LandWeight = Math.Clamp(LandWeight, 0d, 1d);
            HumanWeight = Math.Clamp(HumanWeight, 0d, 1d);
            EquipmentWeight = Math.Clamp(EquipmentWeight, 0d, 1d);
            MaintenanceWeight = Math.Clamp(MaintenanceWeight, 0d, 1d);
            SoftConstraintPenalty = Math.Clamp(SoftConstraintPenalty, 0d, 50d);
            PenaltyWeight = Math.Clamp(PenaltyWeight, 0d, 10d);
            BonusWeight = Math.Clamp(BonusWeight, 0d, 10d);
            MaximumBonus = Math.Clamp(MaximumBonus, 0d, 5d);

            var totalWeight = LandWeight + HumanWeight + EquipmentWeight + MaintenanceWeight;
            if (totalWeight <= 0d)
            {
                LandWeight = 0.20d;
                HumanWeight = 0.25d;
                EquipmentWeight = 0.40d;
                MaintenanceWeight = 0.15d;
            }
            else if (Math.Abs(totalWeight - 1d) > 0.000001d)
            {
                LandWeight /= totalWeight;
                HumanWeight /= totalWeight;
                EquipmentWeight /= totalWeight;
                MaintenanceWeight /= totalWeight;
            }
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
                LandWeight = LandWeight,
                HumanWeight = HumanWeight,
                EquipmentWeight = EquipmentWeight,
                MaintenanceWeight = MaintenanceWeight,
                SoftConstraintPenalty = SoftConstraintPenalty,
                MaximumBonus = MaximumBonus,
                HardConstraintPenalty = HardConstraintPenalty,
                PenaltyWeight = PenaltyWeight,
                BonusWeight = BonusWeight
            };
        }
    }
}

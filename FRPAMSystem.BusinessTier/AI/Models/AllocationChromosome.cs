namespace FRPAMSystem.BusinessTier.AI.Models
{
    public class AllocationChromosome
    {
        public List<AllocationGene> Genes { get; set; } = new();

        public double FitnessScore { get; set; }

        public double PenaltyScore { get; set; }

        public double BonusScore { get; set; }

        public double LandScore { get; set; }

        public double HumanScore { get; set; }

        public double EquipmentScore { get; set; }

        public double MaintenanceScore { get; set; }

        public int ConflictCount { get; set; }

        public int HardViolationCount { get; set; }

        public int SoftViolationCount { get; set; }

        public bool IsFeasible => HardViolationCount == 0;

        public FitnessBreakdown FitnessBreakdown { get; set; } = new();

        public ConstraintReport ConstraintReport { get; set; } = new();

        public List<string> Advantages { get; set; } = new();

        public List<string> Disadvantages { get; set; } = new();

        public AllocationChromosome Clone()
        {
            return new AllocationChromosome
            {
                Genes = Genes.Select(g => g.Clone()).ToList(),
                FitnessScore = FitnessScore,
                PenaltyScore = PenaltyScore,
                BonusScore = BonusScore,
                LandScore = LandScore,
                HumanScore = HumanScore,
                EquipmentScore = EquipmentScore,
                MaintenanceScore = MaintenanceScore,
                ConflictCount = ConflictCount,
                HardViolationCount = HardViolationCount,
                SoftViolationCount = SoftViolationCount,
                FitnessBreakdown = new FitnessBreakdown
                {
                    LandScore = FitnessBreakdown.LandScore,
                    HumanScore = FitnessBreakdown.HumanScore,
                    EquipmentScore = FitnessBreakdown.EquipmentScore,
                    MaintenanceScore = FitnessBreakdown.MaintenanceScore,
                    PenaltyScore = FitnessBreakdown.PenaltyScore,
                    BonusScore = FitnessBreakdown.BonusScore,
                    FinalScore = FitnessBreakdown.FinalScore,
                    OverallCalculation = FitnessBreakdown.OverallCalculation,
                    Land = FitnessBreakdown.Land.Clone(),
                    Human = FitnessBreakdown.Human.Clone(),
                    Equipment = FitnessBreakdown.Equipment.Clone(),
                    Maintenance = FitnessBreakdown.Maintenance.Clone(),
                    Penalties = FitnessBreakdown.Penalties.Select(p => p.Clone()).ToList(),
                    Bonuses = FitnessBreakdown.Bonuses.Select(b => b.Clone()).ToList()
                },
                ConstraintReport = new ConstraintReport
                {
                    HardViolationCount = ConstraintReport.HardViolationCount,
                    SoftViolationCount = ConstraintReport.SoftViolationCount,
                    LandConflicts = ConstraintReport.LandConflicts.ToList(),
                    HumanConflicts = ConstraintReport.HumanConflicts.ToList(),
                    EquipmentConflicts = ConstraintReport.EquipmentConflicts.ToList(),
                    MaintenanceConflicts = ConstraintReport.MaintenanceConflicts.ToList(),
                    SkillConflicts = ConstraintReport.SkillConflicts.ToList(),
                    RoleConflicts = ConstraintReport.RoleConflicts.ToList(),
                    DeadlineConflicts = ConstraintReport.DeadlineConflicts.ToList()
                },
                Advantages = Advantages.ToList(),
                Disadvantages = Disadvantages.ToList()
            };
        }
    }
}

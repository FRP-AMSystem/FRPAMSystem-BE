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

        public double ScheduleScore { get; set; }

        public int ConflictCount { get; set; }

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
                ScheduleScore = ScheduleScore,
                ConflictCount = ConflictCount,
                FitnessBreakdown = new FitnessBreakdown
                {
                    LandScore = FitnessBreakdown.LandScore,
                    HumanScore = FitnessBreakdown.HumanScore,
                    EquipmentScore = FitnessBreakdown.EquipmentScore,
                    ScheduleScore = FitnessBreakdown.ScheduleScore,
                    PenaltyScore = FitnessBreakdown.PenaltyScore,
                    BonusScore = FitnessBreakdown.BonusScore,
                    FinalScore = FitnessBreakdown.FinalScore
                },
                ConstraintReport = new ConstraintReport
                {
                    LandConflicts = ConstraintReport.LandConflicts.ToList(),
                    HumanConflicts = ConstraintReport.HumanConflicts.ToList(),
                    EquipmentConflicts = ConstraintReport.EquipmentConflicts.ToList(),
                    ScheduleConflicts = ConstraintReport.ScheduleConflicts.ToList(),
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

namespace FRPAMSystem.BusinessTier.AI.Models
{
    public class FitnessResult
    {
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

        public FitnessBreakdown Breakdown { get; set; } = new();

        public ConstraintReport ConstraintReport { get; set; } = new();

        public List<string> Advantages { get; set; } = new();

        public List<string> Disadvantages { get; set; } = new();
    }

    public class FitnessBreakdown
    {
        public double LandScore { get; set; }

        public double HumanScore { get; set; }

        public double EquipmentScore { get; set; }

        public double MaintenanceScore { get; set; }

        public double PenaltyScore { get; set; }

        public double BonusScore { get; set; }

        public double FinalScore { get; set; }

        public string OverallCalculation { get; set; } = string.Empty;

        public ScoreExplanation Land { get; set; } = new();

        public ScoreExplanation Human { get; set; } = new();

        public ScoreExplanation Equipment { get; set; } = new();

        public ScoreExplanation Maintenance { get; set; } = new();

        public List<ScoreAdjustment> Penalties { get; set; } = new();

        public List<ScoreAdjustment> Bonuses { get; set; } = new();
    }

    public class ScoreAdjustment
    {
        public string Factor { get; set; } = string.Empty;

        public double Points { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;

        public string Calculation { get; set; } = string.Empty;

        public ScoreAdjustment Clone()
        {
            return new ScoreAdjustment
            {
                Factor = Factor,
                Points = Points,
                Type = Type,
                Reason = Reason,
                Calculation = Calculation
            };
        }
    }

    public class ScoreExplanation
    {
        public double BaseScore { get; set; }

        public double FinalScore { get; set; }

        public string Calculation { get; set; } = string.Empty;

        public List<ScoreAdjustment> Adjustments { get; set; } = new();

        public List<ScoreAdjustment> Penalties { get; set; } = new();

        public List<ScoreAdjustment> Bonuses { get; set; } = new();

        public List<PhaseScoreExplanation> Phases { get; set; } = new();

        public ScoreExplanation Clone()
        {
            return new ScoreExplanation
            {
                BaseScore = BaseScore,
                FinalScore = FinalScore,
                Calculation = Calculation,
                Adjustments = Adjustments.Select(a => a.Clone()).ToList(),
                Penalties = Penalties.Select(p => p.Clone()).ToList(),
                Bonuses = Bonuses.Select(b => b.Clone()).ToList()
                ,
                Phases = Phases.Select(p => p.Clone()).ToList()
            };
        }
    }

    public class PhaseScoreExplanation
    {
        public int PhaseId { get; set; }
        public double BaseScore { get; set; }
        public List<ScoreAdjustment> SubScores { get; set; } = new();
        public List<ScoreAdjustment> Adjustments { get; set; } = new();
        public List<ScoreAdjustment> Bonuses { get; set; } = new();
        public List<ScoreAdjustment> Penalties { get; set; } = new();
        public double FinalScore { get; set; }
        public string Calculation { get; set; } = string.Empty;

        public PhaseScoreExplanation Clone()
        {
            return new PhaseScoreExplanation
            {
                PhaseId = PhaseId,
                BaseScore = BaseScore,
                SubScores = SubScores.Select(s => s.Clone()).ToList(),
                Adjustments = Adjustments.Select(a => a.Clone()).ToList(),
                Bonuses = Bonuses.Select(b => b.Clone()).ToList(),
                Penalties = Penalties.Select(p => p.Clone()).ToList(),
                FinalScore = FinalScore,
                Calculation = Calculation
            };
        }
    }

    public class ConstraintReport
    {
        public int HardViolationCount { get; set; }

        public int SoftViolationCount { get; set; }

        public bool IsFeasible => HardViolationCount == 0;

        public List<string> LandConflicts { get; set; } = new();

        public List<string> HumanConflicts { get; set; } = new();

        public List<string> EquipmentConflicts { get; set; } = new();

        public List<string> MaintenanceConflicts { get; set; } = new();

        public List<string> SkillConflicts { get; set; } = new();

        public List<string> RoleConflicts { get; set; } = new();

        public List<string> DeadlineConflicts { get; set; } = new();
    }
}

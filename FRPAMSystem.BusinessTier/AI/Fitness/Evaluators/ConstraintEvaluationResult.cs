using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness.Evaluators
{
    public class ConstraintEvaluationResult
    {
        public double Score { get; set; }

        public double Penalty { get; set; }

        public double Bonus { get; set; }

        public List<ConstraintViolation> Violations { get; set; } = new();

        public List<string> Advantages { get; set; } = new();

        public List<string> Disadvantages { get; set; } = new();

        public ScoreExplanation Explanation { get; set; } = new();

        public List<ScoreAdjustment> BonusAdjustments { get; set; } = new();

        public List<ScoreAdjustment> PenaltyAdjustments { get; set; } = new();
    }
}


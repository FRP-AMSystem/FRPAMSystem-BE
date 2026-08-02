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
    }
}

using FRPAMSystem.BusinessTier.AI.Fitness.Evaluators;
using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness
{
    public class FitnessCalculator : IFitnessCalculator
    {
        private readonly IReadOnlyList<IConstraintEvaluator> _evaluators;

        public FitnessCalculator(IEnumerable<IConstraintEvaluator> evaluators)
        {
            _evaluators = evaluators.ToList();
        }

        public FitnessResult Evaluate(AllocationChromosome chromosome, OptimizationInput input)
        {
            input.Settings.Normalize();

            var evaluationResults = _evaluators
                .Where(evaluator => !string.Equals(evaluator.Category, "Schedule", StringComparison.OrdinalIgnoreCase))
                .Select(evaluator => (Evaluator: evaluator, Result: evaluator.Evaluate(chromosome, input)))
                .ToList();

            var landResult = GetResult<ILandConstraintEvaluator>(evaluationResults);
            var humanResult = GetResult<IHumanConstraintEvaluator>(evaluationResults);
            var equipmentResult = GetResult<IEquipmentConstraintEvaluator>(evaluationResults);
            var maintenanceResult = GetResult<IMaintenanceConstraintEvaluator>(evaluationResults);

            var land = Clamp(landResult?.Score ?? 0d);
            var human = Clamp(humanResult?.Score ?? 0d);
            var equipment = Clamp(equipmentResult?.Score ?? 0d);
            var maintenance = Clamp(maintenanceResult?.Score ?? 0d);
            var weightedScore =
                land * input.Settings.LandWeight +
                human * input.Settings.HumanWeight +
                equipment * input.Settings.EquipmentWeight +
                maintenance * input.Settings.MaintenanceWeight;

            var violations = evaluationResults
                .SelectMany(r => r.Result.Violations)
                .GroupBy(v => (v.Category, v.Severity, v.Message))
                .Select(g => g.First())
                .ToList();
            var hardCount = violations.Count(v => v.Severity == ConstraintSeverity.Hard);
            var softCount = violations.Count(v => v.Severity == ConstraintSeverity.Soft);
            var penalty = softCount * input.Settings.SoftConstraintPenalty * input.Settings.PenaltyWeight;
            var bonus = Math.Clamp(
                evaluationResults.Sum(r => r.Result.Bonus) * input.Settings.BonusWeight,
                0d,
                input.Settings.MaximumBonus);
            var finalScore = Clamp(weightedScore - penalty + bonus);

            var result = new FitnessResult
            {
                FitnessScore = finalScore,
                PenaltyScore = -penalty,
                BonusScore = bonus,
                LandScore = land,
                HumanScore = human,
                EquipmentScore = equipment,
                MaintenanceScore = maintenance,
                ConflictCount = violations.Count,
                HardViolationCount = hardCount,
                SoftViolationCount = softCount,
                Advantages = evaluationResults.SelectMany(r => r.Result.Advantages).Distinct().Take(8).ToList(),
                Disadvantages = evaluationResults.SelectMany(r => r.Result.Disadvantages).Distinct().Take(12).ToList()
            };

            if (hardCount == 0)
            {
                result.Advantages.Insert(0, "Allocation candidate is fully feasible with zero hard constraint violations.");
            }

            var penalties = violations
                .Where(v => v.Severity == ConstraintSeverity.Soft)
                .Select(v => new ScoreAdjustment
                {
                    Factor = $"Soft Constraint ({v.Category})",
                    Points = -input.Settings.SoftConstraintPenalty,
                    Type = "Penalty",
                    Reason = v.Message,
                    Calculation = $"1 × -{input.Settings.SoftConstraintPenalty:F2}"
                })
                .ToList();
            var bonuses = evaluationResults
                .SelectMany(r => r.Result.BonusAdjustments)
                .Select(a => a.Clone())
                .ToList();

            result.Breakdown = new FitnessBreakdown
            {
                LandScore = land,
                HumanScore = human,
                EquipmentScore = equipment,
                MaintenanceScore = maintenance,
                PenaltyScore = -penalty,
                BonusScore = bonus,
                FinalScore = finalScore,
                OverallCalculation =
                    $"Land: {land:F2} × {input.Settings.LandWeight:P0} = {land * input.Settings.LandWeight:F2}; " +
                    $"Human: {human:F2} × {input.Settings.HumanWeight:P0} = {human * input.Settings.HumanWeight:F2}; " +
                    $"Equipment: {equipment:F2} × {input.Settings.EquipmentWeight:P0} = {equipment * input.Settings.EquipmentWeight:F2}; " +
                    $"Maintenance: {maintenance:F2} × {input.Settings.MaintenanceWeight:P0} = {maintenance * input.Settings.MaintenanceWeight:F2}; " +
                    $"Weighted Score = {weightedScore:F2}; Soft Penalty = {softCount} × {input.Settings.SoftConstraintPenalty:F2} = -{penalty:F2}; " +
                    $"Bonus = +{bonus:F2}; Final Fitness = Clamp({weightedScore:F2} - {penalty:F2} + {bonus:F2}, 0, 100) = {finalScore:F2}",
                Land = CloneExplanation(landResult?.Explanation),
                Human = CloneExplanation(humanResult?.Explanation),
                Equipment = CloneExplanation(equipmentResult?.Explanation),
                Maintenance = CloneExplanation(maintenanceResult?.Explanation),
                Penalties = penalties,
                Bonuses = bonuses
            };

            PopulateConstraintReport(result.ConstraintReport, violations);
            ApplyToChromosome(chromosome, result);
            return result;
        }

        private static double Clamp(double value) => Math.Clamp(value, 0d, 100d);

        private static ScoreExplanation CloneExplanation(ScoreExplanation? explanation) =>
            explanation?.Clone() ?? new ScoreExplanation();

        private static ConstraintEvaluationResult? GetResult<TEvaluator>(
            IEnumerable<(IConstraintEvaluator Evaluator, ConstraintEvaluationResult Result)> results)
            where TEvaluator : IConstraintEvaluator =>
            results.FirstOrDefault(r => r.Evaluator is TEvaluator).Result;

        private static void PopulateConstraintReport(ConstraintReport report, IEnumerable<ConstraintViolation> violations)
        {
            foreach (var violation in violations)
            {
                switch (violation.Category)
                {
                    case "Land": report.LandConflicts.Add(violation.Message); break;
                    case "Human": report.HumanConflicts.Add(violation.Message); break;
                    case "Equipment": report.EquipmentConflicts.Add(violation.Message); break;
                    case "Maintenance": report.MaintenanceConflicts.Add(violation.Message); break;
                    case "Skill": report.SkillConflicts.Add(violation.Message); break;
                    case "Role": report.RoleConflicts.Add(violation.Message); break;
                    case "Deadline": report.DeadlineConflicts.Add(violation.Message); break;
                    default: report.HumanConflicts.Add(violation.Message); break;
                }
            }

            report.HardViolationCount = violations.Count(v => v.Severity == ConstraintSeverity.Hard);
            report.SoftViolationCount = violations.Count(v => v.Severity == ConstraintSeverity.Soft);
        }

        private static void ApplyToChromosome(AllocationChromosome chromosome, FitnessResult result)
        {
            chromosome.FitnessScore = result.FitnessScore;
            chromosome.PenaltyScore = result.PenaltyScore;
            chromosome.BonusScore = result.BonusScore;
            chromosome.ConflictCount = result.ConflictCount;
            chromosome.HardViolationCount = result.HardViolationCount;
            chromosome.SoftViolationCount = result.SoftViolationCount;
            chromosome.LandScore = result.LandScore;
            chromosome.HumanScore = result.HumanScore;
            chromosome.EquipmentScore = result.EquipmentScore;
            chromosome.MaintenanceScore = result.MaintenanceScore;
            chromosome.FitnessBreakdown = result.Breakdown;
            chromosome.ConstraintReport = result.ConstraintReport;
            chromosome.Advantages = result.Advantages.ToList();
            chromosome.Disadvantages = result.Disadvantages.ToList();
        }
    }
}

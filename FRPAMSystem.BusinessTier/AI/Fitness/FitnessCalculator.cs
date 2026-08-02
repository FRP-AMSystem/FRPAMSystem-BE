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
                .Select(evaluator => (Evaluator: evaluator, Result: evaluator.Evaluate(chromosome, input)))
                .ToList();

            var land = GetScore<ILandConstraintEvaluator>(evaluationResults);
            var human = GetScore<IHumanConstraintEvaluator>(evaluationResults);
            var equipment = GetScore<IEquipmentConstraintEvaluator>(evaluationResults);
            var maintenance = GetScore<IMaintenanceConstraintEvaluator>(evaluationResults);
            var schedule = GetScore<IScheduleConstraintEvaluator>(evaluationResults);

            var equipmentScore = Math.Clamp((equipment * 0.75d) + (maintenance * 0.25d), 0d, 100d);
            var weightedScore = CalculateWeightedScore(input.Settings, land, human, equipmentScore, schedule);

            var violations = evaluationResults.SelectMany(r => r.Result.Violations).ToList();
            var hardPenalty = violations.Count(v => v.Severity == ConstraintSeverity.Hard) * input.Settings.HardConstraintPenalty;
            var softPenalty = violations.Count(v => v.Severity == ConstraintSeverity.Soft) * input.Settings.SoftConstraintPenalty;
            var evaluatorPenalty = evaluationResults.Sum(r => r.Result.Penalty);
            var penalty = (hardPenalty + softPenalty + evaluatorPenalty) * input.Settings.PenaltyWeight;
            var bonus = evaluationResults.Sum(r => r.Result.Bonus) * input.Settings.BonusWeight;
            var finalScore = Math.Clamp(weightedScore - penalty + bonus, 0d, 100d);

            var result = new FitnessResult
            {
                FitnessScore = finalScore,
                PenaltyScore = -Math.Round(penalty, 2),
                BonusScore = Math.Round(bonus, 2),
                LandScore = Math.Round(land, 2),
                HumanScore = Math.Round(human, 2),
                EquipmentScore = Math.Round(equipmentScore, 2),
                ScheduleScore = Math.Round(schedule, 2),
                ConflictCount = violations.Count,
                Advantages = evaluationResults.SelectMany(r => r.Result.Advantages).Distinct().Take(8).ToList(),
                Disadvantages = evaluationResults.SelectMany(r => r.Result.Disadvantages).Distinct().Take(12).ToList()
            };

            result.Breakdown = new FitnessBreakdown
            {
                LandScore = result.LandScore,
                HumanScore = result.HumanScore,
                EquipmentScore = result.EquipmentScore,
                ScheduleScore = result.ScheduleScore,
                PenaltyScore = result.PenaltyScore,
                BonusScore = result.BonusScore,
                FinalScore = Math.Round(finalScore, 2)
            };

            PopulateConstraintReport(result.ConstraintReport, violations);
            ApplyToChromosome(chromosome, result);
            return result;
        }

        private static double CalculateWeightedScore(
            OptimizationSettings settings,
            double land,
            double human,
            double equipment,
            double schedule)
        {
            var totalWeight = settings.LandWeight +
                              settings.HumanWeight +
                              settings.EquipmentWeight +
                              settings.ScheduleWeight;

            if (totalWeight <= 0d)
            {
                return (land + human + equipment + schedule) / 4d;
            }

            return ((land * settings.LandWeight) +
                    (human * settings.HumanWeight) +
                    (equipment * settings.EquipmentWeight) +
                    (schedule * settings.ScheduleWeight)) / totalWeight;
        }

        private static double GetScore<TEvaluator>(
            IEnumerable<(IConstraintEvaluator Evaluator, ConstraintEvaluationResult Result)> results)
            where TEvaluator : IConstraintEvaluator
        {
            var match = results.FirstOrDefault(r => r.Evaluator is TEvaluator);
            return match.Result?.Score ?? 0d;
        }

        private static void PopulateConstraintReport(
            ConstraintReport report,
            IEnumerable<ConstraintViolation> violations)
        {
            foreach (var violation in violations)
            {
                var target = violation.Category switch
                {
                    "Land" => report.LandConflicts,
                    "Human" => report.HumanConflicts,
                    "Equipment" => report.EquipmentConflicts,
                    "Schedule" => report.ScheduleConflicts,
                    "Maintenance" => report.MaintenanceConflicts,
                    "Skill" => report.SkillConflicts,
                    "Role" => report.RoleConflicts,
                    "Deadline" => report.DeadlineConflicts,
                    _ => report.ScheduleConflicts
                };

                if (!target.Contains(violation.Message))
                {
                    target.Add(violation.Message);
                }
            }
        }

        private static void ApplyToChromosome(AllocationChromosome chromosome, FitnessResult result)
        {
            chromosome.FitnessScore = result.FitnessScore;
            chromosome.PenaltyScore = result.PenaltyScore;
            chromosome.ConflictCount = result.ConflictCount;
            chromosome.LandScore = result.LandScore;
            chromosome.HumanScore = result.HumanScore;
            chromosome.EquipmentScore = result.EquipmentScore;
            chromosome.ScheduleScore = result.ScheduleScore;
            chromosome.BonusScore = result.BonusScore;
            chromosome.FitnessBreakdown = result.Breakdown;
            chromosome.ConstraintReport = result.ConstraintReport;
            chromosome.Advantages = result.Advantages.ToList();
            chromosome.Disadvantages = result.Disadvantages.ToList();
        }
    }
}

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

            var landResult = GetResult<ILandConstraintEvaluator>(evaluationResults);
            var humanResult = GetResult<IHumanConstraintEvaluator>(evaluationResults);
            var equipmentResult = GetResult<IEquipmentConstraintEvaluator>(evaluationResults);
            var maintenanceResult = GetResult<IMaintenanceConstraintEvaluator>(evaluationResults);
            var scheduleResult = GetResult<IScheduleConstraintEvaluator>(evaluationResults);

            var land = landResult?.Score ?? 0d;
            var human = humanResult?.Score ?? 0d;
            var equipment = equipmentResult?.Score ?? 0d;
            var maintenance = maintenanceResult?.Score ?? 0d;
            var schedule = scheduleResult?.Score ?? 0d;

            var equipmentScore = Math.Clamp((equipment * 0.75d) + (maintenance * 0.25d), 0d, 100d);
            var weightedScore = CalculateWeightedScore(input.Settings, land, human, equipmentScore, schedule);

            var violations = evaluationResults.SelectMany(r => r.Result.Violations).ToList();
            var hardCount = violations.Count(v => v.Severity == ConstraintSeverity.Hard);
            var softCount = violations.Count(v => v.Severity == ConstraintSeverity.Soft);
            var hardPenalty = hardCount * input.Settings.HardConstraintPenalty;
            var softPenalty = softCount * input.Settings.SoftConstraintPenalty;
            var evaluatorPenalty = evaluationResults.Sum(r => r.Result.Penalty);
            var penalty = (hardPenalty + softPenalty + evaluatorPenalty) * input.Settings.PenaltyWeight;
            var bonus = evaluationResults.Sum(r => r.Result.Bonus) * input.Settings.BonusWeight;
            var finalScore = Math.Clamp(weightedScore - penalty + bonus, 0d, 100d);

            var advantages = evaluationResults.SelectMany(r => r.Result.Advantages).Distinct().Take(8).ToList();
            if (hardCount == 0 && !advantages.Contains("Allocation candidate is fully feasible with zero hard constraint violations."))
            {
                advantages.Insert(0, "Allocation candidate is fully feasible with zero hard constraint violations.");
            }

            var penalties = new List<ScoreAdjustment>();
            foreach (var hardViolation in violations.Where(v => v.Severity == ConstraintSeverity.Hard))
            {
                var pts = -input.Settings.HardConstraintPenalty * input.Settings.PenaltyWeight;
                penalties.Add(new ScoreAdjustment
                {
                    Factor = $"Hard Constraint ({hardViolation.Category})",
                    Points = Math.Round(pts, 2),
                    Type = "Penalty",
                    Reason = hardViolation.Message,
                    Calculation = $"1 × -{input.Settings.HardConstraintPenalty} × {input.Settings.PenaltyWeight} = {pts:F2}"
                });
            }

            foreach (var softViolation in violations.Where(v => v.Severity == ConstraintSeverity.Soft))
            {
                var pts = -input.Settings.SoftConstraintPenalty * input.Settings.PenaltyWeight;
                penalties.Add(new ScoreAdjustment
                {
                    Factor = $"Soft Constraint ({softViolation.Category})",
                    Points = Math.Round(pts, 2),
                    Type = "Penalty",
                    Reason = softViolation.Message,
                    Calculation = $"1 × -{input.Settings.SoftConstraintPenalty} × {input.Settings.PenaltyWeight} = {pts:F2}"
                });
            }

            foreach (var evaluatorPenaltyAdj in evaluationResults.SelectMany(r => r.Result.PenaltyAdjustments))
            {
                var pts = evaluatorPenaltyAdj.Points * input.Settings.PenaltyWeight;
                penalties.Add(new ScoreAdjustment
                {
                    Factor = evaluatorPenaltyAdj.Factor,
                    Points = Math.Round(pts, 2),
                    Type = "Penalty",
                    Reason = evaluatorPenaltyAdj.Reason,
                    Calculation = $"{evaluatorPenaltyAdj.Calculation} × {input.Settings.PenaltyWeight} = {pts:F2}"
                });
            }

            var bonuses = new List<ScoreAdjustment>();
            foreach (var bonusAdj in evaluationResults.SelectMany(r => r.Result.BonusAdjustments))
            {
                var pts = bonusAdj.Points * input.Settings.BonusWeight;
                bonuses.Add(new ScoreAdjustment
                {
                    Factor = bonusAdj.Factor,
                    Points = Math.Round(pts, 2),
                    Type = "Bonus",
                    Reason = bonusAdj.Reason,
                    Calculation = input.Settings.BonusWeight == 1d
                        ? bonusAdj.Calculation
                        : $"{bonusAdj.Calculation} × {input.Settings.BonusWeight} = {pts:F2}"
                });
            }

            var equipmentAdjustments = new List<ScoreAdjustment>();
            if (equipmentResult?.Explanation != null)
            {
                equipmentAdjustments.AddRange(equipmentResult.Explanation.Adjustments);
            }
            if (maintenanceResult?.Explanation != null)
            {
                equipmentAdjustments.AddRange(maintenanceResult.Explanation.Adjustments);
            }

            var eqBaseScore = Math.Round(
                ((equipmentResult?.Explanation.BaseScore ?? 10d) * 0.75d) +
                ((maintenanceResult?.Explanation.BaseScore ?? 100d) * 0.25d),
                2);

            var equipmentCalc = $"({equipment:F2} [Resource Score] × 0.75) + ({maintenance:F2} [Maintenance Score] × 0.25) = {equipmentScore:F2}";

            var equipmentExplanation = new ScoreExplanation
            {
                BaseScore = eqBaseScore,
                FinalScore = Math.Round(equipmentScore, 2),
                Calculation = equipmentCalc,
                Adjustments = equipmentAdjustments,
                Bonuses = (equipmentResult?.Explanation.Bonuses ?? new List<ScoreAdjustment>())
                    .Concat(maintenanceResult?.Explanation.Bonuses ?? new List<ScoreAdjustment>())
                    .Select(b => b.Clone())
                    .ToList(),
                Penalties = (equipmentResult?.Explanation.Penalties ?? new List<ScoreAdjustment>())
                    .Concat(maintenanceResult?.Explanation.Penalties ?? new List<ScoreAdjustment>())
                    .Select(p => p.Clone())
                    .ToList()
            };

            var totalWeight = input.Settings.LandWeight +
                              input.Settings.HumanWeight +
                              input.Settings.EquipmentWeight +
                              input.Settings.ScheduleWeight;

            var weightedCalc = totalWeight > 0d
                ? $"Weighted: ({land:F2}×{input.Settings.LandWeight:F1} + {human:F2}×{input.Settings.HumanWeight:F1} + {equipmentScore:F2}×{input.Settings.EquipmentWeight:F1} + {schedule:F2}×{input.Settings.ScheduleWeight:F1})/{totalWeight:F1} = {weightedScore:F2}"
                : $"Weighted: ({land:F2} + {human:F2} + {equipmentScore:F2} + {schedule:F2})/4 = {weightedScore:F2}";

            var overallCalc = $"{weightedCalc} | Penalty: -{penalty:F2} | Bonus: +{bonus:F2} | Final Fitness: Clamp({weightedScore:F2} - {penalty:F2} + {bonus:F2}, 0, 100) = {finalScore:F2}";

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
                HardViolationCount = hardCount,
                SoftViolationCount = softCount,
                Advantages = advantages,
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
                FinalScore = Math.Round(finalScore, 2),
                OverallCalculation = overallCalc,
                Land = landResult?.Explanation.Clone() ?? new ScoreExplanation(),
                Human = humanResult?.Explanation.Clone() ?? new ScoreExplanation(),
                Equipment = equipmentExplanation,
                Schedule = scheduleResult?.Explanation.Clone() ?? new ScoreExplanation(),
                Penalties = penalties,
                Bonuses = bonuses
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

        private static ConstraintEvaluationResult? GetResult<TEvaluator>(
            IEnumerable<(IConstraintEvaluator Evaluator, ConstraintEvaluationResult Result)> results)
            where TEvaluator : IConstraintEvaluator
        {
            var match = results.FirstOrDefault(r => r.Evaluator is TEvaluator);
            return match.Result;
        }

        private static void PopulateConstraintReport(
            ConstraintReport report,
            IEnumerable<ConstraintViolation> violations)
        {
            var violationList = violations.ToList();
            report.HardViolationCount = violationList.Count(v => v.Severity == ConstraintSeverity.Hard);
            report.SoftViolationCount = violationList.Count(v => v.Severity == ConstraintSeverity.Soft);

            foreach (var violation in violationList)
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
            chromosome.HardViolationCount = result.HardViolationCount;
            chromosome.SoftViolationCount = result.SoftViolationCount;
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

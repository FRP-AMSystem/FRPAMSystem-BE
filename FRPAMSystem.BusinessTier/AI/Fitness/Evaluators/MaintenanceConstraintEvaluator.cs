using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness.Evaluators
{
    public class MaintenanceConstraintEvaluator : IMaintenanceConstraintEvaluator
    {
        public string Category => "Maintenance";

        public ConstraintEvaluationResult Evaluate(AllocationChromosome chromosome, OptimizationInput input)
        {
            var result = new ConstraintEvaluationResult();
            var equipment = input.EquipmentInstances.ToDictionary(e => e.EquipmentInstanceId);
            var phaseScores = new List<double>();
            var phases = new List<PhaseScoreExplanation>();

            foreach (var gene in chromosome.Genes)
            {
                var scores = new List<double>();
                var phase = new PhaseScoreExplanation { PhaseId = gene.PhaseId };
                foreach (var assignment in gene.EquipmentAssignments)
                {
                    if (assignment.EquipmentInstanceId is null ||
                        !equipment.TryGetValue(assignment.EquipmentInstanceId.Value, out var instance))
                    {
                        continue;
                    }

                    var baseInterval = instance.EquipmentType?.BaseMaintenanceIntervalHours ??
                                       instance.EffectiveIntervalHour ?? 0d;
                    if (baseInterval <= 0d)
                    {
                        scores.Add(70d);
                        phase.SubScores.Add(SubScore($"Equipment {instance.AssetCode}", 70d));
                        continue;
                    }

                    var effectiveInterval = baseInterval *
                        GetConditionFactor(instance.ConditionLevel) *
                        GetMaintenanceCountFactor(instance.MaintenanceCount);
                    var remainingHours = effectiveInterval - instance.UsageHoursSinceLastMaintenance;
                    var estimatedUsage = FitnessEvaluationHelper.EstimatePhaseUsageHours(gene);
                    var rawScore = FitnessEvaluationHelper.ClampScore(
                        remainingHours / Math.Max(1d, effectiveInterval) * 100d);
                    var score = rawScore;

                    if (remainingHours <= 0d || FitnessEvaluationHelper.IsMaintenanceStatus(instance.Status))
                    {
                        Add(result, ConstraintSeverity.Hard, $"Equipment {instance.AssetCode} is due for maintenance.");
                        score = 0d;
                        phase.Adjustments.Add(Adjustment("Maintenance Due", -rawScore, "Adjustment",
                            $"Remaining hours: {remainingHours:F1}; status: {instance.Status}.", $"{rawScore:F2} → 0"));
                    }
                    else if (remainingHours < estimatedUsage)
                    {
                        Add(result, ConstraintSeverity.Soft,
                            $"Equipment {instance.AssetCode} may not safely finish phase {gene.PhaseId} before maintenance.");
                        var windowScore = FitnessEvaluationHelper.ClampScore(remainingHours / Math.Max(1d, estimatedUsage) * 100d);
                        score = Math.Min(rawScore, windowScore);
                        phase.Adjustments.Add(Adjustment("Insufficient Maintenance Window", score - rawScore, "Adjustment",
                            $"Remaining {remainingHours:F1}h < estimated phase usage {estimatedUsage:F1}h.",
                            $"{rawScore:F2} → {score:F2}"));
                    }
                    else if (remainingHours < effectiveInterval * 0.2d)
                    {
                        Add(result, ConstraintSeverity.Soft,
                            $"Equipment {instance.AssetCode} has low remaining maintenance hours.");
                        phase.Adjustments.Add(Adjustment("Low Maintenance Margin", 0d, "Adjustment",
                            $"Remaining {remainingHours:F1}h is below 20% of effective interval {effectiveInterval:F1}h.",
                            $"score remains {score:F2}"));
                    }
                    else
                    {
                        result.Bonus += 1d;
                        var adjustment = Adjustment("Equipment Maintenance Health", 1d, "Bonus",
                            $"Equipment {instance.AssetCode} has healthy maintenance margin ({remainingHours:F1}h remaining).", "+1");
                        result.BonusAdjustments.Add(adjustment);
                        phase.Bonuses.Add(adjustment.Clone());
                    }

                    phase.SubScores.Add(SubScore($"Equipment {instance.AssetCode}", score));
                    scores.Add(score);
                }

                var phaseScore = scores.Count == 0 ? 100d : scores.Average();
                phase.FinalScore = phaseScore;
                phase.Calculation = scores.Count == 0
                    ? "No equipment assignments = 100.00"
                    : $"Average({string.Join(", ", scores.Select(s => s.ToString("F2")))}) = {phaseScore:F2}";
                phaseScores.Add(phaseScore);
                phases.Add(phase);
            }

            result.Score = phaseScores.Count == 0 ? 100d : FitnessEvaluationHelper.ClampScore(phaseScores.Average());
            result.Explanation = new ScoreExplanation
            {
                FinalScore = result.Score,
                Calculation = phaseScores.Count == 0
                    ? "No maintenance phases = 100.00"
                    : $"Average({string.Join(", ", phaseScores.Select(s => s.ToString("F2")))}) = {result.Score:F2}",
                Adjustments = phases.SelectMany(p => p.Adjustments).ToList(),
                Bonuses = result.BonusAdjustments.Select(b => b.Clone()).ToList(),
                Phases = phases
            };
            return result;
        }

        private static ScoreAdjustment SubScore(string factor, double score) =>
            new() { Factor = factor, Points = score, Type = "SubScore", Reason = "Normalized maintenance score.", Calculation = $"{score:F2}" };

        private static ScoreAdjustment Adjustment(string factor, double points, string type, string reason, string calculation) =>
            new() { Factor = factor, Points = points, Type = type, Reason = reason, Calculation = calculation };

        private static double GetConditionFactor(string? conditionLevel) =>
            conditionLevel?.Trim().ToLowerInvariant() switch
            {
                "good" => 1.0d,
                "fair" => 0.85d,
                "poor" => 0.60d,
                "critical" => 0.30d,
                _ => 0d
            };

        private static double GetMaintenanceCountFactor(int maintenanceCount) =>
            maintenanceCount <= 2 ? 1.0d :
            maintenanceCount <= 5 ? 0.9d :
            maintenanceCount <= 10 ? 0.75d : 0.60d;

        private static void Add(ConstraintEvaluationResult result, ConstraintSeverity severity, string message)
        {
            if (result.Violations.Any(v => v.Severity == severity && v.Message == message))
            {
                return;
            }
            result.Violations.Add(new ConstraintViolation("Maintenance", severity, message));
            result.Disadvantages.Add(message);
        }
    }
}

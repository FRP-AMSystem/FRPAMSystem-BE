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
            var scoreParts = new List<double>();

            var adjustments = new List<ScoreAdjustment>();
            var baseScores = new List<double>();

            foreach (var gene in chromosome.Genes)
            {
                foreach (var assignment in gene.EquipmentAssignments)
                {
                    if (assignment.EquipmentInstanceId is null ||
                        !equipment.TryGetValue(assignment.EquipmentInstanceId.Value, out var instance))
                    {
                        continue;
                    }

                    var baseInterval = instance.EquipmentType?.BaseMaintenanceIntervalHours ??
                                       instance.EffectiveIntervalHour ??
                                       0d;

                    if (baseInterval <= 0d)
                    {
                        scoreParts.Add(70d);
                        baseScores.Add(70d);
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Default Maintenance Baseline",
                            Points = 0d,
                            Type = "Addition",
                            Reason = $"Equipment {instance.AssetCode} has no specified base maintenance interval.",
                            Calculation = "Default baseline = 70"
                        });
                        continue;
                    }

                    var effectiveInterval = baseInterval *
                                            GetConditionFactor(instance.ConditionLevel) *
                                            GetMaintenanceCountFactor(instance.MaintenanceCount);
                    var remainingHours = effectiveInterval - instance.UsageHoursSinceLastMaintenance;
                    var assignedHours = FitnessEvaluationHelper.EstimatePhaseUsageHours(gene);
                    var rawPercent = FitnessEvaluationHelper.Percent(Math.Max(0d, remainingHours), effectiveInterval);
                    var maintenanceScore = 100d * rawPercent;
                    baseScores.Add(Math.Round(maintenanceScore, 2));

                    if (remainingHours <= 0d || FitnessEvaluationHelper.IsMaintenanceStatus(instance.Status))
                    {
                        Add(result, ConstraintSeverity.Hard, $"Equipment {instance.AssetCode} is due for maintenance.");
                        maintenanceScore -= 60d;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Maintenance Due",
                            Points = -60d,
                            Type = "Deduction",
                            Reason = $"Equipment {instance.AssetCode} is due for maintenance (remaining: {remainingHours:F1}h, status: {instance.Status}).",
                            Calculation = "-60"
                        });
                    }
                    else if (remainingHours < assignedHours)
                    {
                        Add(result, ConstraintSeverity.Soft, $"Equipment {instance.AssetCode} may not safely finish phase {gene.PhaseId} before maintenance.");
                        maintenanceScore -= 30d;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Insufficient Maintenance Window",
                            Points = -30d,
                            Type = "Deduction",
                            Reason = $"Equipment {instance.AssetCode} remaining hours ({remainingHours:F1}h) < estimated phase duration ({assignedHours:F1}h).",
                            Calculation = "-30"
                        });
                    }
                    else if (remainingHours < effectiveInterval * 0.2d)
                    {
                        Add(result, ConstraintSeverity.Soft, $"Equipment {instance.AssetCode} has low remaining maintenance hours.");
                        maintenanceScore -= 15d;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Low Maintenance Margin",
                            Points = -15d,
                            Type = "Deduction",
                            Reason = $"Equipment {instance.AssetCode} remaining hours ({remainingHours:F1}h) is below 20% threshold of effective interval ({effectiveInterval:F1}h).",
                            Calculation = "-15"
                        });
                    }
                    else
                    {
                        result.Bonus += 2d;
                        result.BonusAdjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Equipment Maintenance Health",
                            Points = 2d,
                            Type = "Bonus",
                            Reason = $"Equipment {instance.AssetCode} has healthy maintenance margin ({remainingHours:F1}h remaining).",
                            Calculation = "+2"
                        });
                    }

                    scoreParts.Add(FitnessEvaluationHelper.ClampScore(maintenanceScore));
                }
            }

            result.Score = scoreParts.Count == 0 ? 100d : scoreParts.Average();
            var baseScore = baseScores.Count == 0 ? 100d : Math.Round(baseScores.Average(), 2);
            string calcStr;
            if (scoreParts.Count <= 1)
            {
                var calcParts = $"{baseScore:F0} (Base) " + string.Join(" ", adjustments.Select(a => $"{(a.Points >= 0 ? "+" : "")}{a.Points:F2} ({a.Factor})"));
                calcStr = $"{calcParts} = {result.Score:F2}";
            }
            else
            {
                calcStr = "Instances: " + string.Join(" + ", scoreParts.Select(s => $"{s:F2}")) + $" / {scoreParts.Count} = {result.Score:F2}";
            }

            result.Explanation = new ScoreExplanation
            {
                BaseScore = baseScore,
                FinalScore = Math.Round(result.Score, 2),
                Calculation = calcStr,
                Adjustments = adjustments,
                Bonuses = result.BonusAdjustments.Select(b => b.Clone()).ToList(),
                Penalties = result.PenaltyAdjustments.Select(p => p.Clone()).ToList()
            };
            return result;
        }

        private static double GetConditionFactor(string? conditionLevel)
        {
            return conditionLevel?.Trim().ToLowerInvariant() switch
            {
                "good" => 1.0d,
                "fair" => 0.85d,
                "poor" => 0.60d,
                "critical" => 0.30d,
                _ => 0.85d
            };
        }

        private static double GetMaintenanceCountFactor(int maintenanceCount)
        {
            return maintenanceCount switch
            {
                <= 2 => 1.0d,
                <= 5 => 0.9d,
                <= 10 => 0.75d,
                _ => 0.60d
            };
        }

        private static void Add(ConstraintEvaluationResult result, ConstraintSeverity severity, string message)
        {
            result.Violations.Add(new ConstraintViolation("Maintenance", severity, message));
            result.Disadvantages.Add(message);
        }
    }
}

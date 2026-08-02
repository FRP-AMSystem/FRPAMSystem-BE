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
                        continue;
                    }

                    var effectiveInterval = baseInterval *
                                            GetConditionFactor(instance.ConditionLevel) *
                                            GetMaintenanceCountFactor(instance.MaintenanceCount);
                    var remainingHours = effectiveInterval - instance.UsageHoursSinceLastMaintenance;
                    var assignedHours = FitnessEvaluationHelper.EstimatePhaseUsageHours(gene);
                    var maintenanceScore = 100d * FitnessEvaluationHelper.Percent(Math.Max(0d, remainingHours), effectiveInterval);

                    if (remainingHours <= 0d || FitnessEvaluationHelper.IsMaintenanceStatus(instance.Status))
                    {
                        Add(result, ConstraintSeverity.Hard, $"Equipment {instance.AssetCode} is due for maintenance.");
                        maintenanceScore -= 60d;
                    }
                    else if (remainingHours < assignedHours)
                    {
                        Add(result, ConstraintSeverity.Soft, $"Equipment {instance.AssetCode} may not safely finish phase {gene.PhaseId} before maintenance.");
                        maintenanceScore -= 30d;
                    }
                    else if (remainingHours < effectiveInterval * 0.2d)
                    {
                        Add(result, ConstraintSeverity.Soft, $"Equipment {instance.AssetCode} has low remaining maintenance hours.");
                        maintenanceScore -= 15d;
                    }
                    else
                    {
                        result.Bonus += 2d;
                    }

                    scoreParts.Add(FitnessEvaluationHelper.ClampScore(maintenanceScore));
                }
            }

            result.Score = scoreParts.Count == 0 ? 100d : scoreParts.Average();
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

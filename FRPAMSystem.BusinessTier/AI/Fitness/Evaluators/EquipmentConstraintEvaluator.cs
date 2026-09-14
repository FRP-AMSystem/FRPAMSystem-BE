using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness.Evaluators
{
    public class EquipmentConstraintEvaluator : IEquipmentConstraintEvaluator
    {
        public string Category => "Equipment";

        public ConstraintEvaluationResult Evaluate(AllocationChromosome chromosome, OptimizationInput input)
        {
            var result = new ConstraintEvaluationResult();
            var instances = input.EquipmentInstances.ToDictionary(e => e.EquipmentInstanceId);
            var equipmentTypes = input.EquipmentInstances
                .Select(e => e.EquipmentType)
                .Where(t => t is not null)
                .GroupBy(t => t.EquipmentTypeId)
                .ToDictionary(g => g.Key, g => g.First());
            var scoreParts = new List<double>();

            var adjustments = new List<ScoreAdjustment>();
            var baseScores = new List<double>();

            foreach (var gene in chromosome.Genes)
            {
                var requirements = FitnessEvaluationHelper.GetEquipmentRequirements(gene.PhaseId, input).ToList();
                var geneScore = requirements.Count == 0 ? 80d : 10d;
                baseScores.Add(geneScore);

                foreach (var requirement in requirements)
                {
                    var assignments = gene.EquipmentAssignments
                        .Where(e =>
                        {
                            if (requirement.PhaseEquipmentRequirementId.HasValue)
                            {
                                return e.PhaseEquipmentRequirementId == requirement.PhaseEquipmentRequirementId.Value;
                            }

                            if (requirement.ExperimentEquipmentRequirementId.HasValue)
                            {
                                return e.ExperimentEquipmentRequirementId == requirement.ExperimentEquipmentRequirementId.Value;
                            }

                            return e.RequiredEquipmentTypeId == requirement.EquipmentTypeId;
                        })
                        .ToList();

                    var typeScore = EvaluateQuantity(requirement, assignments, equipmentTypes, result, gene.PhaseId);
                    var qtyPts = typeScore * 0.35d;
                    geneScore += qtyPts;
                    adjustments.Add(new ScoreAdjustment
                    {
                        Factor = "Equipment Quantity",
                        Points = Math.Round(qtyPts, 2),
                        Type = qtyPts >= 35d ? "Addition" : "Deduction",
                        Reason = $"Phase {gene.PhaseId} equipment requirement (Type {requirement.EquipmentTypeId}) quantity fulfillment: {assignments.Count}/{requirement.Quantity}.",
                        Calculation = $"typeScore ({typeScore:F1}) × 0.35 = {qtyPts:F2}"
                    });

                    foreach (var assignment in assignments)
                    {
                        if (assignment.EquipmentInstanceId is null ||
                            !instances.TryGetValue(assignment.EquipmentInstanceId.Value, out var instance))
                        {
                            Add(result, ConstraintSeverity.Hard, $"Phase {gene.PhaseId} has a missing equipment instance.");
                            geneScore -= 25d;
                            adjustments.Add(new ScoreAdjustment
                            {
                                Factor = "Missing Equipment Instance",
                                Points = -25d,
                                Type = "Deduction",
                                Reason = $"Phase {gene.PhaseId} has a missing equipment instance.",
                                Calculation = "-25"
                            });
                            continue;
                        }

                        var assignmentScore = 30d;
                        if (instance.EquipmentTypeId == requirement.EquipmentTypeId)
                        {
                            assignmentScore += 30d;
                            adjustments.Add(new ScoreAdjustment
                            {
                                Factor = "Primary Equipment Match",
                                Points = 30d,
                                Type = "Addition",
                                Reason = $"Equipment {instance.AssetCode} matches required type {requirement.EquipmentTypeId}.",
                                Calculation = "+30"
                            });
                        }
                        else if (assignment.IsSubstitute && requirement.AllowSubstitute)
                        {
                            var efficiencyRate = Math.Clamp(assignment.EfficiencyRate, 0d, 1d);

                            if (requirement.MinAcceptableEfficiency.HasValue &&
                                efficiencyRate < requirement.MinAcceptableEfficiency.Value)
                            {
                                Add(result, ConstraintSeverity.Hard, $"Equipment {instance.AssetCode} substitute efficiency is below the minimum.");
                                assignmentScore -= 20d;
                                assignmentScore = (assignmentScore + 20d) * efficiencyRate;
                                adjustments.Add(new ScoreAdjustment
                                {
                                    Factor = "Equipment Substitution",
                                    Points = Math.Round(assignmentScore - 60d, 2),
                                    Type = "Substitution",
                                    Reason = $"Substitute equipment {instance.AssetCode} (type {instance.EquipmentTypeId}) has efficiency {efficiencyRate:P0} below required {requirement.MinAcceptableEfficiency:P0}.",
                                    Calculation = $"(30 - 20 + 20) × {efficiencyRate:F2} = {assignmentScore:F1} (vs primary 60)"
                                });
                            }
                            else
                            {
                                result.Disadvantages.Add($"Equipment {instance.AssetCode} is a substitute allocation ({efficiencyRate:P0} efficiency).");
                                assignmentScore = (assignmentScore + 20d) * efficiencyRate;
                                adjustments.Add(new ScoreAdjustment
                                {
                                    Factor = "Equipment Substitution",
                                    Points = Math.Round(assignmentScore - 60d, 2),
                                    Type = "Substitution",
                                    Reason = $"Required equipment type {requirement.EquipmentTypeId} substituted by type {instance.EquipmentTypeId} ({instance.AssetCode}) at {efficiencyRate:P0} efficiency (Time Multiplier: {assignment.TimeMultiplier:F2}x).",
                                    Calculation = $"(30 + 20) × {efficiencyRate:F2} = {assignmentScore:F1} (vs primary 60; Time Multiplier: {assignment.TimeMultiplier:F2}x)"
                                });
                            }
                        }
                        else
                        {
                            Add(result, ConstraintSeverity.Hard, $"Equipment {instance.AssetCode} does not match required type.");
                            assignmentScore -= 40d;
                            adjustments.Add(new ScoreAdjustment
                            {
                                Factor = "Invalid Equipment Type",
                                Points = -40d,
                                Type = "Deduction",
                                Reason = $"Equipment {instance.AssetCode} does not match required type {requirement.EquipmentTypeId}.",
                                Calculation = "-40"
                            });
                        }

                        if (!FitnessEvaluationHelper.IsAvailableStatus(instance.Status))
                        {
                            var severity = FitnessEvaluationHelper.IsMaintenanceStatus(instance.Status)
                                ? ConstraintSeverity.Hard
                                : ConstraintSeverity.Soft;
                            Add(result, severity, $"Equipment {instance.AssetCode} status is {instance.Status}.");
                            var statusDeduction = severity == ConstraintSeverity.Hard ? 40d : 20d;
                            assignmentScore -= statusDeduction;
                            adjustments.Add(new ScoreAdjustment
                            {
                                Factor = "Equipment Status",
                                Points = -statusDeduction,
                                Type = "Deduction",
                                Reason = $"Equipment {instance.AssetCode} status is {instance.Status}.",
                                Calculation = $"-{statusDeduction}"
                            });
                        }

                        if (input.ExistingEquipmentAllocations.Any(a =>
                                a.EquipmentInstanceId == instance.EquipmentInstanceId &&
                                FitnessEvaluationHelper.Overlaps(gene.StartDate, gene.EndDate, a.StartDate, a.EndDate)))
                        {
                            Add(result, ConstraintSeverity.Hard, $"Equipment {instance.AssetCode} overlaps with an existing allocation.");
                            assignmentScore -= 35d;
                            adjustments.Add(new ScoreAdjustment
                            {
                                Factor = "Equipment External Overlap",
                                Points = -35d,
                                Type = "Deduction",
                                Reason = $"Equipment {instance.AssetCode} overlaps with an existing allocation.",
                                Calculation = "-35"
                            });
                        }

                        var condScore = ConditionScore(instance.ConditionLevel);
                        assignmentScore += condScore;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Equipment Condition",
                            Points = condScore,
                            Type = condScore >= 0 ? "Addition" : "Deduction",
                            Reason = $"Equipment {instance.AssetCode} condition level is {instance.ConditionLevel ?? "Fair"}.",
                            Calculation = $"{condScore:+0;-0;0}"
                        });

                        geneScore += FitnessEvaluationHelper.ClampScore(assignmentScore) * 0.65d / Math.Max(1, requirement.Quantity);
                    }
                }

                scoreParts.Add(FitnessEvaluationHelper.ClampScore(geneScore));
            }

            var internalOverlaps = FitnessEvaluationHelper.CountInternalOverlaps(
                chromosome.Genes.SelectMany(g => g.EquipmentAssignments
                    .Select(e => (e.EquipmentInstanceId, g.StartDate, g.EndDate))));

            for (var i = 0; i < internalOverlaps; i++)
            {
                Add(result, ConstraintSeverity.Hard, "Equipment is double-booked inside the candidate plan.");
            }

            result.Score = scoreParts.Count == 0 ? 0d : scoreParts.Average();

            var baseScore = baseScores.Count == 0 ? 10d : Math.Round(baseScores.Average(), 2);
            string calcStr;
            if (scoreParts.Count <= 1)
            {
                var calcParts = $"{baseScore:F0} (Base) " + string.Join(" ", adjustments.Select(a => $"{(a.Points >= 0 ? "+" : "")}{a.Points:F2} ({a.Factor})"));
                calcStr = $"{calcParts} = {result.Score:F2}";
            }
            else
            {
                calcStr = "Phases: " + string.Join(" + ", scoreParts.Select((s, i) => $"Phase {chromosome.Genes[i].PhaseId} ({s:F2})")) + $" / {scoreParts.Count} = {result.Score:F2}";
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

        private static double EvaluateQuantity(
            EquipmentRequirementSnapshot requirement,
            IReadOnlyCollection<EquipmentAssignmentGene> assignments,
            IReadOnlyDictionary<int, DataTier.Models.EquipmentType> equipmentTypes,
            ConstraintEvaluationResult result,
            int phaseId)
        {
            if (equipmentTypes.TryGetValue(requirement.EquipmentTypeId, out var type) &&
                string.Equals(type.TrackingType, "Quantity", StringComparison.OrdinalIgnoreCase))
            {
                var availableQuantity = Math.Max(type.AvailableQuantity, assignments.Count);
                if (availableQuantity < requirement.Quantity)
                {
                    Add(result, ConstraintSeverity.Soft, $"Phase {phaseId} has quantity-based equipment shortage for type {requirement.EquipmentTypeId}.");
                }

                return 100d * FitnessEvaluationHelper.Percent(availableQuantity, requirement.Quantity);
            }

            if (assignments.Count < requirement.Quantity)
            {
                Add(result, ConstraintSeverity.Soft, $"Phase {phaseId} has insufficient equipment quantity.");
            }

            return 100d * FitnessEvaluationHelper.Percent(assignments.Count, requirement.Quantity);
        }

        private static double ConditionScore(string? conditionLevel)
        {
            return conditionLevel?.Trim().ToLowerInvariant() switch
            {
                "good" => 15d,
                "fair" => 9d,
                "poor" => 3d,
                "critical" => -15d,
                _ => 5d
            };
        }

        private static void Add(ConstraintEvaluationResult result, ConstraintSeverity severity, string message)
        {
            result.Violations.Add(new ConstraintViolation("Equipment", severity, message));
            result.Disadvantages.Add(message);
        }
    }
}

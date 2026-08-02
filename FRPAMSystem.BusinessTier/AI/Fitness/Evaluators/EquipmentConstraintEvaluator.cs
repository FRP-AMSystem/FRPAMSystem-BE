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

            foreach (var gene in chromosome.Genes)
            {
                var requirements = FitnessEvaluationHelper.GetEquipmentRequirements(gene.PhaseId, input).ToList();
                var geneScore = requirements.Count == 0 ? 80d : 10d;

                foreach (var requirement in requirements)
                {
                    var assignments = gene.EquipmentAssignments
                        .Where(e => e.PhaseEquipmentRequirementId == requirement.PhaseEquipmentRequirementId ||
                                    e.ExperimentEquipmentRequirementId == requirement.ExperimentEquipmentRequirementId)
                        .ToList();

                    var typeScore = EvaluateQuantity(requirement, assignments, equipmentTypes, result, gene.PhaseId);
                    geneScore += typeScore * 0.35d;

                    foreach (var assignment in assignments)
                    {
                        if (assignment.EquipmentInstanceId is null ||
                            !instances.TryGetValue(assignment.EquipmentInstanceId.Value, out var instance))
                        {
                            Add(result, ConstraintSeverity.Hard, $"Phase {gene.PhaseId} has a missing equipment instance.");
                            geneScore -= 25d;
                            continue;
                        }

                        var assignmentScore = 30d;
                        if (instance.EquipmentTypeId == requirement.EquipmentTypeId)
                        {
                            assignmentScore += 30d;
                        }
                        else if (assignment.IsSubstitute && requirement.AllowSubstitute)
                        {
                            var substitution = input.EquipmentSubstitutions.FirstOrDefault(s =>
                                s.PrimaryEquipmentTypeId == requirement.EquipmentTypeId &&
                                s.SubEquipmentTypeId == instance.EquipmentTypeId);
                            var efficiencyRate = substitution?.EfficiencyRate ?? assignment.EfficiencyRate;

                            if (requirement.MinAcceptableEfficiency.HasValue &&
                                efficiencyRate < requirement.MinAcceptableEfficiency.Value)
                            {
                                Add(result, ConstraintSeverity.Soft, $"Equipment {instance.AssetCode} substitute efficiency is below the minimum.");
                            }

                            assignmentScore = (assignmentScore + 20d) * Math.Clamp(efficiencyRate, 0d, 1d);
                            Add(result, ConstraintSeverity.Soft, $"Equipment {instance.AssetCode} is a substitute allocation.");
                        }
                        else
                        {
                            Add(result, ConstraintSeverity.Hard, $"Equipment {instance.AssetCode} does not match required type.");
                            assignmentScore -= 40d;
                        }

                        if (!FitnessEvaluationHelper.IsAvailableStatus(instance.Status))
                        {
                            var severity = FitnessEvaluationHelper.IsMaintenanceStatus(instance.Status)
                                ? ConstraintSeverity.Hard
                                : ConstraintSeverity.Soft;
                            Add(result, severity, $"Equipment {instance.AssetCode} status is {instance.Status}.");
                            assignmentScore -= severity == ConstraintSeverity.Hard ? 40d : 20d;
                        }

                        if (input.ExistingEquipmentAllocations.Any(a =>
                                a.EquipmentInstanceId == instance.EquipmentInstanceId &&
                                FitnessEvaluationHelper.Overlaps(gene.StartDate, gene.EndDate, a.StartDate, a.EndDate)))
                        {
                            Add(result, ConstraintSeverity.Hard, $"Equipment {instance.AssetCode} overlaps with an existing allocation.");
                            assignmentScore -= 35d;
                        }

                        assignmentScore += ConditionScore(instance.ConditionLevel);
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
                type.TrackingType.Equals("Quantity", StringComparison.OrdinalIgnoreCase))
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

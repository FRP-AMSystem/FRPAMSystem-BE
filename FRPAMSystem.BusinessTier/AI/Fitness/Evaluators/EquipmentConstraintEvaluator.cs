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
                .Where(e => e.EquipmentType is not null)
                .GroupBy(e => e.EquipmentTypeId)
                .ToDictionary(g => g.Key, g => g.First().EquipmentType!);
            var phaseScores = new List<double>();
            var phases = new List<PhaseScoreExplanation>();

            foreach (var gene in chromosome.Genes)
            {
                var requirements = FitnessEvaluationHelper.GetEquipmentRequirements(gene.PhaseId, input).ToList();
                var phase = new PhaseScoreExplanation { PhaseId = gene.PhaseId };
                var phaseParts = new List<(double Quantity, double Type, double Availability, double Condition, double Substitution)>();

                foreach (var requirement in requirements)
                {
                    var assignments = gene.EquipmentAssignments.Where(a =>
                        requirement.PhaseEquipmentRequirementId.HasValue
                            ? a.PhaseEquipmentRequirementId == requirement.PhaseEquipmentRequirementId
                            : requirement.ExperimentEquipmentRequirementId.HasValue
                                ? a.ExperimentEquipmentRequirementId == requirement.ExperimentEquipmentRequirementId
                                : a.RequiredEquipmentTypeId == requirement.EquipmentTypeId).ToList();
                    var quantity = FitnessEvaluationHelper.Percent(assignments.Count, requirement.Quantity) * 100d;
                    phase.Adjustments.Add(Adjustment("Equipment Quantity", quantity >= 100d ? 100d : quantity, quantity >= 100d ? "Adjustment" : "Partial",
                        $"Phase {gene.PhaseId} equipment requirement (Type {requirement.EquipmentTypeId}) quantity fulfillment: {assignments.Count}/{requirement.Quantity}.",
                        $"{quantity:F2}"));
                    if (assignments.Count < requirement.Quantity)
                    {
                        Add(result, ConstraintSeverity.Soft,
                            $"Phase {gene.PhaseId} has insufficient equipment quantity.");
                    }
                    if (equipmentTypes.TryGetValue(requirement.EquipmentTypeId, out var type) &&
                        string.Equals(type.TrackingType, "Quantity", StringComparison.OrdinalIgnoreCase) &&
                        type.AvailableQuantity < requirement.Quantity)
                    {
                        Add(result, ConstraintSeverity.Soft,
                            $"Phase {gene.PhaseId} has quantity-based equipment shortage for type {requirement.EquipmentTypeId}.");
                    }

                    var evaluated = new List<(double Type, double Availability, double Condition, double Substitution)>();
                    foreach (var assignment in assignments)
                    {
                        if (assignment.EquipmentInstanceId is null ||
                            !instances.TryGetValue(assignment.EquipmentInstanceId.Value, out var instance))
                        {
                            Add(result, ConstraintSeverity.Hard, $"Phase {gene.PhaseId} has a missing equipment instance.");
                            evaluated.Add((0d, 0d, 0d, 0d));
                            continue;
                        }

                        var primary = instance.EquipmentTypeId == requirement.EquipmentTypeId &&
                                      assignment.AllocatedEquipmentTypeId == requirement.EquipmentTypeId;
                        var efficiency = FitnessEvaluationHelper.ClampScore(assignment.EfficiencyRate * 100d);
                        var validSubstitution = assignment.IsSubstitute &&
                                                requirement.AllowSubstitute &&
                                                (!requirement.MinAcceptableEfficiency.HasValue ||
                                                 assignment.EfficiencyRate >= requirement.MinAcceptableEfficiency.Value);
                        var typeScore = primary ? 100d : validSubstitution ? efficiency : 0d;
                        var substitutionScore = primary ? 100d : validSubstitution ? efficiency : 0d;

                        if (!primary && !validSubstitution)
                        {
                            Add(result, ConstraintSeverity.Hard,
                                $"Equipment {instance.AssetCode} is an invalid substitution or does not match required type.");
                            phase.Adjustments.Add(Adjustment("Equipment Substitution", 0d, "Substitution",
                                $"RequiredEquipmentTypeId={requirement.EquipmentTypeId}, AllocatedEquipmentTypeId={assignment.AllocatedEquipmentTypeId}, " +
                                $"EquipmentInstanceId={assignment.EquipmentInstanceId}, AssetCode={instance.AssetCode}, EfficiencyRate={assignment.EfficiencyRate:F2}, " +
                                $"TimeMultiplier={assignment.TimeMultiplier:F2}, IsSubstitute={assignment.IsSubstitute}.", "Invalid substitution → 0"));
                        }
                        else if (validSubstitution)
                        {
                            phase.Adjustments.Add(Adjustment("Equipment Substitution", efficiency, "Substitution",
                                $"RequiredEquipmentTypeId={requirement.EquipmentTypeId}, AllocatedEquipmentTypeId={assignment.AllocatedEquipmentTypeId}, " +
                                $"EquipmentInstanceId={assignment.EquipmentInstanceId}, AssetCode={instance.AssetCode}, EfficiencyRate={assignment.EfficiencyRate:F2}, " +
                                $"TimeMultiplier={assignment.TimeMultiplier:F2}, IsSubstitute=true.", $"{assignment.EfficiencyRate:F2} × 100 = {efficiency:F2}"));
                        }

                        var availability = FitnessEvaluationHelper.IsAvailableStatus(instance.Status) ? 100d : 0d;
                        if (availability == 0d)
                        {
                            var severity = FitnessEvaluationHelper.IsMaintenanceStatus(instance.Status)
                                ? ConstraintSeverity.Hard : ConstraintSeverity.Soft;
                            Add(result, severity, $"Equipment {instance.AssetCode} status is {instance.Status}.");
                        }
                        var condition = ConditionScore(instance.ConditionLevel);
                        if (input.ExistingEquipmentAllocations.Any(a =>
                                a.EquipmentInstanceId == instance.EquipmentInstanceId &&
                                FitnessEvaluationHelper.Overlaps(gene.StartDate, gene.EndDate, a.StartDate, a.EndDate)))
                        {
                            Add(result, ConstraintSeverity.Hard, $"Equipment {instance.AssetCode} overlaps with an existing allocation.");
                        }

                        evaluated.Add((typeScore, availability, condition, substitutionScore));
                    }

                    var averages = evaluated.Count == 0
                        ? (0d, 0d, 0d, 0d)
                        : (evaluated.Average(e => e.Type), evaluated.Average(e => e.Availability),
                           evaluated.Average(e => e.Condition), evaluated.Average(e => e.Substitution));
                    phaseParts.Add((quantity, averages.Item1, averages.Item2, averages.Item3, averages.Item4));
                }

                var score = phaseParts.Count == 0
                    ? 100d
                    : phaseParts.Average(p =>
                        p.Quantity * 0.25d + p.Type * 0.30d + p.Availability * 0.15d +
                        p.Condition * 0.10d + p.Substitution * 0.20d);
                foreach (var part in phaseParts)
                {
                    phase.SubScores.Add(SubScore("Equipment Quantity", part.Quantity, 0.25d));
                    phase.SubScores.Add(SubScore("Equipment Type Match", part.Type, 0.30d));
                    phase.SubScores.Add(SubScore("Equipment Availability", part.Availability, 0.15d));
                    phase.SubScores.Add(SubScore("Equipment Condition", part.Condition, 0.10d));
                    phase.SubScores.Add(SubScore("Equipment Substitution", part.Substitution, 0.20d));
                }
                phase.FinalScore = score;
                phase.Calculation = phaseParts.Count == 0
                    ? "No equipment requirements = 100.00"
                    : $"Average weighted equipment requirements = {score:F2}";
                phaseScores.Add(score);
                phases.Add(phase);
            }

            var internalOverlaps = FitnessEvaluationHelper.CountInternalOverlaps(
                chromosome.Genes.SelectMany(g => g.EquipmentAssignments
                    .Select(e => (e.EquipmentInstanceId, g.StartDate, g.EndDate))));
            if (internalOverlaps > 0)
            {
                Add(result, ConstraintSeverity.Hard, "Equipment is double-booked inside the candidate plan.");
            }

            result.Score = phaseScores.Count == 0 ? 0d : FitnessEvaluationHelper.ClampScore(phaseScores.Average());
            result.Explanation = new ScoreExplanation
            {
                FinalScore = result.Score,
                Calculation = phaseScores.Count == 0
                    ? "No equipment phases = 0.00"
                    : $"Average({string.Join(", ", phaseScores.Select(s => s.ToString("F2")))}) = {result.Score:F2}",
                Adjustments = phases.SelectMany(p => p.Adjustments).ToList(),
                Phases = phases
            };
            return result;
        }

        private static double ConditionScore(string? conditionLevel) =>
            conditionLevel?.Trim().ToLowerInvariant() switch
            {
                "good" => 100d,
                "fair" => 75d,
                "poor" => 40d,
                "critical" => 0d,
                _ => 75d
            };

        private static ScoreAdjustment SubScore(string factor, double score, double weight) =>
            new() { Factor = factor, Points = score * weight, Type = "SubScore", Reason = "Normalized equipment component score.", Calculation = $"{score:F2} × {weight:P0} = {score * weight:F2}" };

        private static ScoreAdjustment Adjustment(string factor, double points, string type, string reason, string calculation) =>
            new() { Factor = factor, Points = points, Type = type, Reason = reason, Calculation = calculation };

        private static void Add(ConstraintEvaluationResult result, ConstraintSeverity severity, string message)
        {
            if (result.Violations.Any(v => v.Severity == severity && v.Message == message))
            {
                return;
            }
            result.Violations.Add(new ConstraintViolation("Equipment", severity, message));
            result.Disadvantages.Add(message);
        }
    }
}

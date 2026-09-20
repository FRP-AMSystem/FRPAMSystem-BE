using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness.Evaluators
{
    public class HumanConstraintEvaluator : IHumanConstraintEvaluator
    {
        public string Category => "Human";

        public ConstraintEvaluationResult Evaluate(AllocationChromosome chromosome, OptimizationInput input)
        {
            var result = new ConstraintEvaluationResult();
            var humans = input.HumanResources.ToDictionary(h => h.HumanResourceId);
            var phaseScores = new List<double>();
            var phases = new List<PhaseScoreExplanation>();

            foreach (var gene in chromosome.Genes)
            {
                var requirements = FitnessEvaluationHelper.GetHumanRequirements(gene.PhaseId, input).ToList();
                var assigned = gene.AssignedHumanResourceIds
                    .Distinct()
                    .Where(id => humans.ContainsKey(id))
                    .Select(id => humans[id])
                    .ToList();
                var phase = new PhaseScoreExplanation { PhaseId = gene.PhaseId };

                foreach (var id in gene.AssignedHumanResourceIds.Where(id => !humans.ContainsKey(id)))
                {
                    Add(result, "Human", ConstraintSeverity.Hard, $"Phase {gene.PhaseId} references an unknown human resource.");
                }

                var requiredQuantity = requirements.Sum(r => r.Quantity);
                var quantity = FitnessEvaluationHelper.Percent(assigned.Count, requiredQuantity) * 100d;
                var roleMatches = requirements.Sum(r => Math.Min(r.Quantity,
                    assigned.Count(h => FitnessEvaluationHelper.HasRole(h, r.RoleId))));
                var skillMatches = requirements.Sum(r => Math.Min(r.Quantity,
                    assigned.Count(h => FitnessEvaluationHelper.HasRole(h, r.RoleId) &&
                                       FitnessEvaluationHelper.HasSkill(h, r.RequiredSkillId))));
                var role = FitnessEvaluationHelper.Percent(roleMatches, requiredQuantity) * 100d;
                var skill = FitnessEvaluationHelper.Percent(skillMatches, requiredQuantity) * 100d;
                var availability = assigned.Count == 0 && requiredQuantity > 0
                    ? 0d
                    : assigned.Count == 0 ? 100d
                    : assigned.All(h => FitnessEvaluationHelper.IsAvailableStatus(h.Status)) ? 100d : 0d;
                var workload = assigned.Count == 0
                    ? (requiredQuantity == 0 ? 100d : 0d)
                    : assigned.Average(h => h.MaxWorkingHoursPerDay <= 0d
                        ? 0d
                        : FitnessEvaluationHelper.ClampScore(
                            (h.MaxWorkingHoursPerDay - h.CurrentWorkload) / h.MaxWorkingHoursPerDay * 100d));

                if (requiredQuantity > 0 && assigned.Count < requiredQuantity)
                {
                    Add(result, "Human", ConstraintSeverity.Soft, $"Phase {gene.PhaseId} has insufficient human resource quantity.");
                    phase.Adjustments.Add(Adjustment("Staff Quantity Shortage", quantity - 100d, "Partial",
                        $"Staff count is {assigned.Count}/{requiredQuantity}.", $"{quantity:F2}"));
                }
                foreach (var requirement in requirements)
                {
                    var matchingRoles = assigned.Count(h => FitnessEvaluationHelper.HasRole(h, requirement.RoleId));
                    var matchingSkills = assigned.Count(h => FitnessEvaluationHelper.HasRole(h, requirement.RoleId) &&
                                                             FitnessEvaluationHelper.HasSkill(h, requirement.RequiredSkillId));
                    if (matchingRoles < requirement.Quantity)
                    {
                        Add(result, "Role", ConstraintSeverity.Hard,
                            $"Phase {gene.PhaseId} is missing required role {requirement.RoleId}.");
                        phase.Adjustments.Add(Adjustment("Role Mismatch", -100d, "Adjustment",
                            $"Role {requirement.RoleId}: {matchingRoles}/{requirement.Quantity} matched.", "100 → 0"));
                    }
                    if (matchingSkills < requirement.Quantity)
                    {
                        Add(result, "Skill", ConstraintSeverity.Hard,
                            $"Phase {gene.PhaseId} is missing required skill {requirement.RequiredSkillId}.");
                        phase.Adjustments.Add(Adjustment("Skill Mismatch", -100d, "Adjustment",
                            $"Skill {requirement.RequiredSkillId}: {matchingSkills}/{requirement.Quantity} matched.", "100 → 0"));
                    }

                    var assignedHours = FitnessEvaluationHelper.EstimateAssignedHoursPerDay(gene, requirement);
                    foreach (var human in assigned)
                    {
                        if (human.CurrentWorkload + assignedHours > human.MaxWorkingHoursPerDay)
                        {
                            Add(result, "Human", ConstraintSeverity.Hard,
                                $"Human resource {human.HumanResourceId} exceeds max working hours per day.");
                        }
                    }
                }
                foreach (var human in assigned)
                {
                    if (!FitnessEvaluationHelper.IsAvailableStatus(human.Status))
                    {
                        Add(result, "Human", ConstraintSeverity.Hard, $"Human resource {human.HumanResourceId} is unavailable.");
                    }
                    if (input.ExistingHumanAllocations.Any(a =>
                            a.HumanResourceId == human.HumanResourceId &&
                            FitnessEvaluationHelper.Overlaps(gene.StartDate, gene.EndDate, a.StartDate, a.EndDate)))
                    {
                        Add(result, "Human", ConstraintSeverity.Hard, $"Human resource {human.HumanResourceId} is double-booked.");
                    }
                }

                var internalOverlaps = FitnessEvaluationHelper.CountInternalOverlaps(
                    chromosome.Genes.SelectMany(g => g.AssignedHumanResourceIds
                        .Select(id => ((int?)id, g.StartDate, g.EndDate))));
                if (internalOverlaps > 0)
                {
                    Add(result, "Human", ConstraintSeverity.Hard, "A human resource is double-booked inside the candidate plan.");
                }

                var score = quantity * 0.25d + role * 0.20d + skill * 0.25d +
                            availability * 0.15d + workload * 0.15d;
                phase.SubScores.AddRange(new[]
                {
                    SubScore("Quantity Score", quantity, 0.25d),
                    SubScore("Role Score", role, 0.20d),
                    SubScore("Skill Score", skill, 0.25d),
                    SubScore("Human Availability", availability, 0.15d),
                    SubScore("Workload Capacity", workload, 0.15d)
                });
                phase.FinalScore = score;
                phase.Calculation = $"{quantity:F2}×25% + {role:F2}×20% + {skill:F2}×25% + {availability:F2}×15% + {workload:F2}×15% = {score:F2}";
                phaseScores.Add(score);
                phases.Add(phase);
            }

            ApplyWorkloadBalance(chromosome, result, phases);
            result.Score = phaseScores.Count == 0 ? 0d : phaseScores.Average();
            result.Explanation = new ScoreExplanation
            {
                FinalScore = result.Score,
                Calculation = phaseScores.Count == 0
                    ? "No human phases = 0.00"
                    : $"Average({string.Join(", ", phaseScores.Select(s => s.ToString("F2")))}) = {result.Score:F2}",
                Adjustments = phases.SelectMany(p => p.Adjustments).ToList(),
                Bonuses = result.BonusAdjustments.Select(b => b.Clone()).ToList(),
                Phases = phases
            };
            return result;
        }

        private static ScoreAdjustment SubScore(string factor, double score, double weight) =>
            new() { Factor = factor, Points = score * weight, Type = "SubScore", Reason = "Normalized component score.", Calculation = $"{score:F2} × {weight:P0} = {score * weight:F2}" };

        private static ScoreAdjustment Adjustment(string factor, double points, string type, string reason, string calculation) =>
            new() { Factor = factor, Points = points, Type = type, Reason = reason, Calculation = calculation };

        private static void ApplyWorkloadBalance(
            AllocationChromosome chromosome,
            ConstraintEvaluationResult result,
            IReadOnlyList<PhaseScoreExplanation> phases)
        {
            var counts = chromosome.Genes.SelectMany(g => g.AssignedHumanResourceIds)
                .GroupBy(id => id).Select(g => g.Count()).ToList();
            if (counts.Count > 1 && counts.Max() - counts.Min() <= 1)
            {
                result.Bonus += 1d;
                var adjustment = Adjustment("Human Workload Balance", 1d, "Bonus",
                    "Human workload is balanced across assigned staff.", "spread ≤ 1 → +1");
                result.BonusAdjustments.Add(adjustment);
                if (phases.Count > 0)
                {
                    phases[0].Bonuses.Add(adjustment.Clone());
                }
                result.Advantages.Add("Human workload is balanced across assigned staff.");
            }
            else if (counts.Count > 1)
            {
                Add(result, "Human", ConstraintSeverity.Soft, "Human workload is imbalanced across assigned staff.");
            }
        }

        private static void Add(ConstraintEvaluationResult result, string category, ConstraintSeverity severity, string message)
        {
            if (result.Violations.Any(v => v.Category == category && v.Severity == severity && v.Message == message))
            {
                return;
            }
            result.Violations.Add(new ConstraintViolation(category, severity, message));
            result.Disadvantages.Add(message);
        }
    }
}

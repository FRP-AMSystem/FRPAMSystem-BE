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
            var scoreParts = new List<double>();

            foreach (var gene in chromosome.Genes)
            {
                var requirements = FitnessEvaluationHelper.GetHumanRequirements(gene.PhaseId, input).ToList();
                var assigned = gene.AssignedHumanResourceIds
                    .Distinct()
                    .Where(humans.ContainsKey)
                    .Select(id => humans[id])
                    .ToList();

                if (gene.AssignedHumanResourceIds.Any(id => !humans.ContainsKey(id)))
                {
                    Add(result, "Human", ConstraintSeverity.Hard, $"Phase {gene.PhaseId} references an unknown human resource.");
                }

                var requiredQuantity = requirements.Sum(r => r.Quantity);
                var geneScore = requiredQuantity == 0 ? 75d : 10d;

                if (assigned.Count >= requiredQuantity)
                {
                    geneScore += 20d;
                }
                else
                {
                    Add(result, "Human", ConstraintSeverity.Soft, $"Phase {gene.PhaseId} has insufficient human resource quantity.");
                    geneScore += 20d * FitnessEvaluationHelper.Percent(assigned.Count, requiredQuantity);
                }

                foreach (var human in assigned)
                {
                    if (!FitnessEvaluationHelper.IsAvailableStatus(human.Status))
                    {
                        Add(result, "Human", ConstraintSeverity.Hard, $"Human resource {human.HumanResourceId} is unavailable.");
                        geneScore -= 30d;
                    }

                    if (input.ExistingHumanAllocations.Any(a =>
                            a.HumanResourceId == human.HumanResourceId &&
                            FitnessEvaluationHelper.Overlaps(gene.StartDate, gene.EndDate, a.StartDate, a.EndDate)) ||
                        input.ExistingSchedules.Any(s =>
                            s.AssignedHumanResourceId == human.HumanResourceId &&
                            FitnessEvaluationHelper.Overlaps(gene.StartDate, gene.EndDate, s.StartDate, s.EndDate)))
                    {
                        Add(result, "Human", ConstraintSeverity.Hard, $"Human resource {human.HumanResourceId} is double-booked.");
                        geneScore -= 35d;
                    }
                }

                foreach (var requirement in requirements)
                {
                    var roleMatches = assigned.Count(h => FitnessEvaluationHelper.HasRole(h, requirement.RoleId));
                    if (roleMatches >= requirement.Quantity)
                    {
                        geneScore += 15d;
                    }
                    else
                    {
                        Add(result, "Role", ConstraintSeverity.Hard, $"Phase {gene.PhaseId} is missing required role {requirement.RoleId}.");
                        geneScore += 15d * FitnessEvaluationHelper.Percent(roleMatches, requirement.Quantity);
                        geneScore -= 25d;
                    }

                    var skillMatches = assigned.Count(h =>
                        FitnessEvaluationHelper.HasRole(h, requirement.RoleId) &&
                        FitnessEvaluationHelper.HasSkill(h, requirement.RequiredSkillId));

                    if (skillMatches >= requirement.Quantity)
                    {
                        geneScore += 20d;
                    }
                    else
                    {
                        Add(result, "Skill", ConstraintSeverity.Hard, $"Phase {gene.PhaseId} is missing required skill {requirement.RequiredSkillId}.");
                        geneScore += 20d * FitnessEvaluationHelper.Percent(skillMatches, requirement.Quantity);
                        geneScore -= 25d;
                    }

                    var assignedHours = FitnessEvaluationHelper.EstimateAssignedHoursPerDay(gene, requirement);
                    foreach (var human in assigned)
                    {
                        if (human.CurrentWorkload + assignedHours > human.MaxWorkingHoursPerDay)
                        {
                            Add(result, "Human", ConstraintSeverity.Hard, $"Human resource {human.HumanResourceId} exceeds max working hours per day.");
                            geneScore -= 35d;
                        }
                    }
                }

                var workloadComponent = assigned.Count == 0
                    ? 0d
                    : assigned.Average(h => Math.Max(0d, 15d - h.CurrentWorkload));
                geneScore += workloadComponent;
                scoreParts.Add(FitnessEvaluationHelper.ClampScore(geneScore));
            }

            var internalOverlaps = FitnessEvaluationHelper.CountInternalOverlaps(
                chromosome.Genes.SelectMany(g => g.AssignedHumanResourceIds
                    .Select(id => ((int?)id, g.StartDate, g.EndDate))));

            for (var i = 0; i < internalOverlaps; i++)
            {
                Add(result, "Human", ConstraintSeverity.Hard, "A human resource is double-booked inside the candidate plan.");
            }

            ApplyWorkloadBalance(chromosome, result);
            result.Score = scoreParts.Count == 0 ? 0d : scoreParts.Average();
            return result;
        }

        private static void ApplyWorkloadBalance(AllocationChromosome chromosome, ConstraintEvaluationResult result)
        {
            var counts = chromosome.Genes
                .SelectMany(g => g.AssignedHumanResourceIds)
                .GroupBy(id => id)
                .Select(g => g.Count())
                .ToList();

            if (counts.Count <= 1)
            {
                return;
            }

            var spread = counts.Max() - counts.Min();
            if (spread <= 1)
            {
                result.Bonus += 5d;
                result.Advantages.Add("Human workload is balanced across assigned staff.");
            }
            else
            {
                Add(result, "Human", ConstraintSeverity.Soft, "Human workload is imbalanced across assigned staff.");
            }
        }

        private static void Add(ConstraintEvaluationResult result, string category, ConstraintSeverity severity, string message)
        {
            result.Violations.Add(new ConstraintViolation(category, severity, message));
            result.Disadvantages.Add(message);
        }
    }
}

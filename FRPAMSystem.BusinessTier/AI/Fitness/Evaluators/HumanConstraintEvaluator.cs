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

            var adjustments = new List<ScoreAdjustment>();
            var baseScores = new List<double>();

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
                baseScores.Add(geneScore);

                if (assigned.Count >= requiredQuantity)
                {
                    geneScore += 20d;
                    adjustments.Add(new ScoreAdjustment
                    {
                        Factor = "Staff Quantity",
                        Points = 20d,
                        Type = "Addition",
                        Reason = $"Phase {gene.PhaseId} staff count ({assigned.Count}/{requiredQuantity}) meets requirement.",
                        Calculation = "+20"
                    });
                }
                else
                {
                    Add(result, "Human", ConstraintSeverity.Soft, $"Phase {gene.PhaseId} has insufficient human resource quantity.");
                    var qtyPts = 20d * FitnessEvaluationHelper.Percent(assigned.Count, requiredQuantity);
                    geneScore += qtyPts;
                    adjustments.Add(new ScoreAdjustment
                    {
                        Factor = "Staff Quantity Shortage",
                        Points = Math.Round(qtyPts, 2),
                        Type = "Deduction",
                        Reason = $"Phase {gene.PhaseId} has insufficient staff ({assigned.Count}/{requiredQuantity}).",
                        Calculation = $"20 × ({assigned.Count}/{requiredQuantity}) = {qtyPts:F2}"
                    });
                }

                foreach (var human in assigned)
                {
                    if (!FitnessEvaluationHelper.IsAvailableStatus(human.Status))
                    {
                        Add(result, "Human", ConstraintSeverity.Hard, $"Human resource {human.HumanResourceId} is unavailable.");
                        geneScore -= 30d;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Staff Availability",
                            Points = -30d,
                            Type = "Deduction",
                            Reason = $"Staff {human.User?.FullName ?? human.HumanResourceId.ToString()} is unavailable (status: {human.Status}).",
                            Calculation = "-30"
                        });
                    }

                    if (input.ExistingHumanAllocations.Any(a =>
                            a.HumanResourceId == human.HumanResourceId &&
                            FitnessEvaluationHelper.Overlaps(gene.StartDate, gene.EndDate, a.StartDate, a.EndDate)))
                    {
                        Add(result, "Human", ConstraintSeverity.Hard, $"Human resource {human.HumanResourceId} is double-booked.");
                        geneScore -= 35d;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Staff External Overlap",
                            Points = -35d,
                            Type = "Deduction",
                            Reason = $"Staff {human.User?.FullName ?? human.HumanResourceId.ToString()} overlaps with an existing allocation.",
                            Calculation = "-35"
                        });
                    }
                }

                foreach (var requirement in requirements)
                {
                    var roleMatches = assigned.Count(h => FitnessEvaluationHelper.HasRole(h, requirement.RoleId));
                    if (roleMatches >= requirement.Quantity)
                    {
                        geneScore += 15d;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Role Match",
                            Points = 15d,
                            Type = "Addition",
                            Reason = $"Phase {gene.PhaseId} role requirement (Role ID: {requirement.RoleId}) satisfied ({roleMatches}/{requirement.Quantity}).",
                            Calculation = "+15"
                        });
                    }
                    else
                    {
                        Add(result, "Role", ConstraintSeverity.Hard, $"Phase {gene.PhaseId} is missing required role {requirement.RoleId}.");
                        var rolePercent = FitnessEvaluationHelper.Percent(roleMatches, requirement.Quantity);
                        var rolePts = 15d * rolePercent - 25d;
                        geneScore += rolePts;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Role Mismatch",
                            Points = Math.Round(rolePts, 2),
                            Type = "Deduction",
                            Reason = $"Phase {gene.PhaseId} missing required role {requirement.RoleId} ({roleMatches}/{requirement.Quantity} matched).",
                            Calculation = $"15 × ({roleMatches}/{requirement.Quantity}) - 25 = {rolePts:F2}"
                        });
                    }

                    var skillMatches = assigned.Count(h =>
                        FitnessEvaluationHelper.HasRole(h, requirement.RoleId) &&
                        FitnessEvaluationHelper.HasSkill(h, requirement.RequiredSkillId));

                    if (skillMatches >= requirement.Quantity)
                    {
                        geneScore += 20d;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Skill Match",
                            Points = 20d,
                            Type = "Addition",
                            Reason = $"Phase {gene.PhaseId} skill requirement (Skill ID: {requirement.RequiredSkillId}) satisfied ({skillMatches}/{requirement.Quantity}).",
                            Calculation = "+20"
                        });
                    }
                    else
                    {
                        Add(result, "Skill", ConstraintSeverity.Hard, $"Phase {gene.PhaseId} is missing required skill {requirement.RequiredSkillId}.");
                        var skillPercent = FitnessEvaluationHelper.Percent(skillMatches, requirement.Quantity);
                        var skillPts = 20d * skillPercent - 25d;
                        geneScore += skillPts;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Skill Mismatch",
                            Points = Math.Round(skillPts, 2),
                            Type = "Deduction",
                            Reason = $"Phase {gene.PhaseId} missing required skill {requirement.RequiredSkillId} ({skillMatches}/{requirement.Quantity} matched).",
                            Calculation = $"20 × ({skillMatches}/{requirement.Quantity}) - 25 = {skillPts:F2}"
                        });
                    }

                    var assignedHours = FitnessEvaluationHelper.EstimateAssignedHoursPerDay(gene, requirement);
                    foreach (var human in assigned)
                    {
                        if (human.CurrentWorkload + assignedHours > human.MaxWorkingHoursPerDay)
                        {
                            Add(result, "Human", ConstraintSeverity.Hard, $"Human resource {human.HumanResourceId} exceeds max working hours per day.");
                            geneScore -= 35d;
                            adjustments.Add(new ScoreAdjustment
                            {
                                Factor = "Workload Overload",
                                Points = -35d,
                                Type = "Deduction",
                                Reason = $"Staff {human.User?.FullName ?? human.HumanResourceId.ToString()} exceeds max daily hours ({human.CurrentWorkload + assignedHours:F1}h > {human.MaxWorkingHoursPerDay}h).",
                                Calculation = "-35"
                            });
                        }
                    }
                }

                var workloadComponent = assigned.Count == 0
                    ? 0d
                    : assigned.Average(h => Math.Max(0d, 15d - h.CurrentWorkload));
                geneScore += workloadComponent;
                if (workloadComponent > 0d)
                {
                    adjustments.Add(new ScoreAdjustment
                    {
                        Factor = "Workload Capacity",
                        Points = Math.Round(workloadComponent, 2),
                        Type = "Addition",
                        Reason = $"Assigned staff have available capacity (average capacity score: {workloadComponent:F1}).",
                        Calculation = $"avg(15 - workload) = +{workloadComponent:F2}"
                    });
                }

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
                result.BonusAdjustments.Add(new ScoreAdjustment
                {
                    Factor = "Human Workload Balance",
                    Points = 5d,
                    Type = "Bonus",
                    Reason = "Human workload is balanced across assigned staff.",
                    Calculation = $"spread = {spread} <= 1 → +5"
                });
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

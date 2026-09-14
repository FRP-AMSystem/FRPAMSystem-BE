using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness.Evaluators
{
    public class ScheduleConstraintEvaluator : IScheduleConstraintEvaluator
    {
        public string Category => "Schedule";

        public ConstraintEvaluationResult Evaluate(AllocationChromosome chromosome, OptimizationInput input)
        {
            var result = new ConstraintEvaluationResult();
            var phases = input.ExperimentPhases.ToDictionary(p => p.PhaseId);
            var orderedGenes = chromosome.Genes
                .OrderBy(g => phases.TryGetValue(g.PhaseId, out var phase) ? phase.PhaseOrder : int.MaxValue)
                .ToList();
            var scoreParts = new List<double>();

            var adjustments = new List<ScoreAdjustment>();

            foreach (var gene in orderedGenes)
            {
                if (!phases.TryGetValue(gene.PhaseId, out var phase))
                {
                    Add(result, "Schedule", ConstraintSeverity.Hard, $"Unknown phase {gene.PhaseId}.");
                    adjustments.Add(new ScoreAdjustment
                    {
                        Factor = "Unknown Phase",
                        Points = -20d,
                        Type = "Deduction",
                        Reason = $"Unknown phase {gene.PhaseId}.",
                        Calculation = "-20"
                    });
                    scoreParts.Add(0d);
                    continue;
                }

                var geneScore = 20d;
                if (gene.StartDate <= gene.EndDate)
                {
                    geneScore += 20d;
                    adjustments.Add(new ScoreAdjustment
                    {
                        Factor = "Date Range Validity",
                        Points = 20d,
                        Type = "Addition",
                        Reason = $"Phase {gene.PhaseId} date range is valid ({gene.StartDate:yyyy-MM-dd} to {gene.EndDate:yyyy-MM-dd}).",
                        Calculation = "+20"
                    });
                }
                else
                {
                    Add(result, "Schedule", ConstraintSeverity.Hard, $"Phase {gene.PhaseId} has invalid date range.");
                    geneScore -= 50d;
                    adjustments.Add(new ScoreAdjustment
                    {
                        Factor = "Invalid Date Range",
                        Points = -50d,
                        Type = "Deduction",
                        Reason = $"Phase {gene.PhaseId} start date ({gene.StartDate:yyyy-MM-dd}) is after end date ({gene.EndDate:yyyy-MM-dd}).",
                        Calculation = "-50"
                    });
                }

                var startDelta = Math.Abs((gene.StartDate.Date - phase.ExpectedStartDate.Date).TotalDays);
                var endDelta = Math.Abs((gene.EndDate.Date - phase.ExpectedEndDate.Date).TotalDays);
                var startPts = Math.Max(0d, 25d - startDelta);
                var endPts = Math.Max(0d, 25d - endDelta);
                geneScore += startPts;
                geneScore += endPts;

                adjustments.Add(new ScoreAdjustment
                {
                    Factor = "Start Date Alignment",
                    Points = Math.Round(startPts, 2),
                    Type = startPts >= 25d ? "Addition" : "Deduction",
                    Reason = $"Phase {gene.PhaseId} start date is {startDelta:F0} day(s) from expected ({phase.ExpectedStartDate:yyyy-MM-dd}).",
                    Calculation = $"max(0, 25 - {startDelta:F0}) = +{startPts:F2}"
                });

                adjustments.Add(new ScoreAdjustment
                {
                    Factor = "End Date Alignment",
                    Points = Math.Round(endPts, 2),
                    Type = endPts >= 25d ? "Addition" : "Deduction",
                    Reason = $"Phase {gene.PhaseId} end date is {endDelta:F0} day(s) from expected ({phase.ExpectedEndDate:yyyy-MM-dd}).",
                    Calculation = $"max(0, 25 - {endDelta:F0}) = +{endPts:F2}"
                });

                if (input.Experiment.Deadline.HasValue && gene.EndDate > input.Experiment.Deadline.Value)
                {
                    Add(result, "Deadline", ConstraintSeverity.Soft, $"Phase {gene.PhaseId} ends after the experiment deadline.");
                    geneScore -= 25d;
                    adjustments.Add(new ScoreAdjustment
                    {
                        Factor = "Deadline Exceeded",
                        Points = -25d,
                        Type = "Deduction",
                        Reason = $"Phase {gene.PhaseId} end date ({gene.EndDate:yyyy-MM-dd}) exceeds experiment deadline ({input.Experiment.Deadline.Value:yyyy-MM-dd}).",
                        Calculation = "-25"
                    });
                }

                scoreParts.Add(FitnessEvaluationHelper.ClampScore(geneScore));
            }

            for (var i = 1; i < orderedGenes.Count; i++)
            {
                var previous = orderedGenes[i - 1];
                var current = orderedGenes[i];
                var previousPhase = phases.GetValueOrDefault(previous.PhaseId);
                var currentPhase = phases.GetValueOrDefault(current.PhaseId);

                if (previousPhase is not null &&
                    currentPhase is not null &&
                    previousPhase.PhaseOrder > currentPhase.PhaseOrder)
                {
                    Add(result, "Schedule", ConstraintSeverity.Hard, $"Phase order violation between {previous.PhaseId} and {current.PhaseId}.");
                }

                if (current.StartDate < previous.EndDate)
                {
                    Add(result, "Schedule", ConstraintSeverity.Hard, $"Phase {current.PhaseId} starts before phase {previous.PhaseId} completes.");
                }

                var idleDays = Math.Max(0d, (current.StartDate.Date - previous.EndDate.Date).TotalDays - 1d);
                if (idleDays <= 1d)
                {
                    result.Bonus += 2d;
                    result.BonusAdjustments.Add(new ScoreAdjustment
                    {
                        Factor = "Optimal Phase Transition",
                        Points = 2d,
                        Type = "Bonus",
                        Reason = $"Smooth transition between phase {previous.PhaseId} and phase {current.PhaseId} (idle gap: {idleDays:F0} day(s)).",
                        Calculation = $"idleDays = {idleDays:F0} <= 1 → +2"
                    });
                }
                else
                {
                    Add(result, "Schedule", ConstraintSeverity.Soft, $"There is an unnecessary idle gap before phase {current.PhaseId}.");
                }
            }

            result.Score = scoreParts.Count == 0 ? 0d : scoreParts.Average();
            var baseScore = chromosome.Genes.Count == 0 ? 0d : 20d;
            string calcStr;
            if (scoreParts.Count <= 1)
            {
                var calcParts = $"{baseScore:F0} (Base) " + string.Join(" ", adjustments.Select(a => $"{(a.Points >= 0 ? "+" : "")}{a.Points:F2} ({a.Factor})"));
                calcStr = $"{calcParts} = {result.Score:F2}";
            }
            else
            {
                calcStr = "Phases: " + string.Join(" + ", scoreParts.Select((s, i) => $"Phase {orderedGenes[i].PhaseId} ({s:F2})")) + $" / {scoreParts.Count} = {result.Score:F2}";
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

            if (result.Violations.Count == 0)
            {
                result.Advantages.Add("Schedule follows phase order without resource overlap.");
            }

            return result;
        }

        private static void Add(ConstraintEvaluationResult result, string category, ConstraintSeverity severity, string message)
        {
            result.Violations.Add(new ConstraintViolation(category, severity, message));
            result.Disadvantages.Add(message);
        }
    }
}

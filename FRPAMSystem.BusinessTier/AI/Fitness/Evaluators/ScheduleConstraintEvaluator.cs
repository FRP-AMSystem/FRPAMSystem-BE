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

            foreach (var gene in orderedGenes)
            {
                if (!phases.TryGetValue(gene.PhaseId, out var phase))
                {
                    Add(result, "Schedule", ConstraintSeverity.Hard, $"Unknown phase {gene.PhaseId}.");
                    scoreParts.Add(0d);
                    continue;
                }

                var geneScore = 20d;
                if (gene.StartDate <= gene.EndDate)
                {
                    geneScore += 20d;
                }
                else
                {
                    Add(result, "Schedule", ConstraintSeverity.Hard, $"Phase {gene.PhaseId} has invalid date range.");
                    geneScore -= 50d;
                }

                var startDelta = Math.Abs((gene.StartDate.Date - phase.ExpectedStartDate.Date).TotalDays);
                var endDelta = Math.Abs((gene.EndDate.Date - phase.ExpectedEndDate.Date).TotalDays);
                geneScore += Math.Max(0d, 25d - startDelta);
                geneScore += Math.Max(0d, 25d - endDelta);

                if (input.Experiment.Deadline.HasValue && gene.EndDate > input.Experiment.Deadline.Value)
                {
                    Add(result, "Deadline", ConstraintSeverity.Soft, $"Phase {gene.PhaseId} ends after the experiment deadline.");
                    geneScore -= 25d;
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
                }
                else
                {
                    Add(result, "Schedule", ConstraintSeverity.Soft, $"There is an unnecessary idle gap before phase {current.PhaseId}.");
                }
            }

            AddResourceOverlapViolations(chromosome, result);
            result.Score = scoreParts.Count == 0 ? 0d : scoreParts.Average();

            if (result.Violations.Count == 0)
            {
                result.Advantages.Add("Schedule follows phase order without resource overlap.");
            }

            return result;
        }

        private static void AddResourceOverlapViolations(AllocationChromosome chromosome, ConstraintEvaluationResult result)
        {
            var landOverlaps = FitnessEvaluationHelper.CountInternalOverlaps(
                chromosome.Genes.Select(g => (g.LandId, g.StartDate, g.EndDate)));
            var humanOverlaps = FitnessEvaluationHelper.CountInternalOverlaps(
                chromosome.Genes.SelectMany(g => g.AssignedHumanResourceIds.Select(id => ((int?)id, g.StartDate, g.EndDate))));
            var equipmentOverlaps = FitnessEvaluationHelper.CountInternalOverlaps(
                chromosome.Genes.SelectMany(g => g.EquipmentAssignments.Select(e => (e.EquipmentInstanceId, g.StartDate, g.EndDate))));

            for (var i = 0; i < landOverlaps + humanOverlaps + equipmentOverlaps; i++)
            {
                Add(result, "Schedule", ConstraintSeverity.Hard, "Candidate schedule contains a resource overlap.");
            }
        }

        private static void Add(ConstraintEvaluationResult result, string category, ConstraintSeverity severity, string message)
        {
            result.Violations.Add(new ConstraintViolation(category, severity, message));
            result.Disadvantages.Add(message);
        }
    }
}

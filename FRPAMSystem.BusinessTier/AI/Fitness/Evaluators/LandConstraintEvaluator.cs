using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness.Evaluators
{
    public class LandConstraintEvaluator : ILandConstraintEvaluator
    {
        public string Category => "Land";

        public ConstraintEvaluationResult Evaluate(AllocationChromosome chromosome, OptimizationInput input)
        {
            var result = new ConstraintEvaluationResult();
            var lands = input.LandResources.ToDictionary(l => l.LandId);
            var requirements = input.ExperimentLandRequirements.ToList();
            var scoreParts = new List<double>();

            foreach (var gene in chromosome.Genes)
            {
                var requirement = gene.ExperimentLandRequirementId.HasValue
                    ? requirements.FirstOrDefault(r => r.ExpLandReqId == gene.ExperimentLandRequirementId.Value)
                    : requirements.OrderByDescending(r => r.RequiredArea).FirstOrDefault();

                if (gene.LandId is null || !lands.TryGetValue(gene.LandId.Value, out var land))
                {
                    Add(result, ConstraintSeverity.Hard, $"Phase {gene.PhaseId} has no valid land allocation.");
                    scoreParts.Add(0d);
                    continue;
                }

                var geneScore = 20d;

                if (!FitnessEvaluationHelper.IsAvailableStatus(land.Status))
                {
                    Add(result, ConstraintSeverity.Hard, $"Land {land.LandCode} is unavailable.");
                    geneScore -= 50d;
                }

                if (requirement is not null)
                {
                    if (string.Equals(land.SoilType?.Trim(), requirement.RequiredSoilType?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        geneScore += 25d;
                    }
                    else
                    {
                        Add(result, ConstraintSeverity.Soft, $"Land {land.LandCode} soil type does not match requirement.");
                        geneScore -= 15d;
                    }

                    if (land.AreaSize >= requirement.RequiredArea)
                    {
                        var wasteRatio = (double)((land.AreaSize - requirement.RequiredArea) / Math.Max(1m, requirement.RequiredArea));
                        geneScore += 30d;
                        result.Bonus += Math.Max(0d, 5d - wasteRatio * 5d);

                        if (wasteRatio > 0.5d)
                        {
                            Add(result, ConstraintSeverity.Soft, $"Land {land.LandCode} is larger than required and may waste area.");
                        }
                    }
                    else
                    {
                        Add(result, ConstraintSeverity.Hard, $"Land {land.LandCode} has insufficient area.");
                        geneScore += 30d * FitnessEvaluationHelper.Percent((double)land.AreaSize, (double)requirement.RequiredArea);
                        geneScore -= 35d;
                    }
                }
                else
                {
                    geneScore += 25d;
                }

                if (input.ExistingLandAllocations.Any(a =>
                        a.LandId == land.LandId &&
                        FitnessEvaluationHelper.Overlaps(gene.StartDate, gene.EndDate, a.StartDate, a.EndDate)))
                {
                    Add(result, ConstraintSeverity.Hard, $"Land {land.LandCode} overlaps with an existing allocation.");
                    geneScore -= 45d;
                }
                else
                {
                    geneScore += 20d;
                }

                scoreParts.Add(FitnessEvaluationHelper.ClampScore(geneScore));
            }

            var internalOverlaps = FitnessEvaluationHelper.CountInternalOverlaps(
                chromosome.Genes.Select(g => (g.LandId, g.StartDate, g.EndDate)));

            for (var i = 0; i < internalOverlaps; i++)
            {
                Add(result, ConstraintSeverity.Hard, "Land is double-booked inside the candidate plan.");
            }

            result.Score = scoreParts.Count == 0 ? 0d : scoreParts.Average();
            if (result.Score >= 85d)
            {
                result.Advantages.Add("Land allocation matches area, soil, and availability requirements.");
            }

            return result;
        }

        private static void Add(ConstraintEvaluationResult result, ConstraintSeverity severity, string message)
        {
            result.Violations.Add(new ConstraintViolation("Land", severity, message));
            result.Disadvantages.Add(message);
        }
    }
}

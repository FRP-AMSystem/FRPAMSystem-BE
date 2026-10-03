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
            var phaseScores = new List<double>();
            var phases = new List<PhaseScoreExplanation>();
            var internalConflicts = GetInternalConflictLandIds(chromosome);

            foreach (var gene in chromosome.Genes)
            {
                var requirement = gene.ExperimentLandRequirementId.HasValue
                    ? requirements.FirstOrDefault(r => r.ExpLandReqId == gene.ExperimentLandRequirementId.Value)
                    : requirements.OrderByDescending(r => r.RequiredArea).FirstOrDefault();
                var phase = new PhaseScoreExplanation { PhaseId = gene.PhaseId };

                if (gene.LandId is null && requirement is null)
                {
                    phase.SubScores.AddRange(SubScores(100d, 100d, 100d, 100d));
                    phase.FinalScore = 100d;
                    phase.Calculation = $"Phase {gene.PhaseId}: no land requirement = 100.00";
                    phaseScores.Add(100d);
                    phases.Add(phase);
                    continue;
                }

                if (gene.LandId is null || !lands.TryGetValue(gene.LandId.Value, out var land))
                {
                    Add(result, ConstraintSeverity.Hard, $"Phase {gene.PhaseId} has no valid land allocation.");
                    phase.SubScores.AddRange(SubScores(0, 0, 0, 0));
                    phase.FinalScore = 0d;
                    phase.Calculation = $"Phase {gene.PhaseId}: invalid allocation = 0.00";
                    phaseScores.Add(0d);
                    phases.Add(phase);
                    continue;
                }

                var soil = requirement is null || string.Equals(
                    land.SoilType?.Trim(), requirement.RequiredSoilType?.Trim(), StringComparison.OrdinalIgnoreCase)
                    ? 100d : 0d;
                var area = requirement is null
                    ? 100d
                    : requirement.RequiredArea <= 0m
                        ? 100d
                        : FitnessEvaluationHelper.ClampScore((double)(land.AreaSize / requirement.RequiredArea * 100m));
                var availability = FitnessEvaluationHelper.IsLandAvailableForOptimization(land.Status) ? 100d : 0d;
                var externalConflict = input.ExistingLandAllocations.Any(a =>
                    a.LandId == land.LandId &&
                    FitnessEvaluationHelper.Overlaps(gene.StartDate, gene.EndDate, a.StartDate, a.EndDate));
                var conflict = externalConflict || internalConflicts.Contains(gene);

                if (availability == 0d)
                {
                    Add(result, ConstraintSeverity.Hard, $"Land {land.LandCode} is unavailable.");
                    phase.Adjustments.Add(Adjustment("Land Availability", -100d, "Adjustment",
                        $"Land {land.LandCode} is unavailable (status: {land.Status}).", "100 → 0"));
                }
                if (requirement is not null && soil == 0d)
                {
                    Add(result, ConstraintSeverity.Soft, $"Land {land.LandCode} soil type does not match requirement.");
                    phase.Adjustments.Add(Adjustment("Soil Mismatch", -15d, "Adjustment",
                        $"Land {land.LandCode} soil type '{land.SoilType}' does not match required '{requirement.RequiredSoilType}'.", "100 → 0"));
                }
                if (requirement is not null && area < 100d)
                {
                    Add(result, ConstraintSeverity.Hard, $"Land {land.LandCode} has insufficient area.");
                    phase.Adjustments.Add(Adjustment("Area Deficiency", area - 100d, "Adjustment",
                        $"Land {land.LandCode} area ({land.AreaSize:F1} m²) is below required ({requirement.RequiredArea:F1} m²).",
                        $"{area:F2}"));
                }
                if (externalConflict)
                {
                    Add(result, ConstraintSeverity.Hard, $"Land {land.LandCode} overlaps with an existing allocation.");
                    phase.Adjustments.Add(Adjustment("External Land Overlap", -100d, "Adjustment",
                        $"Land {land.LandCode} overlaps with an existing allocation.", "100 → 0"));
                }
                if (internalConflicts.Contains(gene))
                {
                    Add(result, ConstraintSeverity.Hard, "Land is double-booked inside the candidate plan.");
                    phase.Adjustments.Add(Adjustment("Internal Land Overlap", -100d, "Adjustment",
                        "The candidate assigns the same land to overlapping phases.", "100 → 0"));
                }
                if (requirement is not null && land.AreaSize > requirement.RequiredArea * 1.5m)
                {
                    Add(result, ConstraintSeverity.Soft, $"Land {land.LandCode} is larger than required and may waste area.");
                }

                var score = 0.25d * soil + 0.30d * area + 0.20d * availability + 0.25d * (conflict ? 0d : 100d);
                phase.SubScores.AddRange(SubScores(soil, area, availability, conflict ? 0d : 100d));
                phase.FinalScore = score;
                phase.Calculation = $"{soil:F2}×25% + {area:F2}×30% + {availability:F2}×20% + {(conflict ? 0d : 100d):F2}×25% = {score:F2}";
                phaseScores.Add(score);
                phases.Add(phase);
            }

            if (input.Experiment?.Deadline is DateTime deadline)
            {
                foreach (var gene in chromosome.Genes.Where(g => g.EndDate > deadline))
                {
                    var message = $"Phase {gene.PhaseId} ends after the experiment deadline.";
                    if (!result.Violations.Any(v => v.Category == "Deadline" && v.Message == message))
                    {
                        result.Violations.Add(new ConstraintViolation("Deadline", ConstraintSeverity.Soft, message));
                        result.Disadvantages.Add(message);
                    }
                    var phase = phases.FirstOrDefault(p => p.PhaseId == gene.PhaseId);
                    phase?.Adjustments.Add(Adjustment("Deadline", -0d, "Penalty",
                        $"Phase {gene.PhaseId} ends after the experiment deadline.", "Soft penalty applied by FitnessCalculator"));
                }
            }

            result.Score = phaseScores.Count == 0 ? 0d : phaseScores.Average();
            result.Explanation = new ScoreExplanation
            {
                BaseScore = 0d,
                FinalScore = result.Score,
                Calculation = phases.Count == 0
                    ? "No land phases = 0.00"
                    : $"Average({string.Join(", ", phaseScores.Select(s => s.ToString("F2")))}) = {result.Score:F2}",
                Adjustments = phases
                    .SelectMany(p => p.Adjustments)
                    .GroupBy(a => (a.Factor, a.Reason, a.Calculation))
                    .Select(g => g.First())
                    .ToList(),
                Phases = phases
            };
            if (result.Score >= 85d)
            {
                result.Advantages.Add("Land allocation matches area, soil, and availability requirements.");
            }
            return result;
        }

        private static IEnumerable<ScoreAdjustment> SubScores(double soil, double area, double availability, double conflict)
        {
            yield return Adjustment("Soil Score", soil * 0.25d, "SubScore", "Normalized soil match.", $"{soil:F2} × 25% = {soil * 0.25d:F2}");
            yield return Adjustment("Area Score", area * 0.30d, "SubScore", "Normalized area sufficiency.", $"{area:F2} × 30% = {area * 0.30d:F2}");
            yield return Adjustment("Land Availability", availability * 0.20d, "SubScore", "Land availability score.", $"{availability:F2} × 20% = {availability * 0.20d:F2}");
            yield return Adjustment("Land Conflict-Free", conflict * 0.25d, "SubScore", "Conflict-free score.", $"{conflict:F2} × 25% = {conflict * 0.25d:F2}");
        }

        private static ScoreAdjustment Adjustment(string factor, double points, string type, string reason, string calculation) =>
            new() { Factor = factor, Points = points, Type = type, Reason = reason, Calculation = calculation };

        private static HashSet<AllocationGene> GetInternalConflictLandIds(AllocationChromosome chromosome)
        {
            var conflicts = new HashSet<AllocationGene>();
            for (var i = 0; i < chromosome.Genes.Count; i++)
            {
                for (var j = i + 1; j < chromosome.Genes.Count; j++)
                {
                    var first = chromosome.Genes[i];
                    var second = chromosome.Genes[j];
                    if (first.LandId.HasValue && first.LandId == second.LandId &&
                        FitnessEvaluationHelper.Overlaps(first.StartDate, first.EndDate, second.StartDate, second.EndDate))
                    {
                        conflicts.Add(first);
                        conflicts.Add(second);
                    }
                }
            }
            return conflicts;
        }

        private static void Add(ConstraintEvaluationResult result, ConstraintSeverity severity, string message)
        {
            if (result.Violations.Any(v => v.Severity == severity && v.Message == message))
            {
                return;
            }
            result.Violations.Add(new ConstraintViolation("Land", severity, message));
            result.Disadvantages.Add(message);
        }
    }
}

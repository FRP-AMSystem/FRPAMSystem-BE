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

                var adjustments = new List<ScoreAdjustment>();

                foreach (var gene in chromosome.Genes)
                {
                    var requirement = gene.ExperimentLandRequirementId.HasValue
                        ? requirements.FirstOrDefault(r => r.ExpLandReqId == gene.ExperimentLandRequirementId.Value)
                        : requirements.OrderByDescending(r => r.RequiredArea).FirstOrDefault();

                    if (gene.LandId is null || !lands.TryGetValue(gene.LandId.Value, out var land))
                    {
                        Add(result, ConstraintSeverity.Hard, $"Phase {gene.PhaseId} has no valid land allocation.");
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Land Allocation",
                            Points = -20d,
                            Type = "Deduction",
                            Reason = $"Phase {gene.PhaseId} has no valid land allocation.",
                            Calculation = "20 - 20 = 0"
                        });
                        scoreParts.Add(0d);
                        continue;
                    }

                    var geneScore = 20d;

                    if (!FitnessEvaluationHelper.IsAvailableStatus(land.Status))
                    {
                        Add(result, ConstraintSeverity.Hard, $"Land {land.LandCode} is unavailable.");
                        geneScore -= 50d;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Land Availability",
                            Points = -50d,
                            Type = "Deduction",
                            Reason = $"Land {land.LandCode} is unavailable (status: {land.Status}).",
                            Calculation = $"-50 (status: {land.Status})"
                        });
                    }

                    if (requirement is not null)
                    {
                        if (string.Equals(land.SoilType?.Trim(), requirement.RequiredSoilType?.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            geneScore += 25d;
                            adjustments.Add(new ScoreAdjustment
                            {
                                Factor = "Soil Match",
                                Points = 25d,
                                Type = "Addition",
                                Reason = $"Land {land.LandCode} soil type matches requirement ({land.SoilType}).",
                                Calculation = "+25"
                            });
                        }
                        else
                        {
                            Add(result, ConstraintSeverity.Soft, $"Land {land.LandCode} soil type does not match requirement.");
                            geneScore -= 15d;
                            adjustments.Add(new ScoreAdjustment
                            {
                                Factor = "Soil Mismatch",
                                Points = -15d,
                                Type = "Deduction",
                                Reason = $"Land {land.LandCode} soil type '{land.SoilType}' does not match required '{requirement.RequiredSoilType}'.",
                                Calculation = "-15"
                            });
                        }

                        if (land.AreaSize >= requirement.RequiredArea)
                        {
                            var wasteRatio = (double)((land.AreaSize - requirement.RequiredArea) / Math.Max(1m, requirement.RequiredArea));
                            geneScore += 30d;
                            adjustments.Add(new ScoreAdjustment
                            {
                                Factor = "Area Sufficiency",
                                Points = 30d,
                                Type = "Addition",
                                Reason = $"Land {land.LandCode} area ({land.AreaSize:F1} m²) meets or exceeds required ({requirement.RequiredArea:F1} m²).",
                                Calculation = "+30"
                            });

                            var bonusAmount = Math.Max(0d, 5d - wasteRatio * 5d);
                            result.Bonus += bonusAmount;
                            if (bonusAmount > 0d)
                            {
                                result.BonusAdjustments.Add(new ScoreAdjustment
                                {
                                    Factor = "Land Area Efficiency",
                                    Points = Math.Round(bonusAmount, 2),
                                    Type = "Bonus",
                                    Reason = $"Land {land.LandCode} has optimal area utilization (waste ratio {wasteRatio:P1}).",
                                    Calculation = $"5 - ({wasteRatio:F2} × 5) = +{bonusAmount:F2}"
                                });
                            }

                            if (wasteRatio > 0.5d)
                            {
                                Add(result, ConstraintSeverity.Soft, $"Land {land.LandCode} is larger than required and may waste area.");
                            }
                        }
                        else
                        {
                            Add(result, ConstraintSeverity.Hard, $"Land {land.LandCode} has insufficient area.");
                            var percent = FitnessEvaluationHelper.Percent((double)land.AreaSize, (double)requirement.RequiredArea);
                            var areaPts = 30d * percent - 35d;
                            geneScore += areaPts;
                            adjustments.Add(new ScoreAdjustment
                            {
                                Factor = "Area Deficiency",
                                Points = Math.Round(areaPts, 2),
                                Type = "Deduction",
                                Reason = $"Land {land.LandCode} area ({land.AreaSize:F1} m²) is smaller than required ({requirement.RequiredArea:F1} m²).",
                                Calculation = $"30 × ({percent:F2}) - 35 = {areaPts:F2}"
                            });
                        }
                    }
                    else
                    {
                        geneScore += 25d;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Default Land Baseline",
                            Points = 25d,
                            Type = "Addition",
                            Reason = $"Phase {gene.PhaseId} has no specific land requirement.",
                            Calculation = "+25"
                        });
                    }

                    if (input.ExistingLandAllocations.Any(a =>
                            a.LandId == land.LandId &&
                            FitnessEvaluationHelper.Overlaps(gene.StartDate, gene.EndDate, a.StartDate, a.EndDate)))
                    {
                        Add(result, ConstraintSeverity.Hard, $"Land {land.LandCode} overlaps with an existing allocation.");
                        geneScore -= 45d;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "External Land Overlap",
                            Points = -45d,
                            Type = "Deduction",
                            Reason = $"Land {land.LandCode} overlaps with an existing allocation.",
                            Calculation = "-45"
                        });
                    }
                    else
                    {
                        geneScore += 20d;
                        adjustments.Add(new ScoreAdjustment
                        {
                            Factor = "Land Availability",
                            Points = 20d,
                            Type = "Addition",
                            Reason = $"Land {land.LandCode} has no conflict with existing allocations.",
                            Calculation = "+20"
                        });
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
                result.Explanation = new ScoreExplanation
                {
                    BaseScore = chromosome.Genes.Count == 0 ? 0d : 20d,
                    FinalScore = Math.Round(result.Score, 2),
                    Adjustments = adjustments
                };

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

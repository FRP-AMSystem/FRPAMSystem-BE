using FRPAMSystem.BusinessTier.AI.DTO;
using FRPAMSystem.BusinessTier.AI.Fitness;
using FRPAMSystem.BusinessTier.AI.Generator;
using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.BusinessTier.AI.Operators.Crossover;
using FRPAMSystem.BusinessTier.AI.Operators.Mutation;
using FRPAMSystem.BusinessTier.AI.Operators.Selection;

namespace FRPAMSystem.BusinessTier.AI.Services
{
    public class GeneticAlgorithmService : IGeneticAlgorithmService
    {
        private readonly IPopulationGenerator _populationGenerator;
        private readonly IFitnessCalculator _fitnessCalculator;
        private readonly ISelectionOperator _selectionOperator;
        private readonly ICrossoverOperator _crossoverOperator;
        private readonly IMutationOperator _mutationOperator;

        public GeneticAlgorithmService(
            IPopulationGenerator populationGenerator,
            IFitnessCalculator fitnessCalculator,
            ISelectionOperator selectionOperator,
            ICrossoverOperator crossoverOperator,
            IMutationOperator mutationOperator)
        {
            _populationGenerator = populationGenerator;
            _fitnessCalculator = fitnessCalculator;
            _selectionOperator = selectionOperator;
            _crossoverOperator = crossoverOperator;
            _mutationOperator = mutationOperator;
        }

        public IReadOnlyList<AllocationSuggestionDTO> GenerateSuggestions(OptimizationInput input)
        {
            if (input.Experiment is null)
            {
                throw new ArgumentException("Optimization input must include an experiment.");
            }

            input.Settings.Normalize();

            var population = _populationGenerator.Generate(input);
            Evaluate(population.Chromosomes, input);

            for (var generation = 0; generation < input.Settings.GenerationCount; generation++)
            {
                var ordered = population.Chromosomes
                    .OrderBy(c => c.HardViolationCount)
                    .ThenByDescending(c => c.FitnessScore)
                    .ThenBy(c => c.SoftViolationCount)
                    .ToList();

                var nextGeneration = ordered
                    .Take(input.Settings.EliteCount)
                    .Select(c => c.Clone())
                    .ToList();
                var fingerprints = nextGeneration.Select(CreateFingerprint).ToHashSet();
                var reproductionAttempts = 0;

                while (nextGeneration.Count < input.Settings.PopulationSize &&
                       reproductionAttempts < input.Settings.PopulationSize * 5)
                {
                    reproductionAttempts++;
                    var firstParent = _selectionOperator.Select(ordered, input.Settings);
                    var secondParent = _selectionOperator.Select(ordered, input.Settings);
                    var (crossedFirstChild, crossedSecondChild) =
                        _crossoverOperator.Crossover(firstParent, secondParent, input.Settings);
                    var firstChild = crossedFirstChild.Clone();
                    var secondChild = crossedSecondChild.Clone();

                    _mutationOperator.Mutate(firstChild, input, generation);
                    _mutationOperator.Mutate(secondChild, input, generation);

                    AddIfUnique(nextGeneration, fingerprints, firstChild, input.Settings.PopulationSize);
                    AddIfUnique(nextGeneration, fingerprints, secondChild, input.Settings.PopulationSize);
                }

                var fallbackAttempts = 0;
                var maxFallbackAttempts = input.Settings.PopulationSize * 5;

                while (nextGeneration.Count < input.Settings.PopulationSize &&
                       fallbackAttempts < maxFallbackAttempts)
                {
                    fallbackAttempts++;

                    var parent = _selectionOperator.Select(ordered, input.Settings);
                    var candidate = parent.Clone();

                    _mutationOperator.Mutate(candidate, input, generation);

                    AddIfUnique(
                        nextGeneration,
                        fingerprints,
                        candidate,
                        input.Settings.PopulationSize);
                }

                static void AddIfUnique(
                    ICollection<AllocationChromosome> nextGeneration,
                    ISet<string> fingerprints,
                    AllocationChromosome child,
                    int populationSize)
                {
                    if (nextGeneration.Count >= populationSize)
                    {
                        return;
                    }

                    if (fingerprints.Add(CreateFingerprint(child)))
                    {
                        nextGeneration.Add(child);
                    }
                }

                Evaluate(nextGeneration, input);
                population.Chromosomes = nextGeneration;
            }

            return population.Chromosomes
                .OrderBy(c => c.HardViolationCount)
                .ThenByDescending(c => c.FitnessScore)
                .ThenBy(c => c.SoftViolationCount)
                .Take(input.Settings.TopSuggestionCount)
                .Select((chromosome, index) => MapSuggestion(chromosome, input, index + 1))
                .ToList();
        }

        private void Evaluate(IEnumerable<AllocationChromosome> chromosomes, OptimizationInput input)
        {
            foreach (var chromosome in chromosomes)
            {
                _fitnessCalculator.Evaluate(chromosome, input);
            }
        }

        private static AllocationSuggestionDTO MapSuggestion(
            AllocationChromosome chromosome,
            OptimizationInput input,
            int rank)
        {
            var phases = input.ExperimentPhases.ToDictionary(p => p.PhaseId);
            var lands = input.LandResources.ToDictionary(l => l.LandId);
            var humans = input.HumanResources.ToDictionary(h => h.HumanResourceId);
            var equipment = input.EquipmentInstances.ToDictionary(e => e.EquipmentInstanceId);

            var suggestion = new AllocationSuggestionDTO
            {
                Rank = rank,
                FitnessScore = Math.Round(chromosome.FitnessScore, 2),
                PenaltyScore = Math.Round(chromosome.PenaltyScore, 2),
                BonusScore = Math.Round(chromosome.BonusScore, 2),
                ConflictCount = chromosome.ConflictCount,
                HardViolationCount = chromosome.HardViolationCount,
                SoftViolationCount = chromosome.SoftViolationCount,
                IsFeasible = chromosome.IsFeasible,
                EstimatedCompletionTime = chromosome.Genes.Select(g => g.EndDate).DefaultIfEmpty(input.Experiment.ExpectEndDate).Max(),
                FitnessBreakdown = new FitnessBreakdownDTO
                {
                    LandScore = Math.Round(chromosome.FitnessBreakdown.LandScore, 2),
                    HumanScore = Math.Round(chromosome.FitnessBreakdown.HumanScore, 2),
                    EquipmentScore = Math.Round(chromosome.FitnessBreakdown.EquipmentScore, 2),
                    MaintenanceScore = Math.Round(chromosome.FitnessBreakdown.MaintenanceScore, 2),
                    PenaltyScore = Math.Round(chromosome.FitnessBreakdown.PenaltyScore, 2),
                    BonusScore = Math.Round(chromosome.FitnessBreakdown.BonusScore, 2),
                    FinalScore = Math.Round(chromosome.FitnessBreakdown.FinalScore, 2),
                    OverallCalculation = chromosome.FitnessBreakdown.OverallCalculation,
                    Land = MapExplanationDTO(chromosome.FitnessBreakdown.Land),
                    Human = MapExplanationDTO(chromosome.FitnessBreakdown.Human),
                    Equipment = MapExplanationDTO(chromosome.FitnessBreakdown.Equipment),
                    Maintenance = MapExplanationDTO(chromosome.FitnessBreakdown.Maintenance),
                    Penalties = chromosome.FitnessBreakdown.Penalties.Select(MapAdjustmentDTO).ToList(),
                    Bonuses = chromosome.FitnessBreakdown.Bonuses.Select(MapAdjustmentDTO).ToList()
                },
                ConstraintReport = new ConstraintReportDTO
                {
                    HardViolationCount = chromosome.HardViolationCount,
                    SoftViolationCount = chromosome.SoftViolationCount,
                    LandConflicts = chromosome.ConstraintReport.LandConflicts.Distinct().ToList(),
                    HumanConflicts = chromosome.ConstraintReport.HumanConflicts.Distinct().ToList(),
                    EquipmentConflicts = chromosome.ConstraintReport.EquipmentConflicts.Distinct().ToList(),
                    MaintenanceConflicts = chromosome.ConstraintReport.MaintenanceConflicts.Distinct().ToList(),
                    SkillConflicts = chromosome.ConstraintReport.SkillConflicts.Distinct().ToList(),
                    RoleConflicts = chromosome.ConstraintReport.RoleConflicts.Distinct().ToList(),
                    DeadlineConflicts = chromosome.ConstraintReport.DeadlineConflicts.Distinct().ToList()
                },
                Advantages = chromosome.Advantages.Distinct().Take(5).ToList(),
                Disadvantages = chromosome.Disadvantages.Distinct().Take(8).ToList()
            };

            foreach (var gene in chromosome.Genes.OrderBy(g => phases.TryGetValue(g.PhaseId, out var phase) ? phase.PhaseOrder : int.MaxValue))
            {
                phases.TryGetValue(gene.PhaseId, out var phase);

                suggestion.Timeline.Add(new TimelineItemDTO
                {
                    PhaseId = gene.PhaseId,
                    PhaseName = phase?.PhaseName ?? string.Empty,
                    StartDate = gene.StartDate,
                    EndDate = gene.EndDate,
                    DurationDays = Math.Max(1, (gene.EndDate.Date - gene.StartDate.Date).Days + 1)
                });

                if (gene.LandId.HasValue && lands.TryGetValue(gene.LandId.Value, out var land))
                {
                    suggestion.AllocatedLands.Add(new AllocatedLandDTO
                    {
                        PhaseId = gene.PhaseId,
                        PhaseName = phase?.PhaseName ?? string.Empty,
                        LandId = land.LandId,
                        LandCode = land.LandCode,
                        SoilType = land.SoilType,
                        AreaSize = land.AreaSize,
                        StartDate = gene.StartDate,
                        EndDate = gene.EndDate
                    });
                }

                foreach (var humanResourceId in gene.AssignedHumanResourceIds.Distinct())
                {
                    if (!humans.TryGetValue(humanResourceId, out var human))
                    {
                        continue;
                    }

                    suggestion.AllocatedHumans.Add(new AllocatedHumanDTO
                    {
                        PhaseId = gene.PhaseId,
                        PhaseName = phase?.PhaseName ?? string.Empty,
                        HumanResourceId = human.HumanResourceId,
                        FullName = human.User?.FullName,
                        CurrentWorkload = human.CurrentWorkload,
                        StartDate = gene.StartDate,
                        EndDate = gene.EndDate
                    });
                }

                foreach (var assignment in gene.EquipmentAssignments)
                {
                    var instance = assignment.EquipmentInstanceId.HasValue &&
                                   equipment.TryGetValue(assignment.EquipmentInstanceId.Value, out var found)
                        ? found
                        : null;

                    suggestion.AllocatedEquipment.Add(new AllocatedEquipmentDTO
                    {
                        PhaseId = gene.PhaseId,
                        PhaseName = phase?.PhaseName ?? string.Empty,
                        EquipmentInstanceId = instance?.EquipmentInstanceId,
                        AssetCode = instance?.AssetCode,
                        RequiredEquipmentTypeId = assignment.RequiredEquipmentTypeId,
                        AllocatedEquipmentTypeId = assignment.AllocatedEquipmentTypeId,
                        EquipmentTypeName = instance?.EquipmentType?.Name,
                        IsSubstitute = assignment.IsSubstitute,
                        EfficiencyRate = assignment.EfficiencyRate,
                        TimeMultiplier = assignment.TimeMultiplier,
                        StartDate = gene.StartDate,
                        EndDate = gene.EndDate
                    });
                }
            }

            if (suggestion.Advantages.Count == 0)
            {
                suggestion.Advantages.Add("Balanced allocation candidate generated from resource availability and requirements.");
            }

            if (suggestion.Disadvantages.Count == 0)
            {
                suggestion.Disadvantages.Add("No major disadvantages detected by the current fitness model.");
            }

            return suggestion;
        }

        private static ScoreExplanationDTO MapExplanationDTO(ScoreExplanation? explanation)
        {
            if (explanation == null)
            {
                return new ScoreExplanationDTO();
            }

            return new ScoreExplanationDTO
            {
                BaseScore = explanation.BaseScore,
                FinalScore = explanation.FinalScore,
                Calculation = explanation.Calculation,
                Adjustments = explanation.Adjustments.Select(MapAdjustmentDTO).ToList(),
                Penalties = explanation.Penalties.Select(MapAdjustmentDTO).ToList(),
                Bonuses = explanation.Bonuses.Select(MapAdjustmentDTO).ToList(),
                Phases = explanation.Phases.Select(p => new PhaseScoreExplanationDTO
                {
                    PhaseId = p.PhaseId,
                    BaseScore = p.BaseScore,
                    SubScores = p.SubScores.Select(MapAdjustmentDTO).ToList(),
                    Adjustments = p.Adjustments.Select(MapAdjustmentDTO).ToList(),
                    Bonuses = p.Bonuses.Select(MapAdjustmentDTO).ToList(),
                    Penalties = p.Penalties.Select(MapAdjustmentDTO).ToList(),
                    FinalScore = p.FinalScore,
                    Calculation = p.Calculation
                }).ToList()
            };
        }

        private static ScoreAdjustmentDTO MapAdjustmentDTO(ScoreAdjustment adjustment)
        {
            return new ScoreAdjustmentDTO
            {
                Factor = adjustment.Factor,
                Points = adjustment.Points,
                Type = adjustment.Type,
                Reason = adjustment.Reason,
                Calculation = adjustment.Calculation
            };
        }

        private static string CreateFingerprint(AllocationChromosome chromosome)
        {
            return AllocationChromosomeFingerprint.Create(chromosome);
        }
    }
}

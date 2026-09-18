using FRPAMSystem.BusinessTier.AI.Fitness;
using FRPAMSystem.BusinessTier.AI.Fitness.Evaluators;
using FRPAMSystem.BusinessTier.AI.Generator;
using FRPAMSystem.BusinessTier.AI.Mappers;
using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.BusinessTier.AI.Operators.Crossover;
using FRPAMSystem.BusinessTier.AI.Operators.Mutation;
using FRPAMSystem.BusinessTier.AI.Operators.Selection;
using FRPAMSystem.BusinessTier.AI.Services;
using FRPAMSystem.DataTier.Models;
using Moq;
using Xunit;

namespace FRPAMSystem.NotificationTests.Services
{
    public class GeneticAlgorithmServiceTests
    {
        // UT148-TC55
        // Abnormal
        [Fact]
        public void GenerateSuggestions_WhenExperimentIsNull_ShouldThrowArgumentException()
        {
            // Arrange
            var input = new OptimizationInput
            {
                Experiment = null!
            };

            var popGenMock = new Mock<IPopulationGenerator>();
            var fitCalcMock = new Mock<IFitnessCalculator>();
            var selMock = new Mock<ISelectionOperator>();
            var crossMock = new Mock<ICrossoverOperator>();
            var mutMock = new Mock<IMutationOperator>();

            var service = new GeneticAlgorithmService(
                popGenMock.Object,
                fitCalcMock.Object,
                selMock.Object,
                crossMock.Object,
                mutMock.Object);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => service.GenerateSuggestions(input));
            Assert.Equal("Optimization input must include an experiment.", ex.Message);
        }

        // UT148-TC56
        // Normal
        [Fact]
        public void GenerateSuggestions_WithValidOptimizationInput_ShouldProduceSuggestions()
        {
            // Arrange
            var experiment = new Experiment
            {
                ExperimentId = 1,
                ExperimentName = "Pine Growth Test",
                ExpectStartDate = new DateTime(2026, 9, 1),
                ExpectEndDate = new DateTime(2026, 12, 31)
            };

            var phase1 = new ExperimentPhase
            {
                PhaseId = 10,
                PhaseName = "Soil Prep",
                PhaseOrder = 1,
                ExpectedStartDate = new DateTime(2026, 9, 1),
                ExpectedEndDate = new DateTime(2026, 9, 30)
            };

            var input = new OptimizationInput
            {
                Experiment = experiment,
                ExperimentPhases = new List<ExperimentPhase> { phase1 },
                LandResources = new List<LandResource>
                {
                    new LandResource { LandId = 100, LandCode = "L-01", SoilType = "Sandy", AreaSize = 10.0m }
                },
                HumanResources = new List<HumanResourceProfile>
                {
                    new HumanResourceProfile { HumanResourceId = 50, User = new User { FullName = "Researcher A" } }
                },
                EquipmentInstances = new List<EquipmentInstance>
                {
                    new EquipmentInstance { EquipmentInstanceId = 200, AssetCode = "EQ-01", EquipmentType = new EquipmentType { Name = "Tractor" } }
                },
                Settings = new OptimizationSettings
                {
                    PopulationSize = 4,
                    GenerationCount = 2,
                    EliteCount = 1,
                    TopSuggestionCount = 2
                }
            };

            var chromosome1 = new AllocationChromosome
            {
                FitnessScore = 80.0,
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 10,
                        LandId = 100,
                        StartDate = new DateTime(2026, 9, 1),
                        EndDate = new DateTime(2026, 9, 30),
                        AssignedHumanResourceIds = new List<int> { 50 },
                        EquipmentAssignments = new List<EquipmentAssignmentGene>
                        {
                            new EquipmentAssignmentGene { EquipmentInstanceId = 200, RequiredEquipmentTypeId = 1, AllocatedEquipmentTypeId = 1, EfficiencyRate = 1.0 }
                        }
                    }
                }
            };

            var chromosome2 = new AllocationChromosome
            {
                FitnessScore = 90.0,
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 10,
                        LandId = 100,
                        StartDate = new DateTime(2026, 9, 1),
                        EndDate = new DateTime(2026, 9, 30),
                        AssignedHumanResourceIds = new List<int> { 50 },
                        EquipmentAssignments = new List<EquipmentAssignmentGene>
                        {
                            new EquipmentAssignmentGene { EquipmentInstanceId = 200, RequiredEquipmentTypeId = 1, AllocatedEquipmentTypeId = 1, EfficiencyRate = 1.0 }
                        }
                    }
                }
            };

            var popGenMock = new Mock<IPopulationGenerator>();
            popGenMock.Setup(g => g.Generate(input))
                .Returns(new Population { Chromosomes = new List<AllocationChromosome> { chromosome1, chromosome2 } });

            var fitCalcMock = new Mock<IFitnessCalculator>();
            var selMock = new Mock<ISelectionOperator>();
            selMock.Setup(s => s.Select(It.IsAny<IReadOnlyList<AllocationChromosome>>(), It.IsAny<OptimizationSettings>()))
                .Returns(chromosome1);

            var crossMock = new Mock<ICrossoverOperator>();
            crossMock.Setup(c => c.Crossover(It.IsAny<AllocationChromosome>(), It.IsAny<AllocationChromosome>(), It.IsAny<OptimizationSettings>()))
                .Returns((chromosome1.Clone(), chromosome2.Clone()));

            var mutMock = new Mock<IMutationOperator>();

            var service = new GeneticAlgorithmService(
                popGenMock.Object,
                fitCalcMock.Object,
                selMock.Object,
                crossMock.Object,
                mutMock.Object);

            // Act
            var suggestions = service.GenerateSuggestions(input);

            // Assert
            Assert.NotNull(suggestions);
            Assert.NotEmpty(suggestions);
            Assert.True(suggestions.Count <= input.Settings.TopSuggestionCount);
            Assert.Equal(1, suggestions[0].Rank);
        }

        // UT148-TC57
        // Boundary
        [Fact]
        public void FitnessCalculator_WhenLandConflictExists_ShouldApplyLandPenalty()
        {
            // Arrange
            var evaluators = new IConstraintEvaluator[]
            {
                new LandConstraintEvaluator(),
                new HumanConstraintEvaluator(),
                new EquipmentConstraintEvaluator(),
                new MaintenanceConstraintEvaluator()
            };

            var calculator = new FitnessCalculator(evaluators);

            var input = new OptimizationInput
            {
                ExistingLandAllocations = new List<AllocationLandDetail>
                {
                    new AllocationLandDetail
                    {
                        LandId = 1,
                        StartDate = new DateTime(2026, 9, 1),
                        EndDate = new DateTime(2026, 9, 15)
                    }
                }
            };

            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        LandId = 1, // Overlaps with existing land allocation!
                        StartDate = new DateTime(2026, 9, 5),
                        EndDate = new DateTime(2026, 9, 20)
                    }
                }
            };

            // Act
            var result = calculator.Evaluate(chromosome, input);

            // Assert
            Assert.True(result.ConflictCount > 0);
            Assert.True(result.PenaltyScore < 0);
            Assert.NotEmpty(result.ConstraintReport.LandConflicts);
        }

        // UT148-TC58
        // Normal
        [Fact]
        public void GenerateSuggestions_FeasibleCandidate_ShouldRankHigherThanInfeasibleCandidate()
        {
            // Arrange
            var input = new OptimizationInput
            {
                Experiment = new Experiment
                {
                    ExperimentId = 1,
                    ExperimentName = "Test Exp",
                    ExpectStartDate = new DateTime(2026, 9, 1),
                    ExpectEndDate = new DateTime(2026, 9, 30)
                },
                ExperimentPhases = new List<ExperimentPhase>
                {
                    new ExperimentPhase { PhaseId = 1, PhaseOrder = 1, ExpectedStartDate = new DateTime(2026, 9, 1), ExpectedEndDate = new DateTime(2026, 9, 30) }
                },
                Settings = new OptimizationSettings
                {
                    PopulationSize = 2,
                    GenerationCount = 1,
                    EliteCount = 1,
                    TopSuggestionCount = 2
                }
            };

            var infeasible = new AllocationChromosome
            {
                FitnessScore = 88.0,
                HardViolationCount = 2,
                SoftViolationCount = 0,
                Genes = new List<AllocationGene>
                {
                    new AllocationGene { PhaseId = 1, StartDate = new DateTime(2026, 9, 1), EndDate = new DateTime(2026, 9, 30) }
                }
            };

            var feasible = new AllocationChromosome
            {
                FitnessScore = 75.0,
                HardViolationCount = 0,
                SoftViolationCount = 0,
                Genes = new List<AllocationGene>
                {
                    new AllocationGene { PhaseId = 1, StartDate = new DateTime(2026, 9, 1), EndDate = new DateTime(2026, 9, 30) }
                }
            };

            var popGenMock = new Mock<IPopulationGenerator>();
            popGenMock.Setup(g => g.Generate(input))
                .Returns(new Population { Chromosomes = new List<AllocationChromosome> { infeasible, feasible } });

            var fitCalcMock = new Mock<IFitnessCalculator>();
            var selMock = new Mock<ISelectionOperator>();
            selMock.Setup(s => s.Select(It.IsAny<IReadOnlyList<AllocationChromosome>>(), It.IsAny<OptimizationSettings>()))
                .Returns(feasible);

            var crossMock = new Mock<ICrossoverOperator>();
            crossMock.Setup(c => c.Crossover(It.IsAny<AllocationChromosome>(), It.IsAny<AllocationChromosome>(), It.IsAny<OptimizationSettings>()))
                .Returns((feasible.Clone(), infeasible.Clone()));

            var mutMock = new Mock<IMutationOperator>();

            var service = new GeneticAlgorithmService(
                popGenMock.Object,
                fitCalcMock.Object,
                selMock.Object,
                crossMock.Object,
                mutMock.Object);

            // Act
            var suggestions = service.GenerateSuggestions(input);

            // Assert
            Assert.NotNull(suggestions);
            Assert.True(suggestions.Count >= 2);
            Assert.Equal(1, suggestions[0].Rank);
            Assert.Equal(75.0, suggestions[0].FitnessScore);
            Assert.True(suggestions[0].IsFeasible);
            Assert.Equal(0, suggestions[0].HardViolationCount);
        }

        [Fact]
        public void CreateFingerprint_WhenEquipmentAssignmentDetailsChange_ShouldChangeFingerprint()
        {
            var baseline = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        LandId = 10,
                        EquipmentAssignments = new List<EquipmentAssignmentGene>
                        {
                            new EquipmentAssignmentGene
                            {
                                RequiredEquipmentTypeId = 2,
                                AllocatedEquipmentTypeId = 2,
                                EquipmentInstanceId = 20,
                                EfficiencyRate = 1d,
                                TimeMultiplier = 1d
                            }
                        }
                    }
                }
            };
            var changed = baseline.Clone();
            changed.Genes[0].EquipmentAssignments[0].IsSubstitute = true;
            changed.Genes[0].EquipmentAssignments[0].EfficiencyRate = 0.75d;

            var baselineFingerprint = GetFingerprint(baseline);
            var changedFingerprint = GetFingerprint(changed);

            Assert.NotEqual(baselineFingerprint, changedFingerprint);
        }

        [Fact]
        public void GenerateSuggestions_WhenFallbackIsNeeded_ShouldMutateClonesInsteadOfSelectedReferences()
        {
            var input = new OptimizationInput
            {
                Experiment = new Experiment
                {
                    ExperimentId = 1,
                    ExpectStartDate = new DateTime(2026, 9, 1),
                    ExpectEndDate = new DateTime(2026, 9, 30)
                },
                ExperimentPhases = new List<ExperimentPhase>
                {
                    new ExperimentPhase
                    {
                        PhaseId = 1,
                        PhaseOrder = 1,
                        ExpectedStartDate = new DateTime(2026, 9, 1),
                        ExpectedEndDate = new DateTime(2026, 9, 30)
                    }
                },
                Settings = new OptimizationSettings
                {
                    PopulationSize = 2,
                    GenerationCount = 1,
                    EliteCount = 1,
                    TopSuggestionCount = 2
                }
            };
            var selected = new AllocationChromosome
            {
                Genes = new List<AllocationGene> { new AllocationGene { PhaseId = 1 } }
            };
            var mutated = new List<AllocationChromosome>();

            var populationGenerator = new Mock<IPopulationGenerator>();
            populationGenerator.Setup(g => g.Generate(input))
                .Returns(new Population
                {
                    Chromosomes = new List<AllocationChromosome> { selected, selected.Clone() }
                });

            var selection = new Mock<ISelectionOperator>();
            selection.Setup(s => s.Select(
                    It.IsAny<IReadOnlyList<AllocationChromosome>>(),
                    It.IsAny<OptimizationSettings>()))
                .Returns(selected);

            var crossover = new Mock<ICrossoverOperator>();
            crossover.Setup(c => c.Crossover(
                    It.IsAny<AllocationChromosome>(),
                    It.IsAny<AllocationChromosome>(),
                    It.IsAny<OptimizationSettings>()))
                .Returns((selected, selected));

            var mutation = new Mock<IMutationOperator>();
            mutation.Setup(m => m.Mutate(
                    It.IsAny<AllocationChromosome>(),
                    input,
                    It.IsAny<int>()))
                .Callback<AllocationChromosome, OptimizationInput, int>((chromosome, _, _) => mutated.Add(chromosome));

            var service = new GeneticAlgorithmService(
                populationGenerator.Object,
                new Mock<IFitnessCalculator>().Object,
                selection.Object,
                crossover.Object,
                mutation.Object);

            service.GenerateSuggestions(input);

            Assert.NotEmpty(mutated);
            Assert.DoesNotContain(mutated, chromosome => ReferenceEquals(chromosome, selected));
            Assert.Equal(mutated.Count, mutated.Distinct(ReferenceEqualityComparer.Instance).Count());
        }

        private static string GetFingerprint(AllocationChromosome chromosome)
        {
            var method = typeof(GeneticAlgorithmService).GetMethod(
                "CreateFingerprint",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            return (string)method!.Invoke(null, new object[] { chromosome })!;
        }
    }
}

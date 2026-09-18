using FRPAMSystem.BusinessTier.AI.Generator;
using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.BusinessTier.AI.Operators.Mutation;
using FRPAMSystem.DataTier.Models;

namespace FRPAMSystem.NotificationTests.AI
{
    public class AdaptiveMutationOperatorTests
    {
        [Theory]
        [InlineData(0, MutationComponent.Land)]
        [InlineData(1, MutationComponent.Human)]
        [InlineData(2, MutationComponent.Equipment)]
        [InlineData(3, MutationComponent.Schedule)]
        public void Mutate_WhenTriggered_MutatesExactlyOneComponentAndPreservesOtherFields(
            int componentValue,
            MutationComponent expectedComponent)
        {
            var gene = CreateGene();
            var original = gene.Clone();
            var generator = new RecordingPopulationGenerator();
            var chromosome = new AllocationChromosome { Genes = new List<AllocationGene> { gene } };
            var input = CreateInput(initialMutationRate: 1d, finalMutationRate: 1d);

            new AdaptiveMutationOperator(generator, new ScriptedRandom(0d, componentValue))
                .Mutate(chromosome, input, 0);

            Assert.Equal(expectedComponent, generator.Components.Single());
            Assert.Same(gene, chromosome.Genes.Single());
            Assert.Equal(original.PhaseId, gene.PhaseId);
            Assert.Equal(original.ExperimentLandRequirementId, gene.ExperimentLandRequirementId);
            Assert.Equal(original.StartDate, gene.StartDate);
            Assert.Equal(original.EndDate, gene.EndDate);
            Assert.Equal(original.AssignedHumanResourceIds, gene.AssignedHumanResourceIds);
            Assert.Equal(original.AssignedEquipmentInstanceIds, gene.AssignedEquipmentInstanceIds);
            Assert.Equal(original.EquipmentAssignments.Select(e => e.EquipmentInstanceId),
                gene.EquipmentAssignments.Select(e => e.EquipmentInstanceId));
        }

        [Fact]
        public void Mutate_WhenRateIsZero_DoesNotMutate()
        {
            var generator = new RecordingPopulationGenerator();
            var chromosome = new AllocationChromosome { Genes = new List<AllocationGene> { CreateGene() } };

            new AdaptiveMutationOperator(generator, new ScriptedRandom(1d, 0))
                .Mutate(chromosome, CreateInput(0d, 0d), 0);

            Assert.Empty(generator.Components);
        }

        [Fact]
        public void Mutate_WhenChromosomeIsEmpty_DoesNotMutate()
        {
            var generator = new RecordingPopulationGenerator();

            new AdaptiveMutationOperator(generator, new ScriptedRandom(0d, 0))
                .Mutate(new AllocationChromosome(), CreateInput(1d, 1d), 0);

            Assert.Empty(generator.Components);
        }

        [Fact]
        public void EquipmentMutation_PreservesSubstitutionMetadataAndUniqueIds()
        {
            var input = CreateInput(1d, 1d);
            input.ExperimentEquipmentRequirements = new List<ExperimentEquipmentRequirement>
            {
                new()
                {
                    ExpEquipmentReqId = 7,
                    EquipmentTypeId = 1,
                    Quantity = 2,
                    AllowSubstitute = true,
                    MinAcceptableEfficiency = 0.8d
                }
            };
            input.EquipmentInstances = new List<EquipmentInstance>
            {
                new() { EquipmentInstanceId = 10, EquipmentTypeId = 2, Status = "Available", ConditionLevel = "Good" },
                new() { EquipmentInstanceId = 11, EquipmentTypeId = 2, Status = "Available", ConditionLevel = "Good" }
            };
            input.EquipmentSubstitutions = new List<EquipmentSubstitution>
            {
                new()
                {
                    PrimaryEquipmentTypeId = 1,
                    SubEquipmentTypeId = 2,
                    EfficiencyRate = 0.9d,
                    TimeMultiplier = 1.5d
                }
            };
            var gene = CreateGene();
            gene.EndDate = gene.StartDate.AddDays(4);

            new PopulationGenerator(new ScriptedRandom(0d, 0))
                .MutateComponent(gene, input, MutationComponent.Equipment);

            Assert.Equal(new[] { 10, 11 }, gene.AssignedEquipmentInstanceIds.OrderBy(id => id));
            Assert.Equal(2, gene.EquipmentAssignments.Count);
            Assert.All(gene.EquipmentAssignments, assignment =>
            {
                Assert.True(assignment.IsSubstitute);
                Assert.Equal(0.9d, assignment.EfficiencyRate);
                Assert.Equal(1.5d, assignment.TimeMultiplier);
            });
            Assert.Equal(gene.AssignedEquipmentInstanceIds.Distinct().Count(),
                gene.AssignedEquipmentInstanceIds.Count);
            Assert.Equal(gene.StartDate.AddDays(6), gene.EndDate);
        }

        [Fact]
        public void HumanMutation_RespectsRequirementQuantityAndUniqueness()
        {
            var input = CreateInput(1d, 1d);
            input.ExperimentHumanRequirements = new List<ExperimentHumanRequirement>
            {
                new() { RoleId = 5, Quantity = 2 }
            };
            input.HumanResources = new List<HumanResourceProfile>
            {
                new() { HumanResourceId = 60, Status = "Available", User = new User { RoleId = 5 } },
                new() { HumanResourceId = 61, Status = "Available", User = new User { RoleId = 5 } },
                new() { HumanResourceId = 62, Status = "Available", User = new User { RoleId = 5 } }
            };
            var gene = CreateGene();

            new PopulationGenerator(new ScriptedRandom(0d, 0))
                .MutateComponent(gene, input, MutationComponent.Human);

            Assert.Equal(2, gene.AssignedHumanResourceIds.Count);
            Assert.Equal(gene.AssignedHumanResourceIds.Distinct().Count(),
                gene.AssignedHumanResourceIds.Count);
        }

        [Fact]
        public void LandMutation_PreservesRequirementMetadataAndOtherFields()
        {
            var input = CreateInput(1d, 1d);
            input.ExperimentLandRequirements = new List<ExperimentLandRequirement>
            {
                new() { ExpLandReqId = 99, RequiredArea = 10, RequiredSoilType = "Sandy" }
            };
            input.LandResources = new List<LandResource>
            {
                new() { LandId = 70, Status = "Available", SoilType = "Sandy", AreaSize = 10 }
            };
            var gene = CreateGene();
            gene.ExperimentLandRequirementId = 88;
            var originalHumans = gene.AssignedHumanResourceIds.ToList();

            new PopulationGenerator(new ScriptedRandom(0d, 0))
                .MutateComponent(gene, input, MutationComponent.Land);

            Assert.Equal(88, gene.ExperimentLandRequirementId);
            Assert.Equal(originalHumans, gene.AssignedHumanResourceIds);
            Assert.Equal(70, gene.LandId);
        }

        private static AllocationGene CreateGene()
        {
            return new AllocationGene
            {
                PhaseId = 1,
                LandId = 20,
                ExperimentLandRequirementId = 30,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 5),
                AssignedHumanResourceIds = new List<int> { 40, 41 },
                AssignedEquipmentInstanceIds = new List<int> { 50 },
                EquipmentAssignments = new List<EquipmentAssignmentGene>
                {
                    new() { EquipmentInstanceId = 50, RequiredEquipmentTypeId = 1 }
                }
            };
        }

        private static OptimizationInput CreateInput(double initialMutationRate, double finalMutationRate)
        {
            return new OptimizationInput
            {
                ExperimentPhases = new List<ExperimentPhase>
                {
                    new()
                    {
                        PhaseId = 1,
                        ExpectedStartDate = new DateTime(2026, 1, 1),
                        ExpectedEndDate = new DateTime(2026, 1, 5)
                    }
                },
                Settings = new OptimizationSettings
                {
                    GenerationCount = 1,
                    InitialMutationRate = initialMutationRate,
                    FinalMutationRate = finalMutationRate,
                    MaxScheduleShiftDays = 1
                }
            };
        }

        private sealed class RecordingPopulationGenerator : IPopulationGenerator
        {
            public List<MutationComponent> Components { get; } = new();

            public Population Generate(OptimizationInput input) => new();

            public AllocationGene GenerateGene(int phaseId, OptimizationInput input) => new() { PhaseId = phaseId };

            public void MutateComponent(AllocationGene gene, OptimizationInput input, MutationComponent component)
            {
                Components.Add(component);
            }
        }

        private sealed class ScriptedRandom : Random
        {
            private readonly double _nextDouble;
            private readonly int _next;

            public ScriptedRandom(double nextDouble, int next)
            {
                _nextDouble = nextDouble;
                _next = next;
            }

            public override double NextDouble() => _nextDouble;

            public override int Next(int maxValue) => _next;

            public override int Next(int minValue, int maxValue) => _next;
        }
    }
}

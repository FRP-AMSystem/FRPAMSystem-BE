using FRPAMSystem.BusinessTier.AI.Fitness;
using FRPAMSystem.BusinessTier.AI.Fitness.Evaluators;
using FRPAMSystem.BusinessTier.AI.Generator;
using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.DataTier.Models;

var tests = new (string Name, Action Test)[]
{
    ("Primary equipment available uses primary assignment", TestPrimaryEquipmentAvailable),
    ("Primary unavailable uses valid substitute", TestSubstituteWhenPrimaryUnavailable),
    ("Primary unavailable and no substitute keeps existing shortage behavior", TestNoSubstituteLeavesShortage),
    ("Substitution duration can create deadline violation", TestSubstitutionDeadlineViolation),
    ("Substitution duration can create resource overlap", TestSubstitutionResourceOverlap),
    ("Primary is preferred over lower-efficiency longer substitute", TestPrimaryPreferred)
};

foreach (var test in tests)
{
    test.Test();
    Console.WriteLine($"PASS: {test.Name}");
}

static void TestPrimaryEquipmentAvailable()
{
    var input = CreateInput(primaryAvailable: true, includeSubstitution: true);
    var gene = new PopulationGenerator().GenerateGene(10, input);

    var assignment = AssertSingleAssignment(gene);
    Assert(!assignment.IsSubstitute, "Expected primary equipment, not substitute.");
    Assert(assignment.AllocatedEquipmentTypeId == 1, "Expected allocated equipment type 1.");
    Assert(assignment.EfficiencyRate == 1d, "Expected primary efficiency 1.0.");
    Assert(assignment.TimeMultiplier == 1d, "Expected primary time multiplier 1.0.");
    Assert(gene.EndDate == new DateTime(2026, 1, 5), "Primary equipment should preserve base duration.");
}

static void TestSubstituteWhenPrimaryUnavailable()
{
    var input = CreateInput(primaryAvailable: false, includeSubstitution: true);
    var gene = new PopulationGenerator().GenerateGene(10, input);

    var assignment = AssertSingleAssignment(gene);
    Assert(assignment.IsSubstitute, "Expected substitute equipment.");
    Assert(assignment.RequiredEquipmentTypeId == 1, "Expected required type 1.");
    Assert(assignment.AllocatedEquipmentTypeId == 2, "Expected substitute type 2.");
    Assert(Math.Abs(assignment.EfficiencyRate - 0.8d) < 0.0001d, "Expected substitution efficiency 0.8.");
    Assert(Math.Abs(assignment.TimeMultiplier - 1.25d) < 0.0001d, "Expected substitution time multiplier 1.25.");
    Assert(gene.EndDate == new DateTime(2026, 1, 6), "Expected duration to be increased by time multiplier.");
}

static void TestNoSubstituteLeavesShortage()
{
    var input = CreateInput(primaryAvailable: false, includeSubstitution: false);
    var gene = new PopulationGenerator().GenerateGene(10, input);

    Assert(gene.EquipmentAssignments.Count == 0, "Expected no equipment assignment when primary is unavailable and no substitute exists.");
}

static void TestSubstitutionDeadlineViolation()
{
    var input = CreateInput(primaryAvailable: false, includeSubstitution: true, deadline: new DateTime(2026, 1, 5));
    var gene = new PopulationGenerator().GenerateGene(10, input);
    var chromosome = new AllocationChromosome { Genes = [gene] };

    var result = CreateFitnessCalculator().Evaluate(chromosome, input);

    Assert(result.ConstraintReport.DeadlineConflicts.Any(), "Expected adjusted substitute duration to create a deadline conflict.");
}

static void TestSubstitutionResourceOverlap()
{
    var input = CreateInput(primaryAvailable: false, includeSubstitution: true);
    input.ExistingEquipmentAllocations = new[]
    {
        new AllocationEquipmentDetail
        {
            AllocationEquipmentDetailId = 100,
            AllocationPlanId = 1,
            AllocatedEquipmentTypeId = 2,
            EquipmentInstanceId = 2,
            Quantity = 1,
            IsSubstitute = true,
            EfficiencyRate = 0.8d,
            StartDate = new DateTime(2026, 1, 5),
            EndDate = new DateTime(2026, 1, 7),
            Status = "Reserved"
        }
    };

    var gene = new PopulationGenerator().GenerateGene(10, input);
    var chromosome = new AllocationChromosome { Genes = [gene] };
    var result = CreateFitnessCalculator().Evaluate(chromosome, input);

    Assert(result.ConstraintReport.EquipmentConflicts.Any(c => c.Contains("overlaps", StringComparison.OrdinalIgnoreCase)),
        "Expected existing equipment overlap to be detected after substitute duration adjustment.");
}

static void TestPrimaryPreferred()
{
    var input = CreateInput(primaryAvailable: true, includeSubstitution: true);
    var primary = new AllocationChromosome
    {
        Genes =
        [
            new AllocationGene
            {
                PhaseId = 10,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 5),
                EquipmentAssignments =
                [
                    new EquipmentAssignmentGene
                    {
                        PhaseEquipmentRequirementId = 20,
                        RequiredEquipmentTypeId = 1,
                        AllocatedEquipmentTypeId = 1,
                        EquipmentInstanceId = 1,
                        IsSubstitute = false,
                        EfficiencyRate = 1d,
                        TimeMultiplier = 1d
                    }
                ]
            }
        ]
    };

    var substitute = new AllocationChromosome
    {
        Genes =
        [
            new AllocationGene
            {
                PhaseId = 10,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 6),
                EquipmentAssignments =
                [
                    new EquipmentAssignmentGene
                    {
                        PhaseEquipmentRequirementId = 20,
                        RequiredEquipmentTypeId = 1,
                        AllocatedEquipmentTypeId = 2,
                        EquipmentInstanceId = 2,
                        IsSubstitute = true,
                        EfficiencyRate = 0.8d,
                        TimeMultiplier = 1.25d
                    }
                ]
            }
        ]
    };

    var calculator = CreateFitnessCalculator();
    var primaryScore = calculator.Evaluate(primary, input).FitnessScore;
    var substituteScore = calculator.Evaluate(substitute, input).FitnessScore;

    Assert(primaryScore > substituteScore, $"Expected primary score > substitute score, got {primaryScore} <= {substituteScore}.");
}

static OptimizationInput CreateInput(
    bool primaryAvailable,
    bool includeSubstitution,
    DateTime? deadline = null)
{
    var primaryType = new EquipmentType
    {
        EquipmentTypeId = 1,
        EquipmentCategoryId = 1,
        Name = "Primary",
        TrackingType = "Individual",
        BaseMaintenanceIntervalHours = 100,
        TotalQuantity = 1,
        AvailableQuantity = 1
    };

    var substituteType = new EquipmentType
    {
        EquipmentTypeId = 2,
        EquipmentCategoryId = 1,
        Name = "Substitute",
        TrackingType = "Individual",
        BaseMaintenanceIntervalHours = 100,
        TotalQuantity = 1,
        AvailableQuantity = 1
    };

    return new OptimizationInput
    {
        Experiment = new Experiment
        {
            ExperimentId = 1,
            ExperimentName = "Experiment",
            ResearcherId = 1,
            ExpectStartDate = new DateTime(2026, 1, 1),
            ExpectEndDate = new DateTime(2026, 1, 5),
            Deadline = deadline,
            Priority = 2,
            Status = "Draft"
        },
        ExperimentPhases =
        [
            new ExperimentPhase
            {
                PhaseId = 10,
                ExperimentId = 1,
                PhaseName = "Phase 1",
                PhaseOrder = 1,
                ExpectedStartDate = new DateTime(2026, 1, 1),
                ExpectedEndDate = new DateTime(2026, 1, 5),
                Status = "Planned"
            }
        ],
        PhaseEquipmentRequirements =
        [
            new PhaseEquipmentRequirement
            {
                PhaseEquipmentReqId = 20,
                PhaseId = 10,
                EquipmentTypeId = 1,
                Quantity = 1,
                EquipmentType = primaryType
            }
        ],
        EquipmentInstances =
        [
            new EquipmentInstance
            {
                EquipmentInstanceId = 1,
                EquipmentTypeId = 1,
                EquipmentType = primaryType,
                AssetCode = "EQ-A",
                Status = primaryAvailable ? "Available" : "Maintenance",
                ConditionLevel = "Good",
                EffectiveIntervalHour = 100,
                UsageHoursSinceLastMaintenance = 0,
                MaintenanceCount = 0
            },
            new EquipmentInstance
            {
                EquipmentInstanceId = 2,
                EquipmentTypeId = 2,
                EquipmentType = substituteType,
                AssetCode = "EQ-B",
                Status = "Available",
                ConditionLevel = "Good",
                EffectiveIntervalHour = 100,
                UsageHoursSinceLastMaintenance = 0,
                MaintenanceCount = 0
            }
        ],
        EquipmentSubstitutions = includeSubstitution
            ? new[]
            {
                new EquipmentSubstitution
                {
                    EquipmentSubId = 1,
                    PrimaryEquipmentTypeId = 1,
                    SubEquipmentTypeId = 2,
                    EfficiencyRate = 0.8d,
                    TimeMultiplier = 1.25d,
                    PrimaryEquipmentType = primaryType,
                    SubEquipmentType = substituteType
                }
            }
            : Array.Empty<EquipmentSubstitution>(),
        Settings = new OptimizationSettings
        {
            PopulationSize = 20,
            GenerationCount = 1,
            MaxScheduleShiftDays = 0
        }
    };
}

static FitnessCalculator CreateFitnessCalculator()
{
    return new FitnessCalculator(new IConstraintEvaluator[]
    {
        new LandConstraintEvaluator(),
        new HumanConstraintEvaluator(),
        new EquipmentConstraintEvaluator(),
        new MaintenanceConstraintEvaluator(),
        new ScheduleConstraintEvaluator()
    });
}

static EquipmentAssignmentGene AssertSingleAssignment(AllocationGene gene)
{
    Assert(gene.EquipmentAssignments.Count == 1, $"Expected exactly one equipment assignment, got {gene.EquipmentAssignments.Count}.");
    return gene.EquipmentAssignments[0];
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

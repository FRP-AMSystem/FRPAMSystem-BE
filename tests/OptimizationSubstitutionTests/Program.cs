using FRPAMSystem.BusinessTier.AI.DTO;
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

var tests = new (string Name, Action Test)[]
{
    ("1. Valid manual plan -> high fitness", TestValidManualPlanHighFitness),
    ("2. Land conflict -> penalty", TestLandConflictPenalty),
    ("3. Human conflict -> penalty", TestHumanConflictPenalty),
    ("4. Equipment conflict -> penalty", TestEquipmentConflictPenalty),
    ("5. Maintenance conflict -> penalty", TestMaintenanceConflictPenalty),
    ("6. Schedule conflict -> penalty", TestScheduleConflictPenalty),
    ("7. Equipment substitution in manual plan", TestEquipmentSubstitutionManualPlan),
    ("8. Update plan -> fitness recalculated", TestUpdatePlanFitnessRecalculation),
    ("9. Existing AI suggestion generation still works", TestExistingAISuggestionGeneration),
    ("Substitution generator: Primary equipment available uses primary assignment", TestPrimaryEquipmentAvailable),
    ("Substitution generator: Primary unavailable uses valid substitute", TestSubstituteWhenPrimaryUnavailable),
    ("Substitution generator: Primary unavailable and no substitute keeps existing shortage behavior", TestNoSubstituteLeavesShortage),
    ("Substitution generator: Substitution duration can create deadline violation", TestSubstitutionDeadlineViolation),
    ("Substitution generator: Substitution duration can create resource overlap", TestSubstitutionResourceOverlap),
    ("Substitution generator: Primary is preferred over lower-efficiency longer substitute", TestPrimaryPreferred)
};

var passed = 0;
foreach (var test in tests)
{
    try
    {
        test.Test();
        Console.WriteLine($"[PASS] {test.Name}");
        passed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[FAIL] {test.Name}: {ex.Message}");
        throw;
    }
}

Console.WriteLine($"\nAll {passed} tests passed successfully!");

// ==========================================
// TASK 8 TESTS
// ==========================================

static void TestValidManualPlanHighFitness()
{
    var input = CreateComprehensiveOptimizationInput();
    var plan = CreateValidManualPlan();

    var mapper = new AllocationPlanChromosomeMapper();
    var chromosome = mapper.MapToChromosome(plan, input);

    var calculator = CreateFitnessCalculator();
    var result = calculator.Evaluate(chromosome, input);

    Assert(result.FitnessScore >= 70d, $"Expected high fitness score >= 70, got {result.FitnessScore}");
    Assert(result.ConflictCount == 0, $"Expected 0 conflicts for valid plan, got {result.ConflictCount}");
    Assert(result.PenaltyScore == 0d, $"Expected 0 penalty, got {result.PenaltyScore}");
}

static void TestLandConflictPenalty()
{
    var input = CreateComprehensiveOptimizationInput();
    // Add existing land allocation that overlaps
    input.ExistingLandAllocations = new[]
    {
        new AllocationLandDetail
        {
            AllocationLandDetailId = 999,
            AllocationPlanId = 999,
            LandId = 1,
            ExpLandReqId = 1,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 10),
            Status = "Reserved"
        }
    };

    var plan = CreateValidManualPlan();
    var mapper = new AllocationPlanChromosomeMapper();
    var chromosome = mapper.MapToChromosome(plan, input);

    var calculator = CreateFitnessCalculator();
    var result = calculator.Evaluate(chromosome, input);

    Assert(result.ConflictCount > 0, "Expected land conflict to be detected.");
    Assert(result.PenaltyScore < 0d, "Expected penalty score for land conflict.");
    Assert(result.ConstraintReport.LandConflicts.Count > 0, "Expected LandConflicts report to contain violation.");
}

static void TestHumanConflictPenalty()
{
    var input = CreateComprehensiveOptimizationInput();
    // Make assigned human resource unavailable
    var human = input.HumanResources.First(h => h.HumanResourceId == 1);
    human.Status = "Inactive";

    var plan = CreateValidManualPlan();
    var mapper = new AllocationPlanChromosomeMapper();
    var chromosome = mapper.MapToChromosome(plan, input);

    var calculator = CreateFitnessCalculator();
    var result = calculator.Evaluate(chromosome, input);

    Assert(result.ConflictCount > 0, "Expected human resource unavailability conflict.");
    Assert(result.PenaltyScore < 0d, "Expected penalty score for human conflict.");
    Assert(result.ConstraintReport.HumanConflicts.Count > 0, "Expected HumanConflicts report to contain violation.");
}

static void TestEquipmentConflictPenalty()
{
    var input = CreateComprehensiveOptimizationInput();
    // Add existing equipment allocation overlapping with this plan
    input.ExistingEquipmentAllocations = new[]
    {
        new AllocationEquipmentDetail
        {
            AllocationEquipmentDetailId = 999,
            AllocationPlanId = 999,
            AllocatedEquipmentTypeId = 1,
            EquipmentInstanceId = 1,
            Quantity = 1,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 10),
            Status = "Reserved"
        }
    };

    var plan = CreateValidManualPlan();
    var mapper = new AllocationPlanChromosomeMapper();
    var chromosome = mapper.MapToChromosome(plan, input);

    var calculator = CreateFitnessCalculator();
    var result = calculator.Evaluate(chromosome, input);

    Assert(result.ConflictCount > 0, "Expected equipment overlap conflict.");
    Assert(result.PenaltyScore < 0d, "Expected penalty score for equipment overlap.");
    Assert(result.ConstraintReport.EquipmentConflicts.Count > 0, "Expected EquipmentConflicts report to contain violation.");
}

static void TestMaintenanceConflictPenalty()
{
    var input = CreateComprehensiveOptimizationInput();
    // Mark equipment instance as due for maintenance
    var eqInstance = input.EquipmentInstances.First(e => e.EquipmentInstanceId == 1);
    eqInstance.UsageHoursSinceLastMaintenance = eqInstance.EffectiveIntervalHour ?? 100; // exhausted hours

    var plan = CreateValidManualPlan();
    var mapper = new AllocationPlanChromosomeMapper();
    var chromosome = mapper.MapToChromosome(plan, input);

    var calculator = CreateFitnessCalculator();
    var result = calculator.Evaluate(chromosome, input);

    Assert(result.ConstraintReport.MaintenanceConflicts.Count > 0, "Expected maintenance conflict when hours exhausted.");
    Assert(result.PenaltyScore < 0d, "Expected penalty for maintenance conflict.");
}

static void TestScheduleConflictPenalty()
{
    var input = CreateComprehensiveOptimizationInput();
    var plan = CreateValidManualPlan();
    // Invert phase dates so phase 2 starts before phase 1 ends
    var p2Schedule = plan.Schedules.First(s => s.PhaseId == 2);
    p2Schedule.StartDate = new DateTime(2026, 1, 2);
    p2Schedule.EndDate = new DateTime(2026, 1, 4);

    var mapper = new AllocationPlanChromosomeMapper();
    var chromosome = mapper.MapToChromosome(plan, input);

    var calculator = CreateFitnessCalculator();
    var result = calculator.Evaluate(chromosome, input);

    Assert(result.ConflictCount > 0, "Expected schedule order overlap conflict.");
    Assert(result.ConstraintReport.ScheduleConflicts.Count > 0, "Expected ScheduleConflicts report to contain violation.");
}

static void TestEquipmentSubstitutionManualPlan()
{
    var input = CreateComprehensiveOptimizationInput();
    var plan = CreateValidManualPlan();

    // Replace primary equipment with substitute equipment
    var eqDetail = plan.AllocationEquipmentDetails.First();
    eqDetail.AllocatedEquipmentTypeId = 2;
    eqDetail.EquipmentInstanceId = 2;
    eqDetail.IsSubstitute = true;
    eqDetail.EfficiencyRate = 0.8d;

    var mapper = new AllocationPlanChromosomeMapper();
    var chromosome = mapper.MapToChromosome(plan, input);

    Assert(chromosome.Genes[0].EquipmentAssignments.Count == 1, "Expected 1 equipment assignment.");
    var assignment = chromosome.Genes[0].EquipmentAssignments[0];
    Assert(assignment.IsSubstitute, "Expected assignment to be marked substitute.");
    Assert(assignment.AllocatedEquipmentTypeId == 2, "Expected allocated type 2.");
    Assert(Math.Abs(assignment.TimeMultiplier - 1.25d) < 0.001d, "Expected time multiplier 1.25.");

    var calculator = CreateFitnessCalculator();
    var result = calculator.Evaluate(chromosome, input);

    Assert(result.FitnessScore > 0, "Expected positive fitness score with valid substitute.");
}

static void TestUpdatePlanFitnessRecalculation()
{
    var input = CreateComprehensiveOptimizationInput();
    var mapper = new AllocationPlanChromosomeMapper();
    var calculator = CreateFitnessCalculator();

    // Initial state: plan has unavailable land -> low fitness
    var initialPlan = CreateValidManualPlan();
    initialPlan.AllocationLandDetails.First().LandId = 999; // unknown land

    var chromosome1 = mapper.MapToChromosome(initialPlan, input);
    var initialResult = calculator.Evaluate(chromosome1, input);
    Assert(initialResult.ConflictCount > 0, "Initial plan should have conflicts.");

    // Update plan: fix land ID to valid land
    initialPlan.AllocationLandDetails.First().LandId = 1;
    var chromosome2 = mapper.MapToChromosome(initialPlan, input);
    var updatedResult = calculator.Evaluate(chromosome2, input);

    Assert(updatedResult.ConflictCount == 0, "Updated plan should have 0 conflicts.");
    Assert(updatedResult.FitnessScore > initialResult.FitnessScore, "Updated fitness score should be higher than initial.");
}

static void TestExistingAISuggestionGeneration()
{
    var input = CreateComprehensiveOptimizationInput();
    var popGen = new PopulationGenerator();
    var calc = CreateFitnessCalculator();
    var sel = new TournamentSelectionOperator();
    var cross = new SinglePointCrossoverOperator();
    var mut = new AdaptiveMutationOperator(popGen);

    var gaService = new GeneticAlgorithmService(popGen, calc, sel, cross, mut);
    var suggestions = gaService.GenerateSuggestions(input);

    Assert(suggestions.Count > 0, "Expected GA to generate suggestions.");
    Assert(suggestions[0].FitnessScore > 0, "Expected top suggestion to have positive fitness score.");
    Assert(suggestions[0].Timeline.Count > 0, "Expected timeline in suggestion.");
    Assert(suggestions[0].AllocatedLands.Count > 0, "Expected allocated land in suggestion.");
    Assert(suggestions[0].AllocatedHumans.Count > 0, "Expected allocated human in suggestion.");
    Assert(suggestions[0].AllocatedEquipment.Count > 0, "Expected allocated equipment in suggestion.");
}

// ==========================================
// EXISTING SUBSTITUTION TESTS
// ==========================================

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

// ==========================================
// TEST DATA HELPERS
// ==========================================

static OptimizationInput CreateComprehensiveOptimizationInput()
{
    var primaryType = new EquipmentType
    {
        EquipmentTypeId = 1,
        EquipmentCategoryId = 1,
        Name = "Tractor",
        TrackingType = "Individual",
        BaseMaintenanceIntervalHours = 100,
        TotalQuantity = 2,
        AvailableQuantity = 2
    };

    var subType = new EquipmentType
    {
        EquipmentTypeId = 2,
        EquipmentCategoryId = 1,
        Name = "Mini Tractor",
        TrackingType = "Individual",
        BaseMaintenanceIntervalHours = 100,
        TotalQuantity = 2,
        AvailableQuantity = 2
    };

    var skill1 = new Skill { SkillId = 1, SkillName = "Forestry Fieldwork" };

    var user1 = new User { UserId = 1, FullName = "Researcher Alice", RoleId = 1 };
    var user2 = new User { UserId = 2, FullName = "Technician Bob", RoleId = 1 };

    var human1 = new HumanResourceProfile
    {
        HumanResourceId = 1,
        UserId = 1,
        User = user1,
        Status = "Available",
        MaxWorkingHoursPerDay = 8,
        CurrentWorkload = 0,
        HumanResourceSkills = [new HumanResourceSkill { HumanResourceId = 1, SkillId = 1, Skill = skill1 }]
    };

    var human2 = new HumanResourceProfile
    {
        HumanResourceId = 2,
        UserId = 2,
        User = user2,
        Status = "Available",
        MaxWorkingHoursPerDay = 8,
        CurrentWorkload = 0,
        HumanResourceSkills = [new HumanResourceSkill { HumanResourceId = 2, SkillId = 1, Skill = skill1 }]
    };

    var land1 = new LandResource
    {
        LandId = 1,
        LandCode = "LAND-001",
        SoilType = "Clay Loam",
        AreaSize = 1000m,
        Status = "Available"
    };

    var eqInstance1 = new EquipmentInstance
    {
        EquipmentInstanceId = 1,
        EquipmentTypeId = 1,
        EquipmentType = primaryType,
        AssetCode = "EQ-001",
        Status = "Available",
        ConditionLevel = "Good",
        EffectiveIntervalHour = 100,
        UsageHoursSinceLastMaintenance = 10,
        MaintenanceCount = 0
    };

    var eqInstance2 = new EquipmentInstance
    {
        EquipmentInstanceId = 2,
        EquipmentTypeId = 2,
        EquipmentType = subType,
        AssetCode = "EQ-002",
        Status = "Available",
        ConditionLevel = "Good",
        EffectiveIntervalHour = 100,
        UsageHoursSinceLastMaintenance = 10,
        MaintenanceCount = 0
    };

    var exp = new Experiment
    {
        ExperimentId = 100,
        ExperimentName = "Pine Growth Optimization",
        ResearcherId = 1,
        ExpectStartDate = new DateTime(2026, 1, 1),
        ExpectEndDate = new DateTime(2026, 1, 10),
        Deadline = new DateTime(2026, 1, 15),
        Priority = 1,
        Status = "Approved"
    };

    var phase1 = new ExperimentPhase
    {
        PhaseId = 1,
        ExperimentId = 100,
        PhaseName = "Soil Preparation",
        PhaseOrder = 1,
        ExpectedStartDate = new DateTime(2026, 1, 1),
        ExpectedEndDate = new DateTime(2026, 1, 5),
        Status = "Planned"
    };

    var phase2 = new ExperimentPhase
    {
        PhaseId = 2,
        ExperimentId = 100,
        PhaseName = "Planting",
        PhaseOrder = 2,
        ExpectedStartDate = new DateTime(2026, 1, 6),
        ExpectedEndDate = new DateTime(2026, 1, 10),
        Status = "Planned"
    };

    var landReq = new ExperimentLandRequirement
    {
        ExpLandReqId = 1,
        ExperimentId = 100,
        RequiredArea = 800m,
        RequiredSoilType = "Clay Loam"
    };

    var humanReq1 = new PhaseHumanRequirement
    {
        PhaseHumanReqId = 1,
        PhaseId = 1,
        RoleId = 1,
        RequiredSkillId = 1,
        Quantity = 1
    };

    var humanReq2 = new PhaseHumanRequirement
    {
        PhaseHumanReqId = 2,
        PhaseId = 2,
        RoleId = 1,
        RequiredSkillId = 1,
        Quantity = 1
    };

    var eqReq1 = new PhaseEquipmentRequirement
    {
        PhaseEquipmentReqId = 1,
        PhaseId = 1,
        EquipmentTypeId = 1,
        Quantity = 1,
        EquipmentType = primaryType
    };

    var eqReq2 = new PhaseEquipmentRequirement
    {
        PhaseEquipmentReqId = 2,
        PhaseId = 2,
        EquipmentTypeId = 1,
        Quantity = 1,
        EquipmentType = primaryType
    };

    return new OptimizationInput
    {
        Experiment = exp,
        ExperimentPhases = [phase1, phase2],
        LandResources = [land1],
        HumanResources = [human1, human2],
        EquipmentInstances = [eqInstance1, eqInstance2],
        Skills = [skill1],
        ExperimentLandRequirements = [landReq],
        PhaseHumanRequirements = [humanReq1, humanReq2],
        PhaseEquipmentRequirements = [eqReq1, eqReq2],
        EquipmentSubstitutions =
        [
            new EquipmentSubstitution
            {
                EquipmentSubId = 1,
                PrimaryEquipmentTypeId = 1,
                SubEquipmentTypeId = 2,
                EfficiencyRate = 0.8d,
                TimeMultiplier = 1.25d,
                PrimaryEquipmentType = primaryType,
                SubEquipmentType = subType
            }
        ],
        Settings = new OptimizationSettings
        {
            PopulationSize = 10,
            GenerationCount = 2,
            MaxScheduleShiftDays = 0
        }
    };
}

static AllocationPlan CreateValidManualPlan()
{
    var plan = new AllocationPlan
    {
        AllocationPlanId = 1,
        ExperimentId = 100,
        ApproveStatus = "Draft",
        CreatedAt = DateTime.Now
    };

    plan.AllocationLandDetails = new List<AllocationLandDetail>
    {
        new AllocationLandDetail
        {
            AllocationLandDetailId = 1,
            AllocationPlanId = 1,
            LandId = 1,
            ExpLandReqId = 1,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 10),
            Status = "Allocated"
        }
    };

    plan.AllocationHumanDetails = new List<AllocationHumanDetail>
    {
        new AllocationHumanDetail
        {
            AllocationHumanDetailId = 1,
            AllocationPlanId = 1,
            PhaseHumanReqId = 1,
            HumanResourceId = 1,
            WorkingHours = 8,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 5),
            Status = "Allocated"
        },
        new AllocationHumanDetail
        {
            AllocationHumanDetailId = 2,
            AllocationPlanId = 1,
            PhaseHumanReqId = 2,
            HumanResourceId = 2,
            WorkingHours = 8,
            StartDate = new DateTime(2026, 1, 6),
            EndDate = new DateTime(2026, 1, 10),
            Status = "Allocated"
        }
    };

    plan.AllocationEquipmentDetails = new List<AllocationEquipmentDetail>
    {
        new AllocationEquipmentDetail
        {
            AllocationEquipmentDetailId = 1,
            AllocationPlanId = 1,
            PhaseEquipmentReqId = 1,
            AllocatedEquipmentTypeId = 1,
            EquipmentInstanceId = 1,
            Quantity = 1,
            IsSubstitute = false,
            EfficiencyRate = 1.0d,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 5),
            Status = "Allocated"
        },
        new AllocationEquipmentDetail
        {
            AllocationEquipmentDetailId = 2,
            AllocationPlanId = 1,
            PhaseEquipmentReqId = 2,
            AllocatedEquipmentTypeId = 1,
            EquipmentInstanceId = 1,
            Quantity = 1,
            IsSubstitute = false,
            EfficiencyRate = 1.0d,
            StartDate = new DateTime(2026, 1, 6),
            EndDate = new DateTime(2026, 1, 10),
            Status = "Allocated"
        }
    };

    plan.Schedules = new List<Schedule>
    {
        new Schedule
        {
            ScheduleId = 1,
            AllocationPlanId = 1,
            PhaseId = 1,
            AssignedHumanResourceId = 1,
            Title = "Phase 1 Execution",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 5),
            Status = "Scheduled",
            Priority = 1
        },
        new Schedule
        {
            ScheduleId = 2,
            AllocationPlanId = 1,
            PhaseId = 2,
            AssignedHumanResourceId = 2,
            Title = "Phase 2 Execution",
            StartDate = new DateTime(2026, 1, 6),
            EndDate = new DateTime(2026, 1, 10),
            Status = "Scheduled",
            Priority = 1
        }
    };

    return plan;
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

using FRPAMSystem.BusinessTier.AI.Fitness;
using FRPAMSystem.BusinessTier.AI.Fitness.Evaluators;
using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.DataTier.Models;
using Xunit;

namespace FRPAMSystem.NotificationTests.Services
{
    public class FitnessExplainabilityTests
    {
        private static IFitnessCalculator CreateFitnessCalculator()
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

        private static OptimizationInput CreateBaseInput()
        {
            var experiment = new Experiment
            {
                ExperimentId = 1,
                ExperimentName = "Explainability Test Experiment",
                ExpectStartDate = new DateTime(2026, 6, 1),
                ExpectEndDate = new DateTime(2026, 6, 10),
                Deadline = new DateTime(2026, 6, 15)
            };

            var phase1 = new ExperimentPhase
            {
                PhaseId = 1,
                ExperimentId = 1,
                PhaseName = "Soil Prep",
                PhaseOrder = 1,
                ExpectedStartDate = new DateTime(2026, 6, 1),
                ExpectedEndDate = new DateTime(2026, 6, 5)
            };

            var land = new LandResource
            {
                LandId = 10,
                LandCode = "LND-01",
                SoilType = "Loam",
                AreaSize = 100m,
                Status = "Available"
            };

            var landReq = new ExperimentLandRequirement
            {
                ExpLandReqId = 100,
                ExperimentId = 1,
                RequiredSoilType = "Loam",
                RequiredArea = 100m
            };

            var researcher = new HumanResourceProfile
            {
                HumanResourceId = 20,
                Status = "Available",
                CurrentWorkload = 0d,
                MaxWorkingHoursPerDay = 8d,
                User = new User { UserId = 20, FullName = "Dr. Alice", RoleId = 2 },
                HumanResourceSkills = new List<HumanResourceSkill>
                {
                    new HumanResourceSkill { HumanResourceId = 20, SkillId = 5 }
                }
            };

            var humanReq = new PhaseHumanRequirement
            {
                PhaseHumanReqId = 200,
                PhaseId = 1,
                RoleId = 2,
                RequiredSkillId = 5,
                Quantity = 1
            };

            var eqType = new EquipmentType
            {
                EquipmentTypeId = 6,
                Name = "Tractor Primary",
                BaseMaintenanceIntervalHours = 100d
            };

            var eqInstance = new EquipmentInstance
            {
                EquipmentInstanceId = 30,
                AssetCode = "TRC-01",
                EquipmentTypeId = 6,
                EquipmentType = eqType,
                Status = "Available",
                ConditionLevel = "Good",
                UsageHoursSinceLastMaintenance = 10d
            };

            var eqReq = new PhaseEquipmentRequirement
            {
                PhaseEquipmentReqId = 300,
                PhaseId = 1,
                EquipmentTypeId = 6,
                Quantity = 1
            };

            return new OptimizationInput
            {
                Experiment = experiment,
                ExperimentPhases = new[] { phase1 },
                LandResources = new[] { land },
                HumanResources = new[] { researcher },
                EquipmentInstances = new[] { eqInstance },
                ExperimentLandRequirements = new[] { landReq },
                PhaseHumanRequirements = new[] { humanReq },
                PhaseEquipmentRequirements = new[] { eqReq },
                Settings = new OptimizationSettings()
            };
        }

        [Fact]
        public void Test01_PerfectFeasibleAllocation()
        {
            var input = CreateBaseInput();
            var calculator = CreateFitnessCalculator();

            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        ExperimentLandRequirementId = 100,
                        AssignedHumanResourceIds = new List<int> { 20 },
                        EquipmentAssignments = new List<EquipmentAssignmentGene>
                        {
                            new EquipmentAssignmentGene
                            {
                                PhaseEquipmentRequirementId = 300,
                                RequiredEquipmentTypeId = 6,
                                AllocatedEquipmentTypeId = 6,
                                EquipmentInstanceId = 30,
                                IsSubstitute = false,
                                EfficiencyRate = 1.0d,
                                TimeMultiplier = 1.0d
                            }
                        }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);

            Assert.True(result.IsFeasible);
            Assert.Equal(0, result.HardViolationCount);
            Assert.True(result.FitnessScore > 80d);
            Assert.NotEmpty(result.Breakdown.OverallCalculation);
            Assert.NotEmpty(result.Breakdown.Land.Calculation);
            Assert.NotEmpty(result.Breakdown.Human.Calculation);
            Assert.NotEmpty(result.Breakdown.Equipment.Calculation);
            Assert.NotEmpty(result.Breakdown.Schedule.Calculation);
        }

        [Fact]
        public void Test02_LandMismatch_SoilMismatch()
        {
            var input = CreateBaseInput();
            input.LandResources.First().SoilType = "Clay"; // Mismatch vs required Loam

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            var soilMismatchAdj = result.Breakdown.Land.Adjustments.FirstOrDefault(a => a.Factor == "Soil Mismatch");
            Assert.NotNull(soilMismatchAdj);
            Assert.Equal(-15d, soilMismatchAdj.Points);
        }

        [Fact]
        public void Test03_InsufficientLandArea()
        {
            var input = CreateBaseInput();
            input.LandResources.First().AreaSize = 50m; // Required is 100m

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            Assert.False(result.IsFeasible);
            var areaDefAdj = result.Breakdown.Land.Adjustments.FirstOrDefault(a => a.Factor == "Area Deficiency");
            Assert.NotNull(areaDefAdj);
            Assert.True(areaDefAdj.Points < 0);
        }

        [Fact]
        public void Test04_HumanShortage()
        {
            var input = CreateBaseInput();
            var calculator = CreateFitnessCalculator();

            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int>() // Empty staff
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            var shortageAdj = result.Breakdown.Human.Adjustments.FirstOrDefault(a => a.Factor == "Staff Quantity Shortage");
            Assert.NotNull(shortageAdj);
        }

        [Fact]
        public void Test05_HumanRoleMismatch()
        {
            var input = CreateBaseInput();
            input.HumanResources.First().User!.RoleId = 99; // Required is 2

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            var roleMismatchAdj = result.Breakdown.Human.Adjustments.FirstOrDefault(a => a.Factor == "Role Mismatch");
            Assert.NotNull(roleMismatchAdj);
            Assert.Contains("role 2", result.ConstraintReport.RoleConflicts.FirstOrDefault() ?? "");
        }

        [Fact]
        public void Test06_HumanSkillMismatch()
        {
            var input = CreateBaseInput();
            input.HumanResources.First().HumanResourceSkills.Clear(); // No matching skill

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            var skillMismatchAdj = result.Breakdown.Human.Adjustments.FirstOrDefault(a => a.Factor == "Skill Mismatch");
            Assert.NotNull(skillMismatchAdj);
        }

        [Fact]
        public void Test07_EquipmentShortage()
        {
            var input = CreateBaseInput();
            var calculator = CreateFitnessCalculator();

            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 },
                        EquipmentAssignments = new List<EquipmentAssignmentGene>() // 0 equipment
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            var qtyAdj = result.Breakdown.Equipment.Adjustments.FirstOrDefault(a => a.Factor == "Equipment Quantity");
            Assert.NotNull(qtyAdj);
            Assert.Equal(0d, qtyAdj.Points);
        }

        [Fact]
        public void Test08_ValidEquipmentSubstitution()
        {
            var input = CreateBaseInput();
            var subType = new EquipmentType
            {
                EquipmentTypeId = 9,
                Name = "Substitute Mini Tractor",
                BaseMaintenanceIntervalHours = 100d
            };

            var subInstance = new EquipmentInstance
            {
                EquipmentInstanceId = 31,
                AssetCode = "TRC-SUB-01",
                EquipmentTypeId = 9,
                EquipmentType = subType,
                Status = "Available",
                ConditionLevel = "Good",
                UsageHoursSinceLastMaintenance = 10d
            };

            input.EquipmentInstances = new[] { subInstance };
            input.ExperimentEquipmentRequirements = new[]
            {
                new ExperimentEquipmentRequirement
                {
                    ExpEquipmentReqId = 300,
                    ExperimentId = 1,
                    EquipmentTypeId = 6,
                    Quantity = 1,
                    AllowSubstitute = true,
                    MinAcceptableEfficiency = 0.7d
                }
            };

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 6),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 },
                        EquipmentAssignments = new List<EquipmentAssignmentGene>
                        {
                            new EquipmentAssignmentGene
                            {
                                PhaseEquipmentRequirementId = 300,
                                RequiredEquipmentTypeId = 6,
                                AllocatedEquipmentTypeId = 9,
                                EquipmentInstanceId = 31,
                                IsSubstitute = true,
                                EfficiencyRate = 0.8d,
                                TimeMultiplier = 1.25d
                            }
                        }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            Assert.True(result.IsFeasible);
            Assert.Equal(0, result.HardViolationCount);

            var subAdjustment = result.Breakdown.Equipment.Adjustments
                .FirstOrDefault(a => a.Factor == "Equipment Substitution");
            Assert.NotNull(subAdjustment);
            Assert.Equal("Substitution", subAdjustment.Type);
            Assert.Contains("0.8", subAdjustment.Calculation);
            Assert.Contains("1.25", subAdjustment.Reason);
        }

        [Fact]
        public void Test09_InvalidEquipmentSubstitution_BelowMinEfficiency()
        {
            var input = CreateBaseInput();
            var subType = new EquipmentType { EquipmentTypeId = 9, Name = "Weak Tractor" };
            var subInstance = new EquipmentInstance
            {
                EquipmentInstanceId = 31,
                AssetCode = "TRC-WEAK",
                EquipmentTypeId = 9,
                EquipmentType = subType,
                Status = "Available",
                ConditionLevel = "Good",
                UsageHoursSinceLastMaintenance = 10d
            };

            input.EquipmentInstances = new[] { subInstance };
            input.ExperimentEquipmentRequirements = new[]
            {
                new ExperimentEquipmentRequirement
                {
                    ExpEquipmentReqId = 300,
                    ExperimentId = 1,
                    EquipmentTypeId = 6,
                    Quantity = 1,
                    AllowSubstitute = true,
                    MinAcceptableEfficiency = 0.85d // Required 85%
                }
            };

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 6),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 },
                        EquipmentAssignments = new List<EquipmentAssignmentGene>
                        {
                            new EquipmentAssignmentGene
                            {
                                PhaseEquipmentRequirementId = 300,
                                RequiredEquipmentTypeId = 6,
                                AllocatedEquipmentTypeId = 9,
                                EquipmentInstanceId = 31,
                                IsSubstitute = true,
                                EfficiencyRate = 0.6d, // 60% < 85%
                                TimeMultiplier = 1.5d
                            }
                        }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            Assert.False(result.IsFeasible);
            Assert.True(result.HardViolationCount > 0);
        }

        [Fact]
        public void Test10_EquipmentMaintenanceProblem_DueForMaintenance()
        {
            var input = CreateBaseInput();
            input.EquipmentInstances.First().UsageHoursSinceLastMaintenance = 150d; // Exceeds 100h base interval

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 },
                        EquipmentAssignments = new List<EquipmentAssignmentGene>
                        {
                            new EquipmentAssignmentGene
                            {
                                PhaseEquipmentRequirementId = 300,
                                RequiredEquipmentTypeId = 6,
                                AllocatedEquipmentTypeId = 6,
                                EquipmentInstanceId = 30
                            }
                        }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            Assert.False(result.IsFeasible);
            Assert.Contains("due for maintenance", result.ConstraintReport.MaintenanceConflicts.FirstOrDefault() ?? "");
        }

        [Fact]
        public void Test11_ScheduleOverlap_PhasePrecedence()
        {
            var input = CreateBaseInput();
            var phase2 = new ExperimentPhase
            {
                PhaseId = 2,
                ExperimentId = 1,
                PhaseName = "Planting",
                PhaseOrder = 2,
                ExpectedStartDate = new DateTime(2026, 6, 6),
                ExpectedEndDate = new DateTime(2026, 6, 10)
            };
            input.ExperimentPhases = new[] { input.ExperimentPhases.First(), phase2 };

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 8),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 }
                    },
                    new AllocationGene
                    {
                        PhaseId = 2,
                        StartDate = new DateTime(2026, 6, 5), // Starts before Phase 1 ends!
                        EndDate = new DateTime(2026, 6, 10),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int>()
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            Assert.False(result.IsFeasible);
            Assert.Contains(result.ConstraintReport.ScheduleConflicts, s => s.Contains("starts before phase 1 completes"));
        }

        [Fact]
        public void Test12_DeadlineViolation()
        {
            var input = CreateBaseInput();
            input.Experiment.Deadline = new DateTime(2026, 6, 4); // Phase ends June 5, after deadline

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            Assert.True(result.SoftViolationCount > 0);
            Assert.NotEmpty(result.ConstraintReport.DeadlineConflicts);
        }

        [Fact]
        public void Test13_HardViolation_PenaltyCalculation()
        {
            var input = CreateBaseInput();
            input.LandResources.First().Status = "Unavailable";

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            Assert.False(result.IsFeasible);
            Assert.NotEmpty(result.Breakdown.Penalties);
            var hardPenalty = result.Breakdown.Penalties.FirstOrDefault(p => p.Factor.Contains("Hard Constraint"));
            Assert.NotNull(hardPenalty);
            Assert.Equal(-25d, hardPenalty.Points);
        }

        [Fact]
        public void Test14_SoftViolation_ConstraintReport()
        {
            var input = CreateBaseInput();
            input.LandResources.First().AreaSize = 250m; // 250m > 100m, waste area soft warning

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            Assert.True(result.SoftViolationCount > 0);
            Assert.True(result.IsFeasible);
        }

        [Fact]
        public void Test15_ZeroPenaltyConfiguration_Preserved()
        {
            var input = CreateBaseInput();
            input.Settings.HardConstraintPenalty = 0d;
            input.Settings.SoftConstraintPenalty = 0d;
            input.Settings.PenaltyWeight = 0d;
            input.LandResources.First().Status = "Unavailable";

            var calculator = CreateFitnessCalculator();
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);
            Assert.Equal(0d, result.PenaltyScore);
            Assert.Equal(0d, result.Breakdown.PenaltyScore);
        }

        [Fact]
        public void Test16_EquipmentMaintenance_WeightedAggregation()
        {
            var input = CreateBaseInput();
            var calculator = CreateFitnessCalculator();

            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene
                    {
                        PhaseId = 1,
                        StartDate = new DateTime(2026, 6, 1),
                        EndDate = new DateTime(2026, 6, 5),
                        LandId = 10,
                        AssignedHumanResourceIds = new List<int> { 20 },
                        EquipmentAssignments = new List<EquipmentAssignmentGene>
                        {
                            new EquipmentAssignmentGene
                            {
                                PhaseEquipmentRequirementId = 300,
                                RequiredEquipmentTypeId = 6,
                                AllocatedEquipmentTypeId = 6,
                                EquipmentInstanceId = 30,
                                IsSubstitute = false,
                                EfficiencyRate = 1.0d,
                                TimeMultiplier = 1.0d
                            }
                        }
                    }
                }
            };

            var result = calculator.Evaluate(chromosome, input);

            Assert.NotNull(result.Breakdown.Equipment.Calculation);
            Assert.Contains("0.75", result.Breakdown.Equipment.Calculation);
            Assert.Contains("0.25", result.Breakdown.Equipment.Calculation);
        }

        [Fact]
        public void Test17_MultiPhaseChromosome_LandExplanationNotDuplicated()
        {
            var input = CreateBaseInput();
            var phase2 = new ExperimentPhase
            {
                PhaseId = 2,
                ExperimentId = 1,
                PhaseName = "Phase 2",
                PhaseOrder = 2,
                ExpectedStartDate = new DateTime(2026, 6, 6),
                ExpectedEndDate = new DateTime(2026, 6, 10)
            };
            var phase3 = new ExperimentPhase
            {
                PhaseId = 3,
                ExperimentId = 1,
                PhaseName = "Phase 3",
                PhaseOrder = 3,
                ExpectedStartDate = new DateTime(2026, 6, 11),
                ExpectedEndDate = new DateTime(2026, 6, 15)
            };
            input.ExperimentPhases = new[] { input.ExperimentPhases.First(), phase2, phase3 };

            var calculator = CreateFitnessCalculator();

            // 3 phases all using LandId 10
            var chromosome = new AllocationChromosome
            {
                Genes = new List<AllocationGene>
                {
                    new AllocationGene { PhaseId = 1, StartDate = new DateTime(2026, 6, 1), EndDate = new DateTime(2026, 6, 5), LandId = 10, AssignedHumanResourceIds = new List<int> { 20 } },
                    new AllocationGene { PhaseId = 2, StartDate = new DateTime(2026, 6, 6), EndDate = new DateTime(2026, 6, 10), LandId = 10, AssignedHumanResourceIds = new List<int> { 20 } },
                    new AllocationGene { PhaseId = 3, StartDate = new DateTime(2026, 6, 11), EndDate = new DateTime(2026, 6, 15), LandId = 10, AssignedHumanResourceIds = new List<int> { 20 } }
                }
            };

            var result = calculator.Evaluate(chromosome, input);

            // Soil Match, Area Sufficiency, Land Availability should appear EXACTLY ONCE, not 3 times!
            Assert.Single(result.Breakdown.Land.Adjustments, a => a.Factor == "Soil Match");
            Assert.Single(result.Breakdown.Land.Adjustments, a => a.Factor == "Area Sufficiency");
            Assert.Single(result.Breakdown.Land.Adjustments, a => a.Factor == "Land Availability");

            // Mathematical reconciliation: Base 20 + 25 + 30 + 20 = 95
            var landSum = result.Breakdown.Land.BaseScore + result.Breakdown.Land.Adjustments.Sum(a => a.Points);
            Assert.Equal(95d, landSum);
            Assert.Equal(95d, result.LandScore);
        }

        [Fact]
        public void Test18_TopSuggestions_ContainValidCalculationsAndExplanations()
        {
            var input = CreateBaseInput();
            input.Settings.GenerationCount = 2;
            input.Settings.PopulationSize = 6;
            input.Settings.TopSuggestionCount = 5;

            var popGen = new FRPAMSystem.BusinessTier.AI.Generator.PopulationGenerator();
            var gaService = new FRPAMSystem.BusinessTier.AI.Services.GeneticAlgorithmService(
                popGen,
                CreateFitnessCalculator(),
                new FRPAMSystem.BusinessTier.AI.Operators.Selection.TournamentSelectionOperator(),
                new FRPAMSystem.BusinessTier.AI.Operators.Crossover.SinglePointCrossoverOperator(),
                new FRPAMSystem.BusinessTier.AI.Operators.Mutation.AdaptiveMutationOperator(popGen));

            var suggestions = gaService.GenerateSuggestions(input);

            Assert.NotEmpty(suggestions);
            Assert.True(suggestions.Count <= 5);

            foreach (var suggestion in suggestions)
            {
                Assert.NotNull(suggestion.FitnessBreakdown);
                Assert.NotEmpty(suggestion.FitnessBreakdown.OverallCalculation);
                Assert.NotEmpty(suggestion.FitnessBreakdown.Land.Calculation);
                Assert.NotEmpty(suggestion.FitnessBreakdown.Human.Calculation);
                Assert.NotEmpty(suggestion.FitnessBreakdown.Equipment.Calculation);
                Assert.NotEmpty(suggestion.FitnessBreakdown.Schedule.Calculation);
            }
        }
    }
}

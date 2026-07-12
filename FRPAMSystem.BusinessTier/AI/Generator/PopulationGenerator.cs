using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.DataTier.Models;

namespace FRPAMSystem.BusinessTier.AI.Generator
{
    public class PopulationGenerator : IPopulationGenerator
    {
        private readonly Random _random = new();

        public Population Generate(OptimizationInput input)
        {
            input.Settings.Normalize();
            var population = new Population();
            var phases = input.ExperimentPhases.OrderBy(p => p.PhaseOrder).ToList();

            for (var i = 0; i < input.Settings.PopulationSize; i++)
            {
                population.Chromosomes.Add(new AllocationChromosome
                {
                    Genes = phases.Select(p => GenerateGene(p.PhaseId, input)).ToList()
                });
            }

            return population;
        }

        public AllocationGene GenerateGene(int phaseId, OptimizationInput input)
        {
            var phase = input.ExperimentPhases.First(p => p.PhaseId == phaseId);
            var durationDays = Math.Max(1, (phase.ExpectedEndDate.Date - phase.ExpectedStartDate.Date).Days);
            var shift = input.Settings.MaxScheduleShiftDays == 0
                ? 0
                : _random.Next(0, input.Settings.MaxScheduleShiftDays + 1);
            var startDate = phase.ExpectedStartDate.Date.AddDays(shift);
            var endDate = startDate.AddDays(durationDays);

            var gene = new AllocationGene
            {
                PhaseId = phaseId,
                StartDate = startDate,
                EndDate = endDate
            };

            AssignLand(gene, input);
            AssignHumans(gene, input);
            AssignEquipment(gene, input);

            return gene;
        }

        private void AssignLand(AllocationGene gene, OptimizationInput input)
        {
            var requirement = input.ExperimentLandRequirements
                .OrderByDescending(r => r.RequiredArea)
                .FirstOrDefault();

            gene.ExperimentLandRequirementId = requirement?.ExpLandReqId;

            var candidates = input.LandResources
                .Where(l => IsAvailableStatus(l.Status))
                .Select(l => new
                {
                    Land = l,
                    Score = ScoreLand(l, requirement, gene.StartDate, gene.EndDate, input.ExistingLandAllocations)
                })
                .OrderByDescending(x => x.Score)
                .Take(8)
                .ToList();

            gene.LandId = Pick(candidates)?.Land.LandId;
        }

        private void AssignHumans(AllocationGene gene, OptimizationInput input)
        {
            var requirements = GetHumanRequirements(gene.PhaseId, input).ToList();
            var assigned = new HashSet<int>();

            foreach (var requirement in requirements)
            {
                var candidates = input.HumanResources
                    .Where(h => !assigned.Contains(h.HumanResourceId))
                    .Where(h => IsAvailableStatus(h.Status))
                    .Select(h => new
                    {
                        Human = h,
                        Score = ScoreHuman(h, requirement, gene.StartDate, gene.EndDate, input)
                    })
                    .OrderByDescending(x => x.Score)
                    .Take(Math.Max(requirement.Quantity * 3, requirement.Quantity))
                    .ToList();

                foreach (var candidate in candidates.OrderByDescending(c => c.Score).Take(requirement.Quantity))
                {
                    assigned.Add(candidate.Human.HumanResourceId);
                }
            }

            gene.AssignedHumanResourceIds = assigned.ToList();
        }

        private void AssignEquipment(AllocationGene gene, OptimizationInput input)
        {
            var requirements = GetEquipmentRequirements(gene.PhaseId, input).ToList();
            var assignedInstances = new HashSet<int>();

            foreach (var requirement in requirements)
            {
                var candidates = input.EquipmentInstances
                    .Where(e => !assignedInstances.Contains(e.EquipmentInstanceId))
                    .Where(e => IsAvailableStatus(e.Status))
                    .Select(e => new
                    {
                        Equipment = e,
                        Assignment = BuildEquipmentAssignment(e, requirement, input),
                        Score = ScoreEquipment(e, requirement, gene.StartDate, gene.EndDate, input)
                    })
                    .Where(x => x.Assignment is not null)
                    .OrderByDescending(x => x.Score)
                    .Take(Math.Max(requirement.Quantity * 3, requirement.Quantity))
                    .ToList();

                foreach (var candidate in candidates.Take(requirement.Quantity))
                {
                    assignedInstances.Add(candidate.Equipment.EquipmentInstanceId);
                    gene.AssignedEquipmentInstanceIds.Add(candidate.Equipment.EquipmentInstanceId);
                    gene.EquipmentAssignments.Add(candidate.Assignment!);
                }
            }
        }

        private static double ScoreLand(
            LandResource land,
            ExperimentLandRequirement? requirement,
            DateTime startDate,
            DateTime endDate,
            IEnumerable<AllocationLandDetail> existingAllocations)
        {
            var score = 0d;
            if (requirement is null)
            {
                score += 20d;
            }
            else
            {
                if (string.Equals(land.SoilType, requirement.RequiredSoilType, StringComparison.OrdinalIgnoreCase))
                {
                    score += 40d;
                }

                if (land.AreaSize >= requirement.RequiredArea)
                {
                    score += 40d;
                    score += Math.Max(0d, 20d - (double)(land.AreaSize - requirement.RequiredArea));
                }
            }

            if (!existingAllocations.Any(a => a.LandId == land.LandId && Overlaps(startDate, endDate, a.StartDate, a.EndDate)))
            {
                score += 40d;
            }

            return score;
        }

        private static double ScoreHuman(
            HumanResourceProfile human,
            HumanRequirementSnapshot requirement,
            DateTime startDate,
            DateTime endDate,
            OptimizationInput input)
        {
            var score = 0d;
            if (requirement.RequiredSkillId is null ||
                human.HumanResourceSkills.Any(s => s.SkillId == requirement.RequiredSkillId.Value))
            {
                score += 50d;
            }

            if (!input.ExistingHumanAllocations.Any(a => a.HumanResourceId == human.HumanResourceId && Overlaps(startDate, endDate, a.StartDate, a.EndDate)) &&
                !input.ExistingSchedules.Any(s => s.AssignedHumanResourceId == human.HumanResourceId && Overlaps(startDate, endDate, s.StartDate, s.EndDate)))
            {
                score += 35d;
            }

            score += Math.Max(0d, 25d - human.CurrentWorkload);
            return score;
        }

        private static double ScoreEquipment(
            EquipmentInstance equipment,
            EquipmentRequirementSnapshot requirement,
            DateTime startDate,
            DateTime endDate,
            OptimizationInput input)
        {
            var score = 0d;
            if (equipment.EquipmentTypeId == requirement.EquipmentTypeId)
            {
                score += 60d;
            }
            else
            {
                var substitution = input.EquipmentSubstitutions.FirstOrDefault(s =>
                    s.PrimaryEquipmentTypeId == requirement.EquipmentTypeId &&
                    s.SubEquipmentTypeId == equipment.EquipmentTypeId);
                if (substitution is not null)
                {
                    score += 35d * substitution.EfficiencyRate;
                }
            }

            if (!input.ExistingEquipmentAllocations.Any(a =>
                    a.EquipmentInstanceId == equipment.EquipmentInstanceId &&
                    Overlaps(startDate, endDate, a.StartDate, a.EndDate)))
            {
                score += 35d;
            }

            score += equipment.ConditionLevel.Equals("Good", StringComparison.OrdinalIgnoreCase) ? 10d : 0d;
            return score;
        }

        private static EquipmentAssignmentGene? BuildEquipmentAssignment(
            EquipmentInstance equipment,
            EquipmentRequirementSnapshot requirement,
            OptimizationInput input)
        {
            if (equipment.EquipmentTypeId == requirement.EquipmentTypeId)
            {
                return new EquipmentAssignmentGene
                {
                    PhaseEquipmentRequirementId = requirement.PhaseEquipmentRequirementId,
                    ExperimentEquipmentRequirementId = requirement.ExperimentEquipmentRequirementId,
                    RequiredEquipmentTypeId = requirement.EquipmentTypeId,
                    AllocatedEquipmentTypeId = equipment.EquipmentTypeId,
                    EquipmentInstanceId = equipment.EquipmentInstanceId,
                    IsSubstitute = false,
                    EfficiencyRate = 1d
                };
            }

            var substitution = input.EquipmentSubstitutions.FirstOrDefault(s =>
                s.PrimaryEquipmentTypeId == requirement.EquipmentTypeId &&
                s.SubEquipmentTypeId == equipment.EquipmentTypeId);

            if (substitution is null)
            {
                return null;
            }

            return new EquipmentAssignmentGene
            {
                PhaseEquipmentRequirementId = requirement.PhaseEquipmentRequirementId,
                ExperimentEquipmentRequirementId = requirement.ExperimentEquipmentRequirementId,
                RequiredEquipmentTypeId = requirement.EquipmentTypeId,
                AllocatedEquipmentTypeId = equipment.EquipmentTypeId,
                EquipmentInstanceId = equipment.EquipmentInstanceId,
                IsSubstitute = true,
                EfficiencyRate = substitution.EfficiencyRate
            };
        }

        private T? Pick<T>(IReadOnlyList<T> candidates)
        {
            if (candidates.Count == 0)
            {
                return default;
            }

            var upperBound = Math.Min(candidates.Count, 3);
            return candidates[_random.Next(upperBound)];
        }

        private static IEnumerable<HumanRequirementSnapshot> GetHumanRequirements(int phaseId, OptimizationInput input)
        {
            var phaseRequirements = input.PhaseHumanRequirements
                .Where(r => r.PhaseId == phaseId)
                .Select(r => new HumanRequirementSnapshot(r.PhaseHumanReqId, null, r.RoleId, r.RequiredSkillId, r.Quantity))
                .ToList();

            if (phaseRequirements.Count > 0)
            {
                return phaseRequirements;
            }

            return input.ExperimentHumanRequirements
                .Select(r => new HumanRequirementSnapshot(null, r.ExpHumanReqId, r.RoleId, r.RequiredSkillId, r.Quantity));
        }

        private static IEnumerable<EquipmentRequirementSnapshot> GetEquipmentRequirements(int phaseId, OptimizationInput input)
        {
            var phaseRequirements = input.PhaseEquipmentRequirements
                .Where(r => r.PhaseId == phaseId)
                .Select(r => new EquipmentRequirementSnapshot(r.PhaseEquipmentReqId, null, r.EquipmentTypeId, r.Quantity))
                .ToList();

            if (phaseRequirements.Count > 0)
            {
                return phaseRequirements;
            }

            return input.ExperimentEquipmentRequirements
                .Select(r => new EquipmentRequirementSnapshot(null, r.ExpEquipmentReqId, r.EquipmentTypeId, r.Quantity));
        }

        private static bool IsAvailableStatus(string? status)
        {
            return status is null ||
                   status.Equals("Available", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }

        private static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        {
            return startA <= endB && startB <= endA;
        }

        private sealed record HumanRequirementSnapshot(
            int? PhaseHumanRequirementId,
            int? ExperimentHumanRequirementId,
            int RoleId,
            int? RequiredSkillId,
            int Quantity);

        private sealed record EquipmentRequirementSnapshot(
            int? PhaseEquipmentRequirementId,
            int? ExperimentEquipmentRequirementId,
            int EquipmentTypeId,
            int Quantity);
    }
}

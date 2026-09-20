using FRPAMSystem.BusinessTier.AI.Fitness.Evaluators;
using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.DataTier.Models;

namespace FRPAMSystem.BusinessTier.AI.Generator
{
    public class PopulationGenerator : IPopulationGenerator
    {
        private readonly Random _random = new();

        public PopulationGenerator()
        {
        }

        public PopulationGenerator(Random random)
        {
            _random = random;
        }

        public Population Generate(OptimizationInput input)
        {
            input.Settings.Normalize();
            var population = new Population();
            var phases = input.ExperimentPhases.OrderBy(p => p.PhaseOrder).ToList();
            var fingerprints = new HashSet<string>();

            var attempts = 0;
            var maxAttempts = input.Settings.PopulationSize * 5;

            while (population.Chromosomes.Count < input.Settings.PopulationSize &&
                   attempts < maxAttempts)
            {
                attempts++;

                var chromosome = new AllocationChromosome
                {
                    Genes = phases
                        .Select(p => GenerateGene(p.PhaseId, input))
                        .ToList()
                };
                NormalizeLandAssignments(chromosome);

                if (fingerprints.Add(CreateFingerprint(chromosome)))
                {
                    population.Chromosomes.Add(chromosome);
                }
            }

            return population;
        }

        public static void NormalizeLandAssignments(AllocationChromosome chromosome)
        {
            var landId = chromosome.Genes
                .Select(g => g.LandId)
                .FirstOrDefault(id => id.HasValue);
            if (!landId.HasValue)
            {
                return;
            }

            foreach (var gene in chromosome.Genes)
            {
                gene.LandId = landId;
            }
        }

        public AllocationGene GenerateGene(int phaseId, OptimizationInput input)
        {
            var phase = input.ExperimentPhases.First(p => p.PhaseId == phaseId);
            var durationDays = Math.Max(1, (phase.ExpectedEndDate.Date - phase.ExpectedStartDate.Date).Days);
            var startDate = phase.ExpectedStartDate.Date;
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
            ApplySubstitutionDuration(gene, durationDays);

            return gene;
        }

        public void MutateComponent(
            AllocationGene gene,
            OptimizationInput input,
            MutationComponent component)
        {
            switch (component)
            {
                case MutationComponent.Land:
                    AssignLand(gene, input, preserveRequirementMetadata: true);
                    break;
                case MutationComponent.Human:
                    AssignHumans(gene, input);
                    break;
                case MutationComponent.Equipment:
                    gene.AssignedEquipmentInstanceIds.Clear();
                    gene.EquipmentAssignments.Clear();
                    AssignEquipment(gene, input);
                    var baseDurationDays = Math.Max(
                        1,
                        (input.ExperimentPhases.First(p => p.PhaseId == gene.PhaseId).ExpectedEndDate.Date -
                         input.ExperimentPhases.First(p => p.PhaseId == gene.PhaseId).ExpectedStartDate.Date).Days);
                    ApplySubstitutionDuration(gene, baseDurationDays);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(component), component, null);
            }
        }

        private static void ApplySubstitutionDuration(
            AllocationGene gene,
            int baseDurationDays)
        {
            var timeMultiplier = gene.EquipmentAssignments.Count == 0
                ? 1d
                : gene.EquipmentAssignments.Max(e => Math.Max(1d, e.TimeMultiplier));

            if (timeMultiplier <= 1d)
            {
                return;
            }

            var adjustedDurationDays = Math.Max(
                1,
                (int)Math.Ceiling(baseDurationDays * timeMultiplier));

            gene.EndDate = gene.StartDate.AddDays(adjustedDurationDays);
        }

        private void AssignLand(
            AllocationGene gene,
            OptimizationInput input,
            bool preserveRequirementMetadata = false)
        {
            var requirement = input.ExperimentLandRequirements
                .OrderByDescending(r => r.RequiredArea)
                .FirstOrDefault();

            if (!preserveRequirementMetadata)
            {
                gene.ExperimentLandRequirementId = requirement?.ExpLandReqId;
            }

            var candidates = input.LandResources
                .Where(l => FitnessEvaluationHelper.IsLandAvailableForOptimization(l.Status))
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
                    .Where(h => h.User?.RoleId == requirement.RoleId)
                    .Where(h => requirement.RequiredSkillId is null ||
                                h.HumanResourceSkills.Any(s => s.SkillId == requirement.RequiredSkillId.Value))
                    .Where(h => HasWorkloadCapacity(h, requirement))
                    .Select(h => new
                    {
                        Human = h,
                        Score = ScoreHuman(h, requirement, gene.StartDate, gene.EndDate, input)
                    })
                    .OrderByDescending(x => x.Score)
                    .Take(Math.Max(requirement.Quantity * 3, requirement.Quantity))
                    .ToList();

                foreach (var candidate in TakeRandomCandidates(candidates, requirement.Quantity))
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

                var preferredCandidates = candidates.Any(c => !c.Assignment!.IsSubstitute)
                    ? candidates.Where(c => !c.Assignment!.IsSubstitute).ToList()
                    : candidates;

                foreach (var candidate in TakeRandomCandidates(preferredCandidates, requirement.Quantity))
                {
                    assignedInstances.Add(candidate.Equipment.EquipmentInstanceId);
                    gene.AssignedEquipmentInstanceIds.Add(candidate.Equipment.EquipmentInstanceId);
                    gene.EquipmentAssignments.Add(candidate.Assignment!);
                }
            }
        }

        private bool HasWorkloadCapacity(
            HumanResourceProfile human,
            HumanRequirementSnapshot requirement)
        {
            var requiredHours = requirement.WorkingHoursPerDay ?? 0d;
            return human.CurrentWorkload + requiredHours <= human.MaxWorkingHoursPerDay;
        }

        private IEnumerable<T> TakeRandomCandidates<T>(
            IReadOnlyList<T> candidates,
            int quantity)
        {
            var available = candidates.ToList();
            var selected = new List<T>(Math.Min(quantity, available.Count));

            while (selected.Count < quantity && available.Count > 0)
            {
                var index = _random.Next(available.Count);
                selected.Add(available[index]);
                available.RemoveAt(index);
            }

            return selected;
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
            if (human.User?.RoleId == requirement.RoleId)
            {
                score += 50d;
            }
            else
            {
                score -= 40d;
            }

            if (requirement.RequiredSkillId is null ||
                human.HumanResourceSkills.Any(s => s.SkillId == requirement.RequiredSkillId.Value))
            {
                score += 40d;
            }

            if (!input.ExistingHumanAllocations.Any(a => a.HumanResourceId == human.HumanResourceId && Overlaps(startDate, endDate, a.StartDate, a.EndDate)))
            {
                score += 30d;
            }

            score += Math.Max(0d, 20d - human.CurrentWorkload);
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
            else if (requirement.AllowSubstitute)
            {
                var substitution = input.EquipmentSubstitutions.FirstOrDefault(s =>
                    s.PrimaryEquipmentTypeId == requirement.EquipmentTypeId &&
                    s.SubEquipmentTypeId == equipment.EquipmentTypeId);
                if (substitution is not null)
                {
                    if (!requirement.MinAcceptableEfficiency.HasValue ||
                        substitution.EfficiencyRate >= requirement.MinAcceptableEfficiency.Value)
                    {
                        score += 35d * substitution.EfficiencyRate;
                    }
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
                    EfficiencyRate = 1d,
                    TimeMultiplier = 1d
                };
            }

            if (!requirement.AllowSubstitute)
            {
                return null;
            }

            var substitution = input.EquipmentSubstitutions.FirstOrDefault(s =>
                s.PrimaryEquipmentTypeId == requirement.EquipmentTypeId &&
                s.SubEquipmentTypeId == equipment.EquipmentTypeId);

            if (substitution is null)
            {
                return null;
            }

            if (requirement.MinAcceptableEfficiency.HasValue &&
                substitution.EfficiencyRate < requirement.MinAcceptableEfficiency.Value)
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
                EfficiencyRate = substitution.EfficiencyRate,
                TimeMultiplier = substitution.TimeMultiplier
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
                .Select(r => new HumanRequirementSnapshot(r.PhaseHumanReqId, null, r.RoleId, r.RequiredSkillId, r.Quantity, null))
                .ToList();

            if (phaseRequirements.Count > 0)
            {
                return phaseRequirements;
            }

            return input.ExperimentHumanRequirements
                .Select(r => new HumanRequirementSnapshot(null, r.ExpHumanReqId, r.RoleId, r.RequiredSkillId, r.Quantity, r.WorkingHoursPerDay));
        }

        private static IEnumerable<EquipmentRequirementSnapshot> GetEquipmentRequirements(int phaseId, OptimizationInput input)
        {
            var phaseRequirements = input.PhaseEquipmentRequirements
                .Where(r => r.PhaseId == phaseId)
                .Select(r =>
                {
                    var expReq = input.ExperimentEquipmentRequirements.FirstOrDefault(er => er.EquipmentTypeId == r.EquipmentTypeId);
                    var allowSubstitute = expReq?.AllowSubstitute ?? true;
                    var minEfficiency = expReq?.MinAcceptableEfficiency;
                    return new EquipmentRequirementSnapshot(r.PhaseEquipmentReqId, null, r.EquipmentTypeId, r.Quantity, allowSubstitute, minEfficiency);
                })
                .ToList();

            if (phaseRequirements.Count > 0)
            {
                return phaseRequirements;
            }

            return input.ExperimentEquipmentRequirements
                .Select(r => new EquipmentRequirementSnapshot(
                    null,
                    r.ExpEquipmentReqId,
                    r.EquipmentTypeId,
                    r.Quantity,
                    r.AllowSubstitute,
                    r.MinAcceptableEfficiency));
        }

        private static bool IsAvailableStatus(string? status)
        {
            return status is null ||
                   status.Equals("Available", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }

        private static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        {
            return startA.Date < endB.Date && startB.Date < endA.Date;
        }

        private static string CreateFingerprint(AllocationChromosome chromosome)
        {
            return AllocationChromosomeFingerprint.Create(chromosome);
        }

        private sealed record HumanRequirementSnapshot(
            int? PhaseHumanRequirementId,
            int? ExperimentHumanRequirementId,
            int RoleId,
            int? RequiredSkillId,
            int Quantity,
            double? WorkingHoursPerDay = null);

        private sealed record EquipmentRequirementSnapshot(
            int? PhaseEquipmentRequirementId,
            int? ExperimentEquipmentRequirementId,
            int EquipmentTypeId,
            int Quantity,
            bool AllowSubstitute = true,
            double? MinAcceptableEfficiency = null);
    }
}

using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.DataTier.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness
{
    public class FitnessCalculator : IFitnessCalculator
    {
        private const double MaxScore = 1000d;

        public FitnessResult Evaluate(AllocationChromosome chromosome, OptimizationInput input)
        {
            var result = new FitnessResult();

            var landScore = CalculateLandScore(chromosome, input, result);
            var humanScore = CalculateHumanScore(chromosome, input, result);
            var equipmentScore = CalculateEquipmentScore(chromosome, input, result);
            var scheduleScore = CalculateScheduleScore(chromosome, input, result);
            var penalty = CalculatePenalty(chromosome, input, result);

            result.PenaltyScore = penalty;
            result.FitnessScore = Math.Max(0d, landScore + humanScore + equipmentScore + scheduleScore - penalty);

            chromosome.FitnessScore = result.FitnessScore;
            chromosome.PenaltyScore = result.PenaltyScore;
            chromosome.ConflictCount = result.ConflictCount;
            chromosome.Advantages = result.Advantages.ToList();
            chromosome.Disadvantages = result.Disadvantages.ToList();

            return result;
        }

        private static double CalculateLandScore(AllocationChromosome chromosome, OptimizationInput input, FitnessResult result)
        {
            var lands = input.LandResources.ToDictionary(l => l.LandId);
            var requirements = input.ExperimentLandRequirements.ToList();
            var score = 0d;

            foreach (var gene in chromosome.Genes)
            {
                if (gene.LandId is null || !lands.TryGetValue(gene.LandId.Value, out var land))
                {
                    AddIssue(result, "Missing land allocation.", 1);
                    continue;
                }

                var requirement = gene.ExperimentLandRequirementId.HasValue
                    ? requirements.FirstOrDefault(r => r.ExpLandReqId == gene.ExperimentLandRequirementId.Value)
                    : requirements.FirstOrDefault();

                if (requirement is null)
                {
                    score += 45d;
                    continue;
                }

                if (StringEquals(land.SoilType, requirement.RequiredSoilType))
                {
                    score += 45d;
                }
                else
                {
                    AddIssue(result, $"Land {land.LandCode} soil type does not match requirement.", 1);
                }

                if (land.AreaSize >= requirement.RequiredArea)
                {
                    score += 35d;
                    score += Math.Max(0d, 15d - (double)(land.AreaSize - requirement.RequiredArea));
                }
                else
                {
                    AddIssue(result, $"Land {land.LandCode} has insufficient area.", 1);
                }

                if (!HasLandOverlap(land.LandId, gene.StartDate, gene.EndDate, input.ExistingLandAllocations))
                {
                    score += 35d;
                }
                else
                {
                    AddIssue(result, $"Land {land.LandCode} overlaps with existing allocation.", 1);
                }
            }

            return score;
        }

        private static double CalculateHumanScore(AllocationChromosome chromosome, OptimizationInput input, FitnessResult result)
        {
            var humans = input.HumanResources.ToDictionary(h => h.HumanResourceId);
            var score = 0d;

            foreach (var gene in chromosome.Genes)
            {
                var requirements = GetHumanRequirements(gene.PhaseId, input).ToList();
                var assigned = gene.AssignedHumanResourceIds
                    .Distinct()
                    .Where(humans.ContainsKey)
                    .Select(id => humans[id])
                    .ToList();

                var requiredQuantity = requirements.Sum(r => r.Quantity);
                if (assigned.Count >= requiredQuantity)
                {
                    score += 45d;
                }
                else
                {
                    AddIssue(result, $"Phase {gene.PhaseId} has insufficient human resources.", requiredQuantity - assigned.Count);
                }

                foreach (var requirement in requirements)
                {
                    var matching = assigned.Count(h => HasSkill(h, requirement.RequiredSkillId));
                    if (matching >= requirement.Quantity)
                    {
                        score += 35d;
                    }
                    else
                    {
                        AddIssue(result, $"Phase {gene.PhaseId} does not meet required skills.", requirement.Quantity - matching);
                    }
                }

                foreach (var human in assigned)
                {
                    if (!HasHumanOverlap(human.HumanResourceId, gene.StartDate, gene.EndDate, input.ExistingHumanAllocations, input.ExistingSchedules))
                    {
                        score += 20d;
                    }
                    else
                    {
                        AddIssue(result, $"Human resource {human.HumanResourceId} is double-booked.", 1);
                    }

                    score += Math.Max(0d, 20d - human.CurrentWorkload);
                }
            }

            var workloadSpread = chromosome.Genes
                .SelectMany(g => g.AssignedHumanResourceIds)
                .GroupBy(id => id)
                .Select(g => g.Count())
                .DefaultIfEmpty(0)
                .ToList();

            if (workloadSpread.Count > 1)
            {
                score += Math.Max(0d, 50d - ((workloadSpread.Max() - workloadSpread.Min()) * 10d));
            }

            return score;
        }

        private static double CalculateEquipmentScore(AllocationChromosome chromosome, OptimizationInput input, FitnessResult result)
        {
            var equipment = input.EquipmentInstances.ToDictionary(e => e.EquipmentInstanceId);
            var score = 0d;

            foreach (var gene in chromosome.Genes)
            {
                var requirements = GetEquipmentRequirements(gene.PhaseId, input).ToList();

                foreach (var requirement in requirements)
                {
                    var assignments = gene.EquipmentAssignments
                        .Where(e => e.PhaseEquipmentRequirementId == requirement.PhaseEquipmentRequirementId ||
                                    e.ExperimentEquipmentRequirementId == requirement.ExperimentEquipmentRequirementId)
                        .ToList();

                    if (assignments.Count >= requirement.Quantity)
                    {
                        score += 35d;
                    }
                    else
                    {
                        AddIssue(result, $"Phase {gene.PhaseId} has insufficient equipment.", requirement.Quantity - assignments.Count);
                    }

                    foreach (var assignment in assignments)
                    {
                        if (assignment.EquipmentInstanceId is null || !equipment.TryGetValue(assignment.EquipmentInstanceId.Value, out var instance))
                        {
                            AddIssue(result, "Missing equipment instance.", 1);
                            continue;
                        }

                        if (instance.EquipmentTypeId == requirement.EquipmentTypeId)
                        {
                            score += 35d;
                        }
                        else if (assignment.IsSubstitute)
                        {
                            score += 20d * assignment.EfficiencyRate;
                        }
                        else
                        {
                            AddIssue(result, $"Equipment {instance.AssetCode} does not match required type.", 1);
                        }

                        if (!HasEquipmentOverlap(instance.EquipmentInstanceId, gene.StartDate, gene.EndDate, input.ExistingEquipmentAllocations))
                        {
                            score += 20d;
                        }
                        else
                        {
                            AddIssue(result, $"Equipment {instance.AssetCode} overlaps with existing allocation.", 1);
                        }
                    }
                }
            }

            return score;
        }

        private static double CalculateScheduleScore(AllocationChromosome chromosome, OptimizationInput input, FitnessResult result)
        {
            var orderedGenes = chromosome.Genes.OrderBy(g => g.StartDate).ToList();
            var score = 0d;

            foreach (var gene in orderedGenes)
            {
                var phase = input.ExperimentPhases.FirstOrDefault(p => p.PhaseId == gene.PhaseId);
                if (phase is null)
                {
                    AddIssue(result, $"Unknown phase {gene.PhaseId}.", 1);
                    continue;
                }

                if (gene.StartDate <= gene.EndDate)
                {
                    score += 30d;
                }
                else
                {
                    AddIssue(result, $"Phase {gene.PhaseId} has invalid date range.", 1);
                }

                var startDelta = Math.Abs((gene.StartDate.Date - phase.ExpectedStartDate.Date).TotalDays);
                var endDelta = Math.Abs((gene.EndDate.Date - phase.ExpectedEndDate.Date).TotalDays);
                score += Math.Max(0d, 40d - startDelta);
                score += Math.Max(0d, 40d - endDelta);

                if (input.Experiment.Deadline is null || gene.EndDate <= input.Experiment.Deadline.Value)
                {
                    score += 30d;
                }
                else
                {
                    AddIssue(result, $"Phase {gene.PhaseId} ends after experiment deadline.", 1);
                }
            }

            for (var i = 1; i < orderedGenes.Count; i++)
            {
                var gapDays = Math.Max(0d, (orderedGenes[i].StartDate.Date - orderedGenes[i - 1].EndDate.Date).TotalDays);
                score += Math.Max(0d, 25d - gapDays);
            }

            return score;
        }

        private static double CalculatePenalty(AllocationChromosome chromosome, OptimizationInput input, FitnessResult result)
        {
            var penalty = result.ConflictCount * 35d;

            penalty += CountInternalOverlaps(
                chromosome.Genes.Select(g => (ResourceId: g.LandId, g.StartDate, g.EndDate))) * 45d;

            penalty += CountInternalOverlaps(
                chromosome.Genes.SelectMany(g => g.AssignedHumanResourceIds
                    .Select(id => (ResourceId: (int?)id, g.StartDate, g.EndDate)))) * 40d;

            penalty += CountInternalOverlaps(
                chromosome.Genes.SelectMany(g => g.EquipmentAssignments
                    .Select(e => (ResourceId: e.EquipmentInstanceId, g.StartDate, g.EndDate)))) * 40d;

            if (penalty <= 0d)
            {
                result.Advantages.Add("No critical constraint conflicts detected.");
            }
            else
            {
                result.Disadvantages.Add("Some constraints require review before approval.");
            }

            return Math.Min(MaxScore, penalty);
        }

        private static IEnumerable<HumanRequirementSnapshot> GetHumanRequirements(int phaseId, OptimizationInput input)
        {
            var phaseRequirements = input.PhaseHumanRequirements
                .Where(r => r.PhaseId == phaseId)
                .Select(r => new HumanRequirementSnapshot(r.PhaseHumanReqId, null, r.RoleId, r.RequiredSkillId, r.Quantity));

            if (phaseRequirements.Any())
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
                .Select(r => new EquipmentRequirementSnapshot(r.PhaseEquipmentReqId, null, r.EquipmentTypeId, r.Quantity));

            if (phaseRequirements.Any())
            {
                return phaseRequirements;
            }

            return input.ExperimentEquipmentRequirements
                .Select(r => new EquipmentRequirementSnapshot(null, r.ExpEquipmentReqId, r.EquipmentTypeId, r.Quantity));
        }

        private static bool HasSkill(HumanResourceProfile human, int? requiredSkillId)
        {
            return requiredSkillId is null ||
                   human.HumanResourceSkills.Any(s => s.SkillId == requiredSkillId.Value);
        }

        private static bool HasLandOverlap(int landId, DateTime startDate, DateTime endDate, IEnumerable<AllocationLandDetail> allocations)
        {
            return allocations.Any(a => a.LandId == landId && Overlaps(startDate, endDate, a.StartDate, a.EndDate));
        }

        private static bool HasHumanOverlap(
            int humanResourceId,
            DateTime startDate,
            DateTime endDate,
            IEnumerable<AllocationHumanDetail> allocations,
            IEnumerable<Schedule> schedules)
        {
            return allocations.Any(a => a.HumanResourceId == humanResourceId && Overlaps(startDate, endDate, a.StartDate, a.EndDate)) ||
                   schedules.Any(s => s.AssignedHumanResourceId == humanResourceId && Overlaps(startDate, endDate, s.StartDate, s.EndDate));
        }

        private static bool HasEquipmentOverlap(int equipmentInstanceId, DateTime startDate, DateTime endDate, IEnumerable<AllocationEquipmentDetail> allocations)
        {
            return allocations.Any(a => a.EquipmentInstanceId == equipmentInstanceId && Overlaps(startDate, endDate, a.StartDate, a.EndDate));
        }

        private static int CountInternalOverlaps(IEnumerable<(int? ResourceId, DateTime StartDate, DateTime EndDate)> bookings)
        {
            var conflicts = 0;
            foreach (var group in bookings.Where(b => b.ResourceId.HasValue).GroupBy(b => b.ResourceId!.Value))
            {
                var ordered = group.OrderBy(g => g.StartDate).ToList();
                for (var i = 1; i < ordered.Count; i++)
                {
                    if (Overlaps(ordered[i - 1].StartDate, ordered[i - 1].EndDate, ordered[i].StartDate, ordered[i].EndDate))
                    {
                        conflicts++;
                    }
                }
            }

            return conflicts;
        }

        private static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        {
            return startA <= endB && startB <= endA;
        }

        private static bool StringEquals(string? left, string? right)
        {
            return string.Equals(left?.Trim(), right?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static void AddIssue(FitnessResult result, string message, int conflictCount)
        {
            result.ConflictCount += Math.Max(1, conflictCount);
            if (!result.Disadvantages.Contains(message))
            {
                result.Disadvantages.Add(message);
            }
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

using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.DataTier.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness.Evaluators
{
    internal static class FitnessEvaluationHelper
    {
        public static IEnumerable<HumanRequirementSnapshot> GetHumanRequirements(int phaseId, OptimizationInput input)
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

        public static IEnumerable<EquipmentRequirementSnapshot> GetEquipmentRequirements(int phaseId, OptimizationInput input)
        {
            var phaseRequirements = input.PhaseEquipmentRequirements
                .Where(r => r.PhaseId == phaseId)
                .Select(r => new EquipmentRequirementSnapshot(r.PhaseEquipmentReqId, null, r.EquipmentTypeId, r.Quantity, true, null))
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

        public static bool IsAvailableStatus(string? status)
        {
            return status is null ||
                   status.Equals("Available", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsMaintenanceStatus(string? status)
        {
            return status is not null &&
                   (status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase) ||
                    status.Equals("Under Maintenance", StringComparison.OrdinalIgnoreCase));
        }

        public static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        {
            return startA <= endB && startB <= endA;
        }

        public static int CountInternalOverlaps(IEnumerable<(int? ResourceId, DateTime StartDate, DateTime EndDate)> bookings)
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

        public static double ClampScore(double value)
        {
            return Math.Clamp(value, 0d, 100d);
        }

        public static double Percent(double numerator, double denominator)
        {
            return denominator <= 0d ? 1d : Math.Clamp(numerator / denominator, 0d, 1d);
        }

        public static bool HasSkill(HumanResourceProfile human, int? requiredSkillId)
        {
            return requiredSkillId is null ||
                   human.HumanResourceSkills.Any(s => s.SkillId == requiredSkillId.Value);
        }

        public static bool HasRole(HumanResourceProfile human, int roleId)
        {
            return human.User?.RoleId == roleId;
        }

        public static double EstimateAssignedHoursPerDay(AllocationGene gene, HumanRequirementSnapshot requirement)
        {
            if (requirement.WorkingHoursPerDay.HasValue)
            {
                return requirement.WorkingHoursPerDay.Value;
            }

            var durationDays = Math.Max(1d, (gene.EndDate.Date - gene.StartDate.Date).TotalDays + 1d);
            return Math.Min(8d, 8d / durationDays);
        }

        public static double EstimatePhaseUsageHours(AllocationGene gene)
        {
            var durationDays = Math.Max(1d, (gene.EndDate.Date - gene.StartDate.Date).TotalDays + 1d);
            return durationDays * 8d;
        }
    }

    internal sealed record HumanRequirementSnapshot(
        int? PhaseHumanRequirementId,
        int? ExperimentHumanRequirementId,
        int RoleId,
        int? RequiredSkillId,
        int Quantity,
        double? WorkingHoursPerDay);

    internal sealed record EquipmentRequirementSnapshot(
        int? PhaseEquipmentRequirementId,
        int? ExperimentEquipmentRequirementId,
        int EquipmentTypeId,
        int Quantity,
        bool AllowSubstitute,
        double? MinAcceptableEfficiency);
}

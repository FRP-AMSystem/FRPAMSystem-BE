using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.DataTier.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness.Evaluators
{
    public static class FitnessEvaluationHelper
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

        public static bool IsAvailableStatus(string? status)
        {
            return status is null ||
                   status.Equals("Available", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsLandAvailableForOptimization(string? status)
        {
            return status is null ||
                   !status.Equals("Unavailable", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsMaintenanceStatus(string? status)
        {
            return status is not null &&
                   (status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase) ||
                    status.Equals("Under Maintenance", StringComparison.OrdinalIgnoreCase));
        }

        public static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        {
            return startA.Date < endB.Date && startB.Date < endA.Date;
        }

        public static int CountInternalOverlaps(IEnumerable<(int? ResourceId, DateTime StartDate, DateTime EndDate)> bookings)
        {
            var conflicts = 0;
            foreach (var group in bookings.Where(b => b.ResourceId.HasValue).GroupBy(b => b.ResourceId!.Value))
            {
                var list = group.ToList();
                for (var i = 0; i < list.Count; i++)
                {
                    for (var j = i + 1; j < list.Count; j++)
                    {
                        if (Overlaps(list[i].StartDate, list[i].EndDate, list[j].StartDate, list[j].EndDate))
                        {
                            conflicts++;
                        }
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

    public sealed record HumanRequirementSnapshot(
        int? PhaseHumanRequirementId,
        int? ExperimentHumanRequirementId,
        int RoleId,
        int? RequiredSkillId,
        int Quantity,
        double? WorkingHoursPerDay);

    public sealed record EquipmentRequirementSnapshot(
        int? PhaseEquipmentRequirementId,
        int? ExperimentEquipmentRequirementId,
        int EquipmentTypeId,
        int Quantity,
        bool AllowSubstitute,
        double? MinAcceptableEfficiency);
}

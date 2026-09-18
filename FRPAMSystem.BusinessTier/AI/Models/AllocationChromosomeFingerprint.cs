using System.Globalization;

namespace FRPAMSystem.BusinessTier.AI.Models
{
    internal static class AllocationChromosomeFingerprint
    {
        public static string Create(AllocationChromosome chromosome)
        {
            return string.Join('|', chromosome.Genes
                .OrderBy(g => g.PhaseId)
                .Select(g =>
                    $"{g.PhaseId}:{g.ExperimentLandRequirementId}:{g.LandId}:{g.StartDate:yyyyMMdd}:{g.EndDate:yyyyMMdd}:" +
                    $"{string.Join(',', g.AssignedHumanResourceIds.Distinct().OrderBy(id => id))}:" +
                    $"{string.Join(',', g.AssignedEquipmentInstanceIds.Distinct().OrderBy(id => id))}:" +
                    $"{string.Join(';', g.EquipmentAssignments
                        .OrderBy(e => e.PhaseEquipmentRequirementId)
                        .ThenBy(e => e.ExperimentEquipmentRequirementId)
                        .ThenBy(e => e.RequiredEquipmentTypeId)
                        .ThenBy(e => e.AllocatedEquipmentTypeId)
                        .ThenBy(e => e.EquipmentInstanceId)
                        .ThenBy(e => e.IsSubstitute)
                        .ThenBy(e => e.EfficiencyRate)
                        .ThenBy(e => e.TimeMultiplier)
                        .Select(e => string.Join(',',
                            e.PhaseEquipmentRequirementId,
                            e.ExperimentEquipmentRequirementId,
                            e.RequiredEquipmentTypeId,
                            e.AllocatedEquipmentTypeId,
                            e.EquipmentInstanceId,
                            e.IsSubstitute,
                            e.EfficiencyRate.ToString("R", CultureInfo.InvariantCulture),
                            e.TimeMultiplier.ToString("R", CultureInfo.InvariantCulture))))}"));
        }
    }
}

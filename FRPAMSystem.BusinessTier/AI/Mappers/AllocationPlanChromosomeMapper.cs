using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.DataTier.Models;

namespace FRPAMSystem.BusinessTier.AI.Mappers
{
    public class AllocationPlanChromosomeMapper : IAllocationPlanChromosomeMapper
    {
        public AllocationChromosome MapToChromosome(AllocationPlan plan, OptimizationInput input)
        {
            var chromosome = new AllocationChromosome();
            var phases = input.ExperimentPhases.OrderBy(p => p.PhaseOrder).ToList();
            var substitutions = input.EquipmentSubstitutions.ToList();
            var phaseEquipmentReqs = input.PhaseEquipmentRequirements.ToDictionary(r => r.PhaseEquipmentReqId);
            var phaseHumanReqs = input.PhaseHumanRequirements.ToDictionary(r => r.PhaseHumanReqId);
            var expEquipmentReqs = input.ExperimentEquipmentRequirements.ToDictionary(r => r.ExpEquipmentReqId);
            var expHumanReqs = input.ExperimentHumanRequirements.ToDictionary(r => r.ExpHumanReqId);

            var landDetails = plan.AllocationLandDetails?.ToList() ?? new List<AllocationLandDetail>();
            var humanDetails = plan.AllocationHumanDetails?.ToList() ?? new List<AllocationHumanDetail>();
            var equipmentDetails = plan.AllocationEquipmentDetails?.ToList() ?? new List<AllocationEquipmentDetail>();
            var schedules = plan.Schedules?.ToList() ?? new List<Schedule>();

            foreach (var phase in phases)
            {
                var phaseSchedules = schedules.Where(s => s.PhaseId == phase.PhaseId).ToList();
                var phaseHumanDetails = humanDetails.Where(h =>
                    (h.PhaseHumanReqId.HasValue && phaseHumanReqs.TryGetValue(h.PhaseHumanReqId.Value, out var phr) && phr.PhaseId == phase.PhaseId) ||
                    (h.PhaseHumanReq != null && h.PhaseHumanReq.PhaseId == phase.PhaseId)
                ).ToList();
                var phaseEquipmentDetails = equipmentDetails.Where(e =>
                    (e.PhaseEquipmentReqId.HasValue && phaseEquipmentReqs.TryGetValue(e.PhaseEquipmentReqId.Value, out var per) && per.PhaseId == phase.PhaseId) ||
                    (e.PhaseEquipmentReq != null && e.PhaseEquipmentReq.PhaseId == phase.PhaseId)
                ).ToList();

                // If no phase-specific details matched, check for experiment-level details overlapping with the phase
                if (phaseHumanDetails.Count == 0 && humanDetails.Count > 0)
                {
                    phaseHumanDetails = humanDetails.Where(h =>
                        h.ExpHumanReqId.HasValue &&
                        (h.StartDate <= phase.ExpectedEndDate && h.EndDate >= phase.ExpectedStartDate)
                    ).ToList();
                }

                if (phaseEquipmentDetails.Count == 0 && equipmentDetails.Count > 0)
                {
                    phaseEquipmentDetails = equipmentDetails.Where(e =>
                        e.ExpEquipmentReqId.HasValue &&
                        (e.StartDate <= phase.ExpectedEndDate && e.EndDate >= phase.ExpectedStartDate)
                    ).ToList();
                }

                // Determine start and end date for the phase gene
                DateTime startDate = phase.ExpectedStartDate;
                DateTime endDate = phase.ExpectedEndDate;

                if (phaseSchedules.Count > 0)
                {
                    startDate = phaseSchedules.Min(s => s.StartDate);
                    endDate = phaseSchedules.Max(s => s.EndDate);
                }
                else if (phaseHumanDetails.Count > 0 || phaseEquipmentDetails.Count > 0)
                {
                    var allStartDates = phaseHumanDetails.Select(h => h.StartDate)
                        .Concat(phaseEquipmentDetails.Select(e => e.StartDate)).ToList();
                    var allEndDates = phaseHumanDetails.Select(h => h.EndDate)
                        .Concat(phaseEquipmentDetails.Select(e => e.EndDate)).ToList();

                    if (allStartDates.Count > 0)
                    {
                        startDate = allStartDates.Min();
                    }
                    if (allEndDates.Count > 0)
                    {
                        endDate = allEndDates.Max();
                    }
                }

                var gene = new AllocationGene
                {
                    PhaseId = phase.PhaseId,
                    StartDate = startDate,
                    EndDate = endDate
                };

                // Map Land
                var matchedLand = landDetails.FirstOrDefault(l =>
                    (l.StartDate <= endDate && l.EndDate >= startDate) ||
                    (l.ExpLandReqId > 0 && input.ExperimentLandRequirements.Any(elr => elr.ExpLandReqId == l.ExpLandReqId))
                ) ?? landDetails.FirstOrDefault();

                if (matchedLand != null)
                {
                    gene.LandId = matchedLand.LandId;
                    gene.ExperimentLandRequirementId = matchedLand.ExpLandReqId;
                }

                // Map Humans
                var assignedHumanIds = new HashSet<int>();
                foreach (var h in phaseHumanDetails)
                {
                    assignedHumanIds.Add(h.HumanResourceId);
                }
                foreach (var s in phaseSchedules.Where(s => s.AssignedHumanResourceId.HasValue))
                {
                    assignedHumanIds.Add(s.AssignedHumanResourceId!.Value);
                }
                gene.AssignedHumanResourceIds = assignedHumanIds.ToList();

                // Map Equipment
                var assignedEquipmentIds = new HashSet<int>();
                foreach (var eqDetail in phaseEquipmentDetails)
                {
                    if (eqDetail.EquipmentInstanceId.HasValue)
                    {
                        assignedEquipmentIds.Add(eqDetail.EquipmentInstanceId.Value);
                    }

                    int requiredEquipmentTypeId = eqDetail.AllocatedEquipmentTypeId;
                    if (eqDetail.PhaseEquipmentReqId.HasValue && phaseEquipmentReqs.TryGetValue(eqDetail.PhaseEquipmentReqId.Value, out var per))
                    {
                        requiredEquipmentTypeId = per.EquipmentTypeId;
                    }
                    else if (eqDetail.ExpEquipmentReqId.HasValue && expEquipmentReqs.TryGetValue(eqDetail.ExpEquipmentReqId.Value, out var eer))
                    {
                        requiredEquipmentTypeId = eer.EquipmentTypeId;
                    }
                    else if (eqDetail.IsSubstitute)
                    {
                        var matchingSub = substitutions.FirstOrDefault(s => s.SubEquipmentTypeId == eqDetail.AllocatedEquipmentTypeId);
                        if (matchingSub != null)
                        {
                            requiredEquipmentTypeId = matchingSub.PrimaryEquipmentTypeId;
                        }
                    }

                    double efficiencyRate = eqDetail.EfficiencyRate > 0
                        ? eqDetail.EfficiencyRate
                        : (eqDetail.IsSubstitute ? 0.8d : 1.0d);

                    double timeMultiplier = 1.0d;
                    if (eqDetail.IsSubstitute)
                    {
                        var matchingSub = substitutions.FirstOrDefault(s =>
                            s.PrimaryEquipmentTypeId == requiredEquipmentTypeId &&
                            s.SubEquipmentTypeId == eqDetail.AllocatedEquipmentTypeId);

                        if (matchingSub != null && matchingSub.TimeMultiplier > 0)
                        {
                            timeMultiplier = matchingSub.TimeMultiplier;
                        }
                        else if (efficiencyRate > 0)
                        {
                            timeMultiplier = Math.Round(1.0d / efficiencyRate, 2);
                        }
                    }

                    var eqGene = new EquipmentAssignmentGene
                    {
                        PhaseEquipmentRequirementId = eqDetail.PhaseEquipmentReqId,
                        ExperimentEquipmentRequirementId = eqDetail.ExpEquipmentReqId,
                        RequiredEquipmentTypeId = requiredEquipmentTypeId,
                        AllocatedEquipmentTypeId = eqDetail.AllocatedEquipmentTypeId,
                        EquipmentInstanceId = eqDetail.EquipmentInstanceId,
                        IsSubstitute = eqDetail.IsSubstitute,
                        EfficiencyRate = efficiencyRate,
                        TimeMultiplier = timeMultiplier
                    };

                    gene.EquipmentAssignments.Add(eqGene);
                }

                gene.AssignedEquipmentInstanceIds = assignedEquipmentIds.ToList();
                chromosome.Genes.Add(gene);
            }

            return chromosome;
        }
    }
}

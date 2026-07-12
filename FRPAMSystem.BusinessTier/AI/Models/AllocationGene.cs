namespace FRPAMSystem.BusinessTier.AI.Models
{
    public class AllocationGene
    {
        public int PhaseId { get; set; }

        public int? LandId { get; set; }

        public int? ExperimentLandRequirementId { get; set; }

        public List<int> AssignedHumanResourceIds { get; set; } = new();

        public List<int> AssignedEquipmentInstanceIds { get; set; } = new();

        public List<EquipmentAssignmentGene> EquipmentAssignments { get; set; } = new();

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AllocationGene Clone()
        {
            return new AllocationGene
            {
                PhaseId = PhaseId,
                LandId = LandId,
                ExperimentLandRequirementId = ExperimentLandRequirementId,
                AssignedHumanResourceIds = AssignedHumanResourceIds.ToList(),
                AssignedEquipmentInstanceIds = AssignedEquipmentInstanceIds.ToList(),
                EquipmentAssignments = EquipmentAssignments.Select(e => e.Clone()).ToList(),
                StartDate = StartDate,
                EndDate = EndDate
            };
        }
    }
}

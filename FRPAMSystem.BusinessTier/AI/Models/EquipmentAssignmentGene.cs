namespace FRPAMSystem.BusinessTier.AI.Models
{
    public class EquipmentAssignmentGene
    {
        public int? PhaseEquipmentRequirementId { get; set; }

        public int? ExperimentEquipmentRequirementId { get; set; }

        public int RequiredEquipmentTypeId { get; set; }

        public int AllocatedEquipmentTypeId { get; set; }

        public int? EquipmentInstanceId { get; set; }

        public bool IsSubstitute { get; set; }

        public double EfficiencyRate { get; set; } = 1d;

        public double TimeMultiplier { get; set; } = 1d;

        public EquipmentAssignmentGene Clone()
        {
            return new EquipmentAssignmentGene
            {
                PhaseEquipmentRequirementId = PhaseEquipmentRequirementId,
                ExperimentEquipmentRequirementId = ExperimentEquipmentRequirementId,
                RequiredEquipmentTypeId = RequiredEquipmentTypeId,
                AllocatedEquipmentTypeId = AllocatedEquipmentTypeId,
                EquipmentInstanceId = EquipmentInstanceId,
                IsSubstitute = IsSubstitute,
                EfficiencyRate = EfficiencyRate,
                TimeMultiplier = TimeMultiplier
            };
        }
    }
}

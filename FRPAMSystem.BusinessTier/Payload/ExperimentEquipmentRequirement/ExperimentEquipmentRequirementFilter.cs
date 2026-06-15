namespace FRPAMSystem.BusinessTier.Payload.ExperimentEquipmentRequirement
{
    public class ExperimentEquipmentRequirementFilter
    {
        public string? Keyword { get; set; }

        public int? ExperimentId { get; set; }

        public int? EquipmentTypeId { get; set; }

        public bool? AllowSubstitute { get; set; }
    }
}

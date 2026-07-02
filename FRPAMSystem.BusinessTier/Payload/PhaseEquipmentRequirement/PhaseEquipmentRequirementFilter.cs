namespace FRPAMSystem.BusinessTier.Payload.PhaseEquipmentRequirement
{
    public class PhaseEquipmentRequirementFilter
    {
        public string? Keyword { get; set; }

        public int? PhaseId { get; set; }

        public int? ExperimentId { get; set; }

        public int? EquipmentTypeId { get; set; }

        public int? EquipmentCategoryId { get; set; }
    }
}

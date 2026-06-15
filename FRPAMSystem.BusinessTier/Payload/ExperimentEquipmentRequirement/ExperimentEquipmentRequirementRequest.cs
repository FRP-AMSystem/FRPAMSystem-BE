namespace FRPAMSystem.BusinessTier.Payload.ExperimentEquipmentRequirement
{
    public class ExperimentEquipmentRequirementRequest
    {
        public int ExperimentId { get; set; }

        public int EquipmentTypeId { get; set; }

        public int Quantity { get; set; }

        public bool AllowSubstitute { get; set; }

        public double? MinAcceptableEfficiency { get; set; }

        public string? Note { get; set; }
    }
}

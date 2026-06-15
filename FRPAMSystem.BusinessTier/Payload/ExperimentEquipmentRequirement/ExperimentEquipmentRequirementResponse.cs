namespace FRPAMSystem.BusinessTier.Payload.ExperimentEquipmentRequirement
{
    public class ExperimentEquipmentRequirementResponse
    {
        public int ExpEquipmentReqId { get; set; }

        public int ExperimentId { get; set; }

        public string ExperimentName { get; set; } = string.Empty;

        public int EquipmentTypeId { get; set; }

        public string EquipmentTypeName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public bool AllowSubstitute { get; set; }

        public double? MinAcceptableEfficiency { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

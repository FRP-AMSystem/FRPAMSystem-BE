namespace FRPAMSystem.BusinessTier.Payload.PhaseEquipmentRequirement
{
    public class PhaseEquipmentRequirementResponse
    {
        public int PhaseEquipmentReqId { get; set; }

        public int PhaseId { get; set; }

        public string PhaseName { get; set; } = string.Empty;

        public int ExperimentId { get; set; }

        public string ExperimentName { get; set; } = string.Empty;

        public int EquipmentTypeId { get; set; }

        public string EquipmentTypeName { get; set; } = string.Empty;

        public int EquipmentCategoryId { get; set; }

        public string EquipmentCategoryName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

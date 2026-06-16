namespace FRPAMSystem.BusinessTier.Payload.EquipmentSubstitution
{
    public class EquipmentSubstitutionResponse
    {
        public int EquipmentSubId { get; set; }

        public int PrimaryEquipmentTypeId { get; set; }

        public string PrimaryEquipmentTypeName { get; set; } = string.Empty;

        public int SubEquipmentTypeId { get; set; }

        public string SubEquipmentTypeName { get; set; } = string.Empty;

        public double EfficiencyRate { get; set; }

        public double TimeMultiplier { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

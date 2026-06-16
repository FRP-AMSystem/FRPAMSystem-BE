namespace FRPAMSystem.BusinessTier.Payload.EquipmentSubstitution
{
    public class EquipmentSubstitutionRequest
    {
        public int PrimaryEquipmentTypeId { get; set; }

        public int SubEquipmentTypeId { get; set; }

        public double EfficiencyRate { get; set; }

        public double TimeMultiplier { get; set; }

        public string? Note { get; set; }
    }
}

namespace FRPAMSystem.BusinessTier.Payload.EquipmentReturn
{
    public class EquipmentReturnMineRequest
    {
        public string ConditionAfter { get; set; } = string.Empty;

        public bool IsDamaged { get; set; }

        public string? DamageDescription { get; set; }

        public string? Note { get; set; }
    }
}

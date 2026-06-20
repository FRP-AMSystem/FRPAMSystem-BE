namespace FRPAMSystem.BusinessTier.Payload.EquipmentSubstitution
{
    public class EquipmentSubstitutionFilter
    {
        public string? Keyword { get; set; }

        public int? PrimaryEquipmentTypeId { get; set; }

        public int? SubEquipmentTypeId { get; set; }
    }
}

namespace FRPAMSystem.BusinessTier.Payload.PhaseEquipmentRequirement
{
    public class PhaseEquipmentRequirementRequest
    {
        public int PhaseId { get; set; }

        public int EquipmentTypeId { get; set; }

        public int Quantity { get; set; }

        public string? Note { get; set; }
    }
}

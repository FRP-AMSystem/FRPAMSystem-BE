using FRPAMSystem.BusinessTier.Payload.PhaseEquipmentRequirement;

namespace FRPAMSystem.BusinessTier.Validators
{
    public static class PhaseEquipmentRequirementValidator
    {
        public static void Validate(PhaseEquipmentRequirementRequest request)
        {
            if (request.PhaseId <= 0)
            {
                throw new Exception("PhaseId is required.");
            }

            if (request.EquipmentTypeId <= 0)
            {
                throw new Exception("EquipmentTypeId is required.");
            }

            if (request.Quantity <= 0)
            {
                throw new Exception("Quantity must be greater than 0.");
            }
        }
    }
}

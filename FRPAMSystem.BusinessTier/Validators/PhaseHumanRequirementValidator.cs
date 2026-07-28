using FRPAMSystem.BusinessTier.Payload.PhaseHumanRequirement;

namespace FRPAMSystem.BusinessTier.Validators
{
    public static class PhaseHumanRequirementValidator
    {
        public static void Validate(PhaseHumanRequirementRequest request)
        {
            if (request.PhaseId <= 0)
            {
                throw new Exception("PhaseId is required.");
            }

            if (request.RoleId <= 0)
            {
                throw new Exception("RoleId is required.");
            }

            if (request.Quantity <= 0)
            {
                throw new Exception("Quantity must be greater than 0.");
            }
        }
    }
}

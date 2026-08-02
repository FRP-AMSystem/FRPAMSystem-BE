namespace FRPAMSystem.BusinessTier.AI.Fitness.Evaluators
{
    public sealed record ConstraintViolation(
        string Category,
        ConstraintSeverity Severity,
        string Message);
}

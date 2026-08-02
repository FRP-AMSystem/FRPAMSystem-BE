using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness.Evaluators
{
    public interface IConstraintEvaluator
    {
        string Category { get; }

        ConstraintEvaluationResult Evaluate(AllocationChromosome chromosome, OptimizationInput input);
    }
}

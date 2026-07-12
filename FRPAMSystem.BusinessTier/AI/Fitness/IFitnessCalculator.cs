using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Fitness
{
    public interface IFitnessCalculator
    {
        FitnessResult Evaluate(AllocationChromosome chromosome, OptimizationInput input);
    }
}

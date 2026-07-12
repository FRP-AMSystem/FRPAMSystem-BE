using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Operators.Crossover
{
    public interface ICrossoverOperator
    {
        (AllocationChromosome FirstChild, AllocationChromosome SecondChild) Crossover(
            AllocationChromosome firstParent,
            AllocationChromosome secondParent,
            OptimizationSettings settings);
    }
}

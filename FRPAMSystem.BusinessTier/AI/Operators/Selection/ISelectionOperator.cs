using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Operators.Selection
{
    public interface ISelectionOperator
    {
        AllocationChromosome Select(IReadOnlyList<AllocationChromosome> population, OptimizationSettings settings);
    }
}

using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Operators.Mutation
{
    public interface IMutationOperator
    {
        void Mutate(AllocationChromosome chromosome, OptimizationInput input, int generationIndex);
    }
}

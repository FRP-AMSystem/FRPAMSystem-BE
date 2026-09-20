using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Generator
{
    public interface IPopulationGenerator
    {
        Population Generate(OptimizationInput input);

        AllocationGene GenerateGene(int phaseId, OptimizationInput input);

        void MutateComponent(
            AllocationGene gene,
            OptimizationInput input,
            MutationComponent component);
    }
}

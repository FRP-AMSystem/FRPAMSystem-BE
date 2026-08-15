using FRPAMSystem.BusinessTier.AI.Models;
using FRPAMSystem.DataTier.Models;

namespace FRPAMSystem.BusinessTier.AI.Mappers
{
    public interface IAllocationPlanChromosomeMapper
    {
        AllocationChromosome MapToChromosome(AllocationPlan plan, OptimizationInput input);
    }
}

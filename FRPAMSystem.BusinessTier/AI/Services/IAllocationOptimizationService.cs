using FRPAMSystem.BusinessTier.AI.DTO;
using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Services
{
    public interface IAllocationOptimizationService
    {
        Task<IReadOnlyList<AllocationSuggestionDTO>> GenerateTopSuggestionsAsync(
            int experimentId,
            OptimizationSettings? settings = null);
    }
}

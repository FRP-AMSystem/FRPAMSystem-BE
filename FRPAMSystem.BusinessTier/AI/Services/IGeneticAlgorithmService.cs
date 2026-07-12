using FRPAMSystem.BusinessTier.AI.DTO;
using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Services
{
    public interface IGeneticAlgorithmService
    {
        IReadOnlyList<AllocationSuggestionDTO> GenerateSuggestions(OptimizationInput input);
    }
}

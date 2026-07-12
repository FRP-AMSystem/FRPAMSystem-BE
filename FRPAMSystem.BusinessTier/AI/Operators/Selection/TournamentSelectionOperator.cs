using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Operators.Selection
{
    public class TournamentSelectionOperator : ISelectionOperator
    {
        private readonly Random _random = new();

        public AllocationChromosome Select(IReadOnlyList<AllocationChromosome> population, OptimizationSettings settings)
        {
            if (population.Count == 0)
            {
                throw new InvalidOperationException("Population is empty.");
            }

            var tournamentSize = Math.Min(settings.TournamentSize, population.Count);
            var best = population[_random.Next(population.Count)];

            for (var i = 1; i < tournamentSize; i++)
            {
                var candidate = population[_random.Next(population.Count)];
                if (candidate.FitnessScore > best.FitnessScore)
                {
                    best = candidate;
                }
            }

            return best.Clone();
        }
    }
}

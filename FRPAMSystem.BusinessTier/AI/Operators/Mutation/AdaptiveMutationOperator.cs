using FRPAMSystem.BusinessTier.AI.Generator;
using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Operators.Mutation
{
    public class AdaptiveMutationOperator : IMutationOperator
    {
        private readonly IPopulationGenerator _populationGenerator;
        private readonly Random _random = new();

        public AdaptiveMutationOperator(IPopulationGenerator populationGenerator)
        {
            _populationGenerator = populationGenerator;
        }

        public void Mutate(AllocationChromosome chromosome, OptimizationInput input, int generationIndex)
        {
            if (chromosome.Genes.Count == 0)
            {
                return;
            }

            var progress = input.Settings.GenerationCount <= 1
                ? 1d
                : (double)generationIndex / input.Settings.GenerationCount;
            var adaptiveRate = Math.Clamp(input.Settings.MutationRate * (1.5d - progress), 0.001d, 0.8d);

            for (var i = 0; i < chromosome.Genes.Count; i++)
            {
                if (_random.NextDouble() > adaptiveRate)
                {
                    continue;
                }

                var phaseId = chromosome.Genes[i].PhaseId;
                chromosome.Genes[i] = _populationGenerator.GenerateGene(phaseId, input);
            }
        }
    }
}

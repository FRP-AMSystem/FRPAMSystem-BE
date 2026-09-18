using FRPAMSystem.BusinessTier.AI.Models;

namespace FRPAMSystem.BusinessTier.AI.Operators.Crossover
{
    public class SinglePointCrossoverOperator : ICrossoverOperator
    {
        private readonly Random _random = new();

        public (AllocationChromosome FirstChild, AllocationChromosome SecondChild) Crossover(
            AllocationChromosome firstParent,
            AllocationChromosome secondParent,
            OptimizationSettings settings)
        {
            if (firstParent.Genes.Count < 2 ||
                secondParent.Genes.Count < 2 ||
                _random.NextDouble() > settings.CrossoverRate)
            {
                var firstClone = firstParent.Clone();
                var secondClone = secondParent.Clone();
                NormalizeLandAssignments(firstClone);
                NormalizeLandAssignments(secondClone);
                return (firstClone, secondClone);
            }

            var cutPoint = _random.Next(1, Math.Min(firstParent.Genes.Count, secondParent.Genes.Count));

            var firstChild = new AllocationChromosome
            {
                Genes = firstParent.Genes.Take(cutPoint)
                    .Concat(secondParent.Genes.Skip(cutPoint))
                    .Select(g => g.Clone())
                    .ToList()
            };

            var secondChild = new AllocationChromosome
            {
                Genes = secondParent.Genes.Take(cutPoint)
                    .Concat(firstParent.Genes.Skip(cutPoint))
                    .Select(g => g.Clone())
                    .ToList()
            };

            NormalizeLandAssignments(firstChild);
            NormalizeLandAssignments(secondChild);
            return (firstChild, secondChild);
        }

        private static void NormalizeLandAssignments(AllocationChromosome chromosome)
        {
            var landId = chromosome.Genes.Select(g => g.LandId).FirstOrDefault(id => id.HasValue);
            if (!landId.HasValue)
            {
                return;
            }

            foreach (var gene in chromosome.Genes)
            {
                gene.LandId = landId;
            }
        }
    }
}

using FRPAMSystem.BusinessTier.AI.Fitness;
using FRPAMSystem.BusinessTier.AI.Fitness.Evaluators;
using FRPAMSystem.BusinessTier.AI.Models;
using Xunit;

namespace FRPAMSystem.NotificationTests.AI
{
    public class FitnessEvaluationRefactorTests
    {
        [Fact]
        public void FitnessCalculator_UsesExactFourComponentWeightsAndSoftPenaltyOnce()
        {
            var calculator = new FitnessCalculator(new IConstraintEvaluator[]
            {
                new FixedLandEvaluator(90d),
                new FixedHumanEvaluator(80d),
                new FixedEquipmentEvaluator(85d),
                new FixedMaintenanceEvaluator(70d)
            });
            var input = new OptimizationInput
            {
                Settings = new OptimizationSettings
                {
                    PopulationSize = 20,
                    SoftConstraintPenalty = 5d
                }
            };

            var result = calculator.Evaluate(new AllocationChromosome(), input);

            Assert.Equal(82.5d, result.Breakdown.FinalScore - result.BonusScore - result.PenaltyScore, 6);
            Assert.Equal(75.5d, result.FitnessScore, 6);
            Assert.Equal(-10d, result.PenaltyScore, 6);
            Assert.Equal(3d, result.BonusScore, 6);
            Assert.Equal(2, result.SoftViolationCount);
            Assert.Equal(0, result.HardViolationCount);
            Assert.Contains("Maintenance", result.Breakdown.OverallCalculation);
            Assert.Contains("= 75.50", result.Breakdown.OverallCalculation);
        }

        private abstract class FixedEvaluator : IConstraintEvaluator
        {
            private readonly double _score;
            protected FixedEvaluator(double score) => _score = score;
            public abstract string Category { get; }
            public virtual ConstraintEvaluationResult Evaluate(AllocationChromosome chromosome, OptimizationInput input)
            {
                var result = new ConstraintEvaluationResult { Score = _score };
                if (Category == "Land")
                {
                    result.Violations.Add(new ConstraintViolation(Category, ConstraintSeverity.Soft, "soft one"));
                    result.Violations.Add(new ConstraintViolation(Category, ConstraintSeverity.Soft, "soft two"));
                }
                if (Category == "Human")
                {
                    result.Bonus = 3d;
                    result.Penalty = 100d;
                }
                return result;
            }
        }

        private sealed class FixedLandEvaluator : FixedEvaluator, ILandConstraintEvaluator
        {
            public FixedLandEvaluator(double score) : base(score) { }
            public override string Category => "Land";
        }

        private sealed class FixedHumanEvaluator : FixedEvaluator, IHumanConstraintEvaluator
        {
            public FixedHumanEvaluator(double score) : base(score) { }
            public override string Category => "Human";
        }

        private sealed class FixedEquipmentEvaluator : FixedEvaluator, IEquipmentConstraintEvaluator
        {
            public FixedEquipmentEvaluator(double score) : base(score) { }
            public override string Category => "Equipment";
        }

        private sealed class FixedMaintenanceEvaluator : FixedEvaluator, IMaintenanceConstraintEvaluator
        {
            public FixedMaintenanceEvaluator(double score) : base(score) { }
            public override string Category => "Maintenance";
        }
    }
}

using FRPAMSystem.BusinessTier.AI.Models;
using System.Collections.Generic;

namespace FRPAMSystem.BusinessTier.Payload.AllocationPlan
{
    public class SimulatePlanFitnessResponse
    {
        public double FitnessScore { get; set; }

        public bool IsFeasible { get; set; }

        public FitnessBreakdown? FitnessBreakdown { get; set; }

        public ConstraintReport? ConstraintReport { get; set; }

        public List<string> Advantages { get; set; } = new();

        public List<string> Disadvantages { get; set; } = new();

        public List<string> Shortages { get; set; } = new();

        public List<string> Violations { get; set; } = new();
    }
}

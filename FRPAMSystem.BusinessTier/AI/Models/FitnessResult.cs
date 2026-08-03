namespace FRPAMSystem.BusinessTier.AI.Models
{
    public class FitnessResult
    {
        public double FitnessScore { get; set; }

        public double PenaltyScore { get; set; }

        public double BonusScore { get; set; }

        public double LandScore { get; set; }

        public double HumanScore { get; set; }

        public double EquipmentScore { get; set; }

        public double ScheduleScore { get; set; }

        public int ConflictCount { get; set; }

        public FitnessBreakdown Breakdown { get; set; } = new();

        public ConstraintReport ConstraintReport { get; set; } = new();

        public List<string> Advantages { get; set; } = new();

        public List<string> Disadvantages { get; set; } = new();
    }

    public class FitnessBreakdown
    {
        public double LandScore { get; set; }

        public double HumanScore { get; set; }

        public double EquipmentScore { get; set; }

        public double ScheduleScore { get; set; }

        public double PenaltyScore { get; set; }

        public double BonusScore { get; set; }

        public double FinalScore { get; set; }
    }

    public class ConstraintReport
    {
        public List<string> LandConflicts { get; set; } = new();

        public List<string> HumanConflicts { get; set; } = new();

        public List<string> EquipmentConflicts { get; set; } = new();

        public List<string> ScheduleConflicts { get; set; } = new();

        public List<string> MaintenanceConflicts { get; set; } = new();

        public List<string> SkillConflicts { get; set; } = new();

        public List<string> RoleConflicts { get; set; } = new();

        public List<string> DeadlineConflicts { get; set; } = new();
    }
}

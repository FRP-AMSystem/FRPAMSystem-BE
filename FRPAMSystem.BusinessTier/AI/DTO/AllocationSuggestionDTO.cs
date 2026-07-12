namespace FRPAMSystem.BusinessTier.AI.DTO
{
    public class AllocationSuggestionDTO
    {
        public int Rank { get; set; }

        public double FitnessScore { get; set; }

        public double PenaltyScore { get; set; }

        public int ConflictCount { get; set; }

        public DateTime EstimatedCompletionTime { get; set; }

        public List<AllocatedLandDTO> AllocatedLands { get; set; } = new();

        public List<AllocatedHumanDTO> AllocatedHumans { get; set; } = new();

        public List<AllocatedEquipmentDTO> AllocatedEquipment { get; set; } = new();

        public List<TimelineItemDTO> Timeline { get; set; } = new();

        public List<string> Advantages { get; set; } = new();

        public List<string> Disadvantages { get; set; } = new();
    }

    public class AllocatedLandDTO
    {
        public int PhaseId { get; set; }

        public string PhaseName { get; set; } = string.Empty;

        public int? LandId { get; set; }

        public string? LandCode { get; set; }

        public string? SoilType { get; set; }

        public decimal AreaSize { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class AllocatedHumanDTO
    {
        public int PhaseId { get; set; }

        public string PhaseName { get; set; } = string.Empty;

        public int HumanResourceId { get; set; }

        public string? FullName { get; set; }

        public double CurrentWorkload { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class AllocatedEquipmentDTO
    {
        public int PhaseId { get; set; }

        public string PhaseName { get; set; } = string.Empty;

        public int? EquipmentInstanceId { get; set; }

        public string? AssetCode { get; set; }

        public int RequiredEquipmentTypeId { get; set; }

        public int AllocatedEquipmentTypeId { get; set; }

        public string? EquipmentTypeName { get; set; }

        public bool IsSubstitute { get; set; }

        public double EfficiencyRate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class TimelineItemDTO
    {
        public int PhaseId { get; set; }

        public string PhaseName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int DurationDays { get; set; }
    }
}

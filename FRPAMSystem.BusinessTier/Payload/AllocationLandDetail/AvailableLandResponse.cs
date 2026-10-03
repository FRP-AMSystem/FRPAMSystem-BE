using System;

namespace FRPAMSystem.BusinessTier.Payload.AllocationLandDetail
{
    public class AvailableLandResponse
    {
        public int LandId { get; set; }

        public string LandCode { get; set; } = null!;

        public int AreaId { get; set; }

        public string AreaName { get; set; } = null!;

        public decimal AreaSize { get; set; }

        public string? SoilType { get; set; }

        public string Status { get; set; } = null!;

        public bool IsAreaSufficient { get; set; }

        public bool IsSoilMatched { get; set; }

        public string MatchReason { get; set; } = null!;
    }
}

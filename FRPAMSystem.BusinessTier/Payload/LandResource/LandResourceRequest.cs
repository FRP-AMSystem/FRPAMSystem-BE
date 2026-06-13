using FRPAMSystem.BusinessTier.Enums;

namespace FRPAMSystem.BusinessTier.Payload.LandResource
{
    public class LandResourceRequest
    {
        public int AreaId { get; set; }

        public string LandCode { get; set; } = string.Empty;

        public decimal AreaSize { get; set; }

        public string? Location { get; set; }

        public string SoilType { get; set; } = string.Empty;

        public LandResourceStatus Status { get; set; } = LandResourceStatus.Available;
    }
}

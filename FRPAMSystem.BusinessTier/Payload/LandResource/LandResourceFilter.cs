using FRPAMSystem.BusinessTier.Enums;

namespace FRPAMSystem.BusinessTier.Payload.LandResource
{
    public class LandResourceFilter
    {
        public string? Keyword { get; set; }

        public int? AreaId { get; set; }

        public string? SoilType { get; set; }

        public LandResourceStatus? Status { get; set; }
    }
}

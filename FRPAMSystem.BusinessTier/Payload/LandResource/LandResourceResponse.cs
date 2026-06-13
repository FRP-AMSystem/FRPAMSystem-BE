namespace FRPAMSystem.BusinessTier.Payload.LandResource
{
    public class LandResourceResponse
    {
        public int LandId { get; set; }

        public int AreaId { get; set; }

        public string AreaName { get; set; } = string.Empty;

        public string LandCode { get; set; } = string.Empty;

        public decimal AreaSize { get; set; }

        public string? Location { get; set; }

        public string SoilType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}

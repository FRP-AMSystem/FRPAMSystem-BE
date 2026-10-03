using System;
using System.ComponentModel.DataAnnotations;

namespace FRPAMSystem.BusinessTier.Payload.AllocationLandDetail
{
    public class AvailableLandFilter
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal? RequiredArea { get; set; }

        public string? RequiredSoilType { get; set; }

        public int? AreaId { get; set; }

        public int? CurrentAllocationDetailId { get; set; }
    }
}

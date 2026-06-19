using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRPAMSystem.BusinessTier.Payload.AllocationLandDetail
{
    public class AllocationLandDetailResponse
    {
        public int AllocationLandDetailId { get; set; }

        public int AllocationPlanId { get; set; }

        public int ExperimentId { get; set; }

        public string? ExperimentName { get; set; }

        public int LandId { get; set; }

        public string? LandCode { get; set; }

        public int? AreaId { get; set; }

        public string? AreaName { get; set; }

        public decimal? AreaSize { get; set; }

        public string? Location { get; set; }

        public string? SoilType { get; set; }

        public int ExpLandReqId { get; set; }

        public decimal? RequiredArea { get; set; }

        public string? RequiredSoilType { get; set; }

        public string? RequirementNote { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
